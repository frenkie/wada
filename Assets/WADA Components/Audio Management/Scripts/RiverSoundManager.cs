using System;
using System.Collections;
using Unity;
using UnityEngine;

namespace Wada
{
    public class RiverSoundManager : MonoBehaviour
    {
        public float MaxVolume = 1;
        public float MaxDistance = 8;
        public float FadeTime = 5;

        public Transform MovingRiver;
        public AudioSource[] sources;
        public RiverInCave RiverInCave;
        float proxyVolume;

        static RiverSoundManager instance;

        bool fadingIn = false;
        bool fadingOut = false;
        bool muted;
        bool ending;

        void Awake()
        {
            if ( instance != null && instance != this )
            {
                Debug.LogError( "[RiverSoundManager] Already initiated!" );
            }

            instance = this;

            foreach ( AudioSource source in sources )
            {
                source.maxDistance = MaxDistance;
            }

            RiverInCave.OnFadeIn += FadeInCave;
            RiverInCave.OnFadeOut += FadeOutCave;

            proxyVolume = MaxVolume;
            Mute();
        }

        bool isPlaying()
        {
            // we should have enough through testing the first
            return sources.Length > 0 && sources[0].isPlaying;
        }

        public static RiverSoundManager GetInstance()
        {
            return instance;
        }

        public bool FadingIn()
        {
            return fadingIn;
        }

        public bool FadingOut()
        {
            return fadingOut;
        }

        public void FadeIn()
        {
            fadingIn = true;

            Debug.Log( "Fading In RiverSound " );

            if ( !isPlaying() )
            {
                Play();
            }

            if ( fadingOut )
            {
                fadingOut = false;
                iTween.Stop( gameObject );
            }

            Hashtable valueTo = new();

            valueTo.Add( "from", proxyVolume );
            valueTo.Add( "to", MaxVolume );
            valueTo.Add( "time", FadeTime * (MaxVolume - proxyVolume) );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnVolumeValue" );
            valueTo.Add( "oncomplete", "OnFadeInEnd" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void FadeInCave()
        {
            FadeOut();
        }

        public void FadeOut()
        {
            fadingOut = true;

            Debug.Log( "Fading Out RiverSound" );

            if ( fadingIn )
            {
                fadingIn = false;
                iTween.Stop( gameObject );
            }

            Hashtable valueTo = new();

            valueTo.Add( "from", proxyVolume );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", FadeTime * proxyVolume );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnVolumeValue" );
            valueTo.Add( "oncomplete", "OnFadeOutEnd" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void FadeOutCave()
        {
        }

        public void FadeOutEnd()
        {
            ending = true;
            if ( proxyVolume > 0 )
            {
                FadeOut();
            }
        }

        float GetMinimalDistanceToPlayer()
        {
            Vector3 playerPos = PlayerController.GetInstance().GetLocation();
            float distance = Vector3.Distance( MovingRiver.position, playerPos );

            foreach ( AudioSource source in sources )
            {
                distance = Mathf.Min( distance, Vector3.Distance( source.gameObject.transform.position, playerPos ) );
            }

            return distance;
        }

        public bool IsAtMaxVolume()
        {
            return proxyVolume >= MaxVolume;
        }

        void Mute()
        {
            muted = true;
            foreach ( AudioSource source in sources )
            {
                source.volume = 0;
            }
        }

        void OnFadeInEnd()
        {
            fadingIn = false;
        }

        void OnFadeOutEnd()
        {
            fadingOut = false;
            Pause();
            Sync();
        }

        public void OnAnimalResurrected(Ressurectable resurrectable)
        {
            Mute();
            RiverInCave.Mute();
        }

        public void OnAnimalReleased(Ressurectable resurrectable)
        {
            Unmute();
            RiverInCave.Unmute();
        }

        void OnVolumeValue(float value)
        {
            SetVolume( value );
        }

        void Start()
        {
            Ressurectable.OnResurrect += OnAnimalResurrected;
            Ressurectable.OnRelease += OnAnimalReleased;
        }

        void Pause()
        {
            foreach ( AudioSource source in sources )
            {
                source.Pause();
            }
        }

        void Play()
        {
            foreach ( AudioSource source in sources )
            {
                source.Play();
            }
        }

        void SetVolume(float to)
        {
            proxyVolume = to;
            foreach ( AudioSource source in sources )
            {
                if ( !muted )
                {
                    source.volume = to;
                }
            }
        }

        void Sync()
        {
            float time = -1;
            foreach ( AudioSource source in sources )
            {
                if ( time == -1 )
                {
                    time = source.time;
                }
                else
                {
                    source.time = time;
                }
            }
        }

        public void Unmute()
        {
            muted = false;
            SetVolume( proxyVolume );
        }

        void FixedUpdate()
        {
            if ( !muted && !ending )
            {
                float distance = GetMinimalDistanceToPlayer();
                if ( proxyVolume > 0 && !fadingOut && distance >= MaxDistance + .3f )
                {
                    //FadeOut();
                }
                else if ( proxyVolume < MaxVolume && !fadingIn && distance <= MaxDistance - .3f )
                {
                    //FadeIn();
                }
            }
        }
    }
}