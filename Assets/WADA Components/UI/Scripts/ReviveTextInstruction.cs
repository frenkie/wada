using System;
using UnityEngine;
using System.Collections;
using TMPro;

namespace Wada
{
    public class ReviveTextInstruction : TextInstruction
    {
        public float Delay;
        
        void Start()
        {
            Invoke("FadeIn", Delay);
        }

        void OnFadeOut()
        {
            // don't call the base one
        }        
    }
}