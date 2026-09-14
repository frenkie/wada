using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class WorkshopSkybox : MonoBehaviour
    {
        public Material SkyboxMat;
        public Renderer Skybox;

        Color defaultSkyboxColor;

        Material alteredSkybox;

        bool fadedIn = false;

        void Awake()
        {
            alteredSkybox = new Material( SkyboxMat.shader );
            alteredSkybox.CopyPropertiesFromMaterial( SkyboxMat );

            Skybox.material = alteredSkybox;

            defaultSkyboxColor = alteredSkybox.GetColor( "_BaseColor" );
            defaultSkyboxColor.a = 1;
        }

        public void FadeInWorkshop()
        {
            // means fading out skybox and fading in passthrough
            PlayerController.GetInstance().ShowPassthrough();

            Hashtable valueTo = new();

            valueTo.Add( "from", 1 );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", 3 );
            valueTo.Add( "easetype", iTween.EaseType.easeOutCubic );
            valueTo.Add( "onupdate", "FadeValue" );
            valueTo.Add( "oncomplete", "OnWorkshopFadedIn" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void FadeOutWorkshop()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 0 );
            valueTo.Add( "to", 1 );
            valueTo.Add( "time", 3 );
            valueTo.Add( "easetype", iTween.EaseType.easeOutCubic );
            valueTo.Add( "onupdate", "FadeValue" );
            valueTo.Add( "oncomplete", "OnWorkshopFadedOut" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void OnWorkshopFadedIn()
        {
            PlayerController.GetInstance().AdjustPassthroughOpacity( 1 );
        }

        public void OnWorkshopFadedOut()
        {
            PlayerController.GetInstance().AdjustPassthroughOpacity( 0 );
            PlayerController.GetInstance().HidePasshtrough();
        }

        void FadeValue(float value)
        {
            Color adjusted = alteredSkybox.GetColor( "_BaseColor" );
            adjusted.a = value;

            alteredSkybox.SetColor( "_BaseColor", adjusted );
            PlayerController.GetInstance().AdjustPassthroughOpacity( 1 - value );
        }

        void OnDestroy()
        {
            alteredSkybox.SetColor( "_BaseColor", defaultSkyboxColor );
        }
    }
}