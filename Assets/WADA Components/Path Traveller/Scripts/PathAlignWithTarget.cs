using System.Collections;
using UnityEngine;
using PathCreation;

namespace Wada
{
    public class PathAlignWithTarget : MonoBehaviour
    {
        public GameObject Target;
        public GameObject PathFollower;
        public PathCreator Path;
        public float DistanceTravelled = 0f;

        bool automatedMoving = false;
        bool followPath = true;
        bool movingActive = true;

        float speed = 0f;

        public void ActivateMoving()
        {
            if ( !movingActive )
            {
                movingActive = true;
            }
        }

        public void DeactivateMoving()
        {
            movingActive = false;
        }

        public bool GetActiveState()
        {
            return movingActive;
        }

        public float GetClosestDistanceAtPosition(Vector3 worldPosition)
        {
            return Path.path.GetClosestDistanceAlongPath( worldPosition );
        }

        public Vector3 GetPositionAtCurrentDistanceTravelled()
        {
            return Path.path.GetPointAtDistance( DistanceTravelled, EndOfPathInstruction.Stop );
        }

        public Vector3 GetPositionAheadOfCurrentDistanceTravelled()
        {
            return Path.path.GetPointAtDistance( DistanceTravelled + .1f, EndOfPathInstruction.Stop );
        }

        Quaternion GetOrientation()
        {
            Vector3 lookAt = GetPositionAheadOfCurrentDistanceTravelled() - PathFollower.transform.position;
            return Quaternion.LookRotation( lookAt );
        }

        public void Reset()
        {
            DistanceTravelled = 0;
            followPath = true;
        }

        void Update()
        {
            if ( gameObject.activeSelf && followPath )
            {
                if ( movingActive && Target != null )
                {
                    DistanceTravelled = GetClosestDistanceAtPosition( Target.transform.position );
                }

                Vector3 newPosition = GetPositionAtCurrentDistanceTravelled();

                if ( PathFollower.transform.position != newPosition )
                {
                    PathFollower.transform.position = newPosition;
                    PathFollower.transform.rotation = GetOrientation();
                }

                if ( DistanceTravelled >= Path.path.length )
                {
                    DeactivateMoving();
                    followPath = false;
                }
            }
        }
    }
}