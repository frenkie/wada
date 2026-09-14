using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Wada
{
    public class WorkshopToHeaven : MonoBehaviour
    {
        public event Action<AnimalController> OnReleased = delegate { };
        public AnimalController Animal;

        Vector3 startPosition;
        Vector3 startScale;
        Vector3 followVelocity = Vector3.zero;

        bool releasing = false;

        void Awake()
        {
            startScale = transform.localScale;
            transform.localScale = startScale * .5f;
        }

        public void Go()
        {
            releasing = true;
            startPosition = transform.position;
            Vector3 endPos = startPosition + new Vector3( 0, 5f, 0 );

            iTween.Stop( gameObject );

            foreach ( Renderer mesh in GetComponentsInChildren<Renderer>() )
            {
                mesh.enabled = false;
            }

            Hashtable moveTo = new();

            float speed = 1f;
            float time = Vector3.Distance( startPosition, endPos ) / speed;

            Debug.Log( "Going in " + time + " seconds" );

            moveTo.Add( "position", endPos );
            moveTo.Add( "time", time );
            moveTo.Add( "easetype", iTween.EaseType.easeInOutQuad );
            moveTo.Add( "oncomplete", "OnEnded" );

            iTween.MoveTo( gameObject, moveTo );
        }

        void Hover()
        {
            Hashtable valueTo = new();

            valueTo.Add( "position", startPosition + UnityEngine.Random.insideUnitSphere * .04f );
            valueTo.Add( "time", 5 );
            valueTo.Add( "easetype", iTween.EaseType.easeInOutCubic );
            valueTo.Add( "oncomplete", "OnHoverEnd" );

            iTween.MoveTo( gameObject, valueTo );
        }

        public void ForceRelease()
        {
            iTween.Stop( gameObject );
            Debug.Log( "Forcing release of copy to heaven" );
            OnEnded();
        }

        public void OnEnded()
        {
            if ( releasing )
            {
                Debug.Log( "Done with the copy to heaven" );
                releasing = false;
                OnReleased.Invoke( Animal );
                Destroy( gameObject );
            }
        }

        void OnEnable()
        {
            startPosition = transform.position;
            Hover();
        }

        void OnDisable()
        {
            iTween.Stop( gameObject );
        }

        void OnHoverEnd()
        {
            if ( !releasing )
            {
                Hover();
            }
        }

        public bool Releasing()
        {
            return releasing;
        }

        void Update()
        {
            if ( releasing )
            {
                if ( !Animal.IsGrabbed() )
                {
                    Animal.gameObject.transform.position = Vector3.SmoothDamp(
                        Animal.gameObject.transform.position, transform.position, ref followVelocity, .2f );
                }
            }
            else
            {
                transform.Rotate( Vector3.up * (3f * Time.deltaTime), Space.World );
            }
        }
    }
}