using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Wasp : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            Debug.Log( "OnTouch the Wasp!" );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(Wasp), true )]
    public class WaspEditor : RessurectableEditor
    {
    }
#endif
}