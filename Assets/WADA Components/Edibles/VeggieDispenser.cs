using System.Collections;
using UnityEngine;

namespace Wada
{
    public class VeggieDispenser : MonoBehaviour
    {
        public GameObject[] Veggies;
        public int VeggieCount = 10;
        public LayerMask AllowedTerrain;

        public Vector2 Bounds = new( 10, 10 );
        /*
         * within the bounds, throw rays down and when they hit a Navigation layer
         */

        IEnumerator DispenseVeggies()
        {
            int releasedVeggies = 0;
            int tries = 0;

            while ( releasedVeggies < VeggieCount && tries < 100 )
            {
                Vector3 randomPos = GetRandomPosition();

                RaycastHit hit;
                // let ray be blocked by places we don't want to drop our food, like animals and trees
                // let's test it
                if ( Physics.Raycast( randomPos, transform.TransformDirection( -Vector3.up ), out hit,
                        50 ) )
                {
                    if ( ((1 << hit.collider.gameObject.layer) & AllowedTerrain) != 0 )
                    {
                        // TODO maybe double check vicinity with hit.point
                        SpawnRandomVeggie( hit.point + new Vector3( 0, .5f, 0 ) );
                        releasedVeggies++;
                    }
                }

                tries++;
                yield return null; // just 1 veggie every frame should be enough during paradise entering
            }
        }

        Vector3 GetRandomPosition()
        {
            return transform.TransformPoint( new Vector3(
                Random.Range( -Bounds.x / 2, Bounds.x / 2 ), 0.0f,
                Random.Range( -Bounds.y / 2, Bounds.y / 2 ) ) );
        }

        void OnDrawGizmosSelected()
        {
            if ( Bounds.x != 0 && Bounds.y != 0 )
            {
                Gizmos.color = Color.green;

                Vector3 lbCorner = transform.TransformPoint(
                    new Vector3( -Bounds.x / 2, 0, -Bounds.y / 2 )
                );
                Vector3 ltCorner = transform.TransformPoint(
                    new Vector3( -Bounds.x / 2, 0, Bounds.y / 2 )
                );
                Vector3 rtCorner = transform.TransformPoint(
                    new Vector3( Bounds.x / 2, 0, Bounds.y / 2 )
                );
                Vector3 rbCorner = transform.TransformPoint(
                    new Vector3( Bounds.x / 2, 0, -Bounds.y / 2 )
                );

                Gizmos.DrawLine(
                    lbCorner,
                    ltCorner
                );
                Gizmos.DrawLine(
                    ltCorner,
                    rtCorner
                );
                Gizmos.DrawLine(
                    rtCorner,
                    rbCorner
                );
                Gizmos.DrawLine(
                    rbCorner,
                    lbCorner
                );
            }
        }

        void SpawnRandomVeggie(Vector3 position)
        {
            GameObject veggie = Instantiate( Veggies[Random.Range( 0, Veggies.Length )], transform );
            veggie.transform.position = position;
            veggie.transform.rotation = Random.rotation;
            veggie.transform.localScale *= Random.Range( 0.9f, 1.2f );
        }

        void Start()
        {
            if ( Veggies.Length > 0 )
            {
                StartCoroutine( DispenseVeggies() );
            }
        }
    }
}