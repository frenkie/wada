using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class Heart : MonoBehaviour
    {
        float blinkInterval = 0.2f;

        Renderer[] renderer;
        List<Material> materials = new();
        bool shown = true;
        bool blinking = false;

        void Start()
        {
            renderer = GetComponentsInChildren<Renderer>();
            foreach ( Renderer r in renderer )
            {
                Material current = r.material;
                Material heart = new( current.shader );
                heart.CopyPropertiesFromMaterial( current );
                r.material = heart;
                materials.Add( heart );
            }
        }

        void Blink()
        {
            blinking = true;
            CancelInvoke( "Blink" );

            foreach ( Renderer r in renderer )
            {
                r.enabled = !r.enabled;
            }

            Invoke( "Blink", blinkInterval );
        }

        public void Hide()
        {
            if ( shown )
            {
                shown = false;

                StopBlinking();
                foreach ( Renderer r in renderer )
                {
                    r.enabled = false;
                }
            }
        }

        public bool IsBlinking()
        {
            return blinking;
        }

        public void Show()
        {
            if ( !shown )
            {
                shown = true;
                foreach ( Renderer r in renderer )
                {
                    r.enabled = true;
                }
            }
        }

        public void StartBlinking(float duration)
        {
            Blink();
            if ( duration > 0 )
            {
                Invoke( "StopBlinking", duration );
            }
        }

        public void StopBlinking()
        {
            if ( blinking )
            {
                CancelInvoke( "StopBlinking" );
                CancelInvoke( "Blink" );
                blinking = false;
                foreach ( Renderer r in renderer )
                {
                    r.enabled = true;
                }
            }
        }

        public void UpdateProgress(float progress)
        {
            foreach ( Material m in materials )
            {
                m.SetFloat( "_Progress", progress );
            }
        }
    }
}