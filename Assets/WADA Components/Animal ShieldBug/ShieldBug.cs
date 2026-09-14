using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class ShieldBug : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            //Debug.Log( "OnTouch the ShieldBug!" );
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(ShieldBug), true )]
    public class ShieldBugEditor : RessurectableEditor
    {
    }
#endif
}