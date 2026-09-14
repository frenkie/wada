using UnityEngine;

namespace Wada
{
    public class WaterfallAudio : MonoBehaviour
    {
        public SoundEffect Sound;

        bool muted = false;
        bool wasAlreadyMuted;

        void Update()
        {
            Vector3 relativePos = transform.InverseTransformPoint( Camera.main.transform.position );
            if ( relativePos.z < -1f && relativePos.y < 4 )
            {
                if ( !muted )
                {
                    Debug.Log( "Player is behind the waterfall" );
                    muted = true;
                    wasAlreadyMuted = Sound.IsMuted();
                    if ( !wasAlreadyMuted )
                    {
                        Sound.Mute();
                    }
                }
            }
            else if ( muted )
            {
                Debug.Log( "Player moved in front of the waterfall" );
                muted = false;
                if ( !wasAlreadyMuted && Sound.IsMuted() )
                {
                    Sound.Unmute();
                }
            }
        }
    }
}