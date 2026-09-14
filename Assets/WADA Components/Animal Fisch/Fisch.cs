using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Fisch : Ressurectable
    {
        //
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Fisch), true )]
    public class FischEditor : RessurectableEditor
    {
    }
#endif
}