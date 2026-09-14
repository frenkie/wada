using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Duck : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            //Debug.Log( "OnTouch the Duck!" );
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Duck), true )]
    public class DuckEditor : RessurectableEditor
    {
    }
#endif
}