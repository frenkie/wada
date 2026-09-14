using UnityEngine;

namespace Wada
{
    public class PathSpeedTrigger : MonoBehaviour
    {
        public float NextDistance;
        public float TimeToTravel;
        public PathTraveller Traveller;

        public void Trigger(Collider collider)
        {
            if ( collider == null || (collider != null && collider.gameObject.tag == "Player") )
            {
                float newMaxSpeed =
                    (NextDistance - Traveller.DistanceTravelled) /
                    (TimeToTravel - 3); // -3 for the animation to take place
                Traveller.AnimateMaxSpeed( newMaxSpeed );
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            Debug.Log( "[PathSpeedTrigger] OnTriggerEnter" );
            Trigger( collider );
        }
    }
}