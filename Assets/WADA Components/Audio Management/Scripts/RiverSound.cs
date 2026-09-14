using System;
using UnityEngine;

namespace Wada
{
    public class RiverSound : MonoBehaviour
    {
        public bool Active = true;
        RiverSoundManager manager;

        void Awake()
        {
            manager = RiverSoundManager.GetInstance();
        }

        void OnTriggerStay(Collider other)
        {
            if ( Active && (other.gameObject.tag == "Player" || other.gameObject.tag == "MainCamera") &&
                 !manager.FadingIn() && !manager.IsAtMaxVolume() )
            {
                manager.FadeIn();
            }
        }

        void OnTriggerExit(Collider other)
        {
            if ( Active && (other.gameObject.tag == "Player" || other.gameObject.tag == "MainCamera") &&
                 !manager.FadingOut() )
            {
                manager.FadeOut();
            }
        }
    }
}