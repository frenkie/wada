using System.Collections;
using UnityEngine;

namespace Wada
{
    public class FocusCone : MonoBehaviour
    {
        public Vector3 Target;

        bool active = false;
        float rotationSpeed = 60f;

        Renderer renderer;

        void OnEnable()
        {
            if ( renderer == null )
            {
                renderer = GetComponent<Renderer>();
            }

            renderer.material.SetFloat( "_AlphaStrength", 0 );
            active = true;
            FadeIn();
        }

        public void FadeIn()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 0 );
            valueTo.Add( "to", 1 );
            valueTo.Add( "time", .5f );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnFadeValue" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void OnFadeValue(float strength)
        {
            renderer.material.SetFloat( "_AlphaStrength", strength );
        }

        public void FadeOut()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 1 );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", .5f );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnFadeValue" );
            valueTo.Add( "oncomplete", "OnFadedOut" );

            iTween.ValueTo( gameObject, valueTo );
        }

        void OnFadedOut()
        {
            active = false;
            gameObject.SetActive( false );
        }

        void LateUpdate()
        {
            if ( active && Target != Vector3.zero )
            {
                // rotate towards target
                Vector3 lookAtT = Target -
                                  transform.position;

                Quaternion rotateTo = Quaternion.LookRotation( lookAtT );

                if ( transform.rotation != rotateTo )
                {
                    transform.rotation = Quaternion.Slerp( transform.rotation, rotateTo,
                        Time.deltaTime * rotationSpeed );
                }
            }
        }
    }
}