using UnityEngine;

namespace Wada
{
    public class NotRevivableYet : MonoBehaviour
    {
        bool active;

        void OnTriggerStay(Collider other)
        {
            if ( !active && other.gameObject.tag == "Hand" )
            {
                active = true;

                InstructionManager.GetInstance().Play( AudioInstructionType.NotRevivable );
            }
        }

        void OnTriggerExit(Collider other)
        {
            if ( active && other.gameObject.tag == "Hand" )
            {
                CancelInvoke( "ReEnable" );
                Invoke( "ReEnable", 30f );
            }
        }

        void ReEnable()
        {
            active = false;
        }
    }
}