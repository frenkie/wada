using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wada
{
    public class InfoPlaque : MonoBehaviour
    {
        public CanvasGroup CanvasProps;

        public Image ImageField;
        public SoundEffect Sound;


        InfoPlaqueData data;

        void Awake()
        {
            CanvasProps.alpha = 0;
        }

        public void Hide()
        {
            Vector3 endPos = transform.position + new Vector3( 0, .5f, 0 );

            Hashtable valueTo = new();

            float speed = 1;
            float time = Vector3.Distance( transform.position, endPos ) / speed;

            valueTo.Add( "position", endPos );
            valueTo.Add( "time", time );
            valueTo.Add( "easetype", iTween.EaseType.easeOutCubic );
            valueTo.Add( "oncomplete", "OnHide" );

            iTween.MoveTo( gameObject, valueTo );


            Hashtable alphaTo = new();

            alphaTo.Add( "from", 1 );
            alphaTo.Add( "to", 0 );
            alphaTo.Add( "time", time - .05f );
            alphaTo.Add( "easetype", iTween.EaseType.linear );
            alphaTo.Add( "onupdate", "OnAlphaValue" );

            iTween.ValueTo( gameObject, alphaTo );

            Sound.Play( 1 );
        }

        void OnHide()
        {
            Destroy( gameObject );
        }

        public void ParseData(InfoPlaqueData to)
        {
            data = to;
            switch ( GameEngine.GetInstance().ActiveLanguage )
            {
                case Languages.NL:
                    ImageField.sprite = data.Image_NL;                    
                    break;
                
                default:
                    ImageField.sprite = data.Image;
                    break;
            }
            
            ImageField.preserveAspect = true;
        }

        public void OnAlphaValue(float to)
        {
            CanvasProps.alpha = to;
        }

        public void OnHideEnd()
        {
            Destroy( gameObject );
        }

        public void Show(Vector3 lookAtTarget)
        {
            Vector3 endPos = transform.position;
            transform.position += new Vector3( 0, .5f, 0 );

            // For now use the camera, will use lookAtTarget later perhaps

            Vector3 playerLoc = PlayerController.GetInstance().GetLocation();
            Vector3 lookAtT = new Vector3( playerLoc.x, transform.position.y, playerLoc.z ) -
                              transform.position;

            transform.rotation = Quaternion.LookRotation( lookAtT );

            Hashtable valueTo = new();

            float speed = 1;
            float time = Vector3.Distance( transform.position, endPos ) / speed;

            valueTo.Add( "position", endPos );
            valueTo.Add( "time", time );
            valueTo.Add( "easetype", iTween.EaseType.easeOutCubic );

            iTween.MoveTo( gameObject, valueTo );


            Hashtable alphaTo = new();

            alphaTo.Add( "from", 0 );
            alphaTo.Add( "to", 1 );
            alphaTo.Add( "time", time );
            alphaTo.Add( "easetype", iTween.EaseType.linear );
            alphaTo.Add( "onupdate", "OnAlphaValue" );

            iTween.ValueTo( gameObject, alphaTo );

            Sound.Play( 0 );
        }
    }
}