using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using Random = Unity.Mathematics.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    [Serializable]
    public class AnimationTranslation : SerializableDictionary<Animations, Vector3>
    {
    }

    public class Limb : AnimalGrabbable
    {
        public static event Action<Limb> OnGrabbed = delegate { };
        public static event Action<Limb, AnimalController> OnAttached = delegate { };
        public static event Action<Limb> OnGrabReleased = delegate { };

        public AnimalEmbodiment EmbodimentType = AnimalEmbodiment.None;
        public LimbType Type;
        public LimbSocket Socket;
        [HideInInspector] public LimbSocket CloseToSocket;
        public GameObject ReattachCollider;
        public List<GameObject> SubBodyParts;

        List<Material> subBodyMaterials = new();
        Material bodyMaterial;

        const float maxLerpStrength = 7f;
        float bodyMaterialStartStrength;

        public float GodmodeMultiplier = 10;

        public Animations DefaultGodModeMovent = Animations.Walk;

        public GameObject FootSupport;
        public GameObject FlySupport;

        AnimalController originalAnimalController;
        RuntimeAnimatorController animatorController;

        GameObject dummy;
        bool attracting = false;

        FixedJoint joint;
        LimbState state = LimbState.Attached;

        Vector3 preferredAnchor;
        Vector3 preferredConnectedAnchor;

        Vector3 animatorEuler;
        Vector3 animatorScale;
        GameObject animationParent;

        Vector3 keepConnectionSpeed = Vector3.zero;
        bool keepCloseToSocketConnection = false;

        bool readyToBeReleasedInGodMode = false;
        bool godModeEnabled = false;

        float outOfBoundsCheckTime;
        float outOfBoundsCheckInterval = 1;

        /*
         * If state detached, act as a throwable/grabbable/munchable with
         *
         * If attached, disable rigidbody or freeze it? (if that works with animations??)
         */

        [SerializeField] public AnimationTranslation AnimationTranslations = new();


        /*
         * Daarvoor moet je in een animatie, eerste frame bv, op basis van de forward en rotatie
         * van dat attachment point de translatie (rotatie en positie) naar het root point berekenen.
         * In het nieuwe dier pak je voor de juiste animatie vanaf de forward en rotatie van de Limb socket
         * deze translatie om de root van het nieuwe limb te plaatsen.
         */

        public void Activate(RuntimeAnimatorController animController)
        {
            animatorController = animController;

            preferredAnchor = ReattachCollider.transform.localPosition;
            preferredConnectedAnchor =
                Socket.JointRigidbody.transform.InverseTransformPoint( Socket.transform.position );
            joint.anchor = preferredAnchor;
            joint.connectedAnchor = preferredConnectedAnchor;
            joint.connectedBody = Socket.JointRigidbody;

            GetRigidbody().velocity = Vector3.zero;
            GetRigidbody().angularVelocity = Vector3.zero;

            grabber.enabled = true;
            grabbable.enabled = true;
            interactable.enabled = true;

            Repel();

            ActivateBodyParts();

            Invoke( "EnableRagDoll", .1f );

            // TODO if we reactivate a limb, shouldn't it go to animator.SetTrigger( "GodmodeWorkshop" ); 
            // instead of receiving that from it's original parent; so maybe even do it on purpose?
        }

        void ActivateBodyParts()
        {
            Transform parent = transform;
            foreach ( GameObject subPart in SubBodyParts )
            {
                subPart.transform.parent = parent;
                parent = subPart.transform;
            }
        }

        public void Animate()
        {
            readyToBeReleasedInGodMode = true;

            if ( animationParent == null )
            {
                animationParent = new GameObject( gameObject.name + "_AnimatorProxy" );
                animationParent.transform.parent = Socket.Parent;
                animationParent.transform.rotation = Socket.transform.rotation;
                animationParent.transform.position = Socket.transform.position;
                animationParent.transform.localScale = Vector3.one;
                transform.parent = animationParent.transform;

                foreach ( GameObject subPart in SubBodyParts )
                {
                    subPart.transform.parent = animationParent.transform;
                }

                animationParent.transform.localScale =
                    animatorScale / Socket.Parent.localScale.x;

                // TODO: toch die schaal eerder?

                DisableRagDoll();

                Animator parentAnimator = animationParent.AddComponent<Animator>();
                parentAnimator.runtimeAnimatorController = animatorController;

                PassOnAnimation passOn = animationParent.AddComponent<PassOnAnimation>();
                passOn.Controller = this;

                parentAnimator.SetBool( "GodmodeStart", true );
                parentAnimator.SetTrigger( DefaultGodModeMovent.ToString() );

                Invoke( "StopGodmodeStart", 0.2f );
                Invoke( "OrientLimbProxyBasedOnDefaultAnimation", .15f );
                Invoke( "EnablePhysicsSupport", .2f );
            }
        }

        void Awake()
        {
            base.Awake();

            grabbableType = GrabbableType.Limb;

            animatorScale = Socket.Parent.localScale;
            animatorEuler = Socket.Parent.localEulerAngles;

            Destroy( GetComponent<CharacterJoint>() );
            joint = gameObject.AddComponent<FixedJoint>();

            ReattachCollider.SetActive( false );

            originalAnimalController = Socket.GetAnimalController();

            if ( FootSupport != null )
            {
                FootSupport.SetActive( false );
            }

            if ( FlySupport != null )
            {
                FlySupport.SetActive( false );
            }
        }

        void Detach()
        {
            state = LimbState.Detached;
            Debug.Log( "Detached " + gameObject.name );
            Socket.DetachLimb();
            Socket = null;
            CloseToSocket = null;
            joint.breakForce = 0;

            if ( dummy == null )
            {
                CreateDummy();
            }

            // Make a sound? :)
            GameEngine.GetInstance().DetachLimb();

            Invoke( "DestroyJoint", .05f );
            Invoke( "EnableReattachment", .1f );
        }

        void DestroyJoint()
        {
            Destroy( joint );
        }

        void DisableColliders()
        {
            foreach ( Collider collider in interactable.ColliderGroup.Colliders )
            {
                collider.enabled = false;
            }
        }

        void DisableFreeMovement()
        {
            GetRigidbody().isKinematic = true;
            GetRigidbody().velocity = Vector3.zero;
        }

        void DisableRagDoll()
        {
            DisableColliders();
            DisableFreeMovement();

            joint.connectedBody = null;
        }

        void DisableReattachment()
        {
            ReattachCollider.SetActive( false );
        }

        void EnableColliders()
        {
            foreach ( Collider collider in interactable.ColliderGroup.Colliders )
            {
                collider.enabled = true;
            }
        }

        void EnableFreeMovement()
        {
            //GetRigidbody().constraints = RigidbodyConstraints.FreezePosition;

            GetRigidbody().velocity = Vector3.zero;
            GetRigidbody().angularVelocity = Vector3.zero;
            GetRigidbody().isKinematic = false;
        }

        void EnablePhysicsSupport()
        {
            if ( FootSupport != null )
            {
                FootSupport.SetActive( true );
            }

            if ( FlySupport != null )
            {
                FlySupport.SetActive( true );
            }
        }

        void EnableRagDoll()
        {
            EnableColliders();
            Invoke( "EnableFreeMovement", .1f );
        }

        void EnableReattachment()
        {
            ReattachCollider.SetActive( true );
        }

        void FocusGrab()
        {
        }

        public AnimalController GetOriginalAnimalController()
        {
            return originalAnimalController;
        }

        #region ConstructionAttraction

        public void Attract()
        {
            attracting = true;

            if ( dummy != null )
            {
                dummy.SetActive( true );
            }
        }

        void CreateDummy()
        {
            // TODO should always be made sizewise as if it is detached?

            dummy = new GameObject( "dummy _" + gameObject.name );
            dummy.transform.position = ReattachCollider.transform.position;
            dummy.transform.rotation = ReattachCollider.transform.rotation;

            Material body = GetComponent<Renderer>().material;
            bodyMaterial = new Material( body.shader );
            bodyMaterial.CopyPropertiesFromMaterial( body );

            GameObject limbRoot = new( "limbRoot", typeof(MeshFilter), typeof(MeshRenderer) );
            if ( IsAttached() )
            {
                limbRoot.transform.parent = transform.parent;
            }

            limbRoot.GetComponent<MeshFilter>().mesh = GetComponent<MeshFilter>().mesh;
            limbRoot.GetComponent<MeshRenderer>().material = bodyMaterial;

            limbRoot.transform.eulerAngles = transform.eulerAngles;
            limbRoot.transform.position = transform.position;
            limbRoot.transform.localScale = transform.localScale;
            limbRoot.transform.parent = dummy.transform;

            if ( SubBodyParts.Count > 0 )
            {
                GameObject subs = Instantiate( SubBodyParts[0], limbRoot.transform );
                subs.transform.position = SubBodyParts[0].transform.position;
                subs.transform.rotation = SubBodyParts[0].transform.rotation;

                foreach ( MeshRenderer renderer in subs.GetComponentsInChildren<MeshRenderer>() )
                {
                    Material subBodyMaterial = renderer.material;
                    Material subBodyMaterialNew = new( subBodyMaterial.shader );
                    subBodyMaterialNew.CopyPropertiesFromMaterial( subBodyMaterial );
                    renderer.material = subBodyMaterialNew;
                    subBodyMaterials.Add( subBodyMaterialNew );
                }
            }

            bodyMaterial.SetInt( "_Dummy", 1 );
            foreach ( Material mat in subBodyMaterials )
            {
                mat.SetInt( "_Dummy", 1 );
            }

            dummy.SetActive( false );
        }

        public GameObject GetDummy()
        {
            if ( dummy == null )
            {
                CreateDummy();
            }

            return dummy;
        }

        public bool IsAttracting()
        {
            return attracting;
        }

        public void Repel()
        {
            if ( dummy != null )
            {
                dummy.SetActive( false );
            }

            attracting = false;
        }

        void SimulateAttachment()
        {
            // 'Connect' to socket
            if ( dummy != null )
            {
                /*
                 * Connect it according to the socket's orientation
                 *
                 *
                 *
                 */
                dummy.transform.position = CloseToSocket.transform.position;
                dummy.transform.rotation = CloseToSocket.transform.rotation;

                // Vector3 socketOffset = ReattachCollider.transform.InverseTransformDirection(
                //     Socket.transform.position - ReattachCollider.transform.position );
                //
                // animationParent.transform.position = Vector3.SmoothDamp(
                //     animationParent.transform.position,
                //     animationParent.transform.position + ReattachCollider.transform.TransformDirection( socketOffset ),
                //     ref keepConnectionSpeed, .04f );
            }
        }

        #endregion

        void ReleaseFocusGrab()
        {
        }

        void FocusGrabValueMain(float value)
        {
            bodyMaterial.SetFloat( "_LerpStrength", value );
        }

        void FocusGrabValueSub(float value)
        {
            foreach ( Material mat in subBodyMaterials )
            {
                mat.SetFloat( "_LerpStrength", value );
            }
        }


        public Vector3 GetAnchorOffset()
        {
            return preferredAnchor;
        }

        Transform GetPhysicsSupportPoint()
        {
            if ( FootSupport != null )
            {
                return FootSupport.transform;
            }

            if ( FlySupport != null )
            {
                return FlySupport.transform;
            }

            return null; // cause anything else is not rightly oriented
        }

        public override void Grab()
        {
            // TODO fix bug where during god mode animation limbs dissappear; even outside of god mode
            if ( !godModeEnabled )
            {
                base.Grab();
                FocusGrab();

                OnGrabbed.Invoke( this );

                if ( IsDetached() )
                {
                    EnableReattachment();
                }
            }
        }

        public bool IsDetached()
        {
            return state == LimbState.Detached;
        }

        public bool IsAttached()
        {
            return state == LimbState.Attached;
        }

        public override void OnAnimatedThrust()
        {
            switch ( Type )
            {
                case LimbType.Wing:
                case LimbType.BackLeg:
                case LimbType.FrontLeg:
                case LimbType.Tail:
                    Transform orientation = GetPhysicsSupportPoint();
                    if ( orientation != null )
                    {
                        Socket.OnPhysicsThrust( orientation, this );
                    }

                    break;
            }
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            //AnimationTranslations[forAnimation]

            // Debug.DrawLine( Socket.transform.position,
            //     Socket.transform.position +
            //     Socket.transform.TransformDirection( AnimationTranslations[Animations.Walk] ),
            //     Color.blue );

            // if ( godModeEnabled )
            // {
            //     Debug.DrawLine( transform.position,
            //         transform.position +
            //         GetRigidbody().velocity.normalized * 2,
            //         Color.green );
            // }
        }
#endif

        void OnCollisionEnter(Collision collision)
        {
            if ( readyToBeReleasedInGodMode && !godModeEnabled )
            {
                if ( LayerMask.LayerToName( collision.collider.gameObject.layer ) == "Navigation" )
                {
                    if ( Socket.IsAnimalReadyForReleaseInGodMode() )
                    {
                        Socket.ReleaseForGodMode();
                    }
                }
            }
        }

        public void OnEnterHeldMode()
        {
            animationParent.GetComponent<Animator>().speed = 0;
        }

        public void OnExitHeldMode()
        {
            animationParent.GetComponent<Animator>().speed = 1;
        }

        void OrientLimbProxyBasedOnAnimation(Animations animation)
        {
            if ( AnimationTranslations.ContainsKey( animation ) )
            {
                animationParent.transform.position = Socket.transform.position +
                                                     Socket.transform.TransformDirection(
                                                         AnimationTranslations[animation] );

                animationParent.transform.localEulerAngles =
                    new Vector3( 0, animatorEuler.y - Socket.Parent.transform.localEulerAngles.y, 0 );

                keepCloseToSocketConnection = true;
            }
        }

        // TEMP for timeout purposes
        void OrientLimbProxyBasedOnWalkAnimation()
        {
            OrientLimbProxyBasedOnAnimation( Animations.Walk );
        }

        void OrientLimbProxyBasedOnDefaultAnimation()
        {
            OrientLimbProxyBasedOnAnimation( DefaultGodModeMovent );
        }

        void OrientLimbProxyBasedOnIdleAnimation()
        {
            OrientLimbProxyBasedOnAnimation( Animations.Idle );
        }


        void Reattach(LimbSocket socket)
        {
            state = LimbState.Attached;

            Debug.Log( "Reattached " + gameObject.name );

            // re attach to a (new) character joint body and parent
            Socket = socket;
            Socket.AttachLimb( this );

            //Sound
            GameEngine.GetInstance().AttachLimb();

            Repel();

            preferredConnectedAnchor =
                Socket.JointRigidbody.transform.InverseTransformPoint( Socket.transform.position );

            // .. You have to recreate the character joint
            joint = gameObject.AddComponent<FixedJoint>();
            joint.autoConfigureConnectedAnchor = false;
            joint.anchor = preferredAnchor;
            joint.connectedAnchor = preferredConnectedAnchor;
            joint.connectedBody = Socket.JointRigidbody;

            CloseToSocket = null;

            OnAttached.Invoke( this, socket.GetAnimalController() );
        }

        public override void Release()
        {
            if ( IsGrabbed() && !godModeEnabled )
            {
                base.Release();
                ReleaseFocusGrab();

                OnGrabReleased.Invoke( this );

                if ( IsDetached() && CloseToSocket != null && CloseToSocket.CanLimbAttach( this ) )
                {
                    Reattach( CloseToSocket );
                    Invoke( "EnableFreeMovement", Time.deltaTime );
                }
                // TODO maybe throwing can also be allowed when it is attached to the/a big body
                else if ( IsDetached() && grabbingHand != null )
                {
                    float speed = grabbingHand.GetSpeed();
                    Debug.Log( "[Throwing] " + gameObject.name + " : " + speed );
                    if ( speed >= 0.5f )
                    {
                        /*
                         * Mag maximaal 6 procent boven de massa
                         *
                         */
                        float mass = GetRigidbody().mass;
                        Debug.Log( "[Throwing] " + gameObject.name + " with mass : " + mass );
                        StartCoroutine( Throw(
                            (mass + Mathf.Min( mass * .07f,
                                speed * mass * .07f )) *
                            grabbingHand.GetForward().normalized ) );
                    }
                    else
                    {
                        Invoke( "EnableFreeMovement", Time.deltaTime );
                    }
                }

                DisableReattachment();
            }
        }

        public void ReallignAttachmentWithSocket()
        {
            if ( Socket != null && ReattachCollider != null )
            {
                ReattachCollider.transform.position = Socket.transform.position;
                ReattachCollider.transform.rotation = Socket.transform.rotation;
            }
        }

        public void ReleaseForGodMode()
        {
            readyToBeReleasedInGodMode = false;
            godModeEnabled = true;
        }

        public void SaveLimbConfiguration(Animations forAnimation)
        {
            // what to do with scale
            AnimationTranslations[forAnimation] = Socket.transform.InverseTransformDirection(
                Socket.Parent.transform.position - Socket.transform.position
            );
#if UNITY_EDITOR
            EditorUtility.SetDirty( this );
#endif
        }

        public void StopGodmodeStart()
        {
            animationParent.GetComponent<Animator>().SetBool( "GodmodeStart", false );
        }

        void Update()
        {
            if ( keepCloseToSocketConnection && IsAttached() )
            {
                /*
                 * Keep attachment collider close to socket, orientation should already be okay
                 *
                 * When knowing the offset between
                 * Determine Direction between
                 */
                Vector3 socketOffset = ReattachCollider.transform.InverseTransformDirection(
                    Socket.transform.position - ReattachCollider.transform.position );

                animationParent.transform.position = Vector3.SmoothDamp(
                    animationParent.transform.position,
                    animationParent.transform.position + ReattachCollider.transform.TransformDirection( socketOffset ),
                    ref keepConnectionSpeed, .04f );

                // Smooth this with vector smoothdamp
            }

            if ( attracting && CloseToSocket != null )
            {
                SimulateAttachment();
            }

            if ( IsGrabbed() && IsAttached() &&
                 Vector3.Distance( transform.localPosition, grabPosition ) > BREAK_THRESHOLD )
            {
                Detach();
            }

            if ( IsDetached() )
            {
                outOfBoundsCheckTime += Time.deltaTime;
                if ( outOfBoundsCheckTime > outOfBoundsCheckInterval )
                {
                    outOfBoundsCheckTime = 0;
                    if ( Vector3.Distance( transform.position, Workshop.GetInstance().transform.position ) > 25 )
                    {
                        Debug.Log( "Whooops, dropping limb back in the workshop " + gameObject.name );
                        GetRigidbody().velocity = Vector3.zero;
                        GetRigidbody().angularVelocity = Vector3.zero;
                        Workshop.GetInstance().DropInBounds( gameObject );
                    }
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Limb) )]
    public class LimbEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Limb myTarget = (Limb)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Align Reattachment With Socket" ) )
            {
                myTarget.ReallignAttachmentWithSocket();
            }
        }
    }
#endif
}