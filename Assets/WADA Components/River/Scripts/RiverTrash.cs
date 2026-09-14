using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Wada
{
    public class RiverTrash : MonoBehaviour
    {
        public Bounds RiverPartBounds;
        public float DisappearDistance = .3f;
        public float AppearTime = 6f;


        Vector3 localScale;

        bool disappearing = false;
        float lifetime = 0;

        void Awake()
        {
            if ( RiverPartBounds.max.x == 0 && transform.parent.GetComponent<MeshFilter>() != null )
            {
                RiverPartBounds = transform.parent.GetComponent<MeshFilter>().sharedMesh.bounds;
            }

            if ( AppearTime > 0 )
            {
                localScale = transform.localScale;
                transform.localScale = Vector3.zero;

                Invoke( "Appear", Random.Range( 0, .3f ) );
            }
        }

        void Appear()
        {
            Hashtable scale = new();

            scale.Add( "scale", localScale );
            scale.Add( "time", AppearTime );

            iTween.ScaleTo( gameObject, scale );
        }

        void Disappear()
        {
            Hashtable scale = new();

            scale.Add( "scale", Vector3.zero );
            scale.Add( "oncomplete", "OnScaledDown" );
            scale.Add( "time", 6f );

            iTween.ScaleTo( gameObject, scale );
        }

        void OnCollisionEnter(Collision other)
        {
            if ( !disappearing && LayerMask.LayerToName( other.gameObject.layer ) == "CleanUp" )
            {
                disappearing = true;
                Disappear();
            }
        }

        void OnScaledDown()
        {
            Destroy( gameObject );
        }

        void FixedUpdate()
        {
            if ( RiverPartBounds != null && !disappearing && RiverPartBounds.max.x > 0 &&
                 transform.localPosition.x > RiverPartBounds.max.x - DisappearDistance )
            {
                //
                disappearing = true;
                Invoke( "Disappear", Random.Range( 0, 0.4f ) );
            }
        }

        void Update()
        {
            lifetime += Time.deltaTime;

            if ( lifetime > 120 && !disappearing )
            {
                disappearing = true;
                Disappear();
            }
        }
    }
}