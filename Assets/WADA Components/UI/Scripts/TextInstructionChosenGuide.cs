using System;
using UnityEngine;
using System.Collections;
using TMPro;

namespace Wada
{
    public class TextInstructionChosenGuide : TextInstruction
    {
        public AudioClip VoiceDuck;
        public AudioClip VoiceCrow;

        void Start()
        {
            Guide guide = GameEngine.GetInstance().GetGuide();

            if ( guide != null )
            {
                GuideType chosenType = GameEngine.GetInstance().GetGuide().Type;
                textMesh.text = string.Format( textMesh.text, chosenType.ToString() );

                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;

                switch ( chosenType )
                {
                    case GuideType.Duck:
                        audioSource.clip = VoiceDuck;
                        break;

                    case GuideType.Crow:
                        audioSource.clip = VoiceCrow;
                        break;
                }
            }
        }

        void OnFadedIn()
        {
            if ( audioSource != null )
            {
                audioSource.Play();
            }
        }
    }
}