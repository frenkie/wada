using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Magpie : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            // Debug.Log( "OnTouch the Magpie!" );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(Magpie), true )]
    public class MagpieEditor : RessurectableEditor
    {
    }
#endif
}