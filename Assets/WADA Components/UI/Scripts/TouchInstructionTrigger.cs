using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class TouchInstructionTrigger : MonoBehaviour
    {
        public float NudgeInterval = 60;

        bool triggered;
        bool touchedOne;

        float debounceNudgeTime;

        void Start()
        {
            Ressurectable.OnResurrect += OnTouched;
        }

        void OnTouched(Ressurectable ressurectable)
        {
            Ressurectable.OnResurrect -= OnTouched;
            touchedOne = true;

            gameObject.SetActive( false );
        }

        void OnTriggerStay(Collider collider)
        {
            if ( !(triggered && touchedOne) )
            {
                Trigger( collider );
            }
        }

        public void Trigger(Collider collider)
        {
            if ( collider == null || (collider != null && collider.gameObject.tag == "Player") )
            {
                if ( !triggered )
                {
                    Debug.Log( "[InstructionTrigger] triggered" );
                    debounceNudgeTime = NudgeInterval;
                    triggered = true;

                    InstructionManager.GetInstance().Play( AudioInstructionType.FirstTouch );
                }
                else if ( !touchedOne && debounceNudgeTime <= 0 )
                {
                    debounceNudgeTime = NudgeInterval;

                    if ( !(GameEngine.GetInstance().IsFocussedEnvironment() || WadaBook.GetInstance().IsShown()) )
                    {
                        InstructionManager.GetInstance().Play( AudioInstructionType.NudgeGoOnTouchIt );
                    }
                }
            }
        }

        void Update()
        {
            if ( debounceNudgeTime > 0 )
            {
                debounceNudgeTime -= Time.deltaTime;
            }

            if ( !(triggered && touchedOne) )
            {
                Guide guide = GameEngine.GetInstance().GetGuide();
                if ( guide != null && guide.IsScouting() )
                {
                    Ressurectable scoutTarget = guide.GetScoutTarget();
                    if ( scoutTarget != null )
                    {
                        Vector3 pos = scoutTarget.transform.position;
                        transform.position = pos;
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(TouchInstructionTrigger) )]
    public class TouchInstructionTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TouchInstructionTrigger myTarget = (TouchInstructionTrigger)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Trigger" ) )
            {
                myTarget.Trigger( null );
            }
        }
    }
#endif
}