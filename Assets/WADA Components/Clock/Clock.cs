using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Clock : MonoBehaviour
    {
        static Clock instance;
        public static event Action OnCountedDown = delegate { };
        public static event Action<float> OnProgress = delegate(float progress) { };

        public float TotalTimeInSeconds = 600f;
        public float MaxBonusTimeInSeconds = 600f;
        public float FinalTime = 60f;
        public bool UseBonusDecay;
        public float BonusDecayTimeThreshold = 60f;
        public float JumpToTime;

        const float BONUS_TIME_IN_SECONDS = 60f;
        const float BONUS_DECAY_IN_SECONDS = 60f;

        float decaySpeed = 1f;

        float bonusDecayTimer = 0f;

        float elapsedTime;
        int timeInSeconds = 0;

        float normalTime;
        float bonusTime;
        float bonusTimeAdded;
        float currentMaxBonusTime;
        float bonusMaxTime;

        bool allowBonusTime = true;
        bool countingDown;
        bool goingIntoFinalTime;

        void Awake()
        {
            if ( instance != null && instance != this )
            {
                Debug.LogError( "[Clock] Already initiated!" );
            }

            instance = this;
            ResetTimes();
        }

        public void AddBonusTime(float bonusTimeMultiplier = 1f)
        {
            if ( allowBonusTime )
            {
                float bonusTimeAfterAdd = Mathf.Min( bonusTimeAdded + BONUS_TIME_IN_SECONDS * bonusTimeMultiplier,
                    bonusMaxTime );
                if ( bonusTimeAfterAdd - bonusTimeAdded > 0 )
                {
                    bonusTime += bonusTimeAfterAdd - bonusTimeAdded;
                    bonusTimeAdded = bonusTimeAfterAdd;
                    currentMaxBonusTime = bonusTimeAdded;
                }
                else
                {
                    // MAX BONUS REACHED
                }
            }
        }

        public void Countdown()
        {
            ResetTimes();
            countingDown = true;
        }

        void DecayBonusTime()
        {
            bonusDecayTimer += Time.deltaTime;

            if ( bonusDecayTimer > BonusDecayTimeThreshold )
            {
                bonusMaxTime -= BONUS_DECAY_IN_SECONDS;
                bonusDecayTimer = 0;
            }
        }

        void DecayTime()
        {
            if ( bonusTime > 0f )
            {
                bonusTime = Mathf.Max( bonusTime - decaySpeed * Time.deltaTime, 0 );
            }
            else
            {
                normalTime = Mathf.Max( normalTime - decaySpeed * Time.deltaTime, 0 );
            }
        }

        public void DecreaseTime(float seconds)
        {
            float timeChange = decaySpeed * seconds;
            normalTime = Mathf.Max( normalTime - timeChange, 0 );
            elapsedTime += timeChange;
        }

        public void EndDemo()
        {
            bonusTime = 0;
            normalTime = 3;
        }

        public void EndMinuteDemo()
        {
            Debug.Log( "EndMinuteDemo" );
            bonusTime = 0;
            normalTime = FinalTime + 3;
        }

        public float GetCurrentTotalTime()
        {
            return normalTime + bonusTime;
        }

        public float GetCurrentElapsedTime()
        {
            return elapsedTime;
        }

        public float GetCurrentTotalMaxTime()
        {
            return TotalTimeInSeconds + currentMaxBonusTime;
        }

        public static Clock GetInstance()
        {
            return instance;
        }

        public float GetBonusProgress()
        {
            return currentMaxBonusTime > 0 ? bonusTime / currentMaxBonusTime : 0;
        }

        public float GetBonusTime()
        {
            return bonusTime;
        }

        public float GetBonusMaxTime()
        {
            return currentMaxBonusTime;
        }

        public float GetNormalTime()
        {
            return normalTime;
        }

        public float GetNormalProgress()
        {
            return normalTime / TotalTimeInSeconds;
        }

        /**
         * @returns percentage in range 0 - 1 and running up from
         */
        public float GetProgress()
        {
            return Math.Max( GetCurrentTotalTime() / GetCurrentTotalMaxTime(), 0 );
        }

        public float GetCurrentTimeLeft()
        {
            return GetCurrentTotalTime(); // 'cause we are counting down
        }

        public bool InFinalMoments()
        {
            return goingIntoFinalTime;
        }

        public void LowerDecaySpeed()
        {
            decaySpeed -= .1f;
        }

        public void RaiseDecaySpeed()
        {
            decaySpeed += .1f;
        }

        void ResetTimes()
        {
            timeInSeconds = 0;
            elapsedTime = 0;
            bonusDecayTimer = 0;
            bonusTime = 0;
            bonusTimeAdded = 0;
            currentMaxBonusTime = 0;
            bonusMaxTime = MaxBonusTimeInSeconds;
            normalTime = TotalTimeInSeconds;
        }

        public void Stop()
        {
            allowBonusTime = true;
            countingDown = false;
            goingIntoFinalTime = false;
        }

        void Update()
        {
            if ( countingDown )
            {
                elapsedTime += Time.deltaTime * decaySpeed;
                DecayTime();
                if ( UseBonusDecay )
                {
                    DecayBonusTime();
                }

                if ( !goingIntoFinalTime && GetCurrentTotalTime() <= FinalTime )
                {
                    goingIntoFinalTime = true;
                    allowBonusTime = false; // bonus doesn't count anymore, you're dead anyways

                    BackgroundMusicManager.GetInstance().PlayEndingMusic();
                }

                int totalTimeInSeconds = (int)Mathf.Floor( GetCurrentTotalTime() );
                if ( totalTimeInSeconds != timeInSeconds )
                {
                    timeInSeconds = totalTimeInSeconds;
                    // Only update every second?
                    OnProgress( GetProgress() );
                }

                if ( GetCurrentTotalTime() <= 0 )
                {
                    Debug.Log( "[Clock] Counted down!" );
                    countingDown = false;
                    OnCountedDown();
                    GameEngine.GetInstance().JumpToTime( JumpToTime );
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Clock) )]
    public class ClockEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Clock myTarget = (Clock)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Countdown" ) )
            {
                myTarget.Countdown();
            }

            if ( GUILayout.Button( "Decrease 10 seconds" ) )
            {
                myTarget.DecreaseTime( 10 );
            }

            if ( GUILayout.Button( "Decrease 30 seconds" ) )
            {
                myTarget.DecreaseTime( 30 );
            }

            if ( GUILayout.Button( "Add bonus time" ) )
            {
                myTarget.AddBonusTime();
            }

            if ( GUILayout.Button( "Lower Decay Speed" ) )
            {
                myTarget.LowerDecaySpeed();
            }

            if ( GUILayout.Button( "Raise Decay Speed" ) )
            {
                myTarget.RaiseDecaySpeed();
            }

            if ( GUILayout.Button( "End Demo" ) )
            {
                myTarget.EndDemo();
            }

            if ( GUILayout.Button( "End Demo Minute" ) )
            {
                myTarget.EndMinuteDemo();
            }
        }
    }
#endif
}