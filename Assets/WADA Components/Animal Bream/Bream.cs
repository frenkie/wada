using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Bream : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Bream), true )]
    public class BreamEditor : RessurectableEditor
    {
    }
#endif
}