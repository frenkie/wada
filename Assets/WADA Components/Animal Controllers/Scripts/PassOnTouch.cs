using System;
using UnityEngine;

namespace Wada
{
    public class PassOnTouch : MonoBehaviour
    {
        public GameObject TouchReceiver;
        ITouchReceiver receiver;
        PlayerLocomotion locomotion;
        bool active = false;

        void Awake()
        {
            receiver = TouchReceiver.GetComponent<ITouchReceiver>();
        }

        // TODO check if this is too expensive
        void OnTriggerStay(Collider other)
        {
            if ( active && receiver != null && other.gameObject.tag == "Hand" )
            {
                receiver.OnTouch( locomotion.GetWadaHandForCollider( other.gameObject ) );
            }
        }

        void OnTriggerExit(Collider other)
        {
            if ( active && receiver != null && other.gameObject.tag == "Hand" )
            {
                receiver.OnLeaveTouch( locomotion.GetWadaHandForCollider( other.gameObject ) );
            }
        }

        void Start()
        {
            locomotion = PlayerController.GetInstance().GetLocomotion();
            active = true;
        }
    }
}