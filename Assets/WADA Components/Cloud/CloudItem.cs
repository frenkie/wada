using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Wada
{
    public class CloudItem : MonoBehaviour
    {
        public float MaxDistanceFromCenter;
        public float MinDistanceFromCenter;

        Vector3 localScale;

        Vector3 localStartPoint;
        float x;
        float y;
        float z;

        Vector3 inOutTarget;
        Vector3 inOutStart;

        void Appear()
        {
            Hashtable scale = new();

            scale.Add( "scale", localScale );
            scale.Add( "time", .3f );

            iTween.ScaleTo( gameObject, scale );

            transform.rotation = Random.rotation;
        }

        void Awake()
        {
            localScale = transform.localScale * Random.Range( .6f, .75f );
            transform.localScale = Vector3.zero;
        }

        void Start()
        {
            localStartPoint = transform.localPosition;
            x = localStartPoint.x;
            y = localStartPoint.y;
            z = localStartPoint.z;

            Invoke( "Appear", Random.Range( 0, .3f ) );

            /*
             *
             * Maybe sometimes randomly move a little along the center forwards or backwards
             */

            UpDown();
            InOut();
        }

        void InOut()
        {
            Vector3 border = localStartPoint.normalized * MaxDistanceFromCenter;
            Vector3 centerMin = localStartPoint.normalized * MinDistanceFromCenter;

            inOutStart = new Vector3(
                transform.localPosition.x,
                localStartPoint.y,
                transform.localPosition.z
            );

            if ( Vector3.Distance( border, inOutStart ) <
                 Vector3.Distance( centerMin, inOutStart ) )
            {
                // move to the center
                inOutTarget = Vector3.Lerp( centerMin,
                    localStartPoint.normalized * ((MaxDistanceFromCenter - MinDistanceFromCenter) / 2),
                    Random.Range( 0f, 1f ) );
            }
            else
            {
                // move to the border
                inOutTarget = Vector3.Lerp(
                    localStartPoint.normalized * ((MaxDistanceFromCenter - MinDistanceFromCenter) / 2), border,
                    Random.Range( 0f, 1f ) );
            }

            Hashtable valueTo = new();

            float distance = Vector3.Distance( inOutStart, inOutTarget );

            valueTo.Add( "from", 0 );
            valueTo.Add( "to", 1 );
            valueTo.Add( "time", distance / Random.Range( .5f, 1f ) );
            valueTo.Add( "easetype", iTween.EaseType.easeInOutSine );
            valueTo.Add( "onupdate", "InOutValue" );
            valueTo.Add( "oncomplete", "InOutEnd" );

            iTween.ValueTo( gameObject, valueTo );
        }

        void InOutValue(float value)
        {
            Vector3 curr = Vector3.Lerp( inOutStart, inOutTarget, value );
            x = curr.x;
            z = curr.z;
        }

        void InOutEnd()
        {
            InOut();
        }


        void UpDown()
        {
            Hashtable valueTo = new();

            float distance = Random.Range( .3f, .6f );
            float target = y > localStartPoint.y ? localStartPoint.y - distance : distance;

            valueTo.Add( "from", y );
            valueTo.Add( "to", target );
            valueTo.Add( "time", distance / Random.Range( .5f, 1f ) );
            valueTo.Add( "easetype", iTween.EaseType.easeInOutSine );
            valueTo.Add( "onupdate", "UpDownValue" );
            valueTo.Add( "oncomplete", "UpDownEnd" );

            iTween.ValueTo( gameObject, valueTo );
        }

        void UpDownValue(float value)
        {
            y = value;
        }

        void UpDownEnd()
        {
            UpDown();
        }

        void Update()
        {
            transform.localPosition = new Vector3( x, y, z );
        }
    }
}