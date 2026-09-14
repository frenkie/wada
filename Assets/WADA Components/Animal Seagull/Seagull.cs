using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Seagull : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            // Debug.Log( "OnTouch the Seagull!" );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(Seagull), true )]
    public class SeagullEditor : RessurectableEditor
    {
    }
#endif
}