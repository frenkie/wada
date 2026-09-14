using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wada
{
    public class Exposure : MonoBehaviour
    {
        public Volume ppGlobalVolume; // Ref to the PostProcessing Volume

        public bool FadeIn = false;
        public bool FadeOut = false;
        FloatParameter cpExpo = null;
        FloatParameter cpSaturation = null;

        float expoMax = 3.54f;
        float saturationMax = -49;

        IEnumerator coroutine;

        enum FadingDirection
        {
            FadeIn,
            FadeOut
        }

        void Awake()
        {
            ColorAdjustments colorAdjustments = null;
            if ( !ppGlobalVolume.profile.TryGet<ColorAdjustments>( out colorAdjustments ) )
            {
                Debug.LogWarning( "No color adjustments found!" );
            }
            else
            {
                cpExpo = colorAdjustments.postExposure;
                cpSaturation = colorAdjustments.saturation;
            }
        }

        void Update()
        {
            if ( FadeIn )
            {
                FadeIn = false;
                SceneFadeInExposure();
            }

            if ( FadeOut )
            {
                FadeOut = false;
                SceneFadeOutExposure();
            }
        }

        IEnumerator FadeAll(float from, float to, float timing)
        {
            cpExpo.value = from * expoMax;
            cpSaturation.value = from * saturationMax;

            float elapsedTime = 0;
            while ( elapsedTime < timing )
            {
                cpExpo.Interp( from * expoMax, to * expoMax, elapsedTime / timing );
                cpSaturation.Interp( from * saturationMax, to * saturationMax, elapsedTime / timing );
                elapsedTime += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }

            cpExpo.value = to * expoMax;
            cpSaturation.value = to * saturationMax;

            yield return new WaitForEndOfFrame();
        }

        //[Button]
        public void SceneFadeOutExposure(float timeSecs = 5f)
        {
            DoFade( FadingDirection.FadeOut, timeSecs );
        }

        //[Button]
        public void SceneFadeInExposure(float timeSecs = 5f)
        {
            DoFade( FadingDirection.FadeIn, timeSecs );
        }

        void DoFade(FadingDirection fadingDir, float timeSecs)
        {
            float from = 0;
            float to = 1;

            if ( fadingDir == FadingDirection.FadeOut )
            {
                from = 1f;
                to = 0;
            }

            // interrupt started fade and grab current value
            if ( coroutine != null )
            {
                StopCoroutine( coroutine );
                from = cpExpo.value / expoMax;
            }

            coroutine = FadeAll( from, to, timeSecs );
            StartCoroutine( coroutine );
        }
    }
}