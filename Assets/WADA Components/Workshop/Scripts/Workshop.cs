using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    /*
     *  Workshop
     *
     *  Releasing an animal
     * -    Where? Up front in the area, so you have to look for it again? Or close to the
     *      Workshop activation point?
     * -    Paused and only playing when workshop is deactivated?
     */
    public class Workshop : MonoBehaviour
    {
        static Workshop instance;

        public Transform PlayerPosition;
        public GameObject[] Detectors;
        public WorkshopSkybox Skybox;
        public WorkshopReleaser Releaser;
        public SkyboxAdjuster MainSkybox;
        public GameObject Clouds;
        public WorkshopBeamAudio BeamAudio;
        public SoundEffect WorkshopModeAudio;

        public int SpotCount = 10;
        public float Radius = .6f;
        int releaseIndex;

        GameObject relocator;

        List<Vector3> dropLocations = new();
        int dropIndex = 0;
        int animalsSaved = 0;

        List<AnimalController> isHere = new();
        List<AnimalController> toRelease = new(); // animals that are thrown back get removed from the workshop

        bool active = false;
        float farClipPlane;

        bool firstAnimalReleased;

        bool firstExit = true;
        bool firstEnter = true;
        bool firstGrab = true;
        bool firstNewAnimal = true;

        float outOfBoundsCheckTime; // for animal bodies
        float outOfBoundsCheckInterval = 1;

        public void Activate()
        {
            active = true;
            releaseIndex = 0;

            foreach ( AnimalController animal in isHere )
            {
                animal.gameObject.SetActive( true );
            }

            Transform current = PlayerController.GetInstance().GetTransform();
            relocator.transform.position = current.position;
            relocator.transform.rotation = current.rotation;
            PlayerController.GetInstance().EnterWorkshopMode();

            PlayerController.GetInstance().AlignToCenterFloorTarget( PlayerPosition );

            Camera.main.farClipPlane = 250;

            GameEngine.GetInstance().HideHandInstructions();
            BackgroundMusicManager.GetInstance().FadeIn( BackgroundMusicType.Workshop );

            Skybox.FadeInWorkshop();
            if ( MainSkybox != null )
            {
                MainSkybox.FadeInWorkshop();
            }

            Invoke( "ActivateCloud", .4f );

            GameEngine.GetInstance().ToggleBookGestureDetection( false );
            GameEngine.GetInstance().ToggleWorkshopGestureDetection( true );
            Ressurectable.DisableAllRevivals = true;

            InstructionManager.GetInstance().EnterSpecialMode();

            Invoke( "PlayInstructions", 2f );
        }

        public void ActivateCloud()
        {
            Clouds.SetActive( true );
            BeamAudio.FadeInSound();
        }

        public void PlayInstructions()
        {
            if ( firstEnter )
            {
                firstEnter = false;

                NudgeManager.GetInstance().ClearInteractiveNudge( NudgeType.Workshop );
                NudgeManager.GetInstance().ClearInteractiveNudge( NudgeType.WorkshopPlay );

                InstructionManager.GetInstance().Play( AudioInstructionType.WelcomeInWorkshop, true );
                InstructionManager.GetInstance().Play( AudioInstructionType.WorkshopIntro, true );
                InstructionManager.GetInstance().Play( AudioInstructionType.WorkshopIntroPickup, true );

                Invoke( "CheckForWorkshopExitNudge", 3 * 60 );
            }
        }

        public void AddAnimal(AnimalController animal)
        {
            animal.gameObject.transform.position = dropLocations[dropIndex];
            animal.Invoke( "Activate", .2f ); // activating workshop mode
            StartCoroutine( HideAnimalForPerformance( animal ) );

            isHere.Add( animal );

            dropIndex++;
            if ( dropIndex >= dropLocations.Count )
            {
                dropIndex = 0;
            }

            animalsSaved++;
            if ( animalsSaved == 2 )
            {
                foreach ( GameObject detector in Detectors )
                {
                    detector.SetActive( true );
                }
            }
        }

        IEnumerator HideAnimalForPerformance(AnimalController animal)
        {
            yield return new WaitForSeconds( 1f );

            if ( !IsActive() )
            {
                animal.gameObject.SetActive( false );
            }
        }

        void Awake()
        {
            if ( instance != null && instance != this )
            {
                Debug.LogError( "[Workshop] Already initiated!" );
            }

            instance = this;

            relocator = new GameObject( "Relocator" );
            relocator.transform.parent = transform;

            farClipPlane = Camera.main.farClipPlane;

            CreateSpotLocations();
        }

        void CheckForWorkshopExitNudge()
        {
            if ( IsActive() && firstExit &&
                 !InstructionManager.GetInstance().IsScheduled( AudioInstructionType.WorkshopExit ) )
            {
                InstructionManager.GetInstance().Play( AudioInstructionType.WorkshopExit, true );
            }
        }

        void CreateSpotLocations()
        {
            float angleStep = 360 / SpotCount;

            for ( int i = 1; i < SpotCount; i++ )
            {
                Vector3 location = new(
                    Mathf.Sin( i * angleStep ) * Radius,
                    1f,
                    Mathf.Cos( i * angleStep ) * Radius
                );
                dropLocations.Add( transform.position + location );
            }
        }

        public void DeActivate()
        {
            active = false;
            int releaseCount = toRelease.Count;
            firstExit = false;

            // Animate and reposition all animals that should be released
            ReleaseAnimals();

            PlayerController.GetInstance().AlignToFloorTarget( relocator.transform );
            PlayerController.GetInstance().ExitWorkshopMode();

            // hide all animals for performance
            foreach ( AnimalController animal in isHere )
            {
                animal.gameObject.SetActive( false );
            }

            Camera.main.farClipPlane = farClipPlane;

            Skybox.FadeOutWorkshop();
            if ( MainSkybox != null )
            {
                MainSkybox.FadeOutWorkshop();
            }

            if ( !firstAnimalReleased && releaseCount > 0 )
            {
                firstAnimalReleased = true;
                Debug.Log( "Going to musical score because of releasing " + releaseCount );
                BackgroundMusicManager.GetInstance().PlayMusicalScore();
            }

            BackgroundMusicManager.GetInstance().FadeOut( BackgroundMusicType.Workshop );

            Clouds.SetActive( false );
            BeamAudio.FadeOutSound();

            InstructionManager.GetInstance().ExitSpecialMode();
            Ressurectable.DisableAllRevivals = false;
            GameEngine.GetInstance().ToggleWorkshopGestureDetection( true );
            GameEngine.GetInstance().ToggleBookGestureDetection( true );
        }

        public void DemoRelease()
        {
            if ( isHere.Count > 0 )
            {
                PreReleaseAnimal( isHere[0] );
            }
        }

        public void DropInBounds(GameObject limbOrAnimal)
        {
            limbOrAnimal.transform.position = dropLocations[dropIndex];
        }

        public static Workshop GetInstance()
        {
            return instance;
        }

        public bool IsInWorkshop(AnimalController animal)
        {
            return isHere.Contains( animal ) || toRelease.Contains( animal );
        }

        public bool IsActive()
        {
            return active;
        }

        public bool IsFull()
        {
            return isHere.Count >= 7;
        }

        public bool NotShownYet()
        {
            return firstEnter;
        }

        public bool IsReleased(AnimalController animal)
        {
            return toRelease.Contains( animal );
        }

        public void OnAttachedLimb(Limb limb, AnimalController animal)
        {
            if ( IsActive() )
            {
                if ( firstNewAnimal && animal != limb.GetOriginalAnimalController() )
                {
                    firstNewAnimal = false;
                    // Limb.OnAttached -= OnAttachedLimb;

                    if ( toRelease.Count == 0 )
                    {
                        InstructionManager.GetInstance().Play( AudioInstructionType.WorkshopReleaseAnimal, true );

                        // TODO Add an interaction nudge to make sure we remind people of this option?
                        // But we might want to handle it in the Workshop to have control over the timing
                    }
                }

                // Some nice cheers every now and then?
            }
        }

        public void OnGrabbedAnimal(AnimalController animal)
        {
            if ( IsActive() && isHere.Contains( animal ) && firstGrab )
            {
                firstGrab = false;
                AnimalController.OnGrabbed -= OnGrabbedAnimal;

                InstructionManager.GetInstance().ForcePlay( AudioInstructionType.WorkshopIntroPickedUp, true );
                InstructionManager.GetInstance().Play( AudioInstructionType.WorkshopIntroTakeApart, true );
                InstructionManager.GetInstance().Play( AudioInstructionType.WorkshopIntroMakeNewAnimal, true );
                InstructionManager.GetInstance().Play( AudioInstructionType.WorkshopExit, true );
            }
        }

        public void OnGrabbedLimb(Limb limb)
        {
            if ( IsActive() )
            {
                if ( firstGrab )
                {
                    firstGrab = false;
                    //Limb.OnGrabbed -= OnGrabbedLimb;

                    InstructionManager.GetInstance().ForcePlay( AudioInstructionType.WorkshopIntroPickedUp, true );
                    InstructionManager.GetInstance().Play( AudioInstructionType.WorkshopIntroTakeApart, true );
                    InstructionManager.GetInstance().Play( AudioInstructionType.WorkshopIntroMakeNewAnimal, true );
                    InstructionManager.GetInstance().Play( AudioInstructionType.WorkshopExit, true );
                }

                // Maybe some nudges if people don't create animals?
            }
        }

        public void PreReleaseAnimal(AnimalController animal)
        {
            if ( !animal.GetGodModeReadyToBeReleased() )
            {
                firstNewAnimal = false; // cause you apparently already know to find the releaser beam

                float angleStep = 40;

                Vector3 dropLocation = new(
                    Mathf.Cos( releaseIndex * angleStep ) * Radius,
                    .4f,
                    Mathf.Sin( releaseIndex * angleStep ) * Radius
                );

                // TODO double check if that point is above a 'floor'
                animal.gameObject.transform.position = relocator.transform.position + dropLocation;

                isHere.Remove( animal );
                toRelease.Add( animal );

                if ( !firstAnimalReleased && toRelease.Count == 1 && firstExit )
                {
                    InstructionManager.GetInstance().Play( AudioInstructionType.WorkshopExit, true );
                }

                animal.DisableFreeMovement();
                animal.SetGodModeReadyToBeReleased( true );

                releaseIndex++;

                if ( releaseIndex >= dropLocations.Count )
                {
                    releaseIndex = 0;
                }
            }
        }

        public void ReleaseAnimals()
        {
            /*
                Check if animals are being released
                If so, release them directly
             */
            Releaser.ForcePendingReleasers();

            foreach ( AnimalController animal in toRelease )
            {
                animal.Animate();
            }

            toRelease = new List<AnimalController>();
        }

        void Start()
        {
            AnimalController.OnGrabbed += OnGrabbedAnimal;
            Limb.OnGrabbed += OnGrabbedLimb;
            Limb.OnAttached += OnAttachedLimb;
        }

        public void Toggle()
        {
            if ( active )
            {
                DeActivate();
            }
            else
            {
                Activate();
            }
        }

        void Update()
        {
            outOfBoundsCheckTime += Time.deltaTime;
            if ( outOfBoundsCheckTime > outOfBoundsCheckInterval )
            {
                outOfBoundsCheckTime = 0;

                // foreach ( AnimalController animal in isHere )
                // {
                //     if ( !toRelease.Contains( animal ) &&
                //          Vector3.Distance( animal.transform.position, transform.position ) > 45 )
                //     {
                //         Debug.Log( "Whooops, dropping animal back in the workshop " + animal.gameObject.name );
                //         animal.GetRigidbody().velocity = Vector3.zero;
                //         animal.GetRigidbody().angularVelocity = Vector3.zero;
                //         DropInBounds( animal.gameObject );
                //     }
                // }
            }
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(Workshop) )]
    public class WorkshopEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Workshop myTarget = (Workshop)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Activate" ) )
            {
                myTarget.Activate();
            }

            if ( GUILayout.Button( "DeActivate" ) )
            {
                myTarget.DeActivate();
            }

            if ( GUILayout.Button( "Toggle" ) )
            {
                myTarget.Toggle();
            }

            if ( GUILayout.Button( "DemoRelease" ) )
            {
                myTarget.DemoRelease();
            }
        }
    }
#endif
}