using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Crab : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Crab), true )]
    public class CrabEditor : RessurectableEditor
    {
    }
#endif
}