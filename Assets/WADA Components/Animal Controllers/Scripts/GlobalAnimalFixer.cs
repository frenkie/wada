using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class GlobalAnimalFixer : MonoBehaviour
    {
        public void FixCharacterJoints()
        {
            foreach ( CharacterJoint characterJoint in GetComponentsInChildren<CharacterJoint>() )
            {
                characterJoint.twistLimitSpring = new SoftJointLimitSpring() { damper = 5 };
                characterJoint.swingLimitSpring = new SoftJointLimitSpring() { damper = 5 };
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(GlobalAnimalFixer) )]
    public class GlobalAnimalFixerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GlobalAnimalFixer myTarget = (GlobalAnimalFixer)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Fix CharacterJoints" ) )
            {
                myTarget.FixCharacterJoints();
            }
        }
    }
#endif
}