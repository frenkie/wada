using System;
using UnityEngine;
using System.Collections;
using TMPro;

namespace Wada
{
    [RequireComponent( typeof(TextMeshPro) )]
    public class TextInstruction : TimelineInstruction
    {
        public AudioClip Voice;
        public float ShowDuration = 3f;
        public float OverrideGapAfter = -1;
        public bool AdvanceTimelineAtStart = false;
        public bool DurationIsLookAt = false;
        public bool WaitForExternalProgression = false;

        float fadeDuration = .3f;
        float lookAtTime = 0;

        protected TextMeshPro textMesh;

        protected AudioSource audioSource;

        void Awake()
        {
            textMesh = GetComponent<TextMeshPro>();

            Color fadedColor = textMesh.color;
            fadedColor.a = 0;
            textMesh.color = fadedColor;

            if ( Voice != null )
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.clip = Voice;
            }
        }

        public override void FadeIn()
        {
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
                Instructions.AdvanceTimeline();
            }

            if ( !WaitForExternalProgression && !DurationIsLookAt )
            {
                Invoke( "FadeOut", fadeDuration + ShowDuration );
            }
        }

        void OnFade(float fadeToAlpha)
        {
            Color fadedColor = textMesh.color;
            fadedColor.a = fadeToAlpha;

            textMesh.color = fadedColor;
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
            Instructions.ShowNextInstruction( OverrideGapAfter > -1 ? OverrideGapAfter : Instructions.InstructionGap );
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