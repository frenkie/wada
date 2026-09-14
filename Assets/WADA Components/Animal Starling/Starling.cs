using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Starling : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            //Debug.Log( "OnTouch the Starling!" );
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Starling), true )]
    public class StarlingEditor : RessurectableEditor
    {
    }
#endif
}