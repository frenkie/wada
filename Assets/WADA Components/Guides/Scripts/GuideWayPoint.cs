using System;
using UnityEngine;

namespace Wada
{
    public class GuideWayPoint : MonoBehaviour
    {
        public Transform NextWayPoint;
        public float Delay = 0;
        public bool WaitForPlayer = true;

        bool playerArrived = false;

        void OnTriggerEnter(Collider other)
        {
            Debug.Log( "Other is " + other.gameObject.name );
        }

        void OnTriggerStay(Collider other)
        {
            if ( other.gameObject.tag == "Player" )
            {
                playerArrived = true;
            }

            if ( (WaitForPlayer && playerArrived) || !WaitForPlayer )
            {
                Guide guide = other.gameObject.GetComponentInParent<Guide>();
                if ( guide != null && guide.IsGuide() )
                {
                    if ( NextWayPoint != null )
                    {
                        guide.SetTargetPoint( NextWayPoint, Delay );
                    }

                    GameEngine.GetInstance().ResumeTimeline();

                    // disable this trigger
                    gameObject.SetActive( false );
                }
            }
        }
    }
}