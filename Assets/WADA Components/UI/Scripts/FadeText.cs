using UnityEngine;
using System.Collections;
using TMPro;

namespace Wada
{
    public class FadeText : MonoBehaviour
    {
        public AudioClip Voice;
        public float Delay = 0;

        AudioSource audioSource;
        float fadeDuration = .3f;

        TextMeshPro textMesh;

        void Awake()
        {
            textMesh = GetComponent<TextMeshPro>();

            Color fadedColor = textMesh.color;
            fadedColor.a = 0;
            textMesh.color = fadedColor;

            if ( Voice != null )
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.clip = Voice;
            }
        }

        public void FadeIn()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 0 );
            valueTo.Add( "to", 1 );
            valueTo.Add( "time", fadeDuration );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnFade" );
            valueTo.Add( "oncomplete", "OnFadedIn" );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void FadeOut()
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", 1 );
            valueTo.Add( "to", 0 );
            valueTo.Add( "time", fadeDuration );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "OnFade" );
            valueTo.Add( "oncomplete", "OnFadeOut" );

            iTween.ValueTo( gameObject, valueTo );
        }

        void Start()
        {
            if ( Delay > 0 )
            {
                Invoke( "FadeIn", Delay );
            }
            else
            {
                FadeIn();
            }
        }

        void OnFade(float fadeToAlpha)
        {
            Color fadedColor = textMesh.color;
            fadedColor.a = fadeToAlpha;

            textMesh.color = fadedColor;
        }

        void OnFadedIn()
        {
            if ( Voice != null )
            {
                audioSource.Play();
            }
        }

        void OnFadeOut()
        {
            // TODO Hide?
        }
    }
}