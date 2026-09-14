using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Sheep : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Sheep), true )]
    public class SheepEditor : RessurectableEditor
    {
    }
#endif
}