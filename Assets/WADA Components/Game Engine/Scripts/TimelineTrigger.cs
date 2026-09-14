using UnityEngine;

namespace Wada
{
    public class TimelineTrigger : MonoBehaviour
    {
        public double GoToTime;

        public void Trigger(Collider collider)
        {
            if ( collider == null || (collider != null && collider.gameObject.tag == "Player") )
            {
                GameEngine.GetInstance().JumpToTime( GoToTime );
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            Debug.Log( "[TimelineTrigger] OnTriggerEnter" );
            Trigger( collider );
        }
    }
}