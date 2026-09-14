using UnityEngine;

namespace Wada
{
    public class AudioInstruction : TimelineInstruction
    {
        public AudioClip Voice;
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
        }

        public override void FadeIn()
        {
            if ( Voice != null )
            {
                audioSource.Play();
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
                Invoke( "FadeOut", Voice.length );
            }
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