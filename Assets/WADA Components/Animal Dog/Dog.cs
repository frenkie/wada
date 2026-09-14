using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Dog : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Dog), true )]
    public class DogEditor : RessurectableEditor
    {
    }
#endif
}