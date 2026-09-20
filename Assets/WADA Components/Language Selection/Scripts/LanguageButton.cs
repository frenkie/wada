using System;
using UnityEngine;
using NaughtyAttributes;

namespace Wada
{
    public class LanguageButton : MonoBehaviour
    {
        public Languages Language;
        public SoundEffect Sound;
        public SoundEffect ChooseLanguageVoice;
        
        bool triggered;


        void Start()
        {
            if ( ! ChooseLanguageVoice.gameObject.activeSelf )
            {   
                ChooseLanguageVoice.gameObject.SetActive(true);
            }
        }

        [Button]
        public void Trigger()
        {
            if ( !triggered )
            {
                triggered = true;
                //PlayMusic();
                
                ChooseLanguageVoice.FadeOutAndKill();
                
                GameEngine.GetInstance().SwitchLanguage( Language );
                GameEngine.GetInstance().ResumeTimeline();

                // 12-07-2025
                //BackgroundMusicManager.GetInstance().FadeIn( BackgroundMusicType.IntroB );
            }
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
            //Sound.Play();
        }
    }
    
}