using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Boar : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Boar), true )]
    public class BoarEditor : RessurectableEditor
    {
    }
#endif
}