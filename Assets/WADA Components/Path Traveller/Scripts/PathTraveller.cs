using System.Collections;
using UnityEngine;
using PathCreation;
using PathCreation.Examples;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class PathTraveller : MonoBehaviour
    {
        public GameObject PathFollower;
        public PathCreator Path;
        public float DistanceTravelled = 0f;
        public float MinDistanceTravelledForGainingUp = 10;
        public float MaxDistanceTravelledForGainingUp = 100;
        public bool AddToSpeedManually = false;
        public float SlowdownFactor = .06f;
        public float AcceleratorFactor = .12f;

        public float MAX_SPEED_START = .22f;

        bool automatedMoving = false;
        bool followPath = true;
        float maxSpeed = .22f;
        bool movingActive = false;
        bool playerHooked = false;

        float speed = 0f;
        Vector3 velocity = Vector3.zero;

        public void ActivateMoving()
        {
            if ( !movingActive )
            {
                movingActive = true;
            }
        }

        public void AddToSpeed()
        {
            if ( playerHooked )
            {
                speed = Mathf.Min( maxSpeed, speed + AcceleratorFactor * maxSpeed * Time.deltaTime );
            }
        }

        public void AddToSpeedManual(float speedToAdd)
        {
            if ( playerHooked )
            {
                speed = Mathf.Min( maxSpeed, speedToAdd );
            }
        }

        public void AnimateMaxSpeed(float to, float time = 3)
        {
            if ( maxSpeed != to )
            {
                Hashtable valueTo = new();

                valueTo.Add( "from", maxSpeed );
                valueTo.Add( "to", to );
                valueTo.Add( "time", time );
                valueTo.Add( "easetype", iTween.EaseType.linear );
                valueTo.Add( "onupdate", "AnimateMaxSpeedValue" );

                iTween.ValueTo( gameObject, valueTo );
            }
        }

        public void AnimateMaxSpeedValue(float value)
        {
            maxSpeed = value;
        }

        void Awake()
        {
            maxSpeed = MAX_SPEED_START;
        }

        public void ConnectPlayer()
        {
            // Connect the player to the path follower with a rubber band physics connection
            // springJoint.connectedBody = PlayerController.GetInstance().GetRigidbody();
            PlayerController.GetInstance().GetLocomotion().HookOnTo( this );
            playerHooked = true;
        }

        public void DisconnectPlayer()
        {
            // Unhook player
            // springJoint.connectedBody = null;
            PlayerController.GetInstance().GetLocomotion().HookRelease();
            playerHooked = false;
            followPath = false;
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

        public float GetMinimumDistancePerFrame()
        {
            return speed * Time.deltaTime;
        }

        public Vector3 GetPositionAtCurrentDistanceTravelled()
        {
            return Path.path.GetPointAtDistance( DistanceTravelled, EndOfPathInstruction.Stop );
        }

        public float GetSpeed()
        {
            return speed;
        }

        public void MoveToDistance(float distance)
        {
            DeactivateMoving();
            automatedMoving = true;

            Hashtable valueTo = new();

            valueTo.Add( "from", DistanceTravelled );
            valueTo.Add( "to", distance );
            valueTo.Add( "time", (distance - DistanceTravelled) / maxSpeed );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnMoveToValue" );
            valueTo.Add( "onupdatetarget", gameObject );
            valueTo.Add( "oncomplete", "OnMoveDone" );

            iTween.ValueTo( gameObject, valueTo );
            speed = 0f;
        }

        public void OnMoveToValue(float value)
        {
            DistanceTravelled = value;
        }

        public void OnMoveDone()
        {
            automatedMoving = false;
        }

        void OnTriggerStay(Collider other)
        {
            if ( enabled && gameObject.activeSelf && other.gameObject.tag == "Player" && !playerHooked )
            {
                Debug.Log( "[PathTraveller] " + gameObject.name + " Starting the great big adventure" );
                Reset();
                ConnectPlayer();
                ActivateMoving();
                //GameEngine.GetInstance().ResumeTimeline();
            }
        }

        public void Reset()
        {
            DistanceTravelled = 0;
            maxSpeed = MAX_SPEED_START;
            followPath = true;
        }

        public void SetSpeed(float to)
        {
            speed = to;
        }

        void Update()
        {
            if ( gameObject.activeSelf && followPath )
            {
                if ( movingActive )
                {
                    if ( !AddToSpeedManually )
                    {
                        AddToSpeed();
                    }

                    DistanceTravelled += speed * Time.deltaTime;

                    if ( speed > 0f )
                    {
                        speed = Mathf.Max( 0f, speed - SlowdownFactor * maxSpeed * Time.deltaTime );
                    }
                }

                Vector3 newPosition = GetPositionAtCurrentDistanceTravelled();

                if ( PathFollower.transform.position != newPosition )
                {
                    if ( automatedMoving )
                    {
                        PathFollower.transform.position = Vector3.SmoothDamp(
                            PathFollower.transform.position,
                            newPosition, ref velocity, .3f );
                    }
                    else
                    {
                        PathFollower.transform.position = newPosition;
                    }
                }

                if ( DistanceTravelled >= Path.path.length )
                {
                    DeactivateMoving();
                    followPath = false;
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(PathTraveller) )]
    public class PathTravellerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PathTraveller myTarget = (PathTraveller)target;

            DrawDefaultInspector();

            GUILayout.Label( "Moving state active? " + myTarget.GetActiveState().ToString() );

            if ( GUILayout.Button( "Activate Moving" ) )
            {
                myTarget.ActivateMoving();
            }

            if ( GUILayout.Button( "Deactivate Moving" ) )
            {
                myTarget.DeactivateMoving();
            }

            if ( GUILayout.Button( "ConnectingPlayer" ) )
            {
                myTarget.ConnectPlayer();
            }

            if ( GUILayout.Button( "Add To speed" ) )
            {
                myTarget.AddToSpeed();
            }

            if ( GUILayout.Button( "Set speed to 0.1f" ) )
            {
                myTarget.SetSpeed( .1f );
            }

            GUILayout.Label( "Speed: " + myTarget.GetSpeed() );
        }
    }
#endif
}