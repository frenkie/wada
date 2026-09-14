using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    [RequireComponent( typeof(AudioSource) )]
    public class Mouth : MonoBehaviour
    {
        public LayerMask Edible;
        public List<AudioClip> Munches;

        AudioSource mouth;

        float currInMouthTime;
        float currInMouthTimeThreshold = .4f;

        Veggie veggieToEat;
        Limb limbToEat;
        Ressurectable animalToEat; // for held animals

        void Awake()
        {
            mouth = GetComponent<AudioSource>();
        }

        void OnTriggerStay(Collider other)
        {
            if ( other.tag == "Hand" )
            {
                WadaHand hand = PlayerController.GetInstance().GetLocomotion()
                    .GetWadaHandForCollider( other.gameObject );
                GameObject grabInteractable = hand.GetGrabbedElement();

                if ( grabInteractable != null && ((1 << grabInteractable.layer) & Edible) != 0 )
                {
                    Veggie veggie = grabInteractable.GetComponentInParent<Veggie>();
                    Ressurectable animal = grabInteractable.GetComponentInParent<Ressurectable>();
                    Limb limb = grabInteractable.GetComponentInParent<Limb>();

                    // held mode should be obvious since it's the grabbed one, but still
                    if ( animal != null && animal.InHeldMode() )
                    {
                        if ( animalToEat != animal )
                        {
                            Debug.Log( "[Mouth] hit an animal " + animal.gameObject.name );
                            currInMouthTime = 0;
                            animalToEat = animal;
                            veggieToEat = null;
                            limbToEat = null;
                        }
                    }
                    else if ( limb != null && limb.IsDetached() && !limb.Docked && limbToEat != limb )
                    {
                        Debug.Log( "[Mouth] hit a limb " + limb.gameObject.name );
                        currInMouthTime = 0;
                        limbToEat = limb;
                        veggieToEat = null;
                        animalToEat = null;
                    }
                    else if ( veggie != null && veggieToEat != veggie )
                    {
                        Debug.Log( "[Mouth] hit a veggie " + veggie.gameObject.name );
                        currInMouthTime = 0;
                        veggieToEat = veggie;
                        limbToEat = null;
                        animalToEat = null;
                    }
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if ( other.tag == "Hand" )
            {
                WadaHand hand = PlayerController.GetInstance().GetLocomotion()
                    .GetWadaHandForCollider( other.gameObject );
                GameObject grabInteractable = hand.GetGrabbedElement();

                if ( grabInteractable != null && ((1 << grabInteractable.layer) & Edible) != 0 )
                {
                    Ressurectable animal = grabInteractable.GetComponentInParent<Ressurectable>();
                    Limb limb = grabInteractable.GetComponentInParent<Limb>();
                    Veggie veggie = grabInteractable.GetComponentInParent<Veggie>();

                    if ( animal != null && animalToEat == animal )
                    {
                        animalToEat = null;
                        currInMouthTime = 0;
                    }
                    else if ( limb != null && limbToEat == limb )
                    {
                        limbToEat = null;
                        currInMouthTime = 0;
                    }
                    else if ( veggie != null && veggieToEat == veggie )
                    {
                        veggieToEat = null;
                        currInMouthTime = 0;
                    }
                }
            }
        }

        void EatAnimal()
        {
            PlayRandomMunch();

            if ( animalToEat != null )
            {
                if ( animalToEat.EmbodimentType != AnimalEmbodiment.None )
                {
                    PlayerController.GetInstance().EmbodyAnimal( animalToEat.EmbodimentType );
                }

                Flocker flocker = animalToEat.gameObject.GetComponent<Flocker>();
                if ( flocker != null )
                {
                    flocker.RemoveFromFlocker();
                }

                Debug.Log( "[Mouth] Ate Animal Food" );
                GameEngine.GetInstance().AteFood( null ); //chooses default bonus time
                Destroy( animalToEat.gameObject );
                animalToEat = null;
                PlayerController.GetInstance().ExitFocusMode();
            }
        }

        void EatLimb()
        {
            PlayRandomMunch();

            if ( limbToEat != null )
            {
                if ( limbToEat.EmbodimentType != AnimalEmbodiment.None )
                {
                    PlayerController.GetInstance().EmbodyAnimal( limbToEat.EmbodimentType );
                }

                Debug.Log( "[Mouth] Ate Limb Food" );
                GameEngine.GetInstance().AteFood( limbToEat );
                Destroy( limbToEat.gameObject );
                limbToEat = null;
            }
        }

        void EatVeggie()
        {
            PlayRandomMunch();

            if ( veggieToEat != null )
            {
                Debug.Log( "[Mouth] Ate Veggie Food" );
                GameEngine.GetInstance().AteFood( veggieToEat );
                Destroy( veggieToEat.gameObject );
                veggieToEat = null;
            }
        }

        void PlayRandomMunch()
        {
            mouth.clip = Munches[Random.Range( 0, Munches.Count )];
            mouth.Play();
        }

        void Update()
        {
            if ( limbToEat != null || animalToEat != null || veggieToEat != null )
            {
                currInMouthTime += Time.deltaTime;
                // only if we're not reviving animals
                if ( currInMouthTime >= currInMouthTimeThreshold && !GameEngine.GetInstance().IsFocussedEnvironment() &&
                     !Workshop.GetInstance().IsActive() )
                {
                    currInMouthTime = -1;
                    if ( animalToEat != null )
                    {
                        EatAnimal();
                    }
                    else if ( limbToEat != null )
                    {
                        EatLimb();
                    }
                    else if ( veggieToEat != null )
                    {
                        EatVeggie();
                    }

                    NudgeManager.GetInstance().ClearInteractiveNudge( NudgeType.EatAnimal );
                }
            }
        }
    }
}