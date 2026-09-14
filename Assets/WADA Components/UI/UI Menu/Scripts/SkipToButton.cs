using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ImaginaryFriend
{
    public class SkipToButton : MonoBehaviour
    {
        [HideInInspector] public bool _isSelected = false;
        bool _isTriggered = false;
        public int SceneToActivate;

        [Header( "To Assign" )] public Image SelectionImage;


        // Start is called before the first frame update
        void Start()
        {
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
            Debug.Log( "Doing something from the debug menu" );
        }
    }
}