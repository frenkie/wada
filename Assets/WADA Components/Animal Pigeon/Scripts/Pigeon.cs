using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Pigeon : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            // Debug.Log( "OnTouch the pigeone!" );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(Pigeon), true )]
    public class PigeonEditor : RessurectableEditor
    {
    }
#endif
}