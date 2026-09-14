using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class BackgroundMusicRandomizer : BackgroundMusic
    {
        public List<AudioClip> Audio = new();
        public AudioClip FirstSkip;

        protected override void Start()
        {
            base.Start();

            AudioClip randomClip = GetRandomClip( FirstSkip );

            foreach ( AudioSource source in sources )
            {
                source.clip = randomClip;
                source.time = 0;
            }
        }

        AudioClip GetRandomClip(AudioClip skip)
        {
            List<AudioClip> clips = new();
            foreach ( AudioClip clip in Audio )
            {
                if ( clip != skip )
                {
                    clips.Add( clip );
                }
            }

            AudioClip randomClip = clips[Random.Range( 0, clips.Count )];
            return randomClip;
        }

        void ChooseRandomNewClip()
        {
            if ( Audio.Count > 1 && sources.Length > 0 )
            {
                // choose a random one from the list if it's not the current
                AudioClip current = sources[0].clip;
                AudioClip randomClip = GetRandomClip( current );

                Debug.Log( "[BackgroundMusicRandomizer] going to play " + randomClip.name );

                foreach ( AudioSource source in sources )
                {
                    source.clip = randomClip;
                    source.time = 0;
                    source.Play();
                }
            }
            else if ( sources.Length > 0 )
            {
                ResetTime();
                Play();
            }
        }

        protected override void Update()
        {
            if ( Enabled && !looping && sources.Length > 0 &&
                 sources[0].isPlaying && sources[0].time >= sources[0].clip.length - .1f )
            {
                ChooseRandomNewClip();
            }
        }
    }
}