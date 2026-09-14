using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class ScriptTrigger : MonoBehaviour
    {
        public float Delay = 0f;
        public string TriggerMethod;
        public GameObject TriggerSubject;

        void Awake()
        {
            if ( gameObject.layer != LayerMask.NameToLayer( "ScriptInteractable" ) )
            {
                Debug.Log( "[ScriptTrigger] needs the correct layer 'ScriptInteractable' for " + gameObject.name );
            }
        }

        public bool CanTriggerFor(GameObject subject)
        {
            bool allowed = true;

            if ( gameObject.layer != LayerMask.NameToLayer( "ScriptInteractable" ) )
            {
                allowed = false;
                Debug.Log( "[ScriptTrigger] needs the correct layer 'ScriptInteractable' for " + gameObject.name );
            }

            if ( TriggerSubject != null && subject != TriggerSubject )
            {
                Debug.Log( "[ScriptTrigger] caller " + gameObject.name + " is not the TriggerSubject" );
                allowed = false;
            }

            return allowed;
        }
    }
}