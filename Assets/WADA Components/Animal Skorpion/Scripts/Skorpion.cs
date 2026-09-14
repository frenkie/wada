using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Skorpion : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Skorpion), true )]
    public class SkorpionEditor : RessurectableEditor
    {
    }
#endif
}