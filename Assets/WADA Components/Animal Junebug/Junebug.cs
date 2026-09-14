using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Junebug : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            Debug.Log( "OnTouch the Junebug!" );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(Junebug), true )]
    public class JunebugEditor : RessurectableEditor
    {
    }
#endif
}