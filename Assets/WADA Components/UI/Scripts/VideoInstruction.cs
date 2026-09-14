using UnityEngine;
using UnityEngine.Video;
using System.Collections;

namespace Wada
{
    // For now we still have to find out the design, so this might be a child component
    //[RequireComponent( typeof(VideoPlayer) )]
    public class VideoInstruction : TimelineInstruction
    {
        public AudioClip Voice;
        public float ShowDuration = 3f;
        public float OverrideGapAfter = -1;
        public bool AdvanceTimelineAtStart = false;
        public bool DurationIsLookAt = false;
        public bool WaitForExternalProgression = false;
        public bool ShowNextInstructionOnFadeOut = true;

        float fadeDuration = .3f;
        float lookAtTime = 0;

        Material videoMaterial;

        Renderer renderer;
        protected VideoPlayer player;

        protected AudioSource audioSource;

        void Awake()
        {
            player = GetComponentInChildren<VideoPlayer>();
            player.playOnAwake = false; // to be sure

            renderer = GetComponent<Renderer>();
            videoMaterial = renderer.material;
            Color fadedOut = Color.white;
            fadedOut.a = 0;
            videoMaterial.color = fadedOut;

            if ( Voice != null )
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.clip = Voice;
            }
        }

        public override void FadeIn()
        {
            player.Play();

            Hashtable valueTo = new();

            valueTo.Add( "from", 0 );
            valueTo.Add( "to", 1 );
            valueTo.Add( "time", fadeDuration );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnFade" );
            valueTo.Add( "oncomplete", "OnFadedIn" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public override void FadeOut()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 1 );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", fadeDuration );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnFade" );
            valueTo.Add( "oncomplete", "OnFadeOut" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public override void Go()
        {
            FadeIn();
            if ( AdvanceTimelineAtStart )
            {
                Debug.Log( "Should be continuing the timeline..." );
                Instructions.AdvanceTimeline();
            }

            if ( !WaitForExternalProgression && !DurationIsLookAt )
            {
                Invoke( "FadeOut", fadeDuration + ShowDuration );
            }
        }

        void OnFade(float fadeToAlpha)
        {
            Color fadedColor = videoMaterial.color;
            fadedColor.a = fadeToAlpha;

            videoMaterial.color = fadedColor;
        }

        void OnFadedIn()
        {
            if ( Voice != null )
            {
                audioSource.Play();
            }
        }

        void OnFadeOut()
        {
            player.Stop();
            if ( ShowNextInstructionOnFadeOut )
            {
                Instructions.ShowNextInstruction(
                    OverrideGapAfter > -1 ? OverrideGapAfter : Instructions.InstructionGap );
            }

            FinishInstruction();
        }

        void Update()
        {
            if ( DurationIsLookAt )
            {
                // TODO: Check camera raycast and add to lookAtTime
                // Or can we determine when the text is 'in view' so we don't have to have a big raycast
                // Cause you know people's eyes can move while not looking directly at something
                // headset wise.
            }
        }
    }
}