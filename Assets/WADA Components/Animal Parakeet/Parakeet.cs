using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Parakeet : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            Debug.Log( "OnTouch the Parakeet!" );
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Parakeet), true )]
    public class ParakeetEditor : RessurectableEditor
    {
    }
#endif
}