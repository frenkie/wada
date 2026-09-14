using System;
using UnityEngine;

namespace Wada
{
    public class GuideFlockPoint : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            Guide guide = other.gameObject.GetComponentInParent<Guide>();
            Debug.Log( "Guide entered flock point" );
            if ( guide != null && guide.IsFlock() )
            {
                Debug.Log( "Guide gets added to flock" );

                guide.AddToFlocker();
                guide.SetInBirdMode();
                guide.HideVisually();
                guide.EnableFocusFactor();

                // disable this trigger
                gameObject.SetActive( false );
            }
        }
    }
}