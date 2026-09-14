using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Moth : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            Debug.Log( "OnTouch the Moth!" );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(Moth), true )]
    public class MothEditor : RessurectableEditor
    {
    }
#endif
}