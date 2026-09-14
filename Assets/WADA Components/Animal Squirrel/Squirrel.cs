using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Squirrel : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Squirrel), true )]
    public class SquirrelEditor : RessurectableEditor
    {
    }
#endif
}