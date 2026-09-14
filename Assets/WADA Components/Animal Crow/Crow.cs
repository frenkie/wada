using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Crow : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            //Debug.Log( "OnTouch the Crow!" );
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Crow), true )]
    public class CrowEditor : RessurectableEditor
    {
    }
#endif
}