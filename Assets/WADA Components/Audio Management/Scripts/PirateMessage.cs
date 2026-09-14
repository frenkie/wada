using UnityEngine;

namespace Wada
{
    public class PirateMessage : MonoBehaviour
    {
        public AudioSource Source;

        void Start()
        {
            Invoke( "Play", 8 );
        }

        void Play()
        {
            Source.time = 0;
            Source.Play();

            Invoke( "Play", Source.clip.length + 60 );
        }
    }
}