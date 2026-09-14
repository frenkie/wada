using System;
using System.Collections;
using UnityEngine;
using Oculus.Interaction;

namespace Wada
{
    [RequireComponent( typeof(Grabbable) )]
    [RequireComponent( typeof(PhysicsGrabbable) )]
    [RequireComponent( typeof(WadaTouchHandGrabInteractable) )]
    [RequireComponent( typeof(Rigidbody) )]
    public abstract class AnimalGrabbable : MonoBehaviour, IDockable
    {
        protected const float BREAK_THRESHOLD = .5f;

        protected Vector3 grabPosition;
        protected Quaternion grabRotation;
        protected WadaHand grabbingHand;
        protected PointableElement grabber;
        protected WadaTouchHandGrabInteractable interactable;
        protected PhysicsGrabbable grabbable;

        protected GrabbableType grabbableType = GrabbableType.None;

        Rigidbody rigidbody;

        bool grabbed = false;
        bool docked = false;
        bool closeToInventory = false;


        protected void Awake()
        {
            rigidbody = GetComponent<Rigidbody>();
            grabber = GetComponent<Grabbable>();
            grabbable = GetComponent<PhysicsGrabbable>();
            interactable = GetComponent<WadaTouchHandGrabInteractable>();
        }

        public bool Docked
        {
            get => docked;
            set => docked = value;
        }

        public bool CloseToInventory
        {
            get => closeToInventory;
            set => closeToInventory = value;
        }

        public GrabbableType GetGrabbableType()
        {
            return grabbableType;
        }

        public GameObject GetGameObject()
        {
            return gameObject;
        }

        public Rigidbody GetRigidbody()
        {
            return rigidbody;
        }

        public virtual void Grab()
        {
            Debug.Log( "[Grab] " + gameObject.name );
            grabPosition = transform.localPosition;
            grabRotation = transform.localRotation;
            grabbed = true;

            GameEngine.GetInstance().GrabLimb();
        }

        public bool IsGrabbed()
        {
            return grabbed;
        }

        public virtual void OnAnimatorCallForConfiguration()
        {
        }

        public virtual void OnAnimatorMove()
        {
        }

        public virtual void OnAnimatedThrust()
        {
        }

        protected void OnEnable()
        {
            grabber.WhenPointerEventRaised +=
                ProcessPointerEvent;
            interactable.WhenSelectingInteractorViewAdded += ProcessSelectingInteractorAdded;
        }

        protected void OnDisable()
        {
            grabber.WhenPointerEventRaised -=
                ProcessPointerEvent;
            interactable.WhenSelectingInteractorViewAdded -= ProcessSelectingInteractorAdded;
        }

        protected void ProcessPointerEvent(PointerEvent evt)
        {
            switch ( evt.Type )
            {
                case PointerEventType.Select:
                    if ( !grabbed )
                    {
                        Grab();
                    }

                    break;
                case PointerEventType.Unselect:
                case PointerEventType.Cancel:
                    if ( grabbed )
                    {
                        Release();
                    }

                    break;
            }
        }

        protected void ProcessSelectingInteractorAdded(IInteractorView interactorView)
        {
            grabbingHand = PlayerController.GetInstance().GetGrabbingHandForInteractable( interactable );
        }

        public virtual void Release()
        {
            grabbed = false;
        }

        protected IEnumerator Throw(Vector3 force)
        {
            yield return new WaitForSeconds( Time.deltaTime );
            GetRigidbody().isKinematic = false;
            GetRigidbody().AddForce( force, ForceMode.Impulse );
        }
    }
}