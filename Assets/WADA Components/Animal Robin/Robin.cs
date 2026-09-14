using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Robin : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            Debug.Log( "OnTouch the Robin!" );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(Robin), true )]
    public class RobinEditor : RessurectableEditor
    {
    }
#endif
}