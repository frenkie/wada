using System;
using UnityEngine;
using UnityEditor;

namespace Wada
{
    public class StartButton : MonoBehaviour
    {
        public FadeIntroPhotogrammetry[] Title;
        public FadeIntroPhotogrammetry Krebs;
        public SoundEffect KrebsSound;
        public double JumpTime;

        Animator animator;
        bool triggered;

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        public void Trigger()
        {
            if ( !triggered )
            {
                triggered = true;
                animator.SetTrigger( "Start" );
                PlayMusic();

                // 12-07-2025
                BackgroundMusicManager.GetInstance().FadeIn( BackgroundMusicType.IntroB );
            }
        }

        public void OnAnimationFade()
        {
            foreach (FadeIntroPhotogrammetry title in Title)
            {
                if (title.gameObject.activeSelf)
                {
                    title.FadeOut();
                }
            }
            Krebs.FadeOut();
            Invoke( "OnAnimationEnd", .3f );
        }

        public void OnAnimationEnd()
        {
            GameEngine.GetInstance().ResumeTimeline();
        }

        public void OnTouch()
        {
            animator.SetTrigger( "Attention" );
        }

        void OnTriggerEnter(Collider other)
        {
            if ( other.gameObject.tag == "Hand" )
            {
                Trigger();
            }
        }

        public void PlayMusic()
        {
            KrebsSound.Play();
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(StartButton) )]
    public class StartButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            StartButton myTarget = (StartButton)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Trigger" ) )
            {
                myTarget.Trigger();
            }
        }
    }
#endif
}