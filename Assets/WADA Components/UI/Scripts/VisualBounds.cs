using UnityEngine;

namespace Wada
{
    public class VisualBounds : MonoBehaviour
    {
        public Exposure ExposureFader;

        bool outOfBounds;

        public void AlertUser()
        {
            Debug.Log( "[VisualBounds] Alerting user" );
            ExposureFader.SceneFadeInExposure();
        }

        void OnTriggerStay(Collider other)
        {
            if ( other.gameObject.tag == "Player" || other.gameObject.tag == "MainCamera" )
            {
                CancelInvoke( "StopAlertingUser" );
                if ( !outOfBounds )
                {
                    outOfBounds = true;
                    Invoke( "AlertUser", .1f );
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if ( other.gameObject.tag == "Player" || other.gameObject.tag == "MainCamera" )
            {
                CancelInvoke( "StopAlertingUser" );
                Invoke( "StopAlertingUser", .3f );
            }
        }

        void StopAlertingUser()
        {
            Debug.Log( "[VisualBounds] User is back!" );
            CancelInvoke( "AlertUser" );
            outOfBounds = false;
            ExposureFader.SceneFadeOutExposure();
        }
    }
}