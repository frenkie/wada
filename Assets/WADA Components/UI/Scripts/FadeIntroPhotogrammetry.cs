using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class FadeIntroPhotogrammetry : MonoBehaviour
    {
        public float Duration = 4;
        public bool CanGlow = false;

        List<Renderer> renderers = new();
        List<Material> mats = new();

        void Awake()
        {
            foreach ( Renderer renderer in GetComponentsInChildren<Renderer>() )
            {
                renderers.Add( renderer );
                Material mat = renderer.material;
                renderer.material = mat;
                mats.Add( mat );
            }

            OnFadeValue( 0 );

            foreach ( Renderer renderer in renderers )
            {
                renderer.enabled = false;
            }
        }

        void Start()
        {
            Invoke( "FadeIn", .25f );
            if ( CanGlow )
            {
                Invoke( "Glow", .25f );
            }
        }

        public void FadeIn()
        {
            foreach ( Renderer renderer in renderers )
            {
                renderer.enabled = true;
            }

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
            valueTo.Add( "oncomplete", "OnFadedOut" );

            iTween.ValueTo( gameObject, valueTo );
        }

        void Glow()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 0 );
            valueTo.Add( "to", 1 );
            valueTo.Add( "time", 1.1f );
            valueTo.Add( "easetype", iTween.EaseType.easeInOutCubic );
            valueTo.Add( "looptype", iTween.LoopType.pingPong );
            valueTo.Add( "onupdate", "OnGlowValue" );

            iTween.ValueTo( gameObject, valueTo );
        }

        void OnFadeValue(float value)
        {
            foreach ( Material mat in mats )
            {
                mat.SetFloat( "_Alpha", value );
            }
        }

        void OnFadedOut()
        {
            gameObject.SetActive( false );
        }

        void OnGlowValue(float value)
        {
            foreach ( Material mat in mats )
            {
                mat.SetFloat( "_Effect_Amount", value );
            }
        }
    }
}