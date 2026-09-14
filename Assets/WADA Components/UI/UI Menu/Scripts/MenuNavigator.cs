using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ImaginaryFriend
{
    public enum MenuPart
    {
        SCENE,
        DEFAULT_LANGUAGE,
        TOGGLES
    }

    public class MenuNavigator : MonoBehaviour
    {
        public int MenuIndex = 0;

        public SceneButton[] SceneButtons;
        public DefaultLanguageButton[] LanguageButtons;
        public CustomToggle[] LeftToggles;
        public CustomToggle[] RightToggles;

        MenuPart menuPart = MenuPart.SCENE;

        Dictionary<string, int> menuIndices;

        bool CanProgresScene = true;

        void Awake()
        {
            menuIndices = new Dictionary<string, int>();
            menuIndices.Add( MenuPart.SCENE.ToString(), -1 );
            menuIndices.Add( MenuPart.DEFAULT_LANGUAGE.ToString(), -1 );
        }

        void Start()
        {
            SceneButton.OnTriggered += OnButtonTriggered;
            CanProgresScene = true;
            ActivateSelection( MenuIndex );
        }

        public void LeftPressed()
        {
            switch ( menuPart )
            {
                case MenuPart.SCENE:
                case MenuPart.DEFAULT_LANGUAGE:

                    if ( menuIndices[menuPart.ToString()] <= 0 )
                    {
                        ActivateSelection( (
                            menuPart == MenuPart.SCENE ? SceneButtons.Length : LanguageButtons.Length
                        ) - 1 );
                    }
                    else
                    {
                        ActivateSelection( menuIndices[menuPart.ToString()] - 1 );
                    }

                    break;
            }
        }

        public void RightPressed()
        {
            switch ( menuPart )
            {
                case MenuPart.SCENE:
                case MenuPart.DEFAULT_LANGUAGE:

                    if ( menuIndices[menuPart.ToString()] == (
                            menuPart == MenuPart.SCENE ? SceneButtons.Length : LanguageButtons.Length
                        ) - 1 )
                    {
                        ActivateSelection( 0 );
                    }
                    else
                    {
                        ActivateSelection( menuIndices[menuPart.ToString()] + 1 );
                    }

                    break;
            }
        }

        public void DownPressed()
        {
            switch ( menuPart )
            {
                case MenuPart.SCENE:
                    DeActivateSelection( menuIndices[MenuPart.SCENE.ToString()] );

                    menuPart = MenuPart.DEFAULT_LANGUAGE;
                    int activeDefault = menuIndices[MenuPart.DEFAULT_LANGUAGE.ToString()];
                    ActivateSelection( activeDefault > -1 ? activeDefault : 0 );

                    break;
            }
        }

        public void UpPressed()
        {
            switch ( menuPart )
            {
                case MenuPart.DEFAULT_LANGUAGE:
                    DeActivateSelection( menuIndices[MenuPart.DEFAULT_LANGUAGE.ToString()] );

                    menuPart = MenuPart.SCENE;
                    ActivateSelection( menuIndices[MenuPart.SCENE.ToString()] );

                    break;
            }
        }

        public void EnterPressed()
        {
            switch ( menuPart )
            {
                case MenuPart.SCENE:
                    CanProgresScene = false;
                    SceneButtons[menuIndices[menuPart.ToString()]].HasBeenTriggered();
                    break;

                case MenuPart.DEFAULT_LANGUAGE:
                    foreach ( DefaultLanguageButton button in LanguageButtons )
                    {
                        if ( button._isTriggered )
                        {
                            button._isTriggered = false;
                        }
                    }

                    LanguageButtons[menuIndices[menuPart.ToString()]].HasBeenTriggered();
                    break;
            }
        }

        public void ActivateSelection(int ToSelect)
        {
            if ( ToSelect != menuIndices[menuPart.ToString()] )
            {
                DeActivateSelection( menuIndices[menuPart.ToString()] );

                menuIndices[menuPart.ToString()] = ToSelect;
            }

            // always color
            switch ( menuPart )
            {
                case MenuPart.SCENE:
                    SceneButtons[ToSelect]._isSelected = true;
                    break;

                case MenuPart.DEFAULT_LANGUAGE:
                    LanguageButtons[ToSelect]._isSelected = true;
                    break;
            }
        }

        public void DeActivateSelection(int ToDeSelect)
        {
            if ( ToDeSelect > -1 )
            {
                switch ( menuPart )
                {
                    case MenuPart.SCENE:
                        SceneButtons[ToDeSelect]._isSelected = false;
                        break;

                    case MenuPart.DEFAULT_LANGUAGE:
                        LanguageButtons[ToDeSelect]._isSelected = false;
                        break;
                }
            }
        }

        public void OnButtonTriggered()
        {
            gameObject.SetActive( false );
        }
    }
}