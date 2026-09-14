using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Ploetze : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Ploetze), true )]
    public class PloetzeEditor : RessurectableEditor
    {
    }
#endif
}