using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Lizard : Ressurectable
    {
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(Lizard), true )]
    public class LizardEditor : RessurectableEditor
    {
    }
#endif
}