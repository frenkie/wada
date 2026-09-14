using System;
using System.Collections;
using Unity;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class RiverManager : MonoBehaviour
    {
        void Awake()
        {
            SetNextBaseOnOrder();
        }


        public void ClearRivers()
        {
            foreach ( RiverNoPhysics river in GetComponentsInChildren<RiverNoPhysics>() )
            {
                river.ClearRiver();
            }
        }

        public void FillRivers()
        {
            foreach ( RiverNoPhysics river in GetComponentsInChildren<RiverNoPhysics>() )
            {
                river.FillRiver();
            }
        }

        public void SetNextBaseOnOrder()
        {
            RiverNoPhysics[] rivers = GetComponentsInChildren<RiverNoPhysics>();
            int riverCount = rivers.Length;

            for ( int i = 0; i < riverCount; i++ )
            {
                if ( i == riverCount - 1 )
                {
                    rivers[i].NextRiverPart = null;
                }
                else
                {
                    rivers[i].NextRiverPart = rivers[i + 1];
                }
#if UNITY_EDITOR
                EditorUtility.SetDirty( rivers[i].gameObject );
#endif
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(RiverManager) )]
    public class RiverManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            RiverManager myTarget = (RiverManager)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Clear Rivers" ) )
            {
                myTarget.ClearRivers();
            }

            if ( GUILayout.Button( "Fill Rivers" ) )
            {
                myTarget.FillRivers();
            }

            if ( GUILayout.Button( "Create Chain Connections" ) )
            {
                myTarget.SetNextBaseOnOrder();
            }
        }
    }
#endif
}