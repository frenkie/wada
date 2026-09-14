using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Frog : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Frog), true )]
    public class FrogEditor : RessurectableEditor
    {
    }
#endif
}