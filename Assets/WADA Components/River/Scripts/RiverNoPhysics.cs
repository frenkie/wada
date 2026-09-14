using System;
using UnityEngine;
using Random = UnityEngine.Random;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class RiverNoPhysics : MonoBehaviour
    {
        public GameObject[] TrashPrefabs;
        public GameObject TrashContainer;
        public GameObject RiverMesh;
        public Vector2 TrashZRange = new( 8, 11 );
        public Vector2 TrashXRange = new( 12, 15 );

        public float YOffset = 0.3f;
        float RegenerateAfter = 1f;
        public bool CanRegenerate = true;
        const float speed = 0.75f;

        public RiverNoPhysics NextRiverPart;
        public bool IsWaterfallDrop = false;

        float curTime;
        Bounds bounds;

        const float VICINITY_THRESHOLD = 20;
        float vicinityInterval = .5f;
        float vicinityTime = 0;
        bool isInVicinity = false;

        /*
         * Distance to player determines whether this River will start flowing 
         *
         * Add 'Static' trash that just floats, but doesn't stream
         *  -   maybe through
         */

        public void AdoptTrash(RiverTrashNoPhysics trash)
        {
            if ( isInVicinity )
            {
                Vector3 currentPos = trash.gameObject.transform.position;
                Vector3 currentRotation = trash.gameObject.transform.eulerAngles;
                trash.gameObject.transform.parent = TrashContainer.transform;
                trash.gameObject.transform.position = currentPos;
                trash.gameObject.transform.eulerAngles = currentRotation;
                trash.OwningRiverPart = this;
                trash.RiverPartBounds = bounds;
                trash.AdjustToRiver();
            }
            else
            {
                trash.FadeOut();
            }
        }

        void Start()
        {
            bounds = RiverMesh.GetComponent<MeshFilter>().sharedMesh.bounds;
        }

        void CheckVicinity()
        {
            float distance = Vector3.Distance( transform.position, Camera.main.transform.position );
            if ( !isInVicinity && distance <= VICINITY_THRESHOLD )
            {
                isInVicinity = true;
                if ( NextRiverPart != null )
                {
                    NextRiverPart.CanRegenerate = false;
                }

                FillRiver();
            }
            else if ( isInVicinity && distance > VICINITY_THRESHOLD )
            {
                isInVicinity = false;
                foreach ( RiverTrashNoPhysics trash in TrashContainer.GetComponentsInChildren<RiverTrashNoPhysics>() )
                {
                    trash.FadeOut();
                }

                if ( NextRiverPart != null )
                {
                    NextRiverPart.CanRegenerate = true;
                }
            }
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

            bounds = RiverMesh.GetComponent<MeshFilter>().sharedMesh.bounds;

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
                    new Vector3( withX, bounds.size.y + YOffset, localZ ); // TODO randomize around this pos even?
                RiverTrashNoPhysics trash = copy.GetComponent<RiverTrashNoPhysics>();

                trash.OwningRiverPart = this;
                trash.RiverPartBounds = bounds;

                // TODO pass on intended localEndPosition
                trash.LocalEndPosition = new Vector3(
                    bounds.max.x,
                    copy.transform.localPosition.y,
                    copy.transform.localPosition.z
                );

                if ( adjustAppearTime )
                {
                    trash.AppearTime = 3f;
                }

                if ( Random.Range( 0f, 1.0f ) <= .3f )
                {
                    trash.Rotate = true;
                }

                trash.Speed = speed;

                localZ += step2;
            }
        }

        void Update()
        {
            vicinityTime += Time.deltaTime;

            if ( vicinityTime >= vicinityInterval )
            {
                vicinityTime = 0;
                CheckVicinity();
            }

            if ( isInVicinity && CanRegenerate )
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
    [CustomEditor( typeof(RiverNoPhysics) )]
    public class RiverNoPhysicsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            RiverNoPhysics myTarget = (RiverNoPhysics)target;

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