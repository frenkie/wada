using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Lice : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Lice), true )]
    public class LiceEditor : RessurectableEditor
    {
    }
#endif
}