using UnityEngine;

namespace Wada
{
    public class DisplayPlateau : MonoBehaviour
    {
        void Start()
        {
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                Random.Range( -180, 180 ),
                transform.eulerAngles.z
            );
        }
    }
}