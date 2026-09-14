using UnityEngine;

namespace Wada
{
    public class InventoryRotator : MonoBehaviour
    {
        public float Speed = 10f;
        public Vector3 Axis = Vector3.up;

        void Update()
        {
            transform.Rotate( Axis, Speed );
        }
    }
}