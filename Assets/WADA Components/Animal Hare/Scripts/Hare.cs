using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Hare : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Hare), true )]
    public class HareEditor : RessurectableEditor
    {
    }
#endif
}