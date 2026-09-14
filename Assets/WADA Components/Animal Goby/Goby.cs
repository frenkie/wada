using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Goby : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Goby), true )]
    public class GobyEditor : RessurectableEditor
    {
    }
#endif
}