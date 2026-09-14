using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wada;

namespace ImaginaryFriend
{
    public enum SkipTo
    {
        Paradise,
        LastMinute,
        End,
        FixStuckness
    }

    public class SceneButton : MonoBehaviour
    {
        public static event Action OnTriggered = delegate { };

        [HideInInspector] public bool _isSelected = false;
        bool _isTriggered = false;
        public SkipTo PartToActivate;

        [Header( "To Assign" )] public Image SelectionImage;
        BoxCollider _collider;

        void OnTriggerEnter(Collider other)
        {
            if ( other.gameObject.tag == "Hand" )
            {
                HasBeenTriggered();
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            _collider = GetComponent<BoxCollider>();
            Invoke( "SetSize", .1f );
        }

        void SetSize()
        {
            Vector3 size = _collider.size;
            RectTransform rectTransform = GetComponent<RectTransform>();

            size.x = rectTransform.rect.width;
            size.y = rectTransform.rect.height;

            _collider.size = size;
        }

        // Update is called once per frame
        void Update()
        {
            if ( _isSelected && !_isTriggered )
            {
                SelectionImage.color = new Color( 29f / 255f, 121f / 255f, 134f / 255f, 1 );
            }

            if ( _isTriggered )
            {
                SelectionImage.color = new Color( 240f / 255f, 183f / 255f, 66f / 255f, 1 );
            }

            if ( !_isSelected && !_isTriggered )
            {
                SelectionImage.color = new Color( 1, 1, 1, 1 );
            }
        }

        public void HasBeenTriggered()
        {
            _isTriggered = true;
            switch ( PartToActivate )
            {
                case SkipTo.Paradise:
                    GameEngine.GetInstance().DemoGotoParadise();
                    break;

                case SkipTo.LastMinute:
                    GameEngine.GetInstance().DemoGotoLastMinute();
                    break;

                case SkipTo.FixStuckness:
                    PlayerController.GetInstance()
                        .AlignToTarget( GameEngine.GetInstance().GuideFlockPoint.TransformPoint( 0, 0, .5f ) );
                    PlayerController.GetInstance().ExitFocusMode();
                    break;
            }

            OnTriggered.Invoke();
            Debug.Log( "Doing something from the debug menu" );
        }
    }
}