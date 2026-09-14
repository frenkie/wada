using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Wada
{
    public class RiverTrashNoPhysics : MonoBehaviour
    {
        public RiverNoPhysics OwningRiverPart;
        public Bounds RiverPartBounds;
        public float DisappearDistance = .3f;
        public float AppearTime = 6f;
        public float Speed = 0.3f;
        public float RotationSpeed = 10f;
        public bool Rotate = false;
        public Vector3 LocalEndPosition;

        Vector3 direction;

        Vector3 localScale;

        Vector3 localStartPos;
        float totalDistance;

        Vector3 velocity = Vector3.zero;

        bool disappearing = false;
        bool move = true;
        float lifetime = 0;

        public void AdjustToRiver()
        {
            direction = (
                new Vector3(
                    RiverPartBounds.center.x,
                    RiverPartBounds.center.y,
                    RiverPartBounds.max.z
                ) -
                new Vector3(
                    RiverPartBounds.center.x,
                    RiverPartBounds.center.y,
                    RiverPartBounds.min.z
                )
            ).normalized;
            direction = Quaternion.AngleAxis( Random.Range( -60f, 60f ), Vector3.up ) * direction;
            localStartPos = transform.localPosition;
            totalDistance = Vector3.Distance( LocalEndPosition, localStartPos );
        }

        void Awake()
        {
            if ( AppearTime > 0 )
            {
                localScale = transform.localScale;
                transform.localScale = Vector3.zero;

                Invoke( "Appear", Random.Range( 0, .3f ) );
            }

            lifetime = Random.Range( 0f, 10f ); // Give each trash part it's own lifetime offset;
        }

        void Appear()
        {
            Hashtable scale = new();

            scale.Add( "scale", localScale );
            scale.Add( "time", AppearTime );

            iTween.ScaleTo( gameObject, scale );
        }

        void OnCollisionEnter(Collision other)
        {
            if ( !move && other.gameObject.tag == "WaterFallEnd" )
            {
                FadeOut();
            }
        }

        void Disappear()
        {
            Hashtable scale = new();

            scale.Add( "scale", Vector3.zero );
            scale.Add( "oncomplete", "OnScaledDown" );
            scale.Add( "time", 6f );

            iTween.ScaleTo( gameObject, scale );
        }

        public void FadeOut()
        {
            disappearing = true;
            Invoke( "Disappear", Random.Range( 0, 0.4f ) );
        }

        void Fall()
        {
            move = false;

            Rigidbody rb = gameObject.AddComponent( typeof(Rigidbody) ) as Rigidbody;
            BoxCollider collider = gameObject.AddComponent( typeof(BoxCollider) ) as BoxCollider;
            collider.size = Vector3.one * .6f;
            StartCoroutine( ForcedFall( rb ) );
        }

        protected IEnumerator ForcedFall(Rigidbody rb)
        {
            yield return new WaitForSeconds( Time.deltaTime );
            Vector3 localContinue = transform.localPosition + Vector3.right;
            Vector3 push = (transform.parent.TransformPoint( localContinue ) - transform.position).normalized;

            rb.AddForce( push * Random.Range( .75f, 1.25f ), ForceMode.Impulse );
        }

        void OnScaledDown()
        {
            Destroy( gameObject );
        }

        void Start()
        {
            if ( RiverPartBounds.max.x == 0 && transform.parent.GetComponent<MeshFilter>() != null )
            {
                RiverPartBounds = transform.parent.GetComponent<MeshFilter>().sharedMesh.bounds;
            }

            AdjustToRiver();
        }

        void OnDrawGizmos()
        {
            //Gizmos.color = Color.red;
            //Gizmos.DrawRay( transform.position, direction );
        }

        void Update()
        {
            lifetime += Time.deltaTime;

            if ( move )
            {
                // Target pos
                Vector3 localPos = transform.localPosition;

                localPos.x += Speed;

                float progress = Mathf.Min( Vector3.Distance( localPos, localStartPos ) / totalDistance, 1 );

                // TODO should be current target z and y plus the cos thingy
                localPos.y = Mathf.Lerp( localStartPos.y, LocalEndPosition.y, progress ) +
                             (float)Math.Sin( lifetime / 2 ) * .1f;
                localPos.z = Mathf.Lerp( localStartPos.z, LocalEndPosition.z, progress ) +
                             (float)Math.Cos( lifetime / 3 ) * .1f;

                transform.localPosition =
                    Vector3.SmoothDamp( transform.localPosition, localPos, ref velocity, 1 );
            }

            if ( Rotate )
            {
                transform.Rotate(
                    Time.deltaTime * -1 * RotationSpeed * direction,
                    Space.World
                );
            }

            if ( RiverPartBounds != null && !disappearing && RiverPartBounds.max.x > 0 &&
                 transform.localPosition.x > RiverPartBounds.max.x - DisappearDistance )
            {
                // pass on the trash or die out
                if ( OwningRiverPart.NextRiverPart != null )
                {
                    lifetime = 0;
                    OwningRiverPart.NextRiverPart.AdoptTrash( this );
                }
                else if ( OwningRiverPart.IsWaterfallDrop )
                {
                    disappearing = true;
                    Invoke( "Fall", 3f );
                }
                else
                {
                    FadeOut();
                }
            }
        }
    }
}