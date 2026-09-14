using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class RavenOld : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            /*
             *  If chosen as guide go to guide mode
             *
             *  If chosen as 2nd go to flock mode, fly to a specific location and add to flock
             */

            base.OnTouch( hand );

            Invoke( "AddToFlocker", GetUpTime );

            Debug.Log( "OnTouch the raven!" );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(RavenOld), true )]
    public class RavenOldEditor : RessurectableEditor
    {
    }
#endif
}