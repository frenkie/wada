using Unity.XR.CoreUtils;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


namespace Wada
{
    public class TranslationTest : MonoBehaviour
    {
        public GameObject Parent1;
        public GameObject Socket1;
        public GameObject Limb;
        public GameObject Parent2;
        public GameObject Socket2;


        public GameObject CubeSocket;
        public GameObject CubeFollow;

        public void Test()
        {
            /*
             * Simple test, just translation in local root system, no rotations
             * 
             */

            Vector3 localTranslationInRoot =
                Parent1.transform.InverseTransformDirection( Parent1.transform.position - Socket1.transform.position );
            Quaternion localOrientation = Limb.transform.localRotation;

            Vector3 localLimbPos = Limb.transform.localPosition;

            GameObject newParent = new();
            newParent.name = "ProxyParent";

            newParent.transform.position = Socket2.transform.position;
            newParent.transform.rotation = Socket2.transform.rotation;
            newParent.transform.Translate( localTranslationInRoot );
            newParent.transform.localScale = Parent1.transform.localScale;
            newParent.transform.parent = Parent2.transform;
            //
            Limb.transform.parent = newParent.transform;
            Limb.transform.localPosition = localLimbPos;
            Limb.transform.localRotation = localOrientation;
        }


        Vector3 offset;

        void Start()
        {
            offset = CubeSocket.transform.InverseTransformDirection( CubeFollow.transform.position -
                                                                     CubeSocket.transform.position );
            // make offset relative to forward
            Debug.Log( offset );
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            //AnimationTranslations[forAnimation]

            Debug.DrawLine( CubeSocket.transform.position,
                CubeSocket.transform.position + CubeSocket.transform.TransformDirection( offset ),
                Color.yellow );
        }
#endif

        void Update()
        {
            CubeFollow.transform.position =
                CubeSocket.transform.position + CubeSocket.transform.TransformDirection( offset );
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(TranslationTest) )]
    public class TranslationTestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TranslationTest myTarget = (TranslationTest)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Test" ) )
            {
                myTarget.Test();
            }
        }
    }
#endif
}