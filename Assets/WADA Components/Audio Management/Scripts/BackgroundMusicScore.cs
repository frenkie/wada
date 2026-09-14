using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class BackgroundMusicScore : BackgroundMusic
    {
        public List<AudioClip> Audio = new();

        protected override void Start()
        {
            base.Start();

            AudioClip firstClip = GetNextClip();

            foreach ( AudioSource source in sources )
            {
                source.clip = firstClip;
                source.time = 0;
            }
        }

        AudioClip GetNextClip()
        {
            AudioClip nextClip = Audio[0];
            Audio.RemoveAt( 0 );

            return nextClip;
        }

        void ChooseNextClip()
        {
            if ( Audio.Count > 0 && sources.Length > 0 )
            {
                AudioClip nextClip = GetNextClip();

                Debug.Log( "[BackgroundMusicScore] going to play " + nextClip.name );

                foreach ( AudioSource source in sources )
                {
                    source.clip = nextClip;
                    source.time = 0;
                    source.Play();
                }
            }
        }

        protected override void Update()
        {
            if ( Enabled && !ended && !looping && sources.Length > 0 &&
                 sources[0].isPlaying && sources[0].time >= sources[0].clip.length - .1f )
            {
                if ( Audio.Count > 0 )
                {
                    ChooseNextClip();
                }
                else
                {
                    ended = true;
                    EndIt();
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(BackgroundMusicScore), true )]
    public class BackgroundMusicScoreEditor : BackgroundMusicEditor
    {
    }
#endif
}