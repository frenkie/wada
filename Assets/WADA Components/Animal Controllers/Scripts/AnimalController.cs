using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    /**
     * Controller for constructable animals
     */
    public class AnimalController : AnimalGrabbable
    {
        public static event Action<AnimalController> OnGrabbed = delegate { };
        public static event Action<AnimalController> OnGrabReleased = delegate { };

        public GameObject[] Support;
        public float WorkshopScale = 1;
        public List<GameObject> SubBodyParts;
        public Animations DefaultGodModeMovent = Animations.Walk;

        Ressurectable ressurectable;
        Animator animator;

        Vector3 animatorScale;
        GameObject animationParent;
        string latestAnimationTrigger;

        GameObject godMode;
        NavMeshAgent godModeAgent;
        bool godModeEnabled = false;

        float rotationSpeed = 2; // TODO: adjustable per animal (big or small)
        [SerializeField] float intendedSpeed; // private made public for debug mode only
        float thrustAccelTime;
        float thrustDecelTime;
        bool moveWithAnimatedThrust = false;

        bool firstWakeUpWalk = true;
        bool walking = false;
        bool inHeldMode;

        bool useGodModeForceOne = false;
        bool readyToBeReleasedInGodMode = false;

        Vector3 defaultScale;

        float mass = 1;

        void Awake()
        {
            defaultScale = transform.localScale;
            base.Awake();

            grabbableType = GrabbableType.Animal;

            animator = GetComponentInChildren<Animator>();
            ressurectable = GetComponent<Ressurectable>();

            foreach ( GameObject support in Support )
            {
                support.SetActive( false );
            }
        }

        public void Activate()
        {
            transform.localScale = defaultScale * WorkshopScale;

            animator.enabled = false;

            GetRigidbody().isKinematic = false;
            GetRigidbody().velocity = Vector3.zero;
            GetRigidbody().angularVelocity = Vector3.zero;
            grabber.enabled = true;
            grabbable.enabled = true;
            interactable.enabled = true;

            ressurectable.KillSound();

            Invoke( "EnableRagDoll", .1f );

            foreach ( Limb limb in GetComponentsInChildren<Limb>() )
            {
                limb.Activate( animator.runtimeAnimatorController );
            }
        }

        public void Animate()
        {
            transform.localScale = defaultScale;

            grabber.enabled = false;
            grabbable.enabled = false;
            interactable.enabled = false;
            DisableColliders();
            DisableFreeMovement();

            // To make sure there is no hierarchy of playing Animators which somehow influence each other
            // even if they can't reach each others components.
            if ( animationParent == null )
            {
                animationParent = new GameObject( gameObject.name + "_AnimatorProxy" );
                animationParent.transform.parent = animator.transform;
                animationParent.transform.localEulerAngles = Vector3.zero;
                animationParent.transform.localPosition = Vector3.zero;
                animationParent.transform.localScale = Vector3.one;

                foreach ( GameObject subPart in SubBodyParts )
                {
                    subPart.transform.parent = animationParent.transform;
                }

                Animator parentAnimator = animationParent.AddComponent<Animator>();
                parentAnimator.runtimeAnimatorController = animator.runtimeAnimatorController;
                animationParent.AddComponent<PassOnAnimation>(); // to make sure the animations don't generate an error
            }

            foreach ( Limb limb in GetComponentsInChildren<Limb>() )
            {
                limb.Animate();
            }

            // TODO might have to be better scaled for all animals
            transform.position += new Vector3( 0, 1f, 0 );
            transform.eulerAngles = Vector3.zero;

            EnableMainAnimator();
            Invoke( "EnableFreeMovement", 0.2f );
            Invoke( "EnablePhysicsSupport", .2f );

            // if we are already close to the ground
            NavMeshHit navhittie;
            if ( NavMesh.SamplePosition( transform.position, out navhittie, 2f, NavMesh.AllAreas ) )
            {
                Invoke( "ReleaseForGodMode", .2f );
            }
        }

        void AnimateLatestGodModeTrigger()
        {
            if ( godModeAgent.enabled )
            {
                godModeAgent.enabled = false;
                walking = false;
            }

            animator.SetTrigger( latestAnimationTrigger );
        }

        void CreateNavMeshAgent()
        {
            godMode = new GameObject( "GodMode_" + gameObject.name );
            godMode.transform.parent =
                transform.parent; // otherwise this one's movement interferes with the  agent 
            godMode.transform.position = transform.position;
            godMode.transform.rotation = transform.rotation;

            godModeAgent = godMode.AddComponent<NavMeshAgent>();

            NavMeshAgent baseNav = ressurectable.GetNavMeshAgent();

            godModeAgent.baseOffset = baseNav.baseOffset;
            godModeAgent.angularSpeed = baseNav.angularSpeed;
            godModeAgent.acceleration = baseNav.acceleration;
            godModeAgent.radius = baseNav.radius;
            godModeAgent.height = baseNav.height;
            godModeAgent.speed = 0;

            intendedSpeed = ressurectable.GetIntendedSpeed();
        }

#if UNITY_EDITOR
        public void ConfigureSimpleTestColliders()
        {
            PhysicMaterial rough = AssetDatabase.LoadAssetAtPath<PhysicMaterial>(
                "Assets/WADA Components/Animal Controllers/Materials/Rough.physicMaterial" );

            WadaTouchHandGrabInteractable grabInteractable = GetComponent<WadaTouchHandGrabInteractable>();
            List<Collider> colliders = new();

            foreach ( GameObject subPart in SubBodyParts )
            {
                colliders.Add( AddCollider( subPart, rough ) );
                DestroyImmediate( subPart.GetComponent<MeshCollider>() );
            }

            if ( colliders.Count > 0 )
            {
                grabInteractable.InjectColliders( colliders );
            }

            // LIMB
            foreach ( Limb limb in GetComponentsInChildren<Limb>() )
            {
                List<Collider> limbColliders = new();
                WadaTouchHandGrabInteractable limbGrabbable =
                    limb.gameObject.GetComponent<WadaTouchHandGrabInteractable>();

                limbColliders.Add( AddCollider( limb.gameObject, rough ) );
                DestroyImmediate( limb.gameObject.GetComponent<MeshCollider>() );
                foreach ( GameObject limbSubPart in limb.SubBodyParts )
                {
                    DestroyImmediate( limbSubPart.GetComponent<MeshCollider>() );
                    limbColliders.Add( AddCollider( limbSubPart, rough ) );
                }

                if ( limbColliders.Count > 0 )
                {
                    limbGrabbable.InjectColliders( limbColliders );
                }
            }
        }
#endif

        public void DisableSimpleTestColliders()
        {
            foreach ( BoxCollider collide in GetComponentsInChildren<BoxCollider>() )
            {
                if ( collide.name.Contains( "_Collider" ) )
                {
                    collide.enabled = false;
                    DestroyImmediate( collide.GetComponent<MeshRenderer>() );
                    DestroyImmediate( collide.GetComponent<MeshFilter>() );
                }
            }
        }

        BoxCollider AddCollider(GameObject to, PhysicMaterial physicMaterial)
        {
            GameObject sup = GameObject.CreatePrimitive( PrimitiveType.Cube );

            sup.name = to.name + "_Collider";
            sup.transform.parent = to.transform;
            sup.transform.localPosition = Vector3.zero;
            sup.layer = LayerMask.NameToLayer( "Animal" );

            BoxCollider box = sup.GetComponent<BoxCollider>();
            box.material = physicMaterial;

            sup.transform.localScale = Vector3.one * (1f / to.transform.localScale.magnitude);

            return box;
        }

        public void DebugWorkshop()
        {
            if ( gameObject.activeSelf )
            {
                ressurectable.SetResurrected( true );
                animator.SetTrigger( "GodmodeWorkshop" );
                Invoke( "DebugWorkshopDoIt", .1f );
            }
        }

        public void DebugWorkshopDoIt()
        {
            PlayerController.GetInstance().AddAnimalToWorkshop( this );
        }

        public void DebugWorkshopAnimation()
        {
            DisableFreeMovement();
            SetGodModeReadyToBeReleased( true );
            Animate();
        }

        void DisableColliders()
        {
            foreach ( Collider collider in interactable.ColliderGroup.Colliders )
            {
                collider.enabled = false;
            }
        }

        public void DisableFreeMovement()
        {
            GetRigidbody().isKinematic = true;
            GetRigidbody().velocity = Vector3.zero;
        }

        void EnableColliders()
        {
            foreach ( Collider collider in interactable.ColliderGroup.Colliders )
            {
                collider.enabled = true;
            }
        }

        public void EnableFreeMovement()
        {
            GetRigidbody().isKinematic = false;
            GetRigidbody().velocity = Vector3.zero;
        }

        public void EnableGodMode()
        {
            if ( !godModeEnabled )
            {
                if ( godMode == null )
                {
                    CreateNavMeshAgent();
                }
                else
                {
                    godMode.transform.position = transform.position;
                    godMode.transform.rotation = transform.rotation;
                }

                SetGodModeOnNavMesh();

                GetRigidbody().constraints =
                    RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

                ressurectable.AllowHeldMode();
                ressurectable.OnEnteredHeldMode += OnEnterHeldMode;
                ressurectable.OnExitedHeldMode += OnExitHeldMode;

                // TODO for now a predetermined target, but preferably it can just walk around on the closest navmesh
                // and find spots
                GameObject target = ressurectable.GetRandomTarget();

                if ( target == null )
                {
                    Debug.LogError( "No targets for " + gameObject.name );
                }
                else
                {
                    SetGodeModeMoveTarget( target );
                }

                godModeEnabled = true; // we can start moving!   

                OnAnimatedThrust();
            }
        }

        void EnableMainAnimator()
        {
            Animator parentAnimator = animationParent.GetComponent<Animator>();
            parentAnimator.SetBool( "GodmodeStart", true );
            parentAnimator.SetTrigger( DefaultGodModeMovent.ToString() );

            Invoke( "StopGodmodeStart", 0.2f );
        }

        void EnablePhysicsSupport()
        {
            foreach ( GameObject support in Support )
            {
                support.SetActive( true );
            }
        }

        void EnableRagDoll()
        {
            mass = GetTotalMass();
            EnableColliders();
            GetRigidbody().useGravity = true;
            GetRigidbody().velocity = Vector3.zero;
            GetRigidbody().angularVelocity = Vector3.zero;
            Invoke( "KillForces", .2f );
        }

        void KillForces()
        {
            GetRigidbody().velocity = Vector3.zero;
            GetRigidbody().angularVelocity = Vector3.zero;
        }

        public override void Grab()
        {
            base.Grab();
            CancelInvoke( "DoOnGrabReleased" );
            Debug.Log( "Cancelled Invoke" );
            OnGrabbed.Invoke( this );
        }

        float GetTotalMass()
        {
            float mass = 0;

            foreach ( Rigidbody rb in GetComponentsInChildren<Rigidbody>() )
            {
                mass += rb.mass;
            }

            Debug.Log( "Mass " + mass );

            return mass;
        }

        public GameObject GetHighlightedCopy()
        {
            /**
             * TODO Can we make a dummy only if limbs have changed?
             */

            GameObject dummy = new( gameObject.name + "_HighlightedCopy" );
            Transform commonParent = null;

            /**
             * Get bodyParts
             */
            foreach ( GameObject subPart in SubBodyParts )
            {
                if ( commonParent == null )
                {
                    commonParent = subPart.transform.parent;
                    dummy.transform.parent = commonParent.transform.parent;
                    dummy.transform.localScale = commonParent.localScale;
                    dummy.transform.position = commonParent.position;
                    dummy.transform.rotation = commonParent.rotation;
                }

                GameObject subs = Instantiate( subPart, dummy.transform );

                foreach ( MeshRenderer renderer in subs.GetComponentsInChildren<MeshRenderer>() )
                {
                    Material subBodyMaterial = renderer.material;
                    Material subBodyMaterialNew = new( subBodyMaterial.shader );
                    subBodyMaterialNew.CopyPropertiesFromMaterial( subBodyMaterial );
                    subBodyMaterialNew.SetInt( "_Dummy", 1 );
                    renderer.material = subBodyMaterialNew;
                }
            }

            foreach ( Limb limb in GetComponentsInChildren<Limb>() )
            {
                if ( commonParent == null )
                {
                    commonParent = limb.transform.parent;
                    dummy.transform.parent = commonParent.transform.parent;
                    dummy.transform.localScale = commonParent.localScale;
                    dummy.transform.position = commonParent.position;
                    dummy.transform.rotation = commonParent.rotation;
                }

                GameObject original = limb.GetDummy();
                GameObject dummyLimb = Instantiate( original );

                dummyLimb.transform.parent = dummy.transform;
                dummyLimb.transform.position = limb.Socket.transform.position;
                dummyLimb.transform.rotation = limb.Socket.transform.rotation;

                dummyLimb.SetActive( true );
            }

            dummy.transform.parent = null;

            return dummy;
        }

        public bool InGodMode()
        {
            return godModeEnabled;
        }

        void LateUpdate()
        {
            if ( walking && !inHeldMode )
            {
                if ( !godModeAgent.pathPending )
                {
                    // only use the horizontal for this one
                    float distanceToAgent = Vector2.Distance( new Vector2(
                        godMode.transform.position.x,
                        godMode.transform.position.z
                    ), new Vector2(
                        transform.position.x,
                        transform.position.z
                    ) );

                    // TODO test of only rotating while we're moving
                    if ( GetRigidbody().velocity.magnitude > .1f )
                    {
                        Vector3 lookAtT = godMode.transform.position -
                                          transform.position;
                        Quaternion rotateTo = Quaternion.LookRotation( lookAtT );

                        if ( Quaternion.Angle( transform.rotation, rotateTo ) >= 1 )
                        {
                            // TODO? Is this possible with non-kinematic rigidbodies? Or do I have to cancel the 
                            //  rotations on it
                            transform.rotation = Quaternion.Slerp( transform.rotation, rotateTo,
                                Time.deltaTime * rotationSpeed );
                        }
                    }

                    // If close enough to destination? Idle and move on
                    if ( godModeAgent.remainingDistance <= ressurectable.BrakeDistance &&
                         distanceToAgent < ressurectable.BrakeDistance )
                    {
                        Debug.Log( "[" + godMode.name + "] Reached destiny" );
                        // Just continue for now TODO: use the idle animation
                        WalkToRandomTarget();
                    }
                }
            }
        }

        public void OnAnimatedThrust()
        {
            thrustAccelTime = 0;
            thrustDecelTime = 0;
            if ( !inHeldMode )
            {
                moveWithAnimatedThrust = true;
            }
        }

        public void OnEnterHeldMode()
        {
            animationParent.GetComponent<Animator>().speed = 0;

            foreach ( Limb limb in GetComponentsInChildren<Limb>() )
            {
                limb.OnEnterHeldMode();
            }

            inHeldMode = true;
            moveWithAnimatedThrust = false;
            DisableFreeMovement();
        }

        public virtual void OnExitHeldMode(bool lateSaveChoice = false)
        {
            EnableFreeMovement();
            inHeldMode = false;

            animationParent.GetComponent<Animator>().speed = 1;

            foreach ( Limb limb in GetComponentsInChildren<Limb>() )
            {
                limb.OnExitHeldMode();
            }
        }


#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if ( godModeEnabled )
            {
                Debug.DrawLine( godMode.transform.position,
                    godMode.transform.position +
                    godMode.transform.forward.normalized * 2,
                    Color.blue );


                Debug.DrawLine( transform.position,
                    transform.position + new Vector3( 0, .2f, 0 ) +
                    GetRigidbody().velocity.normalized * 2,
                    Color.green );
            }
        }
#endif

        // Called when a walk or movement animation is taking place
        // TODO: currently fly animations don't call this event, add it
        // so we can create 'flywalkers'
        public void OnPhysicsThrust(Transform touchPoint, Limb limb)
        {
            if ( godModeEnabled && !inHeldMode )
            {
                switch ( limb.Type )
                {
                    case LimbType.Wing:
                    case LimbType.BackLeg:
                    case LimbType.FrontLeg:

                        // If agent is close enough let it move on towards the target
                        float distanceToAgent = Vector2.Distance( new Vector2(
                            godMode.transform.position.x,
                            godMode.transform.position.z
                        ), new Vector2(
                            transform.position.x,
                            transform.position.z
                        ) );

                        float distanceToTarget = Vector2.Distance( new Vector2(
                            godModeAgent.destination.x,
                            godModeAgent.destination.z
                        ), new Vector2(
                            transform.position.x,
                            transform.position.z
                        ) );

                        if ( (!moveWithAnimatedThrust && distanceToAgent < ressurectable.BrakeDistance * 2) ||
                             distanceToTarget < distanceToAgent )
                        {
                            OnAnimatedThrust(); // will get multiple calls from all the limbs but that's okay, they will reset each other
                            // TODO: double check this doesn't run to often
                        }


                        // TODO preferably turn this into the same kind of timed force as the original navmesh
                        if ( useGodModeForceOne )
                        {
                            // Version 1: Force at the location (maybe turn of rigidbody rotations
                            // TODO: what does this do with torque?
                            // TODO: use limb weight or data to determine force power

                            Vector3 forward = touchPoint.forward.normalized;

                            if ( limb.Type != LimbType.Wing )
                            {
                                forward.y +=
                                    .2f; // based on the slope and slope direction of the navmesh underneath the touchpoint?

                                GetRigidbody().AddForceAtPosition( forward * mass * limb.GodmodeMultiplier,
                                    touchPoint.position );
                            }
                            else
                            {
                                GetRigidbody().AddForceAtPosition( forward * mass * limb.GodmodeMultiplier,
                                    touchPoint.position );
                            }
                        }
                        else
                        {
                            Vector3 forward = transform.forward.normalized;

                            if ( limb.Type != LimbType.Wing )
                            {
                                forward.y +=
                                    .2f; // based on the slope and slope direction of the navmesh underneath the touchpoint?
                            }
                            else
                            {
                                forward.y +=
                                    .8f;
                            }

                            // Version 2: Force at the origin
                            // Force = 250
                            // velocity is .55f forward
                            GetRigidbody().AddForce( forward * (limb.GodmodeMultiplier / 20),
                                ForceMode.VelocityChange );
                        }

                        break;
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            if ( readyToBeReleasedInGodMode && !godModeEnabled )
            {
                if ( LayerMask.LayerToName( collision.collider.gameObject.layer ) == "Navigation" )
                {
                    ReleaseForGodMode();
                }
            }
        }

        public void ReleaseForGodMode()
        {
            if ( readyToBeReleasedInGodMode && !godModeEnabled )
            {
                Debug.Log(
                    "A part of me (" + gameObject.name + ") hit the ground or we were close, let's do god mode!" );
                readyToBeReleasedInGodMode = false;
                EnableGodMode();

                foreach ( Limb limb in GetComponentsInChildren<Limb>() )
                {
                    limb.ReleaseForGodMode();
                }
            }
        }

        public override void Release()
        {
            base.Release();

            Invoke( "DoOnGrabReleased", .1f );

            if ( grabbingHand != null )
            {
                float speed = grabbingHand.GetSpeed();
                Debug.Log( "speed " + speed + ", direction " + grabbingHand.GetDirection() );
                if ( speed >= 0.3f )
                {
                    StartCoroutine( Throw( Mathf.Min( .5f, speed * .5f ) * grabbingHand.GetForward().normalized ) );
                }
            }
        }

        public void DoOnGrabReleased()
        {
            Debug.Log( "DoOnGrabReleased" );
            OnGrabReleased( this );
        }


        public void SaveLimbConfiguration(Animations forAnimation)
        {
            foreach ( Limb limb in GetComponentsInChildren<Limb>() )
            {
                limb.SaveLimbConfiguration( forAnimation );
            }
        }

        public void StopGodmodeStart()
        {
            animationParent.GetComponent<Animator>().SetBool( "GodmodeStart", false );
        }

        public bool GetGodModeReadyToBeReleased()
        {
            return readyToBeReleasedInGodMode;
        }

        public void SetGodModeReadyToBeReleased(bool to)
        {
            readyToBeReleasedInGodMode = to;
        }

        void SetGodeModeMoveTarget(GameObject to)
        {
            RaycastHit hit;
            if ( Physics.Raycast( to.transform.position, -Vector3.up, out hit, 10f,
                    LayerMask.GetMask( "Navigation", "ColliderForAnimals" ) ) )
            {
                if ( !godModeAgent.enabled )
                {
                    godModeAgent.enabled = true;
                }

                if ( !godModeAgent.isOnNavMesh )
                {
                    SetGodModeOnNavMesh();
                }

                if ( godModeAgent.SetDestination( hit.point ) )
                {
                    Debug.Log( "Destination for " + godMode.name + " " + hit.point );

                    walking = true;
                    if ( latestAnimationTrigger != null )
                    {
                        animator.ResetTrigger( latestAnimationTrigger );
                    }

                    godModeAgent.speed = 0;

                    if ( firstWakeUpWalk )
                    {
                        firstWakeUpWalk = false;
                    }
                    else
                    {
                        latestAnimationTrigger = DefaultGodModeMovent.ToString();
                        animator.SetTrigger( latestAnimationTrigger );
                    }
                }
                else
                {
                    Debug.Log( "Couldn't set destination" );
                }
            }
            else
            {
                Debug.Log( "Target not on a navmesh for " + gameObject.name );
            }
        }

        void SetGodModeOnNavMesh()
        {
            NavMeshHit navhittie;
            if ( NavMesh.SamplePosition( godMode.transform.position, out navhittie, 3f, NavMesh.AllAreas ) )
            {
                if ( godModeAgent.Warp( navhittie.position ) )
                {
                    Debug.Log( "Warped!" );
                }
                else
                {
                    Debug.Log( "Not warped!" );
                }
            }
            else
            {
                Debug.Log( "Hmm, no nav areas?" );
            }
        }

        void Update()
        {
            if ( godModeEnabled )
            {
                if ( moveWithAnimatedThrust )
                {
                    godModeAgent.speed = intendedSpeed * ressurectable.AccelerationCurve.Evaluate(
                        thrustAccelTime / ressurectable.AnimatedThrustAcceleration );

                    if ( thrustAccelTime >= ressurectable.AnimatedThrustAcceleration )
                    {
                        moveWithAnimatedThrust = false;
                    }

                    thrustAccelTime += Time.deltaTime;
                }
                else if ( godModeAgent != null && godModeAgent.speed > 0 )
                {
                    godModeAgent.speed = intendedSpeed * ressurectable.DecelerationCurve.Evaluate(
                        thrustDecelTime / ressurectable.AnimatedThrustDeceleration );

                    thrustDecelTime += Time.deltaTime;
                }
            }
        }

        public void WalkToRandomTarget()
        {
            SetGodeModeMoveTarget( ressurectable.GetRandomTarget() );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(AnimalController) )]
    public class AnimalControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AnimalController myTarget = (AnimalController)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Test Montage" ) )
            {
                myTarget.DebugWorkshop();
            }

            if ( GUILayout.Button( "Test Workshop Animation" ) )
            {
                myTarget.DebugWorkshopAnimation();
            }

            if ( GUILayout.Button( "Save Limb Configuration Idle" ) )
            {
                myTarget.SaveLimbConfiguration( Animations.Idle );
            }

            if ( GUILayout.Button( "Save Limb Configuration Walk" ) )
            {
                myTarget.SaveLimbConfiguration( Animations.Walk );
            }

            if ( GUILayout.Button( "Save Limb Configuration Fly" ) )
            {
                myTarget.SaveLimbConfiguration( Animations.Fly );
            }

            if ( GUILayout.Button( "Add test colliders" ) )
            {
                myTarget.ConfigureSimpleTestColliders();
            }

            if ( GUILayout.Button( "Disable test colliders" ) )
            {
                myTarget.DisableSimpleTestColliders();
            }
        }
    }
#endif
}