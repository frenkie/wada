using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Bambi : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Bambi), true )]
    public class BambiEditor : RessurectableEditor
    {
    }
#endif
}