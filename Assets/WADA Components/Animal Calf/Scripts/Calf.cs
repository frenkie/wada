using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Calf : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Calf), true )]
    public class CalfEditor : RessurectableEditor
    {
    }
#endif
}