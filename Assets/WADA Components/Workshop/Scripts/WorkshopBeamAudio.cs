using System;
using System.Collections;
using UnityEngine;

namespace Wada
{
    public class WorkshopBeamAudio : MonoBehaviour
    {
        AudioSource source;
        public float VolumeMultiplier = 1;

        bool fading = false;
        bool fadedIn = false;

        void Start()
        {
            source = GetComponentInChildren<AudioSource>();
        }

        public void FadeInSound()
        {
            source.Play();
            Hashtable valueTo = new();

            valueTo.Add( "from", 0 );
            valueTo.Add( "to", VolumeMultiplier );
            valueTo.Add( "time", 1 );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnVolumeValue" );
            valueTo.Add( "oncomplete", "OnFadedIn" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void FadeOutSound()
        {
            fadedIn = false;
            Hashtable valueTo = new();

            valueTo.Add( "from", VolumeMultiplier );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", 1 );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnVolumeValue" );
            valueTo.Add( "oncomplete", "OnFadedOut" );

            iTween.ValueTo( gameObject, valueTo );
        }

        void OnFadedIn()
        {
            fadedIn = true;
        }

        void OnFadedOut()
        {
            source.Stop();
        }

        void OnVolumeValue(float value)
        {
            VolumeMultiplier = value;
        }

        void Update()
        {
            if ( fadedIn )
            {
                float distance = Vector2.Distance(
                    new Vector2( transform.position.x, transform.position.z ),
                    new Vector2( Camera.main.transform.position.x, Camera.main.transform.position.z )
                );

                float volumeAtDistance = WadaMath.Remap( distance, 0.8f, 1.2f, 1f, 0.5f );
                volumeAtDistance = Mathf.Clamp( volumeAtDistance, 0.5f, 1f );

                source.volume = VolumeMultiplier * volumeAtDistance;
            }
        }
    }
}