using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class HeartCounter : MonoBehaviour
    {
        public List<Heart> Hearts;

        List<Vector2> heartProgressRange = new();

        float heartProgress = 1; // progress goes down from full to empty
        bool finale = false;
        float blinkTimeRange = 10; // seconds

        int shownCount = 0;
        bool firstEatInstruction = true;

        /**
         *
         * Shows or hides hearts
         * Updates active hearts
         *
         */
        void Awake()
        {
            heartProgressRange.Add( new Vector2( 0.0001f, .2f ) );
            heartProgressRange.Add( new Vector2( .2f, .4f ) );
            heartProgressRange.Add( new Vector2( .4f, .6f ) );
            heartProgressRange.Add( new Vector2( .6f, .8f ) );
            heartProgressRange.Add( new Vector2( .8f, 1f ) );
        }

        public void AddHeart()
        {
            Debug.Log( "[HeartCounter] Adding heart" );
            RenderHearts();
        }

        public void CheckForFoodEaten()
        {
            if ( !GameEngine.GetInstance().HasEatenFood() )
            {
                NudgeManager.GetInstance().AddNudgeToInteractionList( NudgeType.EatAnimal );
            }
        }

        public void Hide()
        {
            for ( int i = 0; i < Hearts.Count; i++ )
            {
                Hearts[i].Hide();
            }
        }

        public void RemoveHeart()
        {
            RenderHearts();
        }

        public void RenderHearts()
        {
            for ( int i = 0; i < Hearts.Count; i++ )
            {
                if ( heartProgressRange.Count > i )
                {
                    if ( heartProgress >= heartProgressRange[i].x )
                    {
                        float partialProgress = (heartProgress - heartProgressRange[i].x) /
                                                (heartProgressRange[i].y - heartProgressRange[i].x);

                        Hearts[i].UpdateProgress( Math.Min( partialProgress, 1 ) ); // 1 is maxed for the lower hearts

                        // Add blinking
                        if ( i == 0 )
                        {
                            if ( Clock.GetInstance().InFinalMoments() && !Hearts[i].IsBlinking() )
                            {
                                Show();
                                Hearts[i].StartBlinking( 0 );
                            }
                        }
                        else
                        {
                            if ( partialProgress > 0 && partialProgress <= 1 &&
                                 (heartProgress - heartProgressRange[i].x) *
                                 Clock.GetInstance().GetCurrentTotalTime() <= blinkTimeRange )
                            {
                                if ( !Hearts[i].IsBlinking() )
                                {
                                    CancelInvoke( "Hide" );
                                    Show();
                                    Invoke( "Hide", blinkTimeRange + 5 );
                                    Hearts[i].StartBlinking( 0 );
                                }
                            }
                            else if ( Hearts[i].IsBlinking() )
                            {
                                Hearts[i].StopBlinking();
                            }
                        }
                    }
                    else
                    {
                        if ( Hearts[i].IsBlinking() )
                        {
                            Hearts[i].StopBlinking();
                        }

                        Hearts[i].Hide();
                    }
                }
            }
        }

        public void Show()
        {
            shownCount++;

            for ( int i = 0; i < Hearts.Count; i++ )
            {
                if ( heartProgress >= heartProgressRange[i].x )
                {
                    Hearts[i].Show();
                }
            }

            if ( firstEatInstruction /* && shownCount > 1 ?? */ )
            {
                firstEatInstruction = false;

                // Will be played when there is time
                InstructionManager.GetInstance().Play( AudioInstructionType.EatToStayAlive );
                InstructionManager.GetInstance().Play( AudioInstructionType.HowToEat );

                Invoke( "CheckForFoodEaten", 90 );
            }
        }

        void Start()
        {
            Clock.OnProgress += UpdateProgress;

            Invoke( "Hide", 5 );
        }

        public void UpdateProgress(float progress)
        {
            heartProgress = progress;
        }

        void Update()
        {
            RenderHearts();
        }
    }
}