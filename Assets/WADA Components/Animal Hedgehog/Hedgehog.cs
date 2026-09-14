using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Hedgehog : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Hedgehog), true )]
    public class HedgehogEditor : RessurectableEditor
    {
    }
#endif
}