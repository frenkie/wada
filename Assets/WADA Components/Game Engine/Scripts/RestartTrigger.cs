using UnityEngine;
using UnityEngine.SceneManagement;

namespace Wada
{
    public class RestartTrigger : MonoBehaviour
    {
        bool triggered;

        public void Trigger(Collider collider)
        {
            if ( (!triggered && collider == null) || (collider != null && collider.gameObject.tag == "Player") )
            {
                triggered = true;
                SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex );
            }
        }

        void OnTriggerEnter(Collider collider)
        {
            Debug.Log( "[TimelineTrigger] OnTriggerEnter" );
            Trigger( collider );
        }
    }
}