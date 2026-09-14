using System;
using System.Collections;
using UnityEngine;

namespace Wada
{
    public class FadeIntroSprite : MonoBehaviour
    {
        public float Duration = 4;

        SpriteRenderer renderer;

        void Awake()
        {
            renderer = GetComponent<SpriteRenderer>();
            OnFadeValue( 0 );
        }

        void Start()
        {
            Invoke( "FadeIn", .25f );
        }

        public void FadeIn()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 0 );
            valueTo.Add( "to", 1 );
            valueTo.Add( "time", .5f );
            valueTo.Add( "easetype", iTween.EaseType.easeOutCubic );
            valueTo.Add( "onupdate", "OnFadeValue" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void FadeOut()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 1 );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", Duration );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnFadeValue" );

            iTween.ValueTo( gameObject, valueTo );
        }

        void OnFadeValue(float value)
        {
            Color curr = renderer.color;
            curr.a = value;
            renderer.color = curr;
        }
    }
}