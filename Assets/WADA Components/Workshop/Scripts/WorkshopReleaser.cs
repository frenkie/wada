using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class WorkshopReleaser : MonoBehaviour
    {
        public Transform[] StartPoints;

        const float BOX = .5f;
        float reachForReleasing = .4f;
        float travelStart = -.05f;
        int startIndex = 0;

        Dictionary<AnimalController, WorkshopToHeaven> releaseCandidates = new();

        bool AnimalGrabbedByAnyHand(AnimalGrabbable animal)
        {
            WadaHand Left = PlayerController.GetInstance().GetLocomotion().LeftHand;
            WadaHand Right = PlayerController.GetInstance().GetLocomotion().RightHand;
            AnimalGrabbable inLeft = Left.GetGrabbedGrabbable();
            AnimalGrabbable inRight = Right.GetGrabbedGrabbable();

            return (inLeft != null && inLeft == animal) || (inRight != null && inRight == animal);
        }

        public void ClearReleaseCandidate()
        {
            // fade out particles (or lower the emission rates) and 
            // releaseCandidate = null;
            // if ( releaseCandidateToHeaven != null )
            // {
            //     releaseCandidateToHeaven.EndFlight();
            // }
        }

        WorkshopToHeaven CreateReleaseCandidate(AnimalController animal)
        {
            Debug.Log( "creating release candidate for " + animal.name );
            GameObject releaseCandidateGo = animal.GetHighlightedCopy();
            releaseCandidateGo.transform.position = GetStartPoint();

            WorkshopToHeaven releaseCandidateToHeaven = releaseCandidateGo.AddComponent<WorkshopToHeaven>();
            releaseCandidateToHeaven.Animal = animal;
            releaseCandidateToHeaven.OnReleased += DoRelease;

            return releaseCandidateToHeaven;
        }

        public void DoRelease(AnimalController animal)
        {
            Workshop.GetInstance().PreReleaseAnimal( animal );
            releaseCandidates.Remove( animal );
            PlayerController.GetInstance().Invoke( "EnableHandGrabs", .5f );
        }

        public void ForcePendingReleasers()
        {
            List<WorkshopToHeaven> releasers = new();

            foreach ( KeyValuePair<AnimalController, WorkshopToHeaven> releaseCandidate in releaseCandidates )
            {
                if ( releaseCandidate.Value.Releasing() )
                {
                    releasers.Add( releaseCandidate.Value );
                }
            }

            foreach ( WorkshopToHeaven releaser in releasers )
            {
                if ( releaser.Releasing() )
                {
                    releaser.ForceRelease();
                }
            }
        }

        Vector3 GetStartPoint()
        {
            int start = startIndex;
            startIndex++;
            if ( startIndex == StartPoints.Length )
            {
                startIndex = 0;
            }

            return StartPoints[start].position;
        }


        bool IsReleaseCandidate(AnimalController animal)
        {
            return releaseCandidates.ContainsKey( animal ) && releaseCandidates[animal].gameObject.activeInHierarchy;
        }

        public void OnAnimalGrabbed(AnimalController animal)
        {
            if ( Workshop.GetInstance().IsInWorkshop( animal ) )
            {
                if ( releaseCandidates.ContainsKey( animal ) )
                {
                    releaseCandidates[animal].transform.position = GetStartPoint();
                    releaseCandidates[animal].gameObject.SetActive( true );
                }
                else
                {
                    releaseCandidates.Add( animal, CreateReleaseCandidate( animal ) );
                }
            }
        }

        public void OnAnimalGrabReleased(AnimalController animal)
        {
            StartCoroutine( DebouncedAnimalReleased( animal ) );
        }

        IEnumerator DebouncedAnimalReleased(AnimalController animal)
        {
            yield return new WaitForSeconds( .1f );

            // make sure it's not grabbed by the other hand
            if ( !AnimalGrabbedByAnyHand( animal ) && releaseCandidates.ContainsKey( animal ) )
            {
                WorkshopToHeaven releaseCandidate = releaseCandidates[animal];
                if ( releaseCandidate != null && !releaseCandidate.Releasing() )
                {
                    releaseCandidates[animal].gameObject.SetActive( false );
                }
            }
        }

        void OnTriggerStay(Collider other)
        {
            AnimalController animal = other.gameObject.GetComponentInParent<AnimalController>();

            if ( !other.isTrigger && animal != null )
            {
                Limb limb = other.gameObject.GetComponent<Limb>();
                if ( limb != null )
                {
                    return;
                }

                if ( animal.IsGrabbed() )
                {
                    WorkshopToHeaven releaseCandidate = releaseCandidates[animal];
                    if ( releaseCandidate != null && !releaseCandidate.Releasing() )
                    {
                        releaseCandidate.Go();
                        Release( animal );
                    }
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            AnimalController animal = other.gameObject.GetComponentInParent<AnimalController>();
            Limb limb = other.gameObject.GetComponent<Limb>();

            if ( limb == null && !other.isTrigger && animal != null && IsReleaseCandidate( animal ) )
            {
                CancelInvoke( "ClearReleaseCandidate" );
                Invoke( "ClearReleaseCandidate", .3f );
            }
        }

        public void Release(AnimalController animal)
        {
            Workshop.GetInstance().WorkshopModeAudio.Play( 0 );
            PlayerController.GetInstance().DisableHandGrabsMomentarily();
            animal.Invoke( "DisableFreeMovement", .12f );
        }

        void Start()
        {
            AnimalController.OnGrabbed += OnAnimalGrabbed;
            AnimalController.OnGrabReleased += OnAnimalGrabReleased;
        }
    }
}