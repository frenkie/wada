using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    [Serializable]
    public class BackgroundMusicTimedEntry
    {
        public Vector2 TimeRange;
        public AudioClip Clip;
        public bool Started;
    }

    public class BackgroundMusicTimed : BackgroundMusic
    {
        public List<BackgroundMusicTimedEntry> AudioOnTime = new();

        bool swapTimedMusic;

        public override void FadeIn()
        {
            if ( swapTimedMusic )
            {
                Debug.Log( "Going to swap music" );

                BackgroundMusicTimedEntry clipForTime =
                    GetClipEntryForTime( Clock.GetInstance().GetCurrentElapsedTime() );

                if ( clipForTime.Clip == null )
                {
                    Debug.LogError( "[BackgroundMusicTimed] No clip found for time" );
                    return;
                }

                if ( sources.Length > 0 )
                {
                    foreach ( AudioSource source in sources )
                    {
                        source.clip = clipForTime.Clip;
                    }
                }

                clipForTime.Started = true;
                swapTimedMusic = false;
            }

            base.FadeIn();
        }

        BackgroundMusicTimedEntry GetClipEntryForTime(float time)
        {
            foreach ( BackgroundMusicTimedEntry clipEntry in AudioOnTime )
            {
                if ( time >= clipEntry.TimeRange.x && time < clipEntry.TimeRange.y )
                {
                    return clipEntry;
                }
            }

            return new BackgroundMusicTimedEntry();
        }

        bool HasStartedClipForTime(float time)
        {
            BackgroundMusicTimedEntry clipEntry = GetClipEntryForTime( time );
            if ( clipEntry.Clip != null )
            {
                return clipEntry.Started;
            }

            return false;
        }

        public void OnCheckForTimedScoreChange(float progress)
        {
            float time = Clock.GetInstance().GetCurrentElapsedTime();

            if ( !swapTimedMusic && !HasStartedClipForTime( time ) )
            {
                swapTimedMusic = true;
                if ( IsPlaying() )
                {
                    FadeOutEntirely();
                }
                else
                {
                    Debug.Log( "[BackgroundMusicTimed] preparing swap next time it fades in" );
                }
            }
        }

        protected override void Start()
        {
            base.Start();

            AudioClip startClip = GetClipEntryForTime( 0 ).Clip;

            foreach ( AudioSource source in sources )
            {
                source.clip = startClip;
                source.time = 0;
            }

            Clock.OnProgress += OnCheckForTimedScoreChange;
        }
    }
}