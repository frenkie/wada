using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Cranefly : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            Debug.Log( "OnTouch the Cranefly!" );
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Cranefly), true )]
    public class CraneflyEditor : RessurectableEditor
    {
    }
#endif
}