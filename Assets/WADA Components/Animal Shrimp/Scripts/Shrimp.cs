using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Shrimp : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Shrimp), true )]
    public class ShrimpEditor : RessurectableEditor
    {
    }
#endif
}