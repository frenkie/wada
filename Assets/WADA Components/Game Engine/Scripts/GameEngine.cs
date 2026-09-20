using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class GameEngine : MonoBehaviour
    {
        public static event Action<Languages> OnLanguageSwitch = delegate(Languages newLanguage) { };
        public PlayableDirector Timeliner;
        public Guide[] Guides;
        public Transform GuideFlockPoint;
        public Transform GuideFirstWayPoint;

        public Languages ActiveLanguage = Languages.EN;
        
        public GameObject[] TestMontageTargets;

        public SkyboxAdjuster SkyboxAdjust;
        public Light MainLight;

        public InfoPlaqueData DummyData;
        public GameObject PlatterPrefab;
        public GameObject InfoPrefab;
        public GameObject FocusLight;

        public bool Pirated;
        public PirateMessage PirateMessage;

        public WadaBook Book;
        public HeartCounter HeartCounter;
        public PathTraveller PathGoingUp;

        public bool UseRevivalModeTransitionSound = true;
        public SoundEffect RevivalMode;
        public SoundEffect RevivalGetups;
        public SoundEffect RevivalGrabs;
        public SoundEffect WorkshopDetach;
        public SoundEffect WorkshopAttach;
        public SoundEffect WorkshopGrabs;

        public InstructionFollowPlayer HandInstructions;

        public GestureRecognition BookGestureDetector;
        public GestureRecognition WorkshopGestureDetector;

        public Color AmbientTarget;
        Color ambientStart;

        public Workshop Workshop;

        [Header( "Instructions That Wait For External Action" )]
        public AbstractInstruction ChooseGuide;
        public AbstractInstruction ChooseGuide_NL;

        public InstructionsController TouchItInstructions;
        public InstructionsController TouchItInstructions_NL;


        static GameEngine instance;

        Guide activeGuide;

        bool ateFood = false;
        bool workshopGestureWasActive;
        bool bookGestureWasActive;
        bool focusedEnvironment;

        int animalsRevived = 0;
        int animalsGrabbed = 0;
        int madeReviveChoice = 0;
        int madeReviveChoiceSaved = 0;
        int madeReviveChoiceReleased = 0;

        void Awake()
        {
            if ( instance != null && instance != this )
            {
                Debug.LogError( "[GameEngine] Already initiated!" );
            }

            instance = this;

            ambientStart = RenderSettings.ambientSkyColor;
        }

        public void AteFood(AnimalGrabbable grabbable)
        {
            if ( !ateFood )
            {
                ateFood = true;
                NudgeManager.GetInstance().ClearInteractiveNudge( NudgeType.EatAnimal );
            }

            HeartCounter.CancelInvoke( "Hide" );
            if ( grabbable != null )
            {
                GrabbableType grabbableType = grabbable.GetGrabbableType();
                switch ( grabbableType )
                {
                    case GrabbableType.Limb:
                    case GrabbableType.Veggie:
                        Clock.GetInstance().AddBonusTime( .7f );
                        break;

                    default:
                        Clock.GetInstance().AddBonusTime();
                        break;
                }
            }
            else
            {
                Clock.GetInstance().AddBonusTime();
            }

            HeartCounter.UpdateProgress( Clock.GetInstance().GetProgress() );

            HeartCounter.Show();

            HeartCounter.Invoke( "Hide", 6 );
        }

        public void AttachLimb()
        {
            WorkshopAttach.Play();
        }

        public void DemoGotoParadise()
        {
            PirateMessage.enabled = Pirated;

            PathGoingUp.enabled = false;
            GuideFlockPoint.gameObject.GetComponent<Collider>().enabled = false;
            GuideFlockPoint.gameObject.GetComponent<GuideFlockPoint>().enabled = false;

            BackgroundMusicManager.GetInstance().FadeOut( BackgroundMusicType.IntroA );

            JumpToTime( 8.91f );

            PlayerController.GetInstance().AlignToTarget( GuideFlockPoint.TransformPoint( 0, 0, .5f ) );
            PlayerController.GetInstance().GetLocomotion().AllowMovement( true );

            Guides[0].gameObject.transform.position = GuideFirstWayPoint.position;
            Guides[0].gameObject.SetActive( true );
            Guides[0].AllowTouch = true;
            Guides[0].Invoke( "OnTouch", .1f );
        }

        public void DemoGotoLastMinute()
        {
            DemoGotoParadise();

            Clock.GetInstance().Invoke( "EndMinuteDemo", 4f );
        }

        public void DetachLimb()
        {
            WorkshopDetach.Play();
        }

        public void GrabLimb()
        {
            WorkshopGrabs.Play();
        }

        public void EnableFirstAnimalTouch(GameObject animal)
        {
            StartCoroutine( EnableAnimalTouch( animal.GetComponent<Ressurectable>() ) );
        }

        IEnumerator EnableAnimalTouch(Ressurectable animal)
        {
            yield return new WaitForSeconds( 6 );
            animal.AllowTouch = true;
        }

        public void EnterScoutMode()
        {
            if ( activeGuide != null )
            {
                activeGuide.EnterScoutMode();
            }
        }

        public void EnterFocussedEnvironment()
        {
            focusedEnvironment = true;

            InstructionManager.GetInstance().EnterSpecialMode();

            if ( SkyboxAdjust != null )
            {
                SkyboxAdjust.EnterFocusMode();
            }

            Hashtable valueTo = new();

            valueTo.Add( "from", 0 );
            valueTo.Add( "to", 1 );
            valueTo.Add( "time", 2 );
            valueTo.Add( "easetype", iTween.EaseType.easeOutCubic );
            valueTo.Add( "onupdate", "OnFocusValue" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void ExitFocussedEnvironment()
        {
            if ( SkyboxAdjust != null )
            {
                SkyboxAdjust.ExitFocusMode();
            }

            Hashtable valueTo = new();

            valueTo.Add( "from", 1 );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", 2 );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnFocusValue" );

            iTween.ValueTo( gameObject, valueTo );
            focusedEnvironment = false;

            InstructionManager.GetInstance().Invoke( "ExitSpecialMode", 2 );
        }

        public static GameEngine GetInstance()
        {
            return instance;
        }


        public Guide GetGuide()
        {
            return activeGuide;
        }

        public void GrabbedAnimal()
        {
            animalsGrabbed++;

            if ( animalsGrabbed == 1 )
            {
                if ( InstructionManager.GetInstance().GetActiveInstructionType() == AudioInstructionType.FirstGrab )
                {
                    InstructionManager.GetInstance().CancelInstruction( AudioInstructionType.FirstGrab );
                }
            }

            RevivalGrabs.Play();
        }

        public bool HasEatenFood()
        {
            return ateFood;
        }

        public bool HasGuide()
        {
            return activeGuide != null;
        }

        public void HideHandInstructions()
        {
            HandInstructions.HideInstructions();
        }

        bool IsAGuide(GameObject gameObjectToCheck)
        {
            bool isGuide = false;

            foreach ( Guide guide in Guides )
            {
                if ( guide.gameObject == gameObjectToCheck )
                {
                    isGuide = true;
                    break;
                }
            }

            return isGuide;
        }

        public bool IsFocussedEnvironment()
        {
            return focusedEnvironment;
        }

        // for signals
        public void JumpToTime(float jumpTo)
        {
            JumpToTime( (double)jumpTo );
        }

        public void JumpToTime(double jumpTo)
        {
            PauseTimeline();
            if ( Timeliner != null )
            {
                Timeliner.time = jumpTo;
            }

            ResumeTimeline();
        }

        public void MakeReviveChoiceLater(RevivalModus modus)
        {
            switch ( modus )
            {
                case RevivalModus.Saved:
                    madeReviveChoiceSaved++;
                    RevivalMode.Play( 1 );

                    if ( madeReviveChoiceSaved == 2 )
                    {
                        NudgeManager.GetInstance().ClearInteractiveNudge( NudgeType.Backpack );
                        Invoke( "OnWorkshopTutorial", 6 );
                    }

                    break;
            }
        }

        public void MakeReviveChoice(RevivalModus modus)
        {
            madeReviveChoice++;

            switch ( modus )
            {
                case RevivalModus.Saved:
                    madeReviveChoiceSaved++;
                    RevivalMode.Play( 1 );

                    if ( madeReviveChoiceSaved == 2 )
                    {
                        NudgeManager.GetInstance().ClearInteractiveNudge( NudgeType.Backpack );
                        Invoke( "OnWorkshopTutorial", 6 );
                    }

                    break;

                case RevivalModus.Released:
                    madeReviveChoiceReleased++;
                    RevivalMode.Play( 2 );
                    break;
            }

            Ressurectable.DisableAllRevivals = false;
            ToggleBookGestureDetection( true );
            ToggleWorkshopGestureDetection( true );

            switch ( madeReviveChoice )
            {
                case 3:
                    BackgroundMusicManager.GetInstance().PlayAfterRevivalScore(); // if it didn't already

                    Invoke( "ActivateBook", 25 );
                    Invoke( "ActivateWorkshopSaveNudge", 60 );
                    break;
            }

            // Fade out any tutorial hands if they didnt finish themselves already
            if ( NudgeManager.GetInstance().HasListenToNudge( NudgeType.ReviveChoice ) )
            {
                InstructionManager.GetInstance()
                    .DisableSpecialQueueParsing(); // to prevent a clearance triggering a play of a queued one

                InstructionManager.GetInstance().CancelInstruction( AudioInstructionType.FirstTouchWellDone );
                InstructionManager.GetInstance().CancelInstruction( AudioInstructionType.FirstGrab );
                InstructionManager.GetInstance().CancelInstruction( AudioInstructionType.KeepOrSetFree );

                InstructionManager.GetInstance().EnableSpecialQueueParsing();
            }

            if ( madeReviveChoiceSaved > 0 && madeReviveChoiceReleased > 0 &&
                 NudgeManager.GetInstance().HasListenToNudge( NudgeType.ReviveChoice ) )
            {
                Debug.Log( "Clear keep or setfree because we did both" );
                NudgeManager.GetInstance().ClearInteractiveNudge( NudgeType.ReviveChoice );
            }

            BackgroundMusicManager.GetInstance().FadeOut( BackgroundMusicType.Revival );
        }

        public void ActivateBook()
        {
            BookGestureDetector.Activate();
            Invoke( "ActivateBookNudge", 90 );
            InstructionManager.GetInstance().Play( AudioInstructionType.BookOpenA );
        }

        public void ActivateBookNudge()
        {
            if ( WadaBook.GetInstance().NotShownYet() )
            {
                NudgeManager.GetInstance().AddNudgeToInteractionList( NudgeType.Book );
            }
        }

        public void ActivateWorkshopSaveNudge()
        {
            if ( madeReviveChoiceSaved < 2 )
            {
                NudgeManager.GetInstance().AddNudgeToInteractionList( NudgeType.Backpack );
            }
        }

        public void OnWorkshopTutorial()
        {
            WorkshopGestureDetector.Activate();
            Invoke( "ActivateWorkshopNudge", 90 );

            // Book still let's normal instructions play, so don't add it then. We still have the above nudge
            if ( !Book.IsShown() )
            {
                // Schedule it
                InstructionManager.GetInstance().Play( AudioInstructionType.WorkshopOpen );
            }
        }

        public void ActivateWorkshopNudge()
        {
            if ( Workshop.GetInstance().NotShownYet() )
            {
                NudgeManager.GetInstance().AddNudgeToInteractionList( NudgeType.Workshop );
                NudgeManager.GetInstance().AddNudgeToInteractionList( NudgeType.WorkshopPlay );
            }
        }

        void OnDestroy()
        {
            Shader.SetGlobalFloat( "Global_Focus_Factor", 0 );
            RenderSettings.ambientSkyColor = ambientStart;
            DynamicGI.UpdateEnvironment();
        }

        void OnFocusValue(float focus)
        {
            Shader.SetGlobalFloat( "Global_Focus_Factor", focus );

            if ( MainLight != null )
            {
                float lightFactor = 1 - focus;
                MainLight.intensity = Mathf.Lerp( .14f, 1, lightFactor );
            }


            RenderSettings.ambientSkyColor = Color.Lerp( ambientStart, AmbientTarget, focus );
            DynamicGI.UpdateEnvironment();
        }

        public void PauseTimeline()
        {
            if ( Timeliner != null )
            {
                Timeliner.Pause();
            }
        }


        public void ResumeTimeline()
        {
            if ( Timeliner != null )
            {
                PirateMessage.enabled = Pirated;
                Timeliner.Play();
            }
        }

        public void ResumeTimelineAfter(float time)
        {
            Invoke( "ResumeTimeline", time );
        }

        IEnumerator PlayReviveMode()
        {
            StartCoroutine( BackgroundMusicManager.GetInstance().FadeOutAnyActiveMusic() );
            yield return new WaitForSeconds( .5f ); // hard coded for now, to get a quick transitioning

            if ( UseRevivalModeTransitionSound )
            {
                RevivalMode.Play( 0 ); // Entering revival mode transition sound
                yield return new WaitForSeconds( RevivalMode.GetPlayingDuration() );
            }

            // Play it immediately full volume
            BackgroundMusicManager.GetInstance().ForcePlay( BackgroundMusicType.Revival );
        }

        public void RevivedAnimal(Ressurectable ressurected)
        {
            if ( !IsAGuide( ressurected.gameObject ) )
            {
                Debug.Log( "[GameEngine] Revived a non-Guide animal" );

                Ressurectable.DisableAllRevivals = false;
                ToggleBookGestureDetection( false );
                ToggleWorkshopGestureDetection( false );

                StartCoroutine( PlayReviveMode() );

                RevivalGetups.Play(); // A nice random awakening

                RevivalMode.Play( 3 ); // Entering revival mode happy sound

                animalsRevived++;
                switch ( animalsRevived )
                {
                    case 1:
                        NudgeManager.GetInstance().AddNudgeToListenList( NudgeType.ReviveChoice );

                        if ( Book != null )
                        {
                            Book.gameObject.SetActive( true );
                        }

                        break;
                }

                if ( animalsGrabbed == 0 )
                {
                    InstructionManager.GetInstance().Play( AudioInstructionType.FirstTouchWellDone, true );
                }

                Invoke( "PlayReviveInstruction", 3 ); // let the music fade out for our
                // repetitive
            }

            if ( Book != null )
            {
                Book.FoundAnimal( ressurected );
            }
        }

        public void PlayReviveInstruction()
        {
            if ( animalsGrabbed == 0 )
            {
                InstructionManager.GetInstance().Play( AudioInstructionType.FirstGrab, true );
            }

            // if KeepOrSetFree audio has not been totally listened to
            if ( NudgeManager.GetInstance().HasListenToNudge( NudgeType.ReviveChoice ) )
            {
                Debug.Log( "Listen to KeepOrSetFree please!" );
                InstructionManager.GetInstance().Play( AudioInstructionType.KeepOrSetFree, true );
            }
        }

        public void SetGuide(Guide to)
        {
            activeGuide = to;

            foreach ( Guide guide in Guides )
            {
                if ( guide != activeGuide )
                {
                    guide.Discard();
                }
            }

            // fade out instructions
            if ( ChooseGuide != null && ChooseGuide.gameObject.activeInHierarchy )
            {
                ChooseGuide.FadeOut();
            }
            if ( ChooseGuide_NL != null && ChooseGuide_NL.gameObject.activeInHierarchy )
            {
                ChooseGuide_NL.FadeOut();
            }
        }

        public void ShowGuides()
        {
            foreach ( Guide guide in Guides )
            {
                guide.gameObject.SetActive( true );
            }
        }

        public void ShowHiddenGuide()
        {
            foreach ( Guide guide in Guides )
            {
                if ( guide != activeGuide )
                {
                    Debug.Log( "Showing hidden guide; TODO not yet" );
                    //guide.ShowVisually();
                }
            }
        }

        public void SwitchLanguage(Languages newLanguage)
        {
            ActiveLanguage = newLanguage;
            OnLanguageSwitch?.Invoke( newLanguage );
        }

        public void StartGuidedJourney()
        {
            activeGuide.SetTargetPoint( GuideFirstWayPoint, 1 );
        }

        public void TestFadeEnvironment()
        {
            Shader.SetGlobalFloat( "Global_Focus_Factor", 1 );
        }

        public void ToggleBookGestureDetection()
        {
            BookGestureDetector.gameObject.SetActive( !BookGestureDetector.gameObject.activeSelf );
        }

        public void ToggleBookGestureDetection(bool to)
        {
            BookGestureDetector.gameObject.SetActive( to );
        }

        public void ToggleWorkshopGestureDetection()
        {
            WorkshopGestureDetector.gameObject.SetActive( !WorkshopGestureDetector.gameObject.activeSelf );
        }

        public void ToggleWorkshopGestureDetection(bool to)
        {
            WorkshopGestureDetector.gameObject.SetActive( to );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(GameEngine) )]
    public class GameEngineEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GameEngine myTarget = (GameEngine)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Test Fade Environment" ) )
            {
                myTarget.TestFadeEnvironment();
            }

            if ( GUILayout.Button( "Test Workshop" ) )
            {
                myTarget.TestFadeEnvironment();
            }

            if ( GUILayout.Button( "Test Book" ) )
            {
                myTarget.Book.Toggle();
            }

            if ( GUILayout.Button( "Test Ate Food" ) )
            {
                myTarget.AteFood( null );
            }

            if ( GUILayout.Button( "Test Go To Paradise" ) )
            {
                myTarget.DemoGotoParadise();
            }
        }
    }
#endif
}