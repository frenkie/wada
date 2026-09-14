using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Bee : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            // Debug.Log( "OnTouch the Bee!" );
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Bee), true )]
    public class BeeEditor : RessurectableEditor
    {
    }
#endif
}