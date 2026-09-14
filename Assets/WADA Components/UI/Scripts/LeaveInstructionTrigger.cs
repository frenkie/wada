using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class LeaveInstructionTrigger : MonoBehaviour
    {
        public AudioInstructionType[] Instructions;
        public float NudgeAfter = 40;

        bool triggered;

        float timer;

        public void Trigger(Collider collider)
        {
            if ( !triggered && (collider == null || (collider != null && collider.gameObject.tag == "Player")) )
            {
                Debug.Log( "[InstructionTrigger] triggered and disabled" );
                triggered = true;
                enabled = false;
            }
        }

        AudioInstructionType GetRandomInstruction()
        {
            return Instructions[Random.Range( 0, Instructions.Length )];
        }

        public void Nudge()
        {
            if ( !triggered )
            {
                InstructionManager.GetInstance().PlayIfSilent( GetRandomInstruction() );
            }
        }

        void OnTriggerExit(Collider collider)
        {
            Debug.Log( "[InstructionTrigger] OnTriggerExit, jeeejj" );
            Trigger( collider );
        }

        void Update()
        {
            if ( !triggered )
            {
                timer += Time.deltaTime;
                if ( timer >= NudgeAfter )
                {
                    timer = 0;
                    Nudge();
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(LeaveInstructionTrigger) )]
    public class LeaveInstructionTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            LeaveInstructionTrigger myTarget = (LeaveInstructionTrigger)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Trigger" ) )
            {
                myTarget.Trigger( null );
            }
        }
    }
#endif
}