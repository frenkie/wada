using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    [RequireComponent( typeof(Rigidbody) )]
    public class PlayerLocomotion : MonoBehaviour
    {
        public bool AllowMovementFromStart = false;
        public float MoveForce;
        public float Resistance;
        public float SoundInterval = 0.2f;
        public float ThrustInterval;
        public float DeadZone;
        public WadaHand LeftHand;
        public WadaHand RightHand;
        public float GravityScale;
        public Vector2 HookOnRange;
        public bool FallInEditor;

        public GameObject SwimSound;
        public List<AudioClip> SwimSounds;

        public InputAction DebugMove;

        public bool DebugValues = false;
        public Text DebugDirection;
        public Text DebugRbVelocity;
        public Text DebugCurrDirection;

        bool hasHands = false;
        bool inParadise = false;

        Vector3 currentDirection;
        float currentThrustWaitTime;
        float currentSoundWaitTime;
        bool canMove;
        bool canFall; // will be activated in paradise
        const float GLOBAL_GRAVITY = -9.81f;
        PathTraveller hookPath;
        Vector3 hookPathOffset;

        const float MAX_SPEED = 2; // anything larger is an error
        Rigidbody rigidbody;
        SphereCollider collider;

        List<float> speeds = new();
        List<Vector3> directions = new();

        Vector3 velocity = Vector3.zero;

        public void AddMovementForce(Vector3 vel)
        {
            rigidbody.AddForce( vel * MoveForce, ForceMode.Acceleration );
            currentDirection = GetDirectionAverage();
        }

        public void AddMovementToPath(Vector3 vel)
        {
            hookPath.AddToSpeedManual( hookPath.MAX_SPEED_START );

            // Vector3 towardsTarget = hookPath.PathFollower.transform.position - transform.position;
            // float distance = Vector3.Distance( hookPath.PathFollower.transform.position, transform.position );
            // float breakForce = WadaMath.Remap(
            //     distance,
            //     HookOnRange.x,
            //     HookOnRange.y,
            //     0, MoveForce + Resistance
            // );
            // if ( breakForce > 0 )
            // {
            //     float pathDistance = hookPath.GetClosestDistanceAtPosition( Camera.main.transform.position );
            //     if ( pathDistance >= hookPath.MinDistanceTravelledForGainingUp &&
            //          pathDistance <= hookPath.MaxDistanceTravelledForGainingUp &&
            //          pathDistance > hookPath.DistanceTravelled + hookPath.GetMinimumDistancePerFrame() )
            //     {
            //         hookPath.DistanceTravelled = pathDistance;
            //     }
            //     else
            //     {
            //         rigidbody.AddForce( breakForce * Time.deltaTime * towardsTarget.normalized,
            //             ForceMode.Acceleration );
            //     }
            // }
        }

        public void AllowMovement(bool allow)
        {
            canMove = allow;
        }

        void Awake()
        {
            hasHands = LeftHand != null && RightHand != null;
            collider = GetComponent<SphereCollider>();
            rigidbody = GetComponent<Rigidbody>();
            rigidbody.useGravity = false;
            currentSoundWaitTime = SoundInterval;
            // start swimming right away
            if ( AllowMovementFromStart )
            {
                AllowMovement( true );
            }
        }

        void ClearCurrentDirection()
        {
            currentDirection = Vector3.zero;
        }

        void CreateSwimSound()
        {
            // GameObject swimSound = Instantiate( SwimSound, Camera.main.transform.position, Quaternion.identity );
            // SwimSound swimSoundScript = swimSound.GetComponent<SwimSound>();
            // swimSoundScript.Audio = SwimSounds[Random.Range( 0, SwimSounds.Count )];
        }

        public void EnterFocusMode()
        {
            canFall = false;
            canMove = false;
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;
        }

        public void EnterCreditsMode()
        {
            canFall = true;
            canMove = false;
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.isKinematic = false;
            rigidbody.velocity = Vector3.zero;
        }

        public void ExitFocusMode()
        {
            rigidbody.isKinematic = false;
            canFall = true;
            canMove = true;
        }

        public void EnterWorkshopMode()
        {
            canFall = false;
            canMove = false;
            rigidbody.isKinematic = true;
            rigidbody.velocity = Vector3.zero;
        }

        public void ExitWorkshopMode()
        {
            rigidbody.isKinematic = false;
            canFall = true;
            canMove = true;
        }

        void FixedUpdate()
        {
            // calculate the moving force and direction and use it
            if ( hasHands && canMove )
            {
                float leftSpeed = LeftHand.GetSpeed();
                float rightSpeed = RightHand.GetSpeed();

                currentSoundWaitTime +=
                    Time.deltaTime; // always ready to make a swim sound the first time we pass the move threshold

                if ( (leftSpeed >= DeadZone || rightSpeed >= DeadZone) &&
                     !(leftSpeed > MAX_SPEED || rightSpeed > MAX_SPEED) )
                {
                    CancelInvoke( "ClearCurrentDirection" );
                    //Debug.Log( "Speed: " + LeftHand.GetSpeed() );
                    currentThrustWaitTime += Time.deltaTime;

                    // first ones
                    if ( speeds.Count < 5 )
                    {
                        speeds.Add( (leftSpeed + rightSpeed) / 2 );
                    }

                    // add to a list of directions if not an insane magnitude or big change vs the previous
                    // plus max of 10? and at the interval the direction will be the average?
                    if ( directions.Count >= 10 )
                    {
                        directions.RemoveAt( 0 );
                    }

                    // with GetForward the hands can determine what direction that is
                    directions.Add(
                        ((leftSpeed >= DeadZone ? LeftHand.GetForward( true ) : Vector3.zero) +
                         (rightSpeed >= DeadZone ? RightHand.GetForward( true ) : Vector3.zero)
                        ) /
                        2 );

                    if ( currentThrustWaitTime > ThrustInterval )
                    {
                        currentThrustWaitTime = 0;

                        Vector3 speed = GetDirectionAverage() * GetSpeedAverage();
                        DebugDirection.text = "" + speed;

                        if ( hookPath != null )
                        {
                            AddMovementToPath( speed );
                        }
                        else
                        {
                            AddMovementForce( speed );
                        }


                        speeds = new List<float>();
                        directions = new List<Vector3>();

                        //Debug.Log( "Current direction: " + currentDirection );

                        if ( currentSoundWaitTime > SoundInterval )
                        {
                            CreateSwimSound();
                            currentSoundWaitTime = 0;
                        }
                    }
                }
                else
                {
                    currentThrustWaitTime = 0;
                }

                if ( hookPath == null )
                {
                    if ( rigidbody.velocity.sqrMagnitude > 0.001f &&
                         !currentDirection.Equals( Vector3.zero ) )
                    {
                        CancelInvoke( "ClearCurrentDirection" );
                        rigidbody.AddForce( -rigidbody.velocity * Resistance, ForceMode.Acceleration );
                    }
                    else
                    {
                        Invoke( "ClearCurrentDirection", Time.deltaTime * 3 );
                    }
                }

                DebugCurrDirection.text = "" + currentDirection;
                DebugRbVelocity.text = "" + rigidbody.velocity.sqrMagnitude;
            }

            if ( canFall && FallInEditor )
            {
                Vector3 gravity = GLOBAL_GRAVITY * GravityScale * Vector3.up;
                rigidbody.AddForce( gravity, ForceMode.Acceleration );
            }
        }

        Vector3 GetDirectionAverage()
        {
            Vector3 direction = Vector3.zero;
            if ( directions.Count > 0 )
            {
                foreach ( Vector3 d in directions )
                {
                    direction += d;
                }

                direction /= directions.Count;
            }

            return direction;
        }

        float GetSpeedAverage()
        {
            float speed = 0;
            if ( speeds.Count > 0 )
            {
                foreach ( float s in speeds )
                {
                    speed += s;
                }

                speed /= speeds.Count;
            }

            return speed;
        }

        public WadaHand GetWadaHandForCollider(GameObject collider)
        {
            if ( LeftHand.Collider == collider )
            {
                return LeftHand;
            }

            if ( RightHand.Collider == collider )
            {
                return RightHand;
            }

            return null;
        }

        public void HookOnTo(PathTraveller target)
        {
            rigidbody.velocity = Vector3.zero;
            hookPathOffset = transform.position - target.GetPositionAtCurrentDistanceTravelled();
            hookPath = target;
        }

        public void HookRelease()
        {
            hookPathOffset = Vector3.zero;
            hookPath = null;
        }

        void LateUpdate()
        {
            if ( hasHands && canMove && hookPath != null )
            {
                Vector3 target = hookPath.PathFollower.transform.position + hookPathOffset;

                transform.position = Vector3.SmoothDamp(
                    transform.position, target
                    , ref velocity, Time.deltaTime * 0.03f );
            }
        }

        public void MoveTo(Vector3 location)
        {
            // TODO do we need to set the camera there or just the rig
            transform.position = location;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if ( Application.isPlaying && DebugValues )
            {
                Vector3 startPoint = Camera.main.transform.position + 2 * Camera.main.transform.forward;
                Debug.DrawRay( startPoint,
                    2.5f * currentDirection,
                    Color.blue );
                //
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(
                    startPoint, 0.06f );

                Debug.DrawRay( LeftHand.transform.position,
                    .25f * LeftHand.GetForward( true ),
                    Color.red );

                Debug.DrawRay( RightHand.transform.position,
                    .25f * RightHand.GetForward( true ),
                    Color.yellow );
            }
        }
#endif

        public void SetInParadise(bool to)
        {
            inParadise = to;
            canFall = true;
        }

        void Start()
        {
            DebugMove.Enable();
        }

        void Update()
        {
            if ( inParadise )
            {
                collider.center = Camera.main.transform.localPosition + new Vector3( 0, -.6f, 0 );
            }
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(PlayerLocomotion) )]
    public class PlayerLocomotionEditor : Editor
    {
        public PlayerLocomotion script;

        public void OnEnable()
        {
            script = (PlayerLocomotion)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Label( "DEBUG" );

            if ( GUILayout.Button( "Send Force" ) )
            {
                script.AddMovementForce( Camera.main.transform.forward );
            }
        }
    }
#endif
}