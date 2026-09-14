using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class SkyboxAdjuster : MonoBehaviour
    {
        public GameObject ProximityTarget;
        public GameObject Traveller;
        public Material SkyboxMat;
        public Material SkyboxIntroMat;
        public Renderer Skybox;

        public Color FocussedSkyboxColor;
        Color defaultSkyboxColor;
        Color defaultIntroSkyboxColor;

        public GameObject[] FadeObjects;

        Dictionary<Material, Material> fadeMaterials = new();

        Material alteredSkybox;
        Material alteredSkyboxIntro;

        bool fadedIn = false;
        bool started = false;

        void Awake()
        {
            alteredSkybox = new Material( SkyboxMat.shader );
            alteredSkybox.CopyPropertiesFromMaterial( SkyboxMat );

            defaultSkyboxColor = alteredSkybox.GetColor( "_BaseColor" );
            defaultSkyboxColor.a = 1;


            alteredSkyboxIntro = new Material( SkyboxIntroMat.shader );
            alteredSkyboxIntro.CopyPropertiesFromMaterial( SkyboxIntroMat );

            defaultIntroSkyboxColor = alteredSkyboxIntro.GetColor( "_BaseColor" );
            defaultIntroSkyboxColor.a = 1;

            Skybox.material = alteredSkybox;

            foreach ( GameObject fader in FadeObjects )
            {
                foreach ( Renderer renderer in fader.GetComponentsInChildren<Renderer>() )
                {
                    if ( !fadeMaterials.ContainsKey( renderer.material ) )
                    {
                        Material altered = new( renderer.material.shader );
                        altered.CopyPropertiesFromMaterial( renderer.material );

                        fadeMaterials.Add( renderer.material, altered );
                        renderer.material = altered;
                    }
                    else
                    {
                        renderer.material = fadeMaterials[renderer.material];
                    }
                }
            }
        }

        public void EnterFocusMode()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 0 );
            valueTo.Add( "to", 1 );
            valueTo.Add( "time", 3 );
            valueTo.Add( "easetype", iTween.EaseType.easeOutCubic );
            valueTo.Add( "onupdate", "EnterFocusValue" );

            iTween.ValueTo( gameObject, valueTo );
        }

        void EnterFocusValue(float value)
        {
            alteredSkybox.SetColor( "_BaseColor", Color.Lerp( defaultSkyboxColor, FocussedSkyboxColor, value ) );
        }

        public void ExitFocusMode()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 1 );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", 3 );
            valueTo.Add( "easetype", iTween.EaseType.easeOutCubic );
            valueTo.Add( "onupdate", "EnterFocusValue" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void EnterWorkshopMode()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 1 );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", 3 );
            valueTo.Add( "easetype", iTween.EaseType.easeOutCubic );
            valueTo.Add( "onupdate", "WorkshopValue" );

            iTween.ValueTo( gameObject, valueTo );
        }

        void WorkshopValue(float value)
        {
            Color skyColor = alteredSkybox.GetColor( "_BaseColor" );
            skyColor.a = value;
            alteredSkybox.SetColor( "_BaseColor", skyColor );
        }

        public void ExitWorkshopMode()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 0 );
            valueTo.Add( "to", 1 );
            valueTo.Add( "time", 3 );
            valueTo.Add( "easetype", iTween.EaseType.easeOutCubic );
            valueTo.Add( "onupdate", "WorkshopValue" );

            iTween.ValueTo( gameObject, valueTo );
        }


        public void FadeInWorkshop()
        {
            EnterWorkshopMode();
        }

        public void FadeOutWorkshop()
        {
            ExitWorkshopMode();
        }

        public void FadeOut()
        {
            GameEngine.GetInstance().Invoke( "ResumeTimeline", 2 );
        }

        public void FadeOutIntro()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 1 );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", 2.2f );
            valueTo.Add( "easetype", iTween.EaseType.easeOutCubic );
            valueTo.Add( "onupdate", "FadeOutIntroValue" );

            iTween.ValueTo( gameObject, valueTo );

            Invoke( "OnFadeOutIntro", 2.2f );
        }

        public void FadeOutIntroValue(float value)
        {
            Color skyColor = alteredSkyboxIntro.GetColor( "_BaseColor" );
            skyColor.a = value;
            alteredSkyboxIntro.SetColor( "_BaseColor", skyColor );
        }

        void OnDestroy()
        {
            alteredSkybox.SetColor( "_BaseColor", defaultSkyboxColor );
            alteredSkyboxIntro.SetColor( "_BaseColor", defaultIntroSkyboxColor );
        }

        void OnFadeOutIntro()
        {
            Skybox.material = alteredSkybox;
            GameEngine.GetInstance().ResumeTimeline();
        }

        public void ResetPassthrough()
        {
            PlayerController.GetInstance().AdjustPassthroughOpacity( 1 );
            PlayerController.GetInstance().ShowPassthrough();
        }

        public void StopPassthrough()
        {
            Color sky = alteredSkybox.GetColor( "_BaseColor" );
            sky.a = 1;
            alteredSkybox.SetColor( "_BaseColor", sky );

            foreach ( KeyValuePair<Material, Material> materials in fadeMaterials )
            {
                Color matColor = materials.Value.GetColor( "_BaseColor" );
                if ( matColor.a != 1 )
                {
                    matColor.a = 1;
                    materials.Value.SetColor( "_BaseColor", matColor );
                }
            }
        }


        void Update()
        {
            if ( !Traveller.activeSelf )
            {
                started = true;

                float distance = Vector3.Distance( ProximityTarget.transform.position,
                    PlayerController.GetInstance().gameObject.transform.position );
                //Debug.Log( "[SkyboxAdjuster] distance " + distance );
                float newOpacity = WadaMath.Remap(
                    distance,
                    0.3f,
                    1.5f,
                    1,
                    0
                );

                newOpacity = Mathf.Min( 1, Mathf.Max( 0, newOpacity ) );

                PlayerController.GetInstance().AdjustPassthroughOpacity( 1 - newOpacity );

                // Adjust the alpha
                Color sky = alteredSkybox.GetColor( "_BaseColor" );
                if ( sky.a != newOpacity )
                {
                    sky.a = newOpacity;
                    alteredSkybox.SetColor( "_BaseColor", sky );
                }

                foreach ( KeyValuePair<Material, Material> materials in fadeMaterials )
                {
                    Color matColor = materials.Value.GetColor( "_BaseColor" );
                    if ( matColor.a != newOpacity )
                    {
                        matColor.a = newOpacity;
                        materials.Value.SetColor( "_BaseColor", matColor );
                    }
                }
            }
            else
            {
                if ( started && !fadedIn )
                {
                    fadedIn = true;

                    PlayerController.GetInstance().AdjustPassthroughOpacity( 0 );
                    PlayerController.GetInstance().HidePasshtrough();
                    Color sky = alteredSkybox.GetColor( "_BaseColor" );
                    sky.a = 1;
                    alteredSkybox.SetColor( "_BaseColor", sky );

                    foreach ( KeyValuePair<Material, Material> materials in fadeMaterials )
                    {
                        Color matColor = materials.Value.GetColor( "_BaseColor" );
                        if ( matColor.a != 1 )
                        {
                            matColor.a = 1;
                            materials.Value.SetColor( "_BaseColor", matColor );
                        }
                    }
                }
            }
        }
    }
}