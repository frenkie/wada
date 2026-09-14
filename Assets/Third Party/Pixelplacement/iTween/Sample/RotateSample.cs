using UnityEngine;
using System.Collections;

public class RotateSample : MonoBehaviour
{
    void Start()
    {
        iTween.RotateBy( gameObject, new Vector3( 0f, 360f, 0f ), 30 );
    }
}