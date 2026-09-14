using UnityEngine;

namespace Wada
{
    public class Veggie : AnimalGrabbable
    {
        void Awake()
        {
            base.Awake();

            grabbableType = GrabbableType.Veggie;
        }
    }
}