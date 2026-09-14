using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Toad : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Toad), true )]
    public class ToadEditor : RessurectableEditor
    {
    }
#endif
}