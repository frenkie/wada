using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public enum GuideType
    {
        Duck,
        Crow
    }

    public class Guide : Flocker
    {
        public GuideType Type;
        public Animator IntroAnimator;
        public Animator ExperienceAnimator;
        public Transform Center;
        public Transform AnimatedCenter;
        public float AvoidanceForce = 1.5f;
        public GameObject GuideAsRevivable;

        Rigidbody rigidbody;

        Quaternion defaultCenterRotation;
        GuideRotation rotationType = GuideRotation.Default;
        float rotationSpeed = .4f;
        float maxHeightRotation = 1f; // don't angle down more than 50cms

        Transform finalTarget; // transform so we can have a rotation target?
        Vector3 wayPointPosition;
        bool moving;

        bool isGuide;
        bool gotUp;
        bool avoiding;

        float acceleration = .7f;
        float speed = 0;
        float targetSpeed = 1f;
        float nextWayPointDistance = 6f;
        float brakeDistance = .5f; // when to slow down flying

        float playerVicinityThreshold = 2.5f;

        bool scouting = true;
        List<Ressurectable> scoutables = new();
        Ressurectable scoutTarget;
        Transform scoutTargetWaypoint;
        float scoutingInterval = 1f;
        float scoutingTimer = 0f;

        public void AllowAvoiding()
        {
            avoiding = false;
        }

        // to allow an Invoke :(
        void AllowMoving()
        {
            moving = true;
        }

        void AllowMoving(bool state = true)
        {
            moving = state;
        }

        void CheckForScoutTarget()
        {
            scoutables.Sort( CompareDistanceToPlayer );
            if ( scoutables[0] != scoutTarget )
            {
                scoutTarget = scoutables[0];
                Debug.Log( "[Guide] setting scout target to " + scoutTarget.name );
                float eyeHeight = Math.Max( PlayerController.GetInstance().GetEyeHeight(), 1.2f );

                Vector2 randomAroundTarget = WadaMath.RandomPointInAnnulus( new Vector2(
                        scoutTarget.transform.position.x,
                        scoutTarget.transform.position.z
                    )
                    , 1.2f, 1.4f );

                scoutTargetWaypoint.position = new Vector3(
                    randomAroundTarget.x,
                    scoutTarget.transform.position.y + eyeHeight - .7f,
                    randomAroundTarget.y
                ); // taking a guess that the eye height that was set from the beginning is correct

                SetWaypoint( scoutTargetWaypoint.position );
                Unstuck();
            }
        }

        int CompareDistanceToPlayer(Ressurectable a, Ressurectable b)
        {
            Vector3 playerCamPos = PlayerController.GetInstance().GetLocation();
            float distanceToA = Vector3.Distance( playerCamPos, a.transform.position );
            float distanceToB = Vector3.Distance( playerCamPos, b.transform.position );

            if ( distanceToA < distanceToB )
            {
                return -1;
            }

            return distanceToA > distanceToB ? 1 : 0;
        }

        public void DetermineNextWaypoint()
        {
            Vector3 direction = finalTarget.position - transform.position;
            if ( direction.magnitude <= nextWayPointDistance )
            {
                wayPointPosition = finalTarget.position;
            }
            else
            {
                wayPointPosition = transform.position + nextWayPointDistance * direction.normalized;
            }
        }

        /**
         * Temporarily fade out the guide and hide it
         */
        public void Discard()
        {
            Hashtable scale = new();

            scale.Add( "scale", Vector3.zero );
            scale.Add( "time", .3f );
            scale.Add( "easetype", iTween.EaseType.easeOutCubic );
            scale.Add( "oncomplete", "OnDiscarded" );

            iTween.ScaleTo( gameObject, scale );
        }

        public void OnDiscarded()
        {
            if ( GuideAsRevivable != null )
            {
                GuideAsRevivable.SetActive( true );
            }

            HideVisually();
        }

        public void EnableFocusFactor()
        {
            foreach ( Renderer renderer in GetComponentsInChildren<Renderer>() )
            {
                renderer.material.SetInt( "_Exclude_From_Focus", 0 );
            }
        }

        public void EnterScoutMode()
        {
            foreach ( Ressurectable animal in AllRessurectables )
            {
                if ( animal != this && !animal.IsResurrected() && animal.isActiveAndEnabled &&
                     animal.gameObject.GetComponent<Guide>() == null )
                {
                    scoutables.Add( animal );
                }
            }

            scoutTargetWaypoint = new GameObject( "scoutTargetWaypoint" ).transform;

            scouting = true;

            SetWaypointDistanceOverride( .8f );

            AddToFlocker();
        }

        public void Flap()
        {
            PlaySound( AnimalSounds.Fly );
        }

        void FlyToFlockPoint()
        {
            Hashtable moveTo = new();
            Transform target = GameEngine.GetInstance().GuideFlockPoint;

            moveTo.Add( "position", target.position );
            moveTo.Add( "time", Vector3.Distance( target.position, transform.position ) / .8f );
            moveTo.Add( "easetype", iTween.EaseType.easeInOutCubic );

            iTween.MoveTo( gameObject, moveTo );


            Hashtable rotateParams = new();

            rotateParams.Add( "y", target.eulerAngles.y );
            rotateParams.Add( "delay", 3 );
            rotateParams.Add( "time", 3 );
            rotateParams.Add( "easetype", iTween.EaseType.easeInOutQuad );

            iTween.RotateTo( gameObject, rotateParams );
        }

        public Ressurectable GetScoutTarget()
        {
            return scoutTarget;
        }

        public void HideVisually()
        {
            foreach ( MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>() )
            {
                renderer.enabled = false;
            }
        }

        public bool IsFlock()
        {
            return !isGuide;
        }

        public bool IsGuide()
        {
            return isGuide;
        }

        public bool IsScouting()
        {
            return scouting;
        }

        public void OnDrawGizmos()
        {
            if ( scouting && scoutTarget != null )
            {
                Gizmos.color = new Color( 0f, 1.0f, 0f, .8f );
                Gizmos.DrawLine( transform.position, scoutTargetWaypoint.position );
            }
        }


        // Called by GuidePassOn
        public void OnEndIntro()
        {
            if ( ExperienceAnimator != null )
            {
                ExperienceAnimator.gameObject.SetActive( true );
                SetAnimator( ExperienceAnimator );
                IntroAnimator.gameObject.SetActive( false );
            }

            if ( isGuide )
            {
                // Will go into guiding mode towards first Tutorial position through a Timeline signal
                rotationType = GuideRotation.Player;

                // Move a little upwards, around eye height
                Hashtable moveTo = new();
                Vector3 aLittleUp = new(
                    transform.position.x,
                    PlayerController.GetInstance().GetLocation().y -
                    transform.InverseTransformPoint( AnimatedCenter.position ).y - .1f,
                    transform.position.z
                );

                moveTo.Add( "position", aLittleUp );
                moveTo.Add( "time", 2f );
                moveTo.Add( "easetype", iTween.EaseType.easeInOutQuad );

                iTween.MoveTo( gameObject, moveTo );
            }
            else
            {
                FlyToFlockPoint();
            }
        }

        public void OnGotUp()
        {
            gotUp = true;
        }

        void OnReachedWayPoint()
        {
            moving = false;

            // In scouting mode, when reached target, fly around it freely avoiding the player and other 
            // colliders
        }

        public override void OnReleased(Ressurectable resurrectable)
        {
            base.OnReleased( resurrectable );

            if ( resurrectable != this && isGuide && scoutables.Contains( resurrectable ) )
            {
                scoutables.Remove( resurrectable );

                if ( scoutables.Count == 0 )
                {
                    /*
                        Everything released!

                        TODO Play Voice if we have one?

                        Set him free as a normal Flocker
                    */
                    rotationType = GuideRotation.Flocker;
                    scoutTarget = null;
                    SetFree();
                }
            }
        }

        public void OnTouch()
        {
            OnTouch( null );
        }

        public override void OnTouch(WadaHand hand)
        {
            if ( !resurrected && AllowTouch && !DisableAllRevivals )
            {
                if ( !GameEngine.GetInstance().HasGuide() )
                {
                    GameEngine.GetInstance().SetGuide( this );
                    isGuide = true;
                }

                base.OnTouch( hand ); // Triggers the Getup animation
                Invoke( "OnGotUp", 2f );

                Debug.Log( "OnTouch the " + gameObject.name );
            }
        }

        void OnTriggerStay(Collider other)
        {
            // only if flying around it needs to avoid the player getting too close
            if ( gotUp && !avoiding &&
                 IsPaused() &&
                 (other.gameObject.tag == "Hand" || other.gameObject.tag == "Player" || other.gameObject.tag == "Mouth")
               )
            {
                avoiding = true;

                Vector3 direction =
                    (transform.position -
                     new Vector3( other.transform.position.x, transform.position.y, other.transform.position.z ))
                    .normalized;

                rigidbody.AddForce( direction * AvoidanceForce, ForceMode.Impulse );

                Invoke( "AllowAvoiding", .4f );
            }
        }

        bool PlayerIsCloseEnough()
        {
            Vector3 playerLoc = PlayerController.GetInstance().GetLocation();

            float distanceToPlayer = Vector3.Distance( transform.position, playerLoc );
            float playerDistanceToFinal = Vector3.Distance( finalTarget.position, playerLoc );
            float distanceToFinal = Vector3.Distance( transform.position, finalTarget.position );

            return distanceToPlayer < playerVicinityThreshold || playerDistanceToFinal < distanceToFinal;
        }

        public void SetInBirdMode()
        {
            Invoke( "SetFree", .4f );
        }

        protected override void SetFree()
        {
            Debug.Log( "[Guide] SetFree" );

            SetWaypointDistanceOverride( -1 );
            EnableFocusFactor();
            EnableFreeRoaming();
            AllowHeldMode();

            OnEnteredHeldMode += OnEnterHeldMode;
            OnExitedHeldMode += OnExitHeldMode;
        }

        public void SetTargetPoint(Transform target, float startAfter = 0)
        {
            Debug.Log( "[Guide] SetTargetPoint " + target );
            finalTarget = target;
            DetermineNextWaypoint();
            Invoke( "AllowMoving", startAfter );
        }

        public void ShowVisually()
        {
            foreach ( MeshRenderer renderer in GetComponentsInChildren<MeshRenderer>( true ) )
            {
                renderer.enabled = true;
            }
        }

        void Start()
        {
            base.Start();

            rigidbody = GetComponent<Rigidbody>();

            defaultCenterRotation = Center.localRotation;
            SetAnimator( IntroAnimator );
            IntroAnimator.gameObject.SetActive( true );

            if ( ExperienceAnimator != null )
            {
                ExperienceAnimator.gameObject.SetActive( false );
            }
        }

        void Update()
        {
            base.Update();

            if ( scouting && scoutables.Count > 0 )
            {
                scoutingTimer += Time.deltaTime;
                if ( scoutingTimer > scoutingInterval )
                {
                    CheckForScoutTarget();
                    scoutingTimer = 0;
                }

                if ( IsPaused() )
                {
                    rotationType = GuideRotation.Player;
                }
                else
                {
                    rotationType = GuideRotation.Flocker;
                }

                UpdateOrientation();
            }
            else
            {
                // If not flocking
                if ( IsPaused() )
                {
                    UpdateMovement();
                    UpdateOrientation();
                }
            }
        }

        void UpdateMovement()
        {
            if ( moving )
            {
                Vector3 directedDistance = wayPointPosition - transform.position;
                if ( directedDistance.magnitude <= brakeDistance )
                {
                    speed = Mathf.Max( 0.01f,
                        speed - Time.deltaTime * acceleration ); // bigger than .01f or we come to a halt
                }
                else
                {
                    speed = Mathf.Min( targetSpeed, speed + Time.deltaTime * acceleration );
                }

                Vector3 add = directedDistance.normalized * (speed * Time.deltaTime);

                if ( directedDistance.magnitude < .05f || add.magnitude > directedDistance.magnitude )
                {
                    transform.position = wayPointPosition;
                    OnReachedWayPoint();
                }
                else
                {
                    transform.position += add;
                }
            }
            else if ( transform.position == wayPointPosition &&
                      !wayPointPosition.Equals( finalTarget.position ) &&
                      PlayerIsCloseEnough()
                    )
            {
                DetermineNextWaypoint();
                AllowMoving();
            }
        }

        void UpdateOrientation()
        {
            switch ( rotationType )
            {
                case GuideRotation.Player:

                    Vector3 playerLoc = PlayerController.GetInstance().GetLocation();
                    float yDistanceP = playerLoc.y - Center.position.y;
                    Vector3 lookAtP = new Vector3( playerLoc.x,
                                          Mathf.Abs( yDistanceP ) <= maxHeightRotation
                                              ? playerLoc.y
                                              : Center.position.y + Mathf.Sign( yDistanceP ) * maxHeightRotation,
                                          playerLoc.z ) -
                                      Center.position;
                    if ( lookAtP != Vector3.zero )
                    {
                        Center.rotation = Quaternion.Slerp( Center.rotation, Quaternion.LookRotation( lookAtP ),
                            Time.deltaTime * rotationSpeed );
                    }

                    break;

                case GuideRotation.Target:

                    if ( !wayPointPosition.Equals( Vector3
                            .zero ) ) // boldly assuming we will never have to move to 0,0,0
                    {
                        float yDistanceT = wayPointPosition.y - Center.position.y;
                        Vector3 lookAtT = new Vector3( wayPointPosition.x,
                                              Mathf.Abs( yDistanceT ) <= maxHeightRotation
                                                  ? wayPointPosition.y
                                                  : Center.position.y + Mathf.Sign( yDistanceT ) * maxHeightRotation,
                                              wayPointPosition.z ) -
                                          Center.position;

                        if ( lookAtT != Vector3.zero )
                        {
                            Center.rotation = Quaternion.Slerp( Center.rotation, Quaternion.LookRotation( lookAtT ),
                                Time.deltaTime * rotationSpeed );
                        }
                    }

                    break;

                case GuideRotation.Flocker:
                default:
                    if ( !Center.localRotation.Equals( defaultCenterRotation ) )
                    {
                        Center.localRotation = Quaternion.Slerp( Center.localRotation, defaultCenterRotation,
                            Time.deltaTime * rotationSpeed );
                    }

                    break;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Guide), true )]
    public class GuideEditor : RessurectableEditor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            Guide myTarget = (Guide)target;

            if ( GUILayout.Button( "EnterScoutMode" ) )
            {
                myTarget.EnterScoutMode();
            }
        }
    }
#endif
}