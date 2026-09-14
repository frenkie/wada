using System;
using UnityEngine;

namespace Wada
{
    public class GuidePassOn : MonoBehaviour
    {
        public Guide Guide;
        public PassOnTouch PassOnTouch;
        public AudioSource IntroSound;
        public AudioClip IntroFall;
        public AudioClip IntroGetUp;

        // Called by the intro animation when reaching the ground
        public void EnablePassOnTouch()
        {
            PassOnTouch.enabled = true;
        }

        public void OnFlap()
        {
            Guide.Flap();
        }

        // Called after the intro animation's getup
        public void OnIntroAnimationEnd()
        {
            Guide.OnEndIntro();
        }

        public void OnIntroFallAnimation()
        {
            IntroSound.clip = IntroFall;
            IntroSound.Play();
        }
    }
}