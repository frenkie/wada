using UnityEngine;

namespace Wada
{
    public class RotateToTester : MonoBehaviour
    {
        public GameObject RotateTo;
        public float RotateSpeed = 1.0f;

        void Update()
        {
            Vector3 rotateDirection = transform.position - new Vector3(
                RotateTo.transform.position.x,
                transform.position.y,
                RotateTo.transform.position.z
            );

            Quaternion rotation = Quaternion.LookRotation( rotateDirection );

            transform.rotation = Quaternion.Slerp( transform.rotation, rotation,
                Time.deltaTime * RotateSpeed );
        }
    }
}