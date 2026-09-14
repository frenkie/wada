using UnityEngine;

namespace Wada
{
    public class BackgroundMusicRandomMuter : MonoBehaviour
    {
        public Vector2 IntervalRangeMute;
        public Vector2 IntervalRangeMutedDuration;

        BackgroundMusic music;


        void Start()
        {
            music = GetComponent<BackgroundMusic>();

            Invoke( "Toggle", Random.Range( IntervalRangeMute.x, IntervalRangeMute.y ) );
        }

        void Toggle()
        {
            if ( music.IsMuted() )
            {
                music.Unmute();
                Invoke( "Toggle", Random.Range( IntervalRangeMute.x, IntervalRangeMute.y ) );
            }
            else
            {
                music.Mute();
                Invoke( "Toggle", Random.Range( IntervalRangeMutedDuration.x, IntervalRangeMutedDuration.y ) );
            }
        }
    }
}