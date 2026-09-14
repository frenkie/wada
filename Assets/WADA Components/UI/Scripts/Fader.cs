using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class Fader : MonoBehaviour
    {
        public float Alpha = 0;

        Material fader;

        void Awake()
        {
            Material current = GetComponent<Renderer>().material;
            fader = new Material( current.shader );
            fader.CopyPropertiesFromMaterial( current );
            GetComponent<Renderer>().material = fader;
        }


        void Update()
        {
            Color fade = fader.GetColor( "_BaseColor" );
            if ( fade.a != Alpha )
            {
                fade.a = Alpha;
                fader.SetColor( "_BaseColor", fade );
            }
        }
    }
}