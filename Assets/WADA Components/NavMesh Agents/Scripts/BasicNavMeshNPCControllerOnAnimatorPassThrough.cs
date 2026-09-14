using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class BasicNavMeshNPCControllerOnAnimatorPassThrough : MonoBehaviour
    {
        public BasicNavMeshNPCController Controller;

        void OnAnimatorMove()
        {
            Controller.OnAnimatorMove();
        }

        void OnAnimatedSound(AnimalSounds animalSound)
        {
            // TODO: because of fading animations we might lose specific triggers, so for now handling audio through code
            Controller.OnAnimatedSound( animalSound );
        }

        void OnAnimatedThrust()
        {
            Controller.OnAnimatedThrust();
        }
    }
}