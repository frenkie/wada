using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Raccoon : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Raccoon), true )]
    public class RaccoonEditor : RessurectableEditor
    {
    }
#endif
}