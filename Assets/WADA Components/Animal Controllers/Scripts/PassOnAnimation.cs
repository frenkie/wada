using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class PassOnAnimation : MonoBehaviour
    {
        public AnimalGrabbable Controller;

        void OnAnimatorMove()
        {
            if ( Controller != null )
            {
                Controller.OnAnimatorMove();
            }
        }

        void OnAnimatedSound(AnimalSounds animalSound)
        {
            // leaving it to the controller that HAS this receiver
        }

        void OnAnimatedThrust()
        {
            if ( Controller != null )
            {
                Controller.OnAnimatedThrust();
            }
        }
    }
}