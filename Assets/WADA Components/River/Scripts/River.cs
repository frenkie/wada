using System;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class River : MonoBehaviour
    {
        public GameObject[] TrashPrefabs;
        public GameObject TrashContainer;
        public Vector2 TrashZRange = new( 8, 11 );
        public Vector2 TrashXRange = new( 12, 15 );

        public float RegenerateAfter = 3f;
        public bool CanRegenerate = true;

        float curTime;
        Bounds bounds;

        /*
         *  
         * 
         * Start off with filled river
         *
         * Spawn row of items unscaled
         *
         * All have an individual scaleUp on awake
         *
         * When reaching the 'end' of the local bounds in X, scaleDown and delete.
         *
         *
         * TODO: Maybe detection of distance with player and choosing a dynamic or LOD version
         *
         * Add 'Static' trash that just floats, but doesn't stream
         *  -   maybe through
         */

        void Start()
        {
            bounds = TrashContainer.GetComponent<MeshFilter>().sharedMesh.bounds;
        }

        public void ClearRiver()
        {
            for ( int i = TrashContainer.transform.childCount - 1; i >= 0; i-- )
            {
                DestroyImmediate( TrashContainer.transform.GetChild( i ).gameObject );
            }
        }

        public void FillRiver()
        {
            ClearRiver();

            bounds = TrashContainer.GetComponent<MeshFilter>().sharedMesh.bounds;

            int x = Random.Range( (int)TrashXRange.x, (int)TrashXRange.y );
            float step = bounds.size.x / x;
            float localX = bounds.min.x + .5f * step;

            for ( int i = x - 1; i >= 0; i-- )
            {
                FillRow( localX );

                localX += step;
            }
        }

        public void FillRow(float withX, bool adjustAppearTime = true)
        {
            int z = Random.Range( (int)TrashZRange.x, (int)TrashZRange.y );
            float step2 = bounds.size.z / z;
            float localZ = bounds.min.z + .5f * step2;

            for ( int j = z - 1; j >= 0; j-- )
            {
                GameObject prefab = TrashPrefabs[Random.Range( 0, TrashPrefabs.Length )].gameObject;
                GameObject copy = Instantiate( prefab, TrashContainer.transform );
                copy.transform.rotation = Random.rotation;
                copy.transform.localPosition =
                    new Vector3( withX, bounds.size.y, localZ ); // TODO randomize around this pos even
                RiverTrash trash = copy.GetComponent<RiverTrash>();

                if ( adjustAppearTime )
                {
                    trash.AppearTime = 3f;
                }

                localZ += step2;
            }
        }

        void Update()
        {
            if ( CanRegenerate )
            {
                curTime += Time.deltaTime;

                if ( curTime >= RegenerateAfter )
                {
                    curTime = 0;

                    int x = Random.Range( (int)TrashXRange.x, (int)TrashXRange.y );
                    float step = bounds.size.x / x;
                    float localX = bounds.min.x + .5f * step;

                    FillRow( localX, false );
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(River) )]
    public class RiverEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            River myTarget = (River)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Clear River" ) )
            {
                myTarget.ClearRiver();
            }

            if ( GUILayout.Button( "Fill River" ) )
            {
                myTarget.FillRiver();
            }
        }
    }
#endif
}