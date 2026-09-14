using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Mouse : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Mouse), true )]
    public class MouseEditor : RessurectableEditor
    {
    }
#endif
}