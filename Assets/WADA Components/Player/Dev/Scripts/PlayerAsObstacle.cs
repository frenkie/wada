using System;
using UnityEngine;

namespace Wada
{
    public class PlayerAsObstacle : MonoBehaviour
    {
        void LateUpdate()
        {
            transform.position = Camera.main.transform.position;
        }
    }
}