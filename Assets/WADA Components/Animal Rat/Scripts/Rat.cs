using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Rat : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Rat), true )]
    public class RatEditor : RessurectableEditor
    {
    }
#endif
}