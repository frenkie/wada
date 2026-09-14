using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class PSAInstruction : AbstractInstruction
    {
        public List<InstructionFeature> DisableFeatures;
        public List<InstructionFeature> EnableFeatures;
        public GameObject HandInstruction;
        public float HandInstructionDelay;
        public GameObject SecondHandInstruction; // not gonna create an array, currently only happens once
        public float SecondHandInstructionDelay;

        SoundEffect psaSound;
        bool finished;

        void Awake()
        {
            psaSound = GetComponent<SoundEffect>();
            if ( psaSound == null )
            {
                Debug.LogError( "PSA instruction is missing the SoundEffect component or is ordered wrongly." );
            }
        }

        public override void FadeIn()
        {
            finished = false;
            psaSound.SetVolume( psaSound.Volume );
            psaSound.Play();
            float playingDuration = psaSound.GetPlayingDuration();
            Debug.Log( gameObject.name + " finishes after " + playingDuration );
            Invoke( "Finish", playingDuration ); // TODO ? FadeTime needed here?

            if ( HandInstruction != null )
            {
                if ( HandInstructionDelay > 0 )
                {
                    Invoke( "ShowHandInstruction", HandInstructionDelay );
                }
                else
                {
                    ShowHandInstruction();
                }
            }

            if ( SecondHandInstruction != null )
            {
                if ( SecondHandInstructionDelay > 0 )
                {
                    Invoke( "ShowSecondHandInstruction", SecondHandInstructionDelay );
                }
                else
                {
                    ShowSecondHandInstruction();
                }
            }
        }

        public override void FadeOut()
        {
            psaSound.FadeOutAndStop();
            Finish();
        }

        public void Finish()
        {
            CancelInvoke( "Finish" );
            if ( !finished )
            {
                finished = true;
                CancelInvoke( "ShowHandInstruction" );
                CancelInvoke( "ShowSecondHandInstruction" );
                if ( HandInstruction != null )
                {
                    HandInstruction.gameObject.SetActive( false );
                }

                if ( SecondHandInstruction != null )
                {
                    SecondHandInstruction.gameObject.SetActive( false );
                }

                ToggleFeaturesEnd();
                FinishInstruction();
            }
        }

        public float GetRemainingTime()
        {
            return psaSound.GetRemainingTime();
        }

        public override void Go()
        {
            ToggleFeaturesStart();
            FadeIn();
        }

        public void ShowHandInstruction()
        {
            if ( !finished )
            {
                InstructionFollowPlayer.GetInstance().ShowInstruction( HandInstruction, true, 0 );
            }
        }

        public void ShowSecondHandInstruction()
        {
            if ( !finished )
            {
                InstructionFollowPlayer.GetInstance().ShowInstruction( SecondHandInstruction, false, 0 );
            }
        }

        void ToggleFeaturesStart()
        {
            // only adding cases for potentials
            foreach ( InstructionFeature feature in EnableFeatures )
            {
                switch ( feature )
                {
                    case InstructionFeature.Hearts:
                        GameEngine.GetInstance().HeartCounter.Show();
                        break;
                }
            }

            foreach ( InstructionFeature feature in DisableFeatures )
            {
                switch ( feature )
                {
                    case InstructionFeature.Book:
                        GameEngine.GetInstance().ToggleBookGestureDetection( false );
                        break;

                    case InstructionFeature.Revival:
                        Ressurectable.DisableAllRevivals = true;
                        break;

                    case InstructionFeature.Workshop:
                        GameEngine.GetInstance().ToggleWorkshopGestureDetection( false );
                        break;
                }
            }
        }

        void ToggleFeaturesEnd()
        {
            foreach ( InstructionFeature feature in EnableFeatures )
            {
                switch ( feature )
                {
                    case InstructionFeature.Hearts:
                        GameEngine.GetInstance().HeartCounter.Hide();
                        break;
                }
            }

            foreach ( InstructionFeature feature in DisableFeatures )
            {
                switch ( feature )
                {
                    case InstructionFeature.Book:
                        if ( !(GameEngine.GetInstance().IsFocussedEnvironment() || Workshop.GetInstance().IsActive()) )
                        {
                            GameEngine.GetInstance().ToggleBookGestureDetection( true );
                        }

                        break;

                    case InstructionFeature.Revival:
                        if (
                            !(GameEngine.GetInstance().IsFocussedEnvironment() || Workshop.GetInstance().IsActive() ||
                              WadaBook.GetInstance().IsShown())
                        )
                        {
                            Ressurectable.DisableAllRevivals = false;
                        }

                        break;

                    case InstructionFeature.Workshop:
                        if ( !(GameEngine.GetInstance().IsFocussedEnvironment() || WadaBook.GetInstance().IsShown()) )
                        {
                            GameEngine.GetInstance().ToggleWorkshopGestureDetection( true );
                        }

                        break;
                }
            }
        }
    }
}