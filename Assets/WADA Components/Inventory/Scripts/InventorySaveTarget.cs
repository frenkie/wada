using UnityEngine;

namespace Wada
{
    public class InventorySaveTarget : MonoBehaviour
    {
        GameObject activeHandGo;

        PlayerLocomotion locomotion;

        public void ClearActiveHand()
        {
            activeHandGo = null;
        }

        public WadaHand GetActiveHand()
        {
            return HasActiveHand() ? locomotion.GetWadaHandForCollider( activeHandGo ) : null;
        }

        public bool HasActiveHand()
        {
            return activeHandGo != null;
        }

        void OnTriggerStay(Collider other)
        {
            if ( other.gameObject.tag == "Hand" )
            {
                CancelInvoke( "ClearActiveHand" );
                activeHandGo = other.gameObject;
            }
        }

        void OnTriggerExit(Collider other)
        {
            if ( other.gameObject.tag == "Hand" && other.gameObject == activeHandGo )
            {
                Invoke( "ClearActiveHand", .08f );
            }
        }

        void Start()
        {
            locomotion = PlayerController.GetInstance().GetLocomotion();
        }
    }
}