using UnityEngine;

namespace Wada
{
    public class CloudRing : MonoBehaviour
    {
        public GameObject Prefab;
        public float Radius;
        public float MinSpawnRadius;

        public int ParticleAmount = 10;

        public float RotateSpeed = -10;

        void CreateParticles()
        {
            /*
             * Create ParticleAmount of items in 360 / Amount degrees per item
             * Within those degrees position it random in an angle
             * and random between MinSpawnRadius and Radius
             */

            float degreeRange = 360 / ParticleAmount;
            for ( int i = 0; i < ParticleAmount; i++ )
            {
                GameObject particle = Instantiate( Prefab, transform );
                float randomAngle = i * degreeRange + Random.Range( 0, degreeRange );
                float randomRadius = Random.Range( MinSpawnRadius, Radius );

                particle.transform.localPosition = new Vector3(
                    Mathf.Cos( Mathf.Deg2Rad * randomAngle ) * randomRadius,
                    0,
                    Mathf.Sin( Mathf.Deg2Rad * randomAngle ) * randomRadius
                );

                CloudItem cloudItem = particle.GetComponent<CloudItem>();
                cloudItem.MinDistanceFromCenter = MinSpawnRadius;
                cloudItem.MaxDistanceFromCenter = Radius;
            }
        }

        void Start()
        {
            CreateParticles();
        }

        void Update()
        {
            // TODO maybe randomize the speed once in a while to make the movement seem jittery
            transform.Rotate( Vector3.up, RotateSpeed * Time.deltaTime );
        }
    }
}