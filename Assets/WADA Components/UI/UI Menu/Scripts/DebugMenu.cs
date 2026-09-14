using UnityEngine;
using UnityEngine.InputSystem;

namespace ImaginaryFriend
{
    public class DebugMenu : MonoBehaviour
    {
        public InputActionReference Menu;

        public InputAction OpenAndClose;
        public InputAction Up;
        public InputAction Down;
        public InputAction Left;
        public InputAction Right;
        public InputAction Enter;

        public InputActionReference Thumbstick;
        public InputActionReference Trigger;

        public MenuNavigator Navigator;

        bool menuPrev;

        public bool UseUpDown = false;
        bool thumbNeedsCoolDown;

        void Awake()
        {
            GetComponentInChildren<Canvas>().worldCamera = Camera.main;
            Navigator.gameObject.SetActive( false );
        }

        // Start is called before the first frame update
        void Start()
        {
            if ( Debug.isDebugBuild || Application.isEditor )
            {
                Debug.Log( "Allowing keys for the DebugMenu" );
                OpenAndClose.Enable();
                if ( UseUpDown )
                {
                    Up.Enable();
                    Down.Enable();
                }

                Left.Enable();
                Right.Enable();
                Enter.Enable();
                Menu.action.Enable();
                Thumbstick.action.Enable();
                Trigger.action.Enable();
            }
            else
            {
                // Destroy?
            }
        }

        void RotateToPlayer()
        {
            transform.position = new Vector3(
                Camera.main.transform.position.x,
                Camera.main.transform.position.y - .1f,
                Camera.main.transform.position.z
            );
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                Camera.main.transform.eulerAngles.y,
                transform.eulerAngles.z
            );
        }

        // Update is called once per frame
        void Update()
        {
            OVRPlugin.ControllerState4 state = OVRPlugin.GetControllerState4( (uint)OVRInput.Controller.Hands );
            bool menuGesture = (state.Buttons & (uint)OVRInput.RawButton.Start) > 0;

            if ( OpenAndClose.triggered || Menu.action.triggered || (menuGesture && !menuPrev) )
            {
                Navigator.gameObject.SetActive( !Navigator.gameObject.activeSelf );
                if ( Navigator.gameObject.activeSelf )
                {
                    RotateToPlayer();
                }
            }

            menuPrev = menuGesture;

            if ( Navigator.gameObject.activeSelf )
            {
                Vector2 thumb = Vector2.zero;
                //if ( Thumbstick.action.triggered )
                //{
                //    Debug.Log( "Thumbstick!" );
                thumb = Thumbstick.action.ReadValue<Vector2>();
                //}

                if ( thumbNeedsCoolDown )
                {
                    if ( thumb.magnitude < 0.02f )
                    {
                        thumbNeedsCoolDown = false;
                    }
                    else
                    {
                        thumb = Vector2.zero;
                    }
                }

                if ( UseUpDown && (Up.triggered || (thumb.y > 0.3f && Mathf.Abs( thumb.x ) < Mathf.Abs( thumb.y ))) )
                {
                    Navigator.UpPressed();
                    thumbNeedsCoolDown = true;
                }

                if ( UseUpDown && (Down.triggered || (thumb.y < 0.3f && Mathf.Abs( thumb.x ) < Mathf.Abs( thumb.y ))) )
                {
                    Navigator.DownPressed();
                    thumbNeedsCoolDown = true;
                }

                if ( Left.triggered || (thumb.x < 0.3f && Mathf.Abs( thumb.y ) < Mathf.Abs( thumb.x )) )
                {
                    Navigator.LeftPressed();
                    thumbNeedsCoolDown = true;
                }

                if ( Right.triggered || (thumb.x > 0.3f && Mathf.Abs( thumb.y ) < Mathf.Abs( thumb.x )) )
                {
                    Navigator.RightPressed();
                    thumbNeedsCoolDown = true;
                }

                if ( Enter.WasReleasedThisFrame() || Trigger.action.WasReleasedThisFrame() )
                {
                    Navigator.EnterPressed();
                }
            }
        }
    }
}