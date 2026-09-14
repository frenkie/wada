using System;
using MirzaBeig.Scripting.Effects;
using UnityEngine;

namespace Wada
{
    public class LimbSocket : MonoBehaviour
    {
        public Rigidbody JointRigidbody;
        public Limb ConnectedLimb;
        public Renderer ProximityRenderer;
        public Transform Parent;

        public LimbTypeMask AllowLimbs = LimbTypeMask.Head |
                                         LimbTypeMask.Tail |
                                         LimbTypeMask.Wing |
                                         LimbTypeMask.FrontLeg |
                                         LimbTypeMask.BackLeg;

        AnimalController controller;

        void Awake()
        {
            controller = GetComponentInParent<AnimalController>();

            ProximityRenderer = GetComponentInChildren<Renderer>();
            ProximityRenderer.enabled = false;
        }

        public void AttachLimb(Limb limb)
        {
            /*
             * TODO Set limb at correct rotation locally?
             */
            ConnectedLimb = limb;
            ConnectedLimb.transform.parent = Parent;
            ConnectedLimb.transform.localPosition =
                Parent.InverseTransformPoint( transform.position ) - ConnectedLimb.GetAnchorOffset();

            // Temp, hide attachpoint, and disable collider?
            ProximityRenderer.enabled = false;
        }

        public bool CanLimbAttach(Limb limb)
        {
            return ConnectedLimb == null &&
                   limb.CloseToSocket == this &&
                   IsLimbAllowed( limb );
        }

        public void DetachLimb()
        {
            if ( ConnectedLimb != null )
            {
                ConnectedLimb.GetGameObject().transform.parent = null;
                ConnectedLimb = null;
            }
        }

        public AnimalController GetAnimalController()
        {
            return controller;
        }

        bool IsLimbAllowed(Limb limb)
        {
            return true;
            // TODO currently testing if we want to allow all locations
            //return (int)(AllowLimbs & (LimbTypeMask)Enum.Parse( typeof(LimbTypeMask), limb.Type.ToString() )) > 0;
        }

        public bool IsAnimalReadyForReleaseInGodMode()
        {
            return controller.GetGodModeReadyToBeReleased();
        }

        public void ReleaseForGodMode()
        {
            controller.ReleaseForGodMode();
        }

        public void OnPhysicsThrust(Transform touchPoint, Limb limb)
        {
            controller.OnPhysicsThrust( touchPoint, limb );
        }

        void OnTriggerExit(Collider other)
        {
            Limb limb = other.gameObject.GetComponentInParent<Limb>();
            if ( limb != null && limb.CloseToSocket == this )
            {
                Debug.Log( "Exitinggg " + limb.GetGameObject().name );
                limb.CloseToSocket = null;

                limb.Repel();

                ProximityRenderer.enabled = false;
            }
        }

        void OnTriggerStay(Collider other)
        {
            if ( ConnectedLimb == null )
            {
                Limb limb = other.gameObject.GetComponentInParent<Limb>();
                if ( limb != null && limb.IsDetached() && !limb.Docked && limb.CloseToSocket != this )
                {
                    if ( limb.CloseToSocket == null )
                    {
                        limb.CloseToSocket = this;
                        if ( IsLimbAllowed( limb ) )
                        {
                            // Signal that it can be attached
                            // ProximityRenderer.enabled = true;
                            ProximityRenderer.enabled = false;

                            limb.Attract();
                        }
                    }
                    else if (
                        Vector3.Distance( limb.GetGameObject().transform.position,
                            limb.CloseToSocket.gameObject.transform.position )
                        > Vector3.Distance( limb.GetGameObject().transform.position, transform.position )
                    )
                    {
                        limb.CloseToSocket = this;
                        if ( IsLimbAllowed( limb ) )
                        {
                            // ProximityRenderer.enabled = true;
                            ProximityRenderer.enabled = false;

                            limb.Attract();
                        }
                    }
                }
            }
        }

        void Start()
        {
            if ( Parent == null )
            {
                Parent = transform;
            }
        }
    }
}