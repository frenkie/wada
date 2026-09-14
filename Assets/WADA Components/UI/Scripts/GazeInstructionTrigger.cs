using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    /**
     * For now a simple x angle detection that triggers the Fade Out of a waiting instruction
     * TODO: Do we need a camera forward raycast?
     */
    public class GazeInstructionTrigger : MonoBehaviour
    {
        [Header( "The local X angle of the Camera" )]
        public float GazeAngle;

        public bool LookBelow;
        public float DelayTrigger;

        public TextInstruction Instruction;

        bool triggered = false;
        bool active = false;

        float threshholdTime = .25f;
        float lookTime;

        void Activate()
        {
            active = true;
        }

        void Start()
        {
            Invoke( "Activate", 1.5f );
        }

        public void Trigger()
        {
            if ( !triggered )
            {
                triggered = true;

                Instruction.Invoke( "FadeOut", DelayTrigger > 0 ? DelayTrigger : Time.deltaTime );
            }
        }

        void Update()
        {
            if ( active && !triggered )
            {
                float localX = Camera.main.transform.localEulerAngles.x;
                localX = Mathf.Repeat( localX + 180, 360 ) - 180;

                if ( (LookBelow && localX >= GazeAngle) || (!LookBelow && localX <= GazeAngle) )
                {
                    lookTime += Time.deltaTime;

                    if ( lookTime >= threshholdTime )
                    {
                        Trigger();
                    }
                }
                else
                {
                    lookTime = 0;
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(GazeInstructionTrigger) )]
    public class GazeInstructionTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GazeInstructionTrigger myTarget = (GazeInstructionTrigger)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Trigger" ) )
            {
                myTarget.Trigger();
            }
        }
    }
#endif
}