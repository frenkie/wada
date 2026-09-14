using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Wada
{
    // TODO transform into a TimelineInstructionController
    public class InstructionsController : MonoBehaviour
    {
        public List<TimelineInstruction> Instructions;
        public PlayableDirector MainTimeline;
        public float StartDelay = 0;
        public float InstructionGap = 2f; // can be overriden by active instruction
        public bool RotateToPlayer = false; // only a Y rotation
        public bool AdvanceTimelineAfterAllInstructions = true;

        TimelineInstruction activeInstruction;

        //bool timelineAdvanced = false;
        bool killed = false;

        public void AdvanceTimeline()
        {
            //timelineAdvanced = true;
            if ( !killed )
            {
                MainTimeline.Play();
            }
        }

        void Awake()
        {
            foreach ( TimelineInstruction instruction in Instructions )
            {
                instruction.Instructions = this;
                instruction.gameObject.SetActive( false );
            }
        }

        public void Kill()
        {
            if ( !killed )
            {
                killed = true;
                if ( activeInstruction != null )
                {
                    activeInstruction.FadeOut();
                }
            }
        }

        void Rotate()
        {
            Hashtable rotateParams = new();

            Vector3 lookAt = new Vector3(
                Camera.main.transform.position.x,
                transform.position.y,
                Camera.main.transform.position.z
            ) - transform.position;

            rotateParams.Add( "y", Quaternion.LookRotation( lookAt ).eulerAngles.y );
            rotateParams.Add( "time", 2 );
            rotateParams.Add( "easetype", iTween.EaseType.easeInOutQuad );

            iTween.RotateTo( gameObject, rotateParams );
        }

        void Start()
        {
            if ( Instructions.Count > 0 )
            {
                Invoke( "ShowInstruction", StartDelay );
            }
        }

        public void ShowNextInstruction(float afterGap)
        {
            if ( !killed )
            {
                Invoke( "ShowInstruction", afterGap );
            }
        }

        void ShowInstruction()
        {
            if ( !killed && Instructions.Count > 0 )
            {
                if ( RotateToPlayer )
                {
                    Rotate();
                }

                activeInstruction = Instructions[0];
                Instructions.RemoveAt( 0 );
                activeInstruction.gameObject.SetActive( true );
                activeInstruction.Go();
            }
            else if ( AdvanceTimelineAfterAllInstructions )
            {
                AdvanceTimeline();
            }
        }
    }
}