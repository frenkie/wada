using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Grasshopper : Flocker
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Grasshopper), true )]
    public class GrasshopperEditor : RessurectableEditor
    {
    }
#endif
}