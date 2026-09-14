using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class BackgroundMusicManager : MonoBehaviour
    {
        static BackgroundMusicManager instance;

        public int StartScoreAfterSeconds = 420;
        public int StartCloserToEndWithSecondsLeft = 180;

        [SerializeField] public List<BackgroundMusic> Music = new();

        BackgroundMusicType activeSpecial = BackgroundMusicType.None;
        BackgroundMusicType activeScore = BackgroundMusicType.SurfaceStart;

        bool playingScore;

        int playedStartScore;
        bool playedAfterRevivalScore;
        bool playedMusicalScore;
        bool playedCloserToEndScore;
        bool ending;

        void Awake()
        {
            if ( instance != null && instance != this )
            {
                Debug.LogError( "[BackgroundMusicManager] Already initiated!" );
            }

            instance = this;
        }

        public void ActivateScore()
        {
            playingScore = true;
        }

        public void EnableAll()
        {
            foreach ( BackgroundMusic music in Music )
            {
                music.Enabled = true;
            }
        }

        public void FadeIn(string backgroundMusicType)
        {
            // try
            // {
            BackgroundMusicType type =
                (BackgroundMusicType)Enum.Parse( typeof(BackgroundMusicType), backgroundMusicType, true );
            FadeIn( type );
            // }
            // catch
            // {
            //     Debug.LogWarning( "[FadeIn] Unsupported backgroundmusic type supported " + backgroundMusicType );
            // }
        }

        public void FadeIn(BackgroundMusicType type)
        {
            StartCoroutine( FadeInMusic( type ) );
        }

        IEnumerator FadeInMusic(BackgroundMusicType type)
        {
            BackgroundMusic musicToFade = GetByType( type );
            if ( !ending && musicToFade != null && !musicToFade.IsPlaying() && !musicToFade.IsFadingIn() )
            {
                if ( activeSpecial != BackgroundMusicType.None )
                {
                    BackgroundMusic oldSpecial = GetByType( activeSpecial );
                    if ( oldSpecial.IsPlaying() && !oldSpecial.IsFadingOut() )
                    {
                        oldSpecial.FadeOut();
                        yield return new WaitForSeconds( oldSpecial.FadeTime );
                    }
                }

                if ( IsSpecial( type ) )
                {
                    activeSpecial = type;

                    StartCoroutine( FadeOutNonSpecialMusicEntirelyFast() ); //totally turn of the score areas
                    FadeOutWindEntirely(); // wind is only disabled during special sounds

                    if ( activeScore != BackgroundMusicType.None && playingScore )
                    {
                        BackgroundMusic oldScore = GetByType( activeScore );
                        if ( oldScore.IsPlaying() && !oldScore.IsFadingOut() )
                        {
                            oldScore.FadeOut( true );
                            // We want to transition really fast to special modes
                            yield return new WaitForSeconds( .1f );
                        }
                        else
                        {
                            yield return new WaitForSeconds( 1 );
                        }
                    }
                    else
                    {
                        yield return new WaitForSeconds( 1 );
                    }
                }
                else if ( IsScore( type ) )
                {
                    if ( activeScore != BackgroundMusicType.None && activeScore != type )
                    {
                        BackgroundMusic oldScore = GetByType( activeScore );
                        if ( oldScore.IsPlaying() && !oldScore.IsFadingOut() )
                        {
                            oldScore.FadeOut();
                            yield return new WaitForSeconds( oldScore.FadeTime );
                        }
                    }

                    activeSpecial = BackgroundMusicType.None;
                    activeScore = type;

                    //?? If there is a playing non special one, it should fade out
                    StartCoroutine( FadeOutNonSpecialMusicButLeaveTriggers() );
                }
                else
                {
                    activeSpecial = BackgroundMusicType.None;
                    foreach ( BackgroundMusic music in Music )
                    {
                        if ( !IsSpecial( music.Type ) && !IsScore( music.Type ) )
                        {
                            // Make sure the non special and wind ones can trigger again based on location
                            // if that wasn't reenabled elsewhere
                            music.UseTrigger = true;
                        }
                    }
                }

                musicToFade.FadeIn();
            }

            yield return null;
        }

        // Officially this already happens when something new is fading in, but okay
        IEnumerator FadeOutMusic(BackgroundMusicType type)
        {
            BackgroundMusic musicToFade = GetByType( type );
            if ( !ending && musicToFade != null && musicToFade.IsPlaying() && !musicToFade.IsFadingOut() )
            {
                musicToFade.FadeOut();
                yield return new WaitForSeconds( musicToFade.FadeTime );

                if ( IsSpecial( type ) && activeSpecial == type )
                {
                    activeSpecial = BackgroundMusicType.None;

                    foreach ( BackgroundMusic music in Music )
                    {
                        if ( !IsSpecial( music.Type ) && !IsScore( music.Type ) && !music.UseTrigger )
                        {
                            // Make sure the non special ones can trigger again based on location
                            music.UseTrigger = true;
                        }
                    }
                }
            }
        }

        public void FadeOutActiveMusic()
        {
            if ( !ending )
            {
                StartCoroutine( FadeOutAnyActiveMusic() );
            }
        }

        public void FadeOutActiveMusicButLeaveTriggers()
        {
            if ( !ending )
            {
                StartCoroutine( FadeOutAnyActiveMusic( true ) );
            }
        }

        public IEnumerator FadeOutAnyActiveMusic(bool leaveTriggersActive = false)
        {
            if ( activeSpecial != BackgroundMusicType.None )
            {
                BackgroundMusic musicToFade = GetByType( activeSpecial );
                if ( musicToFade != null && musicToFade.IsPlaying() && !musicToFade.IsFadingOut() )
                {
                    musicToFade.FadeOut();
                    yield return new WaitForSeconds( musicToFade.FadeTime );
                }
            }
            else
            {
                if ( activeScore != BackgroundMusicType.None && playingScore )
                {
                    BackgroundMusic oldScore = GetByType( activeScore );
                    if ( oldScore.IsPlaying() && !oldScore.IsFadingOut() )
                    {
                        Debug.Log( "Fading out active score " + oldScore );
                        oldScore.FadeOut();
                    }
                }

                if ( leaveTriggersActive )
                {
                    FadeOutWindEntirely( true );
                    yield return StartCoroutine( FadeOutNonSpecialMusicEntirely( true ) );
                }
                else
                {
                    FadeOutWindEntirely();
                    yield return StartCoroutine( FadeOutNonSpecialMusicEntirely() );
                }
            }
        }

        public void FadeOutWind(bool leaveTriggersActive = false)
        {
            foreach ( BackgroundMusic music in Music )
            {
                if ( IsWind( music.Type ) && music.UseTrigger )
                {
                    // Make sure the non special ones can't trigger based on location
                    if ( !leaveTriggersActive )
                    {
                        music.UseTrigger = false;
                    }

                    if ( music.gameObject.activeSelf && music.Enabled && (music.IsPlaying() || music.IsAtMaxVolume()) &&
                         !music.IsFadingOut() )
                    {
                        music.FadeOut();
                        Debug.Log( "Wind music playing, so we'll fade it out" );
                    }
                }
            }
        }

        public void FadeOutWindEntirely(bool leaveTriggersActive = false)
        {
            foreach ( BackgroundMusic music in Music )
            {
                if ( IsWind( music.Type ) && music.UseTrigger )
                {
                    // Make sure the non special ones can't trigger based on location
                    if ( !leaveTriggersActive )
                    {
                        music.UseTrigger = false;
                    }

                    if ( music.gameObject.activeSelf && music.Enabled && (music.IsPlaying() || music.IsAtMaxVolume()) &&
                         !music.IsFadingOut() )
                    {
                        music.FadeOutEntirely();
                        Debug.Log( "Wind music playing, so we'll fade it out entirely" );
                    }
                }
            }
        }

        public IEnumerator FadeOutNonSpecialMusic()
        {
            foreach ( BackgroundMusic music in Music )
            {
                if ( !IsSpecial( music.Type ) && !IsScore( music.Type ) && music.UseTrigger )
                {
                    // Make sure the non special ones can't trigger based on location
                    music.UseTrigger = false;
                    if ( music.gameObject.activeSelf && music.Enabled && (music.IsPlaying() || music.IsAtMaxVolume()) &&
                         !music.IsFadingOut() )
                    {
                        music.FadeOut();
                        Debug.Log( "Music playing, so we'll fade it out" );
                        // yield return
                        //     new WaitForSeconds( music
                        //         .FadeTime ); // it should only be one so this yield should be okay
                    }
                }
            }

            yield return null;
        }

        public IEnumerator FadeOutNonSpecialMusicEntirely(bool leaveTriggersActive = false)
        {
            foreach ( BackgroundMusic music in Music )
            {
                if ( !IsSpecial( music.Type ) && !IsScore( music.Type ) && !IsWind( music.Type ) && music.UseTrigger )
                {
                    // Make sure the non special ones can't trigger based on location
                    if ( !leaveTriggersActive )
                    {
                        music.UseTrigger = false;
                    }

                    if ( music.gameObject.activeSelf && music.Enabled && (music.IsPlaying() || music.IsAtMaxVolume()) &&
                         !music.IsFadingOut() )
                    {
                        music.FadeOutEntirely();
                        Debug.Log( "Music playing, so we'll fade it out entirely" );
                    }
                }
            }

            yield return null;
        }

        public IEnumerator FadeOutNonSpecialMusicEntirelyFast(bool leaveTriggersActive = false)
        {
            foreach ( BackgroundMusic music in Music )
            {
                if ( !IsSpecial( music.Type ) && !IsScore( music.Type ) && !IsWind( music.Type ) && music.UseTrigger )
                {
                    // Make sure the non special ones can't trigger based on location
                    if ( !leaveTriggersActive )
                    {
                        music.UseTrigger = false;
                    }

                    if ( music.gameObject.activeSelf && music.Enabled && (music.IsPlaying() || music.IsAtMaxVolume()) &&
                         !music.IsFadingOut() )
                    {
                        music.FadeOutEntirelyFast();
                        Debug.Log( "Music playing, so we'll fade it out entirely" );
                    }
                }
            }

            yield return null;
        }

        public IEnumerator FadeOutNonSpecialMusicButLeaveTriggers()
        {
            foreach ( BackgroundMusic music in Music )
            {
                if ( !IsSpecial( music.Type ) && !IsScore( music.Type ) && !IsWind( music.Type ) && music.UseTrigger )
                {
                    if ( music.gameObject.activeSelf && music.Enabled && (music.IsPlaying() || music.IsAtMaxVolume()) &&
                         !music.IsFadingOut() )
                    {
                        music.FadeOut();
                        Debug.Log( "Music playing, so we'll fade it out " + music.Type );
                    }
                }
            }

            yield return null;
        }


        public void FadeOut(BackgroundMusicType type)
        {
            StartCoroutine( FadeOutMusic( type ) );
        }

        public void FadeOut(string backgroundMusicType)
        {
            try
            {
                BackgroundMusicType type =
                    (BackgroundMusicType)Enum.Parse( typeof(BackgroundMusicType), backgroundMusicType, true );
                FadeOut( type );
            }
            catch
            {
                Debug.LogWarning( "[FadeOut] Unsupported backgroundmusic type supported" );
            }
        }

        // Doesnt stop anything, just plays this one and sets it as the active special one if it is special
        public void ForcePlay(BackgroundMusicType type)
        {
            if ( IsSpecial( type ) )
            {
                activeSpecial = type;
            }
            else if ( IsScore( type ) )
            {
                activeScore = type;
            }

            BackgroundMusic toPlay = GetByType( type );
            toPlay.ResetTime();
            toPlay.MaxOutVolume();
            toPlay.Play();
        }

        public BackgroundMusic GetByType(BackgroundMusicType type)
        {
            foreach ( BackgroundMusic music in Music )
            {
                if ( music.Type == type )
                {
                    return music;
                }
            }

            return null;
        }

        public static BackgroundMusicManager GetInstance()
        {
            return instance;
        }

        public bool IsInPlayingScoreTime()
        {
            return playingScore && !ending;
        }

        public bool IsPlayingEnding()
        {
            return ending;
        }

        public bool IsScore(BackgroundMusicType type)
        {
            bool isScore = false;

            switch ( type )
            {
                case BackgroundMusicType.MusicalScore:
                case BackgroundMusicType.RandomScore:
                case BackgroundMusicType.SurfaceStart:
                case BackgroundMusicType.CloserToEnd:
                case BackgroundMusicType.AfterFirstAnimal:
                case BackgroundMusicType.Ending:
                    isScore = true;
                    break;
            }

            return isScore;
        }

        public bool IsScoreArea(BackgroundMusicType type)
        {
            bool isScoreArea = true;

            switch ( type )
            {
                case BackgroundMusicType.BeginHigh:
                case BackgroundMusicType.EndHigh:
                case BackgroundMusicType.MiddleHigh:
                case BackgroundMusicType.WindMid:
                case BackgroundMusicType.WindLow:
                case BackgroundMusicType.MiddleLowAmbi:
                case BackgroundMusicType.OuterWind:
                case BackgroundMusicType.Cave:
                    isScoreArea = false;
                    break;
            }

            return isScoreArea;
        }

        public bool IsSpecial(BackgroundMusicType type)
        {
            bool special = false;

            switch ( type )
            {
                case BackgroundMusicType.Book:
                case BackgroundMusicType.IntroA:
                case BackgroundMusicType.IntroB:
                case BackgroundMusicType.Revival:
                case BackgroundMusicType.Workshop:
                    special = true;
                    break;
            }

            return special;
        }

        public bool IsWind(BackgroundMusicType type)
        {
            bool special = false;

            switch ( type )
            {
                case BackgroundMusicType.WindMid:
                case BackgroundMusicType.WindLow:
                case BackgroundMusicType.MiddleLowAmbi:
                case BackgroundMusicType.OuterWind:
                    special = true;
                    break;
            }

            return special;
        }

        public void OnCheckForTimedScoreChange(float progress)
        {
            float secondsInGame = Clock.GetInstance().GetCurrentElapsedTime();
            // float secondsLeftInGame = Clock.GetInstance().GetCurrentTimeLeft();

            // if ( !playedCloserToEndScore && secondsLeftInGame <= StartCloserToEndWithSecondsLeft )
            // {
            //     Debug.Log( "[OnCheckForScoreChange] playing close to end score; seconds left in game: " +
            //                secondsLeftInGame );
            //     PlayCloserToEndScore();
            // }

            if ( !playedMusicalScore && !playedCloserToEndScore && secondsInGame >= StartScoreAfterSeconds )
            {
                Debug.Log( "[OnCheckForScoreChange] playing musical score: seconds in game " + secondsInGame );
                PlayMusicalScore();
            }

            // The ending is triggered by the clock with PlayEndingMusic
        }

        public void OnMusicEnded(BackgroundMusicType type)
        {
            Debug.Log( "[OnMusicEnded] " + type );

            if ( type == BackgroundMusicType.SurfaceStart )
            {
                playedStartScore += 1;

                if ( playedStartScore == 1 )
                {
                    Debug.Log( "[OnMusicEnded] PlayAfterRevivalScore" );
                    PlayAfterRevivalScore();
                }
                else
                {
                    Debug.Log( "[OnMusicEnded] SurfaceStart Replay" );
                    BackgroundMusic music = GetByType( BackgroundMusicType.SurfaceStart );
                    music.Replay();
                }
            }
            else if ( type == BackgroundMusicType.MusicalScore )
            {
                // can only happen if the musical score is active (and ended) right?
                // then set activeScore to random and play it, no need for fading of the musical score
                activeScore = BackgroundMusicType.RandomScore;
                BackgroundMusic music = GetByType( activeScore );
                music.FadeIn();
            }
        }

        public void PlayActiveScore()
        {
            if ( activeScore != BackgroundMusicType.None )
            {
                BackgroundMusic music = GetByType( activeScore );
                if ( !music.IsPlaying() )
                {
                    Debug.Log( "Playing active score " + activeScore );
                    music.FadeIn();
                }
            }
        }

        public void PlayAfterRevivalScore()
        {
            if ( !playedAfterRevivalScore )
            {
                playedAfterRevivalScore = true;

                if ( activeSpecial == BackgroundMusicType.None )
                {
                    // fade out any active music
                    FadeOutActiveMusicButLeaveTriggers();
                }

                activeScore = BackgroundMusicType.AfterFirstAnimal;
                // it will start playing when the user leaves the special mode and/or is in the right area
            }
        }

        public void PlayCloserToEndScore()
        {
            if ( !playedCloserToEndScore )
            {
                playedCloserToEndScore = true;

                if ( activeSpecial == BackgroundMusicType.None )
                {
                    // fade out any active music
                    FadeOutActiveMusicButLeaveTriggers();
                }

                activeScore = BackgroundMusicType.CloserToEnd;
                // it will start playing when the user leaves the special mode and/or is in the right area
            }
        }

        public void PlayEndingMusic()
        {
            // Is this forced? Or do we let people have the workshop/book music etc?

            // if forced, then FadeIn

            // if not, then change the activeScore

            // TEMP FORCE
            FadeOutActiveMusic();
            ending = true;
            activeScore = BackgroundMusicType.Ending;

            BackgroundMusic music = GetByType( activeScore );
            music.UseTrigger = false;
            music.FadeIn();
        }

        public void PlayEndCredits()
        {
            activeScore = BackgroundMusicType.AfterFirstAnimal; // T2
            BackgroundMusic music = GetByType( activeScore );
            music.FadeIn();
        }

        public void StopEndCredits()
        {
            if ( activeScore == BackgroundMusicType.AfterFirstAnimal )
            {
                BackgroundMusic music = GetByType( activeScore );
                music.FadeTime = .5f;
                music.FadeOut();
            }
        }

        public void PlayMusicalScore()
        {
            if ( !playedMusicalScore )
            {
                playedMusicalScore = true;

                if ( activeSpecial == BackgroundMusicType.None )
                {
                    // fade out any active music
                    FadeOutActiveMusicButLeaveTriggers();
                }

                activeScore = BackgroundMusicType.MusicalScore;
                // it will start playing when the user leaves the special mode and/or is in the right area
            }
        }

        void Start()
        {
            Clock.OnProgress += OnCheckForTimedScoreChange;
            BackgroundMusic.OnEnded += OnMusicEnded;
        }

        public void StopActiveScore()
        {
            if ( activeScore != BackgroundMusicType.None && activeScore != BackgroundMusicType.Ending )
            {
                BackgroundMusic music = GetByType( activeScore );
                if ( music.IsPlaying() && !music.IsFadingOut() )
                {
                    music.FadeOut();
                }
            }
        }

        public void StopActiveScoreFast()
        {
            if ( activeScore != BackgroundMusicType.None )
            {
                BackgroundMusic music = GetByType( activeScore );
                if ( music.IsPlaying() && !music.IsFadingOut() )
                {
                    music.FadeOut( true );
                }
            }
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(BackgroundMusicManager) )]
    public class BackgroundMusicManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            BackgroundMusicManager myTarget = (BackgroundMusicManager)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Activate regular music" ) )
            {
                myTarget.FadeIn( BackgroundMusicType.BeginLow );
            }
        }
    }
#endif
}