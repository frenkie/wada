using UnityEngine;

namespace Wada
{
    public class InventoryItemRotator : MonoBehaviour
    {
        float speed = 0;
        Vector3 axis = Vector3.up;

        void Awake()
        {
            speed = Random.Range( .03f, 0.05f );
            transform.rotation = Random.rotation;
        }

        void Update()
        {
            transform.Rotate( axis, speed );
        }
    }
}