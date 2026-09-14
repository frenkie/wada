using UnityEngine;

namespace Wada
{
    public class StartButtonPassOn : MonoBehaviour
    {
        public StartButton TouchReceiver;

        bool active = true;

        void OnTriggerStay(Collider other)
        {
            if ( active && TouchReceiver != null && other.gameObject.tag == "Hand" )
            {
                active = false;
                TouchReceiver.OnTouch();
            }
        }
    }
}