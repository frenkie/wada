using UnityEngine;

namespace Wada
{
    public class TimelineResumeTrigger : MonoBehaviour
    {
        public bool Once = false;

        bool triggered = false;

        public void Trigger(Collider collider)
        {
            if ( collider == null || (collider != null && collider.gameObject.tag == "Player") )
            {
                if ( (Once && !triggered) || !Once )
                {
                    triggered = true;
                    Debug.Log( "[TimelineResumeTrigger] Triggered by Player" );
                    GameEngine.GetInstance().ResumeTimeline();
                }
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            Debug.Log( "[TimelineResumeTrigger] OnTriggerEnter" );
            Trigger( collider );
        }
    }
}