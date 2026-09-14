using System;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class FBXPlayer : MonoBehaviour
    {
        public Transform Root;
        public int FPS = 24;

        List<GameObject> frames = new();

        float interval;
        float timer;
        int index;

        void Awake()
        {
            foreach ( Transform t in Root )
            {
                if ( t != Root )
                {
                    frames.Add( t.gameObject );
                    t.gameObject.SetActive( false );
                }
            }
        }

        void Start()
        {
            ShowFrame( 0 );
        }

        void ShowFrame(int idx)
        {
            frames[idx == 0 ? frames.Count - 1 : idx - 1].SetActive( false );
            frames[idx].SetActive( true );
        }

        void Update()
        {
            timer += Time.deltaTime;
            interval = 1f / FPS; // to allow for adjusting it live
            if ( timer >= interval )
            {
                timer = 0;
                index++;
                if ( index == frames.Count )
                {
                    index = 0;
                }

                ShowFrame( index );
            }
        }
    }
}