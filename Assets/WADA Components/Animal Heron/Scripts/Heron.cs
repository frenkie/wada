using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Heron : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //1Invoke( "AddToFlocker", GetUpTime );

            Debug.Log( "OnTouch the heron!" );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(Heron), true )]
    public class HeronEditor : RessurectableEditor
    {
    }
#endif
}