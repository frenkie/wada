using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class GuidingDuck : MonoBehaviour
    {
        public GameObject Duck;

        Animator ducky;

        void Awake()
        {
            ducky = GetComponentInChildren<Animator>();
        }

        public void OnAnimationEnd()
        {
            Duck.SetActive( false );
        }

        void OnEnable()
        {
            Duck.SetActive( true );
        }

        void OnDisable()
        {
            ducky.playbackTime = 0;
        }
    }
}