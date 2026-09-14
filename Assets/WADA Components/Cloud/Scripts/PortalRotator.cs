using UnityEngine;

namespace Wada
{
    public class PortalRotator : MonoBehaviour
    {
        void Update()
        {
            transform.Rotate( Vector3.up, 360 / 30 * Time.deltaTime );
        }
    }
}