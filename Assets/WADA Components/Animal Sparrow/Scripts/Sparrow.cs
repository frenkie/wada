using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Sparrow : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            Debug.Log( "OnTouch the Sparrow!" );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(Sparrow), true )]
    public class SparrowEditor : RessurectableEditor
    {
    }
#endif
}