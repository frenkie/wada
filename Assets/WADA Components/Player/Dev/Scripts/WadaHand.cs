using System;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.GrabAPI;
using Unity.Labs.SuperScience;
using UnityEngine;
using System.Collections.Generic;

namespace Wada
{
    [Serializable]
    public class EmbodimentDictionary : SerializableDictionary<AnimalEmbodiment, GameObject>
    {
    }

    public class WadaHand : MonoBehaviour
    {
        public bool DebugValues = false;
        public float DebugMinSpeed = 0.7f;
        public OVRInput.Controller Type = OVRInput.Controller.LHand;
        public Transform forwarder;
        public WadaTouchHandGrabInteractor Interactor;
        public InteractorGroup InteractorGroup;
        public HandGrabInteractor GrabInteractor;
        public GameObject Collider;
        public OVRHand HandActiveState;

        [SerializeField] public EmbodimentDictionary Embodiments = new();

        PhysicsTracker physicsTracker = new();
        Vector3 lastPosition; // in local space

        List<float> lastSpeeds = new();

        AnimalGrabbable isGrabbing;

        GameObject heldModeObject;

        GameObject follower;

        bool embodimentShown = true;
        float handIsntTracked;
        float handIsntTrackedThreshold = 0.1f;

        AnimalEmbodiment activeEmbodiment = AnimalEmbodiment.Normal;

        public Vector3 GetDirection(bool local = false)
        {
            if ( local )
            {
                return physicsTracker.Direction;
            }
            else
            {
                return transform.parent.TransformDirection( physicsTracker.Direction );
            }
        }

        public Transform GetFollowerTransform()
        {
            return follower.transform;
        }

        public Vector3 GetForward(bool local = false)
        {
            //float invert = Type == OVRInput.Controller.LHand ? -1 : 1;
            float invert = 1;

            return local
                ? invert * forwarder.forward
                : transform.parent.TransformDirection( invert * forwarder.forward );
        }

        public float GetInwardAngle()
        {
            return Mathf.Repeat( transform.localEulerAngles.x + 180, 360 ) - 180;
        }

        public float GetSpeed()
        {
            // float speed = 0;
            // foreach ( float val in lastSpeeds )
            // {
            //     speed += val;
            // }
            //
            // return speed / lastSpeeds.Count;
            return physicsTracker.Speed;
        }

        public Vector3 GetVelocity(bool local = false)
        {
            if ( local )
            {
                return physicsTracker.Velocity;
            }
            else
            {
                return transform.parent.TransformDirection( physicsTracker.Velocity );
            }
        }

        public GameObject GetGrabbedElement()
        {
            return heldModeObject != null ? heldModeObject :
                Interactor.SelectedInteractable != null ? Interactor.SelectedInteractable.gameObject : null;
        }

        public AnimalGrabbable GetGrabbedGrabbable()
        {
            return Interactor.SelectedInteractable != null
                ? Interactor.SelectedInteractable.gameObject.GetComponent<AnimalGrabbable>()
                : null;
        }

        public float GetUp()
        {
            return Type == OVRInput.Controller.LHand ? -transform.up.y : transform.up.y;
        }

        public bool InClenchedPose()
        {
            return GrabInteractor.HandGrabApi.IsHandPalmGrabbing( GrabbingRule.DefaultPalmRule );
        }

        public bool IsGrabbing()
        {
            return isGrabbing != null;
        }

        public bool IsGrabbing(AnimalGrabbable grabbable)
        {
            return isGrabbing == grabbable;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            if ( Application.isPlaying && DebugValues && physicsTracker.Speed >= DebugMinSpeed )
            {
                Debug.DrawRay( transform.position,
                    physicsTracker.Speed * 0.5f * transform.parent.TransformDirection( physicsTracker.Direction ),
                    Color.blue );

                Gizmos.color = Color.green;
                Gizmos.DrawSphere(
                    transform.position + transform.parent.TransformDirection( physicsTracker.Velocity ) * 0.5f, 0.05f );
            }
        }
#endif

        void OnTriggerStay(Collider other)
        {
            /*
             * if not grabbing anything
             *      if collider is a grabbable
             *          if clenching
             *              switch type of object
             *                  -   Detached Limb: grab limb
             *                  -   Attached Limb:
             *                      1st hand: grab animal
             *                      2nd hand: grab attached limb
             *                  -   Root Limb: grab Animal Controller
             *
             * if grabbing and grabbed is the collider's Limb / Animal Controller
             *      if not clenching (anymore)
             *          -   Release Limb (with speed and direction of hand)
             *
             * TODO: we could add a time threshold for clencing unclenching to fix possible errors of losing a hand in sight?
             */
            string tag = other.gameObject.tag;

            // switch ( tag )
            // {
            //     case "Limb":
            //     case "LimpLimb":
            //     case "AnimalController":
            //         AnimalGrabbable grabbable = other.gameObject.GetComponent<AnimalGrabbable>();
            //
            //         if ( !IsGrabbing() && InClenchedPose() )
            //         {
            //             isGrabbing = grabbable;
            //             grabbable.Grab( this );
            //         }
            //         else if ( IsGrabbing( grabbable ) && !InClenchedPose() )
            //         {
            //             isGrabbing = null;
            //             grabbable.Release(); // Todo add speed and direction, or velocity (which is it's combination)
            //         }
            //
            //         break;
            // }
        }

        public void SetFollower(Transform worldTransform)
        {
            follower.transform.position = worldTransform.position;
            follower.transform.rotation = worldTransform.rotation;
        }

        public void SetHeldModeObject(GameObject to)
        {
            heldModeObject = to;
        }

        public void UnSetHeldModeObject()
        {
            heldModeObject = null;
        }

        public void SetActiveEmbodiment(AnimalEmbodiment newEmbodiment)
        {
            Debug.Log( "[Hand] embodying animal" );
            HideActiveEmbodiment();
            if ( Embodiments.ContainsKey( newEmbodiment ) )
            {
                activeEmbodiment = newEmbodiment;
            }
            else
            {
                activeEmbodiment = AnimalEmbodiment.Normal;
            }

            RenderActiveEmbodiment();
        }

        void HideActiveEmbodiment()
        {
            if ( Embodiments.ContainsKey( activeEmbodiment ) )
            {
                Embodiments[activeEmbodiment].SetActive( false );
                embodimentShown = false;
            }
        }

        void RenderActiveEmbodiment()
        {
            if ( Embodiments.ContainsKey( activeEmbodiment ) )
            {
                Embodiments[activeEmbodiment].SetActive( true );
                embodimentShown = true;
            }
        }

        void Start()
        {
            follower = new GameObject();
            follower.name = "Follower";
            follower.transform.parent = transform;
            follower.transform.localPosition = Vector3.zero;

            Vector3 position = transform.localPosition;
            physicsTracker.Reset( position, transform.localRotation, Vector3.zero, Vector3.zero );
            lastPosition = position;
        }

        void Update()
        {
            Vector3 position = transform.localPosition;
            physicsTracker.Update( position, transform.localRotation, Time.smoothDeltaTime );

            lastPosition = position;

            lastSpeeds.Add( physicsTracker.Speed );
            if ( lastSpeeds.Count > 50 )
            {
                lastSpeeds.RemoveAt( 0 );
            }

            if ( HandActiveState.IsTracked )
            {
                handIsntTracked = 0;
                if ( !embodimentShown )
                {
                    RenderActiveEmbodiment();
                }
            }
            else
            {
                handIsntTracked += Time.deltaTime;
                if ( handIsntTracked >= handIsntTrackedThreshold )
                {
                    HideActiveEmbodiment();
                }
            }
        }
    }
}