using System;
using UnityEngine;

namespace Wada
{
    [RequireComponent( typeof(AudioSource) )]
    public class SwimSound : MonoBehaviour
    {
        public AudioClip Audio;

        AudioSource audioSource;

        void Awake()
        {
            audioSource = GetComponent<AudioSource>();
        }

        void Start()
        {
            audioSource.clip = Audio;
            audioSource.Play();

            Invoke( "Remove", Audio.length + 0.5f );
        }

        void Remove()
        {
            Destroy( gameObject );
        }
    }
}