using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class SquirrelMummy : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(SquirrelMummy), true )]
    public class SquirrelMummyEditor : RessurectableEditor
    {
    }
#endif
}