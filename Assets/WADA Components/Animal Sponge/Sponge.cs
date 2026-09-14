using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Sponge : Ressurectable
    {
        //
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Sponge), true )]
    public class SpongeEditor : RessurectableEditor
    {
    }
#endif
}