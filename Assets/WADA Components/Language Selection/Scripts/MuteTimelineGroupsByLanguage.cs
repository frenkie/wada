using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using NaughtyAttributes;

namespace Wada
{
    /// <summary>
    /// Mutes the language group (EN / NL) inside the timeline that does not match the active
    /// language, and does so while the timeline is already running.
    ///
    /// Note that a track's mute flag lives on the TimelineAsset, which is a shared project asset:
    /// a runtime change to it would survive leaving play mode in the Editor. This component
    /// therefore remembers the original flags the first time it touches a track and puts them
    /// back in OnDisable, which the Editor calls when play mode ends. Nothing persists.
    /// </summary>
    public class MuteTimelineGroupsByLanguage : MonoBehaviour
    {
        [Tooltip( "Leave empty to use the PlayableDirector on this object, or GameEngine.Timeliner." )]
        public PlayableDirector Timeliner;

        [Tooltip( "Group track names, exactly as they read in the Timeline window." )]
        public string EnglishGroup = "EN";
        public string DutchGroup = "NL";

        [Tooltip( "Also apply the language that is active when the scene starts." )]
        public bool ApplyOnStart = true;

        readonly Dictionary<TrackAsset, bool> originalMuted = new Dictionary<TrackAsset, bool>();


        void OnEnable()
        {
            GameEngine.OnLanguageSwitch += Apply;
        }

        void OnDisable()
        {
            GameEngine.OnLanguageSwitch -= Apply;
            RestoreOriginalMuteFlags();
        }

        void Start()
        {
            if ( ApplyOnStart && GameEngine.GetInstance() != null )
            {
                Apply( GameEngine.GetInstance().ActiveLanguage );
            }
        }


        [Button]
        public void TestEnglish() { Apply( Languages.EN ); }

        [Button]
        public void TestDutch() { Apply( Languages.NL ); }


        public void Apply(Languages language)
        {
            PlayableDirector director = GetDirector();
            if ( director == null )
            {
                return;
            }

            TimelineAsset timeline = director.playableAsset as TimelineAsset;
            if ( timeline == null )
            {
                return;
            }

            TrackAsset english = FindTrack( timeline, EnglishGroup );
            TrackAsset dutch = FindTrack( timeline, DutchGroup );

            if ( english == null || dutch == null )
            {
                Debug.LogWarning( "[MuteTimelineGroupsByLanguage] Could not find group '" +
                                  ( english == null ? EnglishGroup : DutchGroup ) + "' in " + timeline.name, this );
                return;
            }

            bool changed = SetMuted( english, language != Languages.EN );
            changed |= SetMuted( dutch, language != Languages.NL );

            if ( changed )
            {
                RebuildKeepingPosition( director );
            }
        }


        PlayableDirector GetDirector()
        {
            if ( Timeliner != null )
            {
                return Timeliner;
            }

            Timeliner = GetComponent<PlayableDirector>();
            if ( Timeliner == null && GameEngine.GetInstance() != null )
            {
                Timeliner = GameEngine.GetInstance().Timeliner;
            }

            return Timeliner;
        }


        /// <summary>Mutes or unmutes a track, remembering what it was before we ever touched it.</summary>
        bool SetMuted(TrackAsset track, bool muted)
        {
            if ( !originalMuted.ContainsKey( track ) )
            {
                originalMuted[ track ] = track.muted;
            }

            if ( track.muted == muted )
            {
                return false;
            }

            track.muted = muted; // child tracks follow through mutedInHierarchy
            return true;
        }


        void RestoreOriginalMuteFlags()
        {
            foreach ( KeyValuePair<TrackAsset, bool> pair in originalMuted )
            {
                if ( pair.Key != null )
                {
                    pair.Key.muted = pair.Value;
                }
            }

            originalMuted.Clear();
        }


        /// <summary>
        /// Timeline bakes the mute flags into the PlayableGraph when it is built, so a change
        /// while the director is live only takes effect after a rebuild. The rebuild resets the
        /// playhead, hence saving and restoring the time and the play state around it.
        /// </summary>
        void RebuildKeepingPosition(PlayableDirector director)
        {
            if ( !director.playableGraph.IsValid() )
            {
                return; // No graph yet: the new flags are used as soon as it starts.
            }

            double time = director.time;
            bool wasPlaying = director.state == PlayState.Playing;

            director.RebuildGraph();
            director.time = time;

            if ( wasPlaying )
            {
                director.Play();
            }
            else
            {
                director.Evaluate(); // paused: refresh the scene at this time without resuming
            }
        }


        static TrackAsset FindTrack(TimelineAsset timeline, string trackName)
        {
            foreach ( TrackAsset root in timeline.GetRootTracks() )
            {
                TrackAsset found = FindTrack( root, trackName );
                if ( found != null )
                {
                    return found;
                }
            }

            return null;
        }

        static TrackAsset FindTrack(TrackAsset track, string trackName)
        {
            if ( track.name == trackName )
            {
                return track;
            }

            // Group tracks have no output, so GetOutputTracks() would not find them; walk the children.
            foreach ( TrackAsset child in track.GetChildTracks() )
            {
                TrackAsset found = FindTrack( child, trackName );
                if ( found != null )
                {
                    return found;
                }
            }

            return null;
        }
    }
}
