using System;
using System.Collections;
using UnityEngine;

namespace Wada
{
    public class Cloud : MonoBehaviour
    {
        AudioSource source;
        public float VolumeMultiplier = 1;
        public GameObject[] Visibles;
        public BackgroundMusic IntroMusic;
        public PathTraveller TravelerUp;

        const float MIN_VALUE = 0.3f;
        float proxyVolume;
        bool inParadise;

        void Start()
        {
            source = GetComponentInChildren<AudioSource>();
            if ( !inParadise )
            {
                FadeInSound();
            }


            Ressurectable.OnResurrect += OnAnimalResurrected;
            Ressurectable.OnRelease += OnAnimalReleased;
        }

        public void EnterParadiseMode()
        {
            inParadise = true;
            // hide
            foreach ( GameObject visible in Visibles )
            {
                visible.SetActive( false );
            }

            SoundForParadise();
        }

        public void ExitParadiseMode()
        {
            IntroMusic = null; // so it doesnt restart;
            //inParadise = false;
            // show
            foreach ( GameObject visible in Visibles )
            {
                visible.SetActive( true );
            }

            FadeOutSound();
        }

        public void FadeOutSound()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", VolumeMultiplier );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", 3 );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnVolumeValue" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void FadeInSound()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", proxyVolume );
            valueTo.Add( "to", VolumeMultiplier );
            valueTo.Add( "time", 3 );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnVolumeValue" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void OnAnimalResurrected(Ressurectable resurrectable)
        {
            source.mute = true;
        }

        public void OnAnimalReleased(Ressurectable resurrectable)
        {
            source.mute = false;
        }


        public void OnVolumeValue(float value)
        {
            proxyVolume = value;
            if ( inParadise )
            {
                source.volume = proxyVolume * 1;
            }
        }

        public void SoundForParadise()
        {
            Debug.Log( "SoundForParadise" );
            Hashtable valueTo = new();

            iTween.Stop( gameObject );

            valueTo.Add( "from", source.volume );
            valueTo.Add( "to", 0.2f );
            valueTo.Add( "time", 2 );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnVolumeValue" );

            iTween.ValueTo( gameObject, valueTo );
        }

        void Update()
        {
            if ( !(inParadise || source.mute) )
            {
                if ( Vector3.Distance( source.transform.position, Camera.main.transform.position ) > .8f &&
                     Camera.main.transform.position.y < source.transform.position.y )
                {
                    float fov = Vector3.AngleBetween( Camera.main.transform.forward,
                        Camera.main.transform.position + Vector3.up
                    ) * Mathf.Rad2Deg;

                    source.volume = Math.Max( WadaMath.Remap( fov, 80, 40, proxyVolume * MIN_VALUE, proxyVolume * 1 ),
                        proxyVolume * MIN_VALUE );

                    if ( IntroMusic != null )
                    {
                        IntroMusic.OnMutedValue(
                            WadaMath.Remap( Camera.main.transform.position.y - source.transform.position.y, -5.5f,
                                -.8f, 1,
                                0 ) );
                    }
                }
                else
                {
                    // Remamp clamp
                    if ( IntroMusic != null )
                    {
                        IntroMusic.OnMutedValue( 0 );
                    }

                    source.volume = proxyVolume * 1;
                }

                if ( TravelerUp != null )
                {
                    TravelerUp.SlowdownFactor = Math.Max( Math.Min( WadaMath.Remap(
                        Camera.main.transform.position.y - source.transform.position.y, -6f,
                        -2f, .2f,
                        0.001f ), .2f ), 0.001f );
                    TravelerUp.AcceleratorFactor = Math.Max( Math.Min( WadaMath.Remap(
                        Camera.main.transform.position.y - source.transform.position.y, -6f,
                        -2f, .6f,
                        1f ), 1f ), .6f );
                }
            }
        }
    }
}