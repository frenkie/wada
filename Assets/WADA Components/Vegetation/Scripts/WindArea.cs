using UnityEngine;

namespace Wada
{
    public class WindArea : MonoBehaviour
    {
        public float Strength = 1;
        public float Turbulence = 1;
        public float PulseMagnitude = .5f;
        public float PulseFrequency = .01f;
        public float PulseVariation = .0025f;

        float windTime = 0;
        Vector3 force = Vector3.zero;

        void Awake()
        {
            Reset();
        }

        void FixedUpdate()
        {
            windTime += Time.fixedDeltaTime;

            if ( windTime >= 0 && windTime <= PulseMagnitude )
            {
                force = (Random.Range( 0, Turbulence ) + Strength) * transform.forward;
            }
            else
            {
                force = Vector3.zero;
                if ( windTime > PulseMagnitude )
                {
                    Reset();
                }
            }
        }

        public Vector3 GetForce()
        {
            return force;
        }

        void Reset()
        {
            force = Vector3.zero;
            windTime = -Mathf.Max( 0, PulseFrequency + Random.Range( -PulseVariation, PulseVariation ) );
        }
    }
}