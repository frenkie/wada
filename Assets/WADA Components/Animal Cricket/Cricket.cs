using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Cricket : Flocker
    {
        void Nothing()
        {
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Cricket), true )]
    public class CricketEditor : RessurectableEditor
    {
    }
#endif
}