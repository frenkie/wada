using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Fox : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Fox), true )]
    public class FoxEditor : RessurectableEditor
    {
    }
#endif
}