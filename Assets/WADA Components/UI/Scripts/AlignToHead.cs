using UnityEngine;

namespace Wada
{
    public class AlignToHead : MonoBehaviour
    {
        void Start()
        {
            transform.position = new Vector3(
                transform.position.x,
                PlayerController.GetInstance().GetLocation().y,
                transform.position.z
            );
        }
    }
}