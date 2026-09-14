using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Wada
{
    public class NudgeManager : MonoBehaviour
    {
        static NudgeManager instance;

        public List<NudgeType> NotListenedTo = new();
        public List<NudgeType> NotInteractedWith = new();
        public float NudgeInterval = 60f;

        float nudgeTimer;
        int nudgeIndex;

        void Awake()
        {
            if ( instance != null && instance != this )
            {
                Debug.LogError( "[NudgeManager] Already initiated!" );
            }

            instance = this;
        }

        public void AddNudgeToLists(NudgeType nudge)
        {
            AddNudgeToInteractionList( nudge );
            AddNudgeToListenList( nudge );
        }

        public void AddNudgeToInteractionList(NudgeType nudge)
        {
            NotInteractedWith.Add( nudge );
        }

        public void AddNudgeToListenList(NudgeType nudge)
        {
            NotListenedTo.Add( nudge );
        }

        public void ClearInteractiveNudge(NudgeType nudge)
        {
            ClearInitialNudge( nudge );

            if ( NotInteractedWith.Contains( nudge ) )
            {
                NotInteractedWith.Remove( nudge );
            }
        }

        public void ClearInitialNudge(NudgeType nudge)
        {
            if ( NotListenedTo.Contains( nudge ) )
            {
                NotListenedTo.Remove( nudge );
            }
        }

        AudioInstructionType GetInitialInstructionForNudge(NudgeType nudge)
        {
            AudioInstructionType instructionType = AudioInstructionType.None;

            switch ( nudge )
            {
                case NudgeType.Book:
                    instructionType = AudioInstructionType.BookOpenA;
                    break;
                case NudgeType.Workshop:
                    instructionType = AudioInstructionType.WorkshopOpen;
                    break;

                case NudgeType.WorkshopPlay:
                    instructionType = AudioInstructionType.WorkshopOpen;
                    break;

                case NudgeType.Backpack:
                    instructionType = AudioInstructionType.NudgeBackpack;
                    break;

                case NudgeType.StoreAnimal:
                case NudgeType.ReleaseAnimal:
                    instructionType = AudioInstructionType.KeepOrSetFree;
                    break;

                case NudgeType.EatAnimal:
                    instructionType = AudioInstructionType.HowToEat;
                    break;

                case NudgeType.WorkshopReleaseAnimal:
                    //for now we handle this one explicitly in the workshop initially
                    //instructionType = AudioInstructionType.WorkshopReleaseAnimal;
                    break;
            }

            return instructionType;
        }

        public static NudgeManager GetInstance()
        {
            return instance;
        }

        bool GetSpecialModeForNudge(NudgeType nudge)
        {
            switch ( nudge )
            {
                case NudgeType.StoreAnimal:
                case NudgeType.ReleaseAnimal:
                case NudgeType.ReviveChoice:
                case NudgeType.WorkshopReleaseAnimal:
                    return true;
            }

            return false;
        }

        bool IsSpecialNudgeAllowed(NudgeType nudge)
        {
            switch ( nudge )
            {
                case NudgeType.StoreAnimal:
                case NudgeType.ReleaseAnimal:
                case NudgeType.ReviveChoice:
                    return GameEngine.GetInstance().IsFocussedEnvironment();

                case NudgeType.WorkshopReleaseAnimal:
                    return Workshop.GetInstance().IsActive();
            }

            return false;
        }

        AudioInstructionType GetNudgeInstructionForNudge(NudgeType nudge)
        {
            AudioInstructionType instructionType = AudioInstructionType.None;

            switch ( nudge )
            {
                case NudgeType.Book:
                    instructionType = AudioInstructionType.BookOpenA;
                    break;
                case NudgeType.Workshop:
                    instructionType = AudioInstructionType.WorkshopOpen;
                    break;

                case NudgeType.WorkshopPlay:
                    instructionType = AudioInstructionType.NudgePlayInWorkshop;
                    break;

                case NudgeType.Backpack:
                    instructionType = AudioInstructionType.NudgeBackpack;
                    break;

                case NudgeType.StoreAnimal:
                case NudgeType.ReleaseAnimal:
                    instructionType = AudioInstructionType.KeepOrSetFree;
                    break;

                case NudgeType.EatAnimal:
                    List<AudioInstructionType> nudges = new()
                    {
                        AudioInstructionType.NudgeEatSomething,
                        AudioInstructionType.NudgeVegetarian
                    };
                    instructionType = nudges[Random.Range( 0, nudges.Count )];
                    break;

                case NudgeType.WorkshopReleaseAnimal:
                    //for now we don't want a reminder yet, maybe handle it in the workshop with longer time intervals
                    //instructionType = AudioInstructionType.WorkshopReleaseAnimal;
                    break;
            }

            return instructionType;
        }

        List<NudgeType> GetEligbleNudgesForGameState()
        {
            List<NudgeType> eligibleNudges = new();

            foreach ( NudgeType nudge in NotInteractedWith )
            {
                bool add = false;
                switch ( nudge )
                {
                    case NudgeType.Book:
                        add = !(GameEngine.GetInstance().IsFocussedEnvironment() || Workshop.GetInstance().IsActive());
                        break;

                    case NudgeType.Workshop:
                    case NudgeType.WorkshopPlay:
                        add = !(GameEngine.GetInstance().IsFocussedEnvironment() || WadaBook.GetInstance().IsShown());
                        break;

                    case NudgeType.Backpack:
                        add = !GameEngine.GetInstance().IsFocussedEnvironment();
                        break;

                    case NudgeType.StoreAnimal:
                    case NudgeType.ReleaseAnimal:
                    case NudgeType.ReviveChoice:
                        add = GameEngine.GetInstance().IsFocussedEnvironment();
                        break;

                    case NudgeType.EatAnimal:
                    case NudgeType.EatVegetarian:
                        add = !GameEngine.GetInstance().IsFocussedEnvironment();
                        break;

                    case NudgeType.WorkshopReleaseAnimal:
                        //for now we don't want a reminder yet, maybe handle it in the workshop with longer time intervals
                        //add = Workshop.GetInstance().IsActive()
                        break;
                }

                if ( add )
                {
                    eligibleNudges.Add( nudge );
                }
            }

            return eligibleNudges;
        }

        public bool HasListenToNudge(NudgeType nudge)
        {
            return NotListenedTo.Contains( nudge );
        }

        public bool HasInteractionNudge(NudgeType nudge)
        {
            return NotInteractedWith.Contains( nudge );
        }

        void Nudge()
        {
            List<NudgeType> nudges = GetEligbleNudgesForGameState();
            if ( nudges.Count > 0 )
            {
                Debug.Log( "Eligble nudges:" );
                Debug.Log( nudges );

                if ( nudgeIndex > nudges.Count - 1 )
                {
                    nudgeIndex = 0;
                }

                NudgeType nudge = nudges[nudgeIndex];
                bool notListenedTo = NotListenedTo.Contains( nudge );

                // to make sure nudge doesn't get double entered or user is overnudged at all
                bool noInstructionsWaiting = InstructionManager.GetInstance().IsEmpty( false );
                bool noSpecialInstructionsWaiting = InstructionManager.GetInstance().IsEmpty( true );
                AudioInstructionType initialInstruction = GetInitialInstructionForNudge( nudge );
                AudioInstructionType nudgeInstruction = GetNudgeInstructionForNudge( nudge );

                switch ( nudge )
                {
                    case NudgeType.EatAnimal:
                        if ( InstructionManager.GetInstance().IsInSpecialMode() )
                        {
                            if ( noSpecialInstructionsWaiting )
                            {
                                InstructionManager.GetInstance()
                                    .Play( notListenedTo ? initialInstruction : nudgeInstruction, true );
                            }
                        }
                        else if ( noInstructionsWaiting )
                        {
                            InstructionManager.GetInstance()
                                .Play( notListenedTo ? initialInstruction : nudgeInstruction );
                        }

                        break;

                    default:
                        if ( GetSpecialModeForNudge( nudge ) )
                        {
                            if ( IsSpecialNudgeAllowed( nudge ) && noSpecialInstructionsWaiting )
                            {
                                InstructionManager.GetInstance()
                                    .Play( notListenedTo ? initialInstruction : nudgeInstruction, true );
                            }
                        }
                        else if ( noInstructionsWaiting )
                        {
                            InstructionManager.GetInstance()
                                .Play( notListenedTo ? initialInstruction : nudgeInstruction );
                        }

                        break;
                }

                nudgeIndex += 1;
            }
        }

        public void OnClockProgress(float progress)
        {
            if ( NotInteractedWith.Count > 0 )
            {
                nudgeTimer += 1; // it's one second each progress call
                if ( nudgeTimer >= NudgeInterval )
                {
                    nudgeTimer = 0;
                    Nudge();
                }
            }
        }

        public void ResetTime()
        {
            nudgeTimer = 0;
        }

        void Start()
        {
            Clock.OnProgress += OnClockProgress;
        }
    }
}