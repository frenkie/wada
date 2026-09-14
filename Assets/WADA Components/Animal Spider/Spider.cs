using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Spider : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Spider), true )]
    public class SpiderEditor : RessurectableEditor
    {
    }
#endif
}