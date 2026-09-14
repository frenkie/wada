using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Fly : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            Debug.Log( "OnTouch the fly!" );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(Fly), true )]
    public class FlyEditor : RessurectableEditor
    {
    }
#endif
}