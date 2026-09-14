using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Mole : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Mole), true )]
    public class MoleEditor : RessurectableEditor
    {
    }
#endif
}