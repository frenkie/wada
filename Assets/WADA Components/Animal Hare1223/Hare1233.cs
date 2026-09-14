using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Hare1223 : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Hare1223), true )]
    public class Hare1223Editor : RessurectableEditor
    {
    }
#endif
}