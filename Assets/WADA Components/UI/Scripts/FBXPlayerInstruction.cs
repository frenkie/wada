using System;
using UnityEngine;
using System.Collections;
using TMPro;

namespace Wada
{
    public class FBXPlayerInstruction : TimelineInstruction
    {
        public AudioClip Voice;
        public FBXPlayer Player;
        public float ShowDuration = 3f;
        public float OverrideGapAfter = -1;
        public bool AdvanceTimelineAtStart = false;
        public bool DurationIsLookAt = false;
        public bool WaitForExternalProgression = false;

        float lookAtTime = 0;

        protected AudioSource audioSource;

        void Awake()
        {
            if ( Voice != null )
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.clip = Voice;
            }

            if ( Player != null )
            {
                Player.enabled = false;
            }
        }

        public override void FadeIn()
        {
            if ( Voice != null )
            {
                audioSource.Play();
            }

            if ( Player != null )
            {
                Player.enabled = true;
            }
        }

        public override void FadeOut()
        {
            Instructions.ShowNextInstruction( OverrideGapAfter > -1 ? OverrideGapAfter : Instructions.InstructionGap );

            audioSource.Stop();
            FinishInstruction();
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
                Invoke( "FadeOut", ShowDuration );
            }
        }

        void Start()
        {
            transform.position = new Vector3(
                transform.position.x,
                PlayerController.GetInstance().GetLocation().y,
                transform.position.z
            );
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