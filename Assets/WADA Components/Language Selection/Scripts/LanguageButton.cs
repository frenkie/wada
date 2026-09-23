using System;
using UnityEngine;
using NaughtyAttributes;

namespace Wada
{
    public class LanguageButton : MonoBehaviour
    {
        public GameObject Container;
        public Languages Language;
        public SoundEffect Sound;
        public SoundEffect ChooseLanguageVoice;

        bool languageChosen;
        bool triggered;

        void PushIt()
        {
            iTween.MoveBy(Container,  iTween.Hash("z", .2f, "easeType", "easeOutCubic", "islocal", true, "duration", .2f) );
            iTween.MoveBy(Container,  iTween.Hash("z", -.2f, "easeType", "easeOutCubic", "islocal", true, "duration", .2f ,"delay", .3f) );
        }
        
        void Start()
        {
            GameEngine.OnLanguageSwitch += OnLanguageSelected;
            if ( ! ChooseLanguageVoice.gameObject.activeSelf )
            {   
                ChooseLanguageVoice.gameObject.SetActive(true);
            }
        }

        void OnDisable()
        {
            GameEngine.OnLanguageSwitch -= OnLanguageSelected;
        }

        void OnLanguageSelected(Languages newLanguage)
        {
            languageChosen = true;
        }

        [Button]
        public void Trigger()
        {
            if ( !triggered && !languageChosen )
            {
                triggered = true;
                PushIt();
                PlayMusic();
                
                ChooseLanguageVoice.FadeOutAndKill();
                
                GameEngine.GetInstance().SwitchLanguage( Language );
                GameEngine.GetInstance().Invoke("ResumeTimeline", 2);

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
            Sound.Play();
        }
    }
    
}