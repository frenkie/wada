using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Mosquito : Flocker
    {
        public override void OnTouch(WadaHand hand)
        {
            base.OnTouch( hand );

            //Invoke( "AddToFlocker", GetUpTime );

            Debug.Log( "OnTouch the Mosquito!" );
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(Mosquito), true )]
    public class MosquitoEditor : RessurectableEditor
    {
    }
#endif
}