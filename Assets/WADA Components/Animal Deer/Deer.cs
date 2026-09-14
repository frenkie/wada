using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Deer : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Deer), true )]
    public class DeerEditor : RessurectableEditor
    {
    }
#endif
}