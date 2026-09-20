using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;


#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    [Serializable]
    public class WadaAnimalBookPage : SerializableDictionary<Ressurectable, Vector2>
    {
    }

    [RequireComponent( typeof(SoundEffect) )]
    public class WadaBook : MonoBehaviour
    {
        static WadaBook instance;

        public List<Texture2D> Textures_NL;

        public float MovementStrength = .02f;
        public float MovementTime = 6;
        public GameObject Container;
        public MegaBookBuilder Book;
        public WadaAnimalBookPage PagePerAnimal;
        public GameObject[] GestureDetectors;
        public SoundEffect OpenCloseSoundEffect;

        SoundEffect soundEffect;

        public GestureRecognition Gesture;

        Vector3 start;
        bool stopped;
        bool shown;

        bool firstShow = true;

        void Awake()
        {
            if ( instance != null && instance != this )
            {
                Debug.LogError( "[Book] Already initiated!" );
            }

            instance = this;

            start = transform.position;
            soundEffect = GetComponent<SoundEffect>();
        }

        public void FoundAnimal(Ressurectable animal)
        {
            if ( PagePerAnimal.ContainsKey( animal ) )
            {
                Vector2 pageData = PagePerAnimal[animal];
                int page = (int)Math.Ceiling( pageData.x / 2 ) - 1;
                Renderer renderer = Book.pages[page].obj.GetComponent<Renderer>();
                Material[] materials = renderer.materials;

                materials[pageData.x % 2 == 0 ? 1 : 0].SetInt( "_Found" + (int)pageData.y, 1 );

                renderer.materials = materials;
            }
        }

        public static WadaBook GetInstance()
        {
            return instance;
        }

        void Hover()
        {
            stopped = false;

            Hashtable valueTo = new();

            valueTo.Add( "position", start + UnityEngine.Random.insideUnitSphere * MovementStrength );
            valueTo.Add( "time", MovementTime );
            valueTo.Add( "easetype", iTween.EaseType.easeInOutCubic );
            valueTo.Add( "oncomplete", "OnHoverEnd" );

            iTween.MoveTo( gameObject, valueTo );
        }

        public bool IsShown()
        {
            return shown;
        }

        void MoveBack()
        {
            Hashtable valueTo = new();

            valueTo.Add( "position", start );
            valueTo.Add( "local", true );
            valueTo.Add( "time", 1 );
            valueTo.Add( "easetype", iTween.EaseType.easeInOutCubic );

            iTween.MoveTo( gameObject, valueTo );
        }

        public void Hide(bool fadeOutMusic = true)
        {
            shown = false;
            Container.SetActive( false );
            if ( fadeOutMusic )
            {
                BackgroundMusicManager.GetInstance().FadeOut( BackgroundMusicType.Book );
            }

            Stop();

            InstructionManager.GetInstance().ExitSpecialMode();
            Ressurectable.DisableAllRevivals = false;
            GameEngine.GetInstance().ToggleWorkshopGestureDetection( true );
            GameEngine.GetInstance().ToggleBookGestureDetection( true );
        }

        [Button]
        public void NextPage()
        {
            if ( shown )
            {
                if ( Book.GetCurrentPage() == Book.GetPageCount() )
                {
                    Invoke( "SimulateGesture", 2f );
                    // close the hard cover
                    OpenCloseSoundEffect.Play( 1 );
                }
                else if ( Book.GetCurrentPage() == -1 )
                {
                    // open the hard cover
                    OpenCloseSoundEffect.Play( 0 );
                }
                else
                {
                    soundEffect.Play();
                }

                Book.NextPage();
            }
        }

        public bool NotShownYet()
        {
            return firstShow;
        }

        void OnHoverEnd()
        {
            if ( !stopped )
            {
                Hover();
            }
        }

        void PlayBookInstructions()
        {
            NudgeManager.GetInstance().ClearInteractiveNudge( NudgeType.Book );

            InstructionManager.GetInstance().Play( AudioInstructionType.InfoOnAnimals, true );
        }

        [Button]
        public void PreviousPage()
        {
            if ( shown )
            {
                if ( Book.GetCurrentPage() == 0 )
                {
                    Invoke( "SimulateGesture", 2f );
                    OpenCloseSoundEffect.Play( 1 );
                }
                else if ( Book.GetCurrentPage() == Book.GetPageCount() + 1 )
                {
                    // open it back up
                    OpenCloseSoundEffect.Play( 0 );
                }
                else
                {
                    soundEffect.Play();
                }

                Book.PrevPage();
            }
        }

        void PrepareLanguage()
        {
            /*
             * Replace textures with the ones for the active language if anything else but EN
             */
            Languages lang = GameEngine.GetInstance().ActiveLanguage;
            if ( lang != Languages.EN )
            {
                if ( lang != Languages.NL )
                {
                    return;
                }
                List<Texture2D> textures = Textures_NL;

                for (int i=0; i<6; i++)
                {
                    Renderer renderer = Book.pages[i].obj.GetComponent<Renderer>();
                    Material[] materials = renderer.materials;

                    int textureIdx1 = -1;
                    int textureIdx2 = -1;
                    switch (i)
                    {
                        case 0:
                        case 1:    
                        case 2:    
                        case 3:    
                        case 4:    
                            textureIdx1 = i * 2;
                            textureIdx2 = textureIdx1 + 1;
                            break;
                        
                        case 5:
                            textureIdx1 = i * 2;
                            break;
                    }

                    if (textureIdx1 != -1)
                    {
                        materials[0].SetTexture("_Page", textures[textureIdx1]);
                    }                    
                    if (textureIdx2 != -1)
                    {
                        materials[1].SetTexture("_Page", textures[textureIdx2]);
                    }
                    
                }
            }
        }

        [Button]
        public void Show()
        {
            shown = true;
            if (firstShow)
            {
                PrepareLanguage();
            }


            Gesture.gameObject.SetActive( false );
            GameEngine.GetInstance().ToggleWorkshopGestureDetection( false );
            Ressurectable.DisableAllRevivals = true;

            Vector3 bookPos = Gesture.Canvas.transform.position;
            bookPos.y -= .2f;
            transform.position = bookPos;

            Vector3 target = PlayerController.GetInstance().GetLocation(); // is camera
            target.y = transform.position.y;
            Vector3 lookAtT = target - transform.position;

            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                Quaternion.LookRotation( lookAtT ).eulerAngles.y,
                transform.eulerAngles.z
            );

            Container.SetActive( true );

            start = transform.position;

            GameEngine.GetInstance().HideHandInstructions();
            BackgroundMusicManager.GetInstance().FadeIn( BackgroundMusicType.Book );

            if ( Book.GetCurrentPage() == -1 )
            {
                Book.SetPage( 0, false );
                OpenCloseSoundEffect.Play( 0 );
            }
            else if ( Book.GetCurrentPage() == Book.GetPageCount() + 1 )
            {
                Book.SetPage( 0, true );
            }

            if ( firstShow )
            {
                firstShow = false;

                bool isBookTutorialPlaying = InstructionManager.GetInstance().GetActiveInstructionType() ==
                                             AudioInstructionType.BookOpenA;
                Debug.Log( "Is book tutorial playing? " + isBookTutorialPlaying );

                InstructionManager.GetInstance().EnterSpecialMode( isBookTutorialPlaying );

                Invoke( "PlayBookInstructions", 3 );
            }
            else
            {
                InstructionManager.GetInstance().EnterSpecialMode();
            }

            Invoke( "ReActivateGesture", 4 );
        }

        public void ReActivateGesture()
        {
            Gesture.gameObject.SetActive( true );
        }

        public void SimulateGesture()
        {
            Gesture.whenMatched.Invoke();
        }

        void Start()
        {
            shown = false;
 
            Container.SetActive( false );

            foreach ( GameObject detector in GestureDetectors )
            {
                detector.SetActive( true );
            }
        }

        public void Stop()
        {
            stopped = true;
            iTween.Stop( gameObject );
        }

        public void Toggle()
        {
            if ( shown )
            {
                Hide();
                PlayerController.GetInstance().ExitFocusMode();
            }
            else
            {
                PlayerController.GetInstance().EnterFocusMode();
                Show();
            }
        }
    }
}