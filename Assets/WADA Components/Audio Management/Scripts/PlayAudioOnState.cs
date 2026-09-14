using UnityEngine;

namespace Wada
{
    public class PlayAudioOnState : StateMachineBehaviour
    {
        public AudioClip audioClip;

        AudioSource audioSource;

        int playCount = 0;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            // if (audioSource == null)
            // {
            //     audioSource = animator.GetComponent<AudioSource>();
            // }
            // audioSource.PlayOneShot(audioClip);
            Debug.Log( "PlayAudioOnState: Enter" );
        }
    }
}