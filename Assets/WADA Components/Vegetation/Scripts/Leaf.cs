using System;
using UnityEngine;

namespace Wada
{
    public class Leaf : MonoBehaviour
    {
        public float ForceMultiplier = 1;

        WindArea windArea;

        Rigidbody rigidBody;

        void Awake()
        {
            rigidBody = GetComponent<Rigidbody>();
        }

        void FixedUpdate()
        {
            if ( windArea != null )
            {
                if ( rigidBody.velocity.magnitude > 5 )
                {
                    Debug.Log( "Leaf is going crazy! Silencing :)" );
                    rigidBody.velocity = Vector3.zero;
                    rigidBody.angularVelocity = Vector3.zero;
                }

                rigidBody.AddForce( windArea.GetForce() * ForceMultiplier );
            }
        }

        void OnTriggerExit(Collider other)
        {
            if ( other.gameObject == windArea.gameObject )
            {
                windArea = null;
            }
        }

        void OnTriggerStay(Collider other)
        {
            if ( other.gameObject.tag == "WindArea" &&
                 (windArea == null || (windArea != null && other.gameObject != windArea.gameObject)) )
            {
                windArea = other.gameObject.GetComponent<WindArea>();
            }
        }
    }
}