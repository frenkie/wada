using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class NavigationTrigger : MonoBehaviour
    {
        public GameObject Target;
        public GameObject TriggerSubject;

        void Awake()
        {
            if ( gameObject.layer != LayerMask.NameToLayer( "NavigationInteractable" ) )
            {
                Debug.Log(
                    "[NavigationTrigger] needs the correct layer 'NavigationInteractable' for " + gameObject.name );
            }
        }

        public bool CanTriggerFor(GameObject subject)
        {
            bool allowed = true;

            if ( gameObject.layer != LayerMask.NameToLayer( "NavigationInteractable" ) )
            {
                allowed = false;
                Debug.Log(
                    "[NavigationTrigger] needs the correct layer 'NavigationInteractable' for " + gameObject.name );
            }

            if ( TriggerSubject != null && subject != TriggerSubject )
            {
                Debug.Log( "[NavigationTrigger] caller " + gameObject.name + " is not the TriggerSubject" );
                allowed = false;
            }

            return allowed;
        }
    }
}