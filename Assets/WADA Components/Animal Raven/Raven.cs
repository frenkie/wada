using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Raven : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            //Debug.Log( "OnTouch the Raven!" );
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Raven), true )]
    public class RavenEditor : RessurectableEditor
    {
    }
#endif
}