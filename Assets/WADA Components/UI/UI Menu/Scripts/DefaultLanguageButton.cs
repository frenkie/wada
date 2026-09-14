using UnityEngine;
using UnityEngine.UI;

namespace ImaginaryFriend
{
    public class DefaultLanguageButton : MonoBehaviour
    {
        [HideInInspector] public bool _isSelected = false;
        [HideInInspector] public bool _isTriggered = false;


        [Header( "To Assign" )] public Image SelectionImage;

        void Update()
        {
            if ( _isSelected )
            {
                SelectionImage.color = new Color( 29f / 255f, 121f / 255f, 134f / 255f, 1 );
            }
            else if ( _isTriggered )
            {
                SelectionImage.color = new Color( 240f / 255f, 183f / 255f, 66f / 255f, 1 );
            }
            else
            {
                SelectionImage.color = new Color( 1, 1, 1, 1 );
            }
        }

        public void HasBeenTriggered()
        {
            _isTriggered = true;
            Debug.Log( "Has Been Triggered" );
        }
    }
}