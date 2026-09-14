using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class Trigger : MonoBehaviour
    {
        public float Delay = 0f;
        public string TriggerKey;
        public GameObject TriggerSubject;

        void Awake()
        {
            if ( gameObject.layer != LayerMask.NameToLayer( "Interactable" ) )
            {
                Debug.Log( "[Trigger] needs the correct layer 'Interactable' for " + gameObject.name );
            }
        }

        public bool CanTriggerFor(GameObject subject)
        {
            bool allowed = true;

            if ( gameObject.layer != LayerMask.NameToLayer( "Interactable" ) )
            {
                allowed = false;
                Debug.Log( "[Trigger] needs the correct layer 'Interactable' for " + gameObject.name );
            }

            if ( TriggerSubject != null && subject != TriggerSubject )
            {
                Debug.Log( "[Trigger] caller " + gameObject.name + " is not the TriggerSubject" );
                allowed = false;
            }

            return allowed;
        }
    }
}