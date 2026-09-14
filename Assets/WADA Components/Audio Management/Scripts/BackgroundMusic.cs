using System;
using System.Collections;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Wada
{
    public class BackgroundMusic : MonoBehaviour
    {
        public static event Action<BackgroundMusicType> OnEnded = delegate(BackgroundMusicType type) { };

        public bool Enabled = true; // we preload everything from the start because

        // of memory and delay issues, but we don't enable everything yet
        public BackgroundMusicType Type;

        public bool UseTrigger = true;
        public float FadeTime = 5;
        public float MaxVolume = 1;
        float multiplier = 1;

        protected AudioSource[] sources;
        float proxyVolume;

        protected bool ended;
        protected bool looping;

        bool isScoreArea;
        bool isRegularBackground;
        bool isWind;

        bool fadingIn = false;
        bool fadingOut = false;
        bool fadeOutEntirely;

        public void EndIt()
        {
            ended = true;
            OnEnded.Invoke( Type );
        }

        public void EndItDemo()
        {
            if ( sources.Length > 0 )
            {
                sources[0].time = sources[0].clip.length - 1;
            }
        }

        public bool IsFadingIn()
        {
            return fadingIn;
        }

        public bool IsFadingOut()
        {
            return fadingOut;
        }

        public bool IsPlaying()
        {
            // we should have enough through testing the first
            return sources.Length > 0 && sources[0].isPlaying;
        }

        public bool IsAtMaxVolume()
        {
            return proxyVolume == MaxVolume;
        }

        float DecibelToLinear(float dB)
        {
            float linear = Mathf.Pow( 10.0f, dB / 20.0f );

            return linear;
        }

        float LinearToDecibel(float linear)
        {
            float dB;

            if ( linear != 0 )
            {
                dB = 20.0f * Mathf.Log10( linear );
            }
            else
            {
                dB = -144.0f;
            }

            return dB;
        }

        protected virtual void Start()
        {
            isScoreArea = BackgroundMusicManager.GetInstance().IsScoreArea( Type );
            isWind = BackgroundMusicManager.GetInstance().IsWind( Type );
            isRegularBackground = !(BackgroundMusicManager.GetInstance().IsScore( Type ) ||
                                    BackgroundMusicManager.GetInstance().IsSpecial( Type ) ||
                                    isWind);

            sources = GetComponentsInChildren<AudioSource>();

            if ( sources.Length > 0 )
            {
                // they should all have the same intention
                looping = sources[0].loop;
            }

            SetVolume( MaxVolume );
            SetVolume( 0 );
            Pause();
            // so at least it's loaded; otherwise we get buffer issue. I am curious what this does though
            // memory/performance wise.
        }

        public virtual void FadeIn()
        {
            if ( fadingIn )
            {
                return;
            }

            fadingIn = true;

            if ( isRegularBackground && BackgroundMusicManager.GetInstance().IsInPlayingScoreTime() )
            {
                if ( isScoreArea )
                {
                    Debug.Log( "Fading In Score part and cancel stopping triggered from " + Type );
                    BackgroundMusicManager.GetInstance().CancelInvoke( "StopActiveScore" );
                    BackgroundMusicManager.GetInstance().CancelInvoke( "StopActiveScoreFast" );
                    BackgroundMusicManager.GetInstance().PlayActiveScore();
                    MaxOutVolume();
                    fadingIn = false;
                    return;
                }

                BackgroundMusicManager.GetInstance().StopActiveScore();
            }

            Debug.Log( "Fading In " + Type.ToString() );

            if ( !IsPlaying() )
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

        public void FadeOut(bool fast = false)
        {
            if ( fadingOut )
            {
                return;
            }

            fadingOut = true;

            if ( isRegularBackground && BackgroundMusicManager.GetInstance().IsInPlayingScoreTime() )
            {
                if ( isScoreArea )
                {
                    Debug.Log( "Fading Out Score part triggered from " + Type );
                    if ( fast )
                    {
                        BackgroundMusicManager.GetInstance().StopActiveScoreFast(); // debounce
                    }
                    else
                    {
                        BackgroundMusicManager.GetInstance().Invoke( "StopActiveScore", 1f ); // debounce
                    }
                }
            }

            Debug.Log( "Fading Out " + Type.ToString() );

            if ( fadingIn )
            {
                fadingIn = false;
                iTween.Stop( gameObject );
            }

            Hashtable valueTo = new();

            valueTo.Add( "from", proxyVolume );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", fast ? .1f : FadeTime * proxyVolume );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnVolumeValue" );
            valueTo.Add( "oncomplete", "OnFadeOutEnd" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void FadeOutEntirely()
        {
            fadeOutEntirely = true;
            FadeOut();
        }

        public void FadeOutEntirelyFast()
        {
            fadeOutEntirely = true;
            FadeOut( true );
        }

        public void MaxOutVolume()
        {
            SetVolume( MaxVolume );
        }

        public bool IsMuted()
        {
            return multiplier == 0;
        }

        public void Mute()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", multiplier );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", FadeTime * multiplier );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnMutedValue" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void Unmute()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", multiplier );
            valueTo.Add( "to", 1 );
            valueTo.Add( "time", FadeTime - FadeTime * multiplier );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnMutedValue" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void OnMutedValue(float value)
        {
            multiplier = value;
            SetVolume( proxyVolume );
        }

        void OnFadeInEnd()
        {
            fadingIn = false;
        }

        void OnFadeOutEnd()
        {
            fadingOut = false;
            fadeOutEntirely = false;
            Pause();
            Sync();
        }

        void OnVolumeValue(float value)
        {
            SetVolume( value );
        }

        void OnTriggerEnter(Collider other)
        {
            if ( Enabled && UseTrigger && (other.gameObject.tag == "Player" || other.gameObject.tag == "MainCamera") &&
                 !fadingIn && !fadeOutEntirely && proxyVolume < MaxVolume )
            {
                FadeIn();
            }
        }

        // OnTriggerStay so we can use multiple collision boxes for defining the area, but let's double check that
        void OnTriggerStay(Collider other)
        {
            if ( Enabled && UseTrigger && (other.gameObject.tag == "Player" || other.gameObject.tag == "MainCamera") &&
                 !fadingIn && !fadeOutEntirely && proxyVolume < MaxVolume )
            {
                FadeIn();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if ( Enabled && UseTrigger && (other.gameObject.tag == "Player" || other.gameObject.tag == "MainCamera") &&
                 !fadingOut && proxyVolume > 0 )
            {
                FadeOut();
            }
        }

        void Pause()
        {
            Debug.Log( "Pause " + Type );
            foreach ( AudioSource source in sources )
            {
                if ( source.gameObject.activeSelf )
                {
                    source.Pause();
                }
            }
        }

        public void Play()
        {
            foreach ( AudioSource source in sources )
            {
                if ( source.gameObject.activeSelf )
                {
                    source.Play();
                }
            }
        }

        public void ReEnableTrigger()
        {
            CancelInvoke( "ReEnablePhysics" );
            UseTrigger = true;
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            rb.isKinematic = false;
            Invoke( "ReEnablePhysics", .05f );
        }

        public void ReEnablePhysics()
        {
            Rigidbody rb = gameObject.GetComponent<Rigidbody>();
            rb.isKinematic = true;
        }

        public void Replay()
        {
            ResetTime();
            Play();
            ended = false;
        }

        public void ResetTime()
        {
            foreach ( AudioSource source in sources )
            {
                if ( source.gameObject.activeSelf )
                {
                    source.time = 0;
                }
            }
        }

        void SetVolume(float to)
        {
            proxyVolume = to;
            foreach ( AudioSource source in sources )
            {
                if ( source.gameObject.activeSelf )
                {
                    source.volume = to * multiplier;
                }
            }
        }

        void Sync()
        {
            float time = -1;
            foreach ( AudioSource source in sources )
            {
                if ( source.gameObject.activeSelf )
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
        }

        protected virtual void Update()
        {
            if ( Enabled && !ended && !looping && sources.Length > 0 &&
                 sources[0].isPlaying && sources[0].time >= sources[0].clip.length - .1f )
            {
                EndIt();
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(BackgroundMusic) )]
    public class BackgroundMusicEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            BackgroundMusic myTarget = (BackgroundMusic)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "End It Demo" ) )
            {
                myTarget.EndItDemo();
            }
        }
    }
#endif
}