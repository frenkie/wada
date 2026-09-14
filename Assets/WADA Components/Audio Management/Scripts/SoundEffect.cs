using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Wada
{
    public class SoundEffect : MonoBehaviour
    {
        public List<AudioClip> Audio = new();
        public bool PlayOneShot = true;
        [HideInInspector] public bool DestroyAfterPlay = false;
        public bool PlayOnStart = false;
        public int PlayOnStartIndex = -1;
        public bool Spatial = false;
        public bool Loop = false;
        public Vector2 SpatialRange = new( 1, 500 );
        public float Volume = 1;
        public bool Muted = false;
        public float TimeOffset;
        public AudioRolloffMode RolloffMode = AudioRolloffMode.Logarithmic;

        public bool MuteDuringRevival;

        [Header( "For use in the Fade methods" )]
        public float FadeTime = 4;

        float volumeForPassingOn;
        bool fadingIn = false;
        bool fadingOut = false;

        AudioSource source;
        bool startedPlaying = false;
        bool killed = false;
        bool muted;
        bool paused;

        void Start()
        {
            muted = Muted;
            float distance = GetDistanceFromPlayer();
            if ( muted || (Spatial && RolloffMode == AudioRolloffMode.Logarithmic && distance >= SpatialRange.y + .3f) )
            {
                volumeForPassingOn = 0;
            }
            else
            {
                volumeForPassingOn = Volume;
            }

            if ( PlayOneShot )
            {
                source = gameObject.GetComponent<AudioSource>();
                if ( source == null )
                {
                    source = gameObject.AddComponent<AudioSource>();
                }

                source.playOnAwake = false;
                source.volume = volumeForPassingOn;
                source.loop = Loop;
                source.spatialize = Spatial;
                source.spatialBlend = Spatial ? 1 : 0;
                source.rolloffMode = RolloffMode;
                source.minDistance = SpatialRange.x;
                source.maxDistance = SpatialRange.y;
            }

            if ( PlayOnStart )
            {
                Play( PlayOnStartIndex );
            }

            if ( MuteDuringRevival )
            {
                Ressurectable.OnResurrect += OnAnimalResurrected;
                Ressurectable.OnRelease += OnAnimalReleased;
            }
        }

        void CreateSoundEffectObject(AudioClip clip)
        {
            GameObject randomShot = new( "soundEffect" );
            randomShot.transform.parent = transform;
            randomShot.transform.localPosition = Vector3.zero;
            SoundEffect effect = randomShot.AddComponent<SoundEffect>();
            effect.DestroyAfterPlay = true;
            effect.PlayOnStart = true;
            effect.Loop = Loop;
            effect.Muted = Muted;
            effect.Audio.Add( clip );
            effect.Spatial = Spatial;
            effect.RolloffMode = RolloffMode;
            effect.TimeOffset = TimeOffset;
            effect.SpatialRange = SpatialRange;
            effect.Volume = volumeForPassingOn;
        }


        // TODO: only for a oneshot
        public void FadeIn()
        {
            if ( paused )
            {
                return;
            }

            fadingOut = false;
            fadingIn = true;

            if ( source != null )
            {
                source.time = 0;
                if ( !source.isPlaying )
                {
                    Play();
                }
            }

            iTween.Stop( gameObject );

            Hashtable valueTo = new();

            valueTo.Add( "from", volumeForPassingOn );
            valueTo.Add( "to", Volume );
            valueTo.Add( "time", FadeTime * (Volume - volumeForPassingOn) );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnVolumeValue" );
            valueTo.Add( "oncomplete", "OnFadeInDone" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void Kill()
        {
            Destroy( gameObject );
        }

        public void FadeOut()
        {
            fadingIn = false;
            fadingOut = true;
            iTween.Stop( gameObject );

            Hashtable valueTo = new();

            valueTo.Add( "from", volumeForPassingOn );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", FadeTime * volumeForPassingOn );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnVolumeValue" );
            valueTo.Add( "oncomplete", "OnFadeOutDone" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void FadeOutAndKill()
        {
            FadeOut();
            Invoke( "Kill", FadeTime * volumeForPassingOn );
        }

        public void FadeOutAndStop()
        {
            FadeOut();
            Invoke( "Stop", FadeTime * volumeForPassingOn );
        }

        float GetDistanceFromPlayer()
        {
            return Vector3.Distance( transform.position, PlayerController.GetInstance().GetLocation() );
        }

        public float GetPlayingDuration()
        {
            if ( source != null && source.isPlaying )
            {
                return source.clip.length;
            }

            return 0;
        }

        public float GetRemainingTime()
        {
            if ( source != null && source.isPlaying )
            {
                return source.clip.length - source.time;
            }

            return 0;
        }

        public float GetVolume()
        {
            if ( source != null && source.isPlaying )
            {
                return source.volume;
            }

            return 0;
        }

        AudioClip GetRandomClip()
        {
            if ( Audio.Count > 1 )
            {
                return Audio[Random.Range( 0, Audio.Count )];
            }

            return Audio[0];
        }

        public bool IsMuted()
        {
            return muted;
        }

        public bool IsPaused()
        {
            return paused;
        }

        public void Mute()
        {
            muted = true;
            OnVolumeValue( 0 );
        }

        public void OnAnimalResurrected(Ressurectable resurrectable)
        {
            Mute();
        }

        public void OnAnimalReleased(Ressurectable resurrectable)
        {
            Unmute();
        }

        void OnFadeInDone()
        {
            fadingIn = false;
        }

        void OnFadeOutDone()
        {
            fadingOut = false;
        }

        void OnVolumeValue(float value)
        {
            SetVolume( value );
        }

        public void Play(int index = -1)
        {
            if ( paused )
            {
                return;
            }

            if ( PlayOneShot )
            {
                source.clip = index > -1 ? Audio[index] : GetRandomClip();
                source.time = TimeOffset;
                source.Play();
            }
            else if ( volumeForPassingOn > 0 )
            {
                CreateSoundEffectObject( index > -1 ? Audio[index] : GetRandomClip() );
            }
        }

        public void Pause()
        {
            if ( !paused )
            {
                paused = true;
                if ( PlayOneShot && source != null && source.isPlaying )
                {
                    source.Pause();
                }
                // Else it will die itself out, but rather we could control this, so TODO
            }
        }

        public void Resume()
        {
            if ( paused )
            {
                paused = false;
                if ( PlayOneShot && source != null && !source.isPlaying )
                {
                    source.Play();
                }
            }
        }

        public void SetSource(int index)
        {
            source.clip = Audio[index];
        }

        public void SetVolume(float to)
        {
            if ( source != null )
            {
                source.volume = to;
            }

            volumeForPassingOn = to;
        }

        public void Stop()
        {
            if ( PlayOneShot && source != null )
            {
                source.Stop();
            }
            // else sound spawned sound object will destroy itself
        }

        public void StopFading()
        {
            iTween.Stop( gameObject );
        }

        void FixedUpdate()
        {
            if ( Spatial && !muted && !paused && source != null && source.rolloffMode != AudioRolloffMode.Linear )
            {
                // Because linear WILL go to zero

                float distance = GetDistanceFromPlayer();
                if ( volumeForPassingOn > 0 && !fadingOut && distance >= SpatialRange.y + .3f )
                {
                    FadeOut();
                }
                else if ( volumeForPassingOn < Volume && !fadingIn && distance <= SpatialRange.y - .3f )
                {
                    FadeIn();
                }
            }
        }

        public void Unmute()
        {
            muted = false;
            if ( !(Spatial && RolloffMode == AudioRolloffMode.Logarithmic) )
            {
                OnVolumeValue( Volume );
            }
        }

        void Update()
        {
            if ( DestroyAfterPlay )
            {
                if ( startedPlaying && !killed && source.clip.length > 0 && source.time >= source.clip.length - .1f )
                {
                    // TODO : did I do the -.1f because sometimes it stopped earlier?
                    killed = true;
                    Invoke( "Kill", .1f );
                }

                if ( source.time > 0 && source.isPlaying )
                {
                    startedPlaying = true;
                }
            }
        }
    }
}