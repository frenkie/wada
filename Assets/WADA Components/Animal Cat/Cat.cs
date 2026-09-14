using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Cat : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Cat), true )]
    public class CatEditor : RessurectableEditor
    {
    }
#endif
}