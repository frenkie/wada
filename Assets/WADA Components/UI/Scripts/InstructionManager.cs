using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wada
{
    [Serializable]
    public class AudioInstructions : SerializableDictionary<AudioInstructionType, AbstractInstruction>
    {
    }

    public class InstructionManager : MonoBehaviour
    {
        static InstructionManager instance;

        public AudioInstructions AudioInstructions;
        public int EndInstructionWithSecondsLeft = 20;
        public Exposure ExposureFader;

        List<AbstractInstruction> instructionQueue = new();
        List<AbstractInstruction> specialInstructionQueue = new();

        AbstractInstruction currentInstruction;
        AbstractInstruction currentSpecialInstruction;

        bool parseNormalQueue = true;
        bool parseSpecialQueue = true;
        bool enableSpecialModeAfterFinish;
        bool playedFinalInstruction;

        bool specialMode;

        void Awake()
        {
            if ( instance != null && instance != this )
            {
                Debug.LogError( "[InstructionManager] Already initiated!" );
            }

            instance = this;
        }

        public void CancelInstruction(AudioInstructionType type)
        {
            if ( AudioInstructions.ContainsKey( type ) )
            {
                AbstractInstruction instruction = AudioInstructions[type];

                if ( IsInSpecialMode() )
                {
                    if ( specialInstructionQueue.Contains( instruction ) )
                    {
                        specialInstructionQueue.Remove( instruction );
                    }

                    if ( currentSpecialInstruction == instruction )
                    {
                        currentSpecialInstruction = null;
                        instruction.FadeOut();
                    }
                }
                else
                {
                    if ( instructionQueue.Contains( instruction ) )
                    {
                        instructionQueue.Remove( instruction );
                    }

                    if ( currentInstruction == instruction )
                    {
                        currentInstruction = null;
                        instruction.FadeOut();
                    }
                }
            }
        }

        void CheckForNudgeClearance(AbstractInstruction instruction)
        {
            AudioInstructionType audioInstructionType = GetAudioInstructionType( instruction );

            Debug.Log( "Finished an instruction, checking for nudge clearance: " + audioInstructionType );

            switch ( audioInstructionType )
            {
                case AudioInstructionType.BookOpenA:
                    NudgeManager.GetInstance().ClearInitialNudge( NudgeType.Book );
                    break;

                case AudioInstructionType.HowToEat:
                    NudgeManager.GetInstance().ClearInitialNudge( NudgeType.EatAnimal );
                    break;

                case AudioInstructionType.KeepOrSetFree:
                    NudgeManager.GetInstance().ClearInitialNudge( NudgeType.ReviveChoice );
                    break;

                case AudioInstructionType.WorkshopOpen:
                    NudgeManager.GetInstance().ClearInitialNudge( NudgeType.Workshop );
                    NudgeManager.GetInstance().ClearInitialNudge( NudgeType.WorkshopPlay );
                    break;

                case AudioInstructionType.WorkshopReleaseAnimal:
                    NudgeManager.GetInstance().ClearInitialNudge( NudgeType.WorkshopReleaseAnimal );
                    break;

                /* Room for the eat instructions: normal and vegetarian */
            }
        }


        public void DisableNormalQueueParsing()
        {
            parseNormalQueue = false;
        }

        public void DisableSpecialQueueParsing()
        {
            parseSpecialQueue = false;
        }

        public void EnableNormalQueueParsing()
        {
            parseNormalQueue = true;
        }

        public void EnableSpecialQueueParsing()
        {
            parseSpecialQueue = true;
        }

        public void EnableSpecialMode()
        {
            specialMode = true;
        }

        public void EnterSpecialMode(bool letLastInstructionFinish = false)
        {
            parseNormalQueue = false;

            if ( currentInstruction != null )
            {
                if ( letLastInstructionFinish )
                {
                    enableSpecialModeAfterFinish = true;
                }
                else
                {
                    EnableSpecialMode();

                    if ( instructionQueue.Contains( currentInstruction ) )
                    {
                        instructionQueue.Remove( currentInstruction );
                    }

                    AbstractInstruction toCancel = currentInstruction;
                    currentInstruction = null; // make sure cancelable one doesn't trigger it's FadeOut End one
                    toCancel.FadeOut();
                }
            }
            else
            {
                EnableSpecialMode();
            }
        }

        public void ExitSpecialMode()
        {
            specialMode = false;

            if ( currentSpecialInstruction != null )
            {
                AbstractInstruction toCancel = currentSpecialInstruction;
                specialInstructionQueue = new List<AbstractInstruction>();
                currentSpecialInstruction = null; // otherwise next time we will not playyyyy
                toCancel.FadeOut();
            }

            NudgeManager.GetInstance().ResetTime(); // don't immediately add extra nudges

            Invoke( "EnableNormalQueueParsing", 3 ); // don't start any waiting instructions immediately again
        }

        public void FinishActiveInstruction()
        {
            if ( IsInSpecialMode() )
            {
                if ( currentSpecialInstruction != null )
                {
                    currentSpecialInstruction.FadeOut();
                }
            }
            else
            {
                if ( currentInstruction != null )
                {
                    currentInstruction.FadeOut();
                }
            }
        }

        public void ForceNextPlay(AudioInstructionType type)
        {
            if ( AudioInstructions.ContainsKey( type ) )
            {
                if ( instructionQueue.Count > 0 && instructionQueue[0] == currentInstruction )
                {
                    instructionQueue.Insert( 1, AudioInstructions[type] ); // could be an Add in the end, but hey
                }
                else if ( instructionQueue.Count > 0 )
                {
                    instructionQueue.Prepend( AudioInstructions[type] );
                }
                else
                {
                    instructionQueue.Add( AudioInstructions[type] ); // could be an Add in the end, but hey
                }
            }
        }

        public void ForceNextPlayNow(AudioInstructionType type)
        {
            if ( AudioInstructions.ContainsKey( type ) )
            {
                ForceNextPlay( type );
                if ( currentInstruction != null )
                {
                    currentInstruction.FadeOut();
                }
            }
        }

        public void ForcePlay(AudioInstructionType type, bool isSpecial = false)
        {
            if ( AudioInstructions.ContainsKey( type ) )
            {
                AbstractInstruction toCancel = null;

                if ( isSpecial )
                {
                    if ( currentSpecialInstruction != null )
                    {
                        toCancel = currentSpecialInstruction;
                    }

                    specialInstructionQueue = new List<AbstractInstruction>();
                    currentSpecialInstruction = AudioInstructions[type];
                    currentSpecialInstruction.Go();
                }
                else
                {
                    if ( currentInstruction != null )
                    {
                        toCancel = currentInstruction;
                    }

                    instructionQueue = new List<AbstractInstruction>();
                    currentInstruction = AudioInstructions[type];
                    currentInstruction.Go();
                }


                // to ignore its finish call when it might be immediate, we cancel the active instruction
                // after changing it.
                if ( toCancel != null )
                {
                    toCancel.FadeOut();
                }
            }
        }

        public AbstractInstruction GetActiveInstruction()
        {
            AbstractInstruction toType = null;
            if ( specialMode )
            {
                toType = currentSpecialInstruction;
            }
            else
            {
                toType = currentInstruction;
            }

            return toType;
        }

        public AudioInstructionType GetActiveInstructionType()
        {
            AbstractInstruction toType = null;
            if ( specialMode )
            {
                toType = currentSpecialInstruction;
            }
            else
            {
                toType = currentInstruction;
            }

            if ( toType != null )
            {
                return GetAudioInstructionType( toType );
            }

            return AudioInstructionType.None;
        }

        public AudioInstructionType GetAudioInstructionType(AbstractInstruction instruction)
        {
            foreach ( KeyValuePair<AudioInstructionType, AbstractInstruction> pair in AudioInstructions )
            {
                if ( pair.Value == instruction )
                {
                    return pair.Key;
                }
            }

            return AudioInstructionType.None;
        }

        public static InstructionManager GetInstance()
        {
            return instance;
        }

        public bool IsScheduled(AudioInstructionType type, bool isSpecial = false)
        {
            if ( AudioInstructions.ContainsKey( type ) )
            {
                if ( isSpecial )
                {
                    return specialInstructionQueue.Contains( AudioInstructions[type] );
                }
                else
                {
                    return instructionQueue.Contains( AudioInstructions[type] );
                }
            }

            return false;
        }

        public bool IsEmpty(bool special)
        {
            return special ? specialInstructionQueue.Count == 0 : instructionQueue.Count == 0;
        }

        public bool IsInSpecialMode()
        {
            return specialMode;
        }

        public void OnCheckForTimedScoreChange(float progress)
        {
            float secondsLeftInGame = Clock.GetInstance().GetCurrentTimeLeft();


            if ( !playedFinalInstruction && secondsLeftInGame < EndInstructionWithSecondsLeft )
            {
                playedFinalInstruction = true;
                ForcePlay( AudioInstructionType.Ending, specialMode ); // no matter the mode

                // plus exposure
                ExposureFader.SceneFadeInExposure();
            }
        }

        public void OnInstructionFinished(AbstractInstruction instruction)
        {
            Debug.Log( "[InstructionManager] Finished: " + instruction.gameObject.name );

            if ( specialMode )
            {
                if ( currentSpecialInstruction == instruction )
                {
                    Debug.Log( "[InstructionManager] Is special current: " + instruction.gameObject.name );
                    CheckForNudgeClearance( instruction );

                    if ( specialInstructionQueue.Contains( instruction ) )
                    {
                        specialInstructionQueue.Remove( instruction );
                    }

                    currentSpecialInstruction = null;
                }
            }
            else
            {
                if ( currentInstruction == instruction )
                {
                    Debug.Log( "[InstructionManager] Is current: " + instruction.gameObject.name );
                    CheckForNudgeClearance( instruction );

                    if ( instructionQueue.Contains( instruction ) )
                    {
                        instructionQueue.Remove( instruction );
                    }

                    currentInstruction = null;
                }
            }

            if ( enableSpecialModeAfterFinish )
            {
                enableSpecialModeAfterFinish = false;
                specialMode = true;
            }
        }

        public void Play(GameObject type)
        {
            Play( type, false );
        }

        public void Play(GameObject type, bool isSpecial = false)
        {
            AbstractInstruction instruction = type.GetComponent<AbstractInstruction>();
            if ( instruction != null )
            {
                if ( isSpecial )
                {
                    specialInstructionQueue.Add( instruction );
                }
                else
                {
                    instructionQueue.Add( instruction );
                }
            }
            else
            {
                Debug.LogError( "[InstructionManager Play] No Audio Instructions found on this object." );
            }
        }

        public void Play(AudioInstructionType type, bool isSpecial = false)
        {
            if ( AudioInstructions.ContainsKey( type ) )
            {
                if ( isSpecial )
                {
                    Debug.Log( "specialInstructionQueue " + specialInstructionQueue.Count );
                    Debug.Log( "can parse " + parseSpecialQueue );
                    specialInstructionQueue.Add( AudioInstructions[type] );
                }
                else
                {
                    Debug.Log( "instructionQueue " + instructionQueue.Count );
                    Debug.Log( "can parse " + parseNormalQueue );
                    instructionQueue.Add( AudioInstructions[type] );
                }
            }
            else
            {
                Debug.Log( "No instruction for type: " + type );
            }
        }

        public bool PlayIfSilent(AudioInstructionType type, bool isSpecial = false)
        {
            if ( AudioInstructions.ContainsKey( type ) )
            {
                if ( isSpecial )
                {
                    if ( specialInstructionQueue.Count == 0 )
                    {
                        specialInstructionQueue.Add( AudioInstructions[type] );
                        return true;
                    }
                }
                else
                {
                    if ( instructionQueue.Count == 0 )
                    {
                        instructionQueue.Add( AudioInstructions[type] );
                        return true;
                    }
                }
            }

            return false;
        }


        // ?? Do we need to change this to a pause function
        public void Silence()
        {
            AbstractInstruction toCancel = null;
            if ( currentInstruction != null )
            {
                toCancel = currentInstruction;
            }

            instructionQueue = new List<AbstractInstruction>();

            // to ignore its finish call when it might be immediate, we cancel the active instruction
            // after changing it.
            if ( toCancel != null )
            {
                toCancel.FadeOut();
            }
        }

        void Start()
        {
            AbstractInstruction.OnFinished += OnInstructionFinished;
            Clock.OnProgress += OnCheckForTimedScoreChange;
        }

        void Update()
        {
            if ( !playedFinalInstruction )
            {
                if ( specialMode )
                {
                    if ( parseSpecialQueue && specialInstructionQueue.Count > 0 && currentSpecialInstruction == null )
                    {
                        currentSpecialInstruction = specialInstructionQueue[0];
                        currentSpecialInstruction.Go();
                    }
                }
                else if ( parseNormalQueue && instructionQueue.Count > 0 && currentInstruction == null )
                {
                    currentInstruction = instructionQueue[0];
                    currentInstruction.Go();
                }
            }
        }
    }
}