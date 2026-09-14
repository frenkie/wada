using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class InventoryItem
    {
        public GameObject Docker;
        public IDockable Docked;
    }

    public class Inventory : MonoBehaviour
    {
        /*
         *  Rotating list of X elements
         *
         *  -   For now any AnimalGrabbable extensions
         *  -   Has a hit box as a trigger
         *  -   When a detached limb enters the inventory it gets an inventory state?
         *  -   When released and in inventory, dock
         *      -   attach to inventory spot
         *      -   scale
         *
         *
         *
         *  NTH (or for later):
         *  -   Inventory spot highlights when limb enters the space,
         */

        public int SpotCount = 10;
        public float Radius = .9f;
        public GameObject Rotator;
        public float ScaleFactor = 0.3f;

        public
            List<InventoryItem> docked;


        float moveTime = -2;
        bool move = false;

        void Awake()
        {
            docked = new List<InventoryItem>();
        }

        void CreateSpots()
        {
            float angleStep = 360 / SpotCount;

            for ( int i = 0; i < SpotCount; i++ )
            {
                GameObject sphere = GameObject.CreatePrimitive( PrimitiveType.Sphere );
                sphere.transform.localScale = Vector3.one * .1f;
                sphere.transform.parent = Rotator.transform;
                sphere.transform.localPosition = new Vector3(
                    Mathf.Cos( i * angleStep ) * Radius,
                    0,
                    Mathf.Sin( i * angleStep ) * Radius
                );

                Destroy( sphere.GetComponent<Collider>() );
                sphere.GetComponent<Renderer>().enabled = false;
                sphere.AddComponent( typeof(InventoryItemRotator) );

                InventoryItem stock = new();
                stock.Docker = sphere;
                docked.Add( stock );
            }
        }

        public void Dock(IDockable dockable)
        {
            /*
             * -    Parent to a free spot
             * -    Animate Scale?
             * -    Make Kinematic
             */

            InventoryItem freeSpot = GetFreeInventorySpot( dockable );

            if ( freeSpot != null )
            {
                freeSpot.Docked = dockable;
                GameObject dockedGameObject = dockable.GetGameObject();

                dockedGameObject.transform.parent = freeSpot.Docker.transform;

                Hashtable moveTo = new();

                moveTo.Add( "position", Vector3.one * -.15f );
                moveTo.Add( "islocal", true );
                moveTo.Add( "time", .4f );
                moveTo.Add( "easetype", iTween.EaseType.easeOutCubic );

                iTween.MoveTo( dockedGameObject, moveTo );

                ScaleDockedItem( dockable, dockedGameObject.transform.localScale * ScaleFactor );

                dockable.Docked = true;

                StartCoroutine( MakeKinetic( dockable ) );
            }
            else
            {
                Debug.LogWarning( "No more inventory spots, just let it fall?" );
            }
        }

        IEnumerator MakeKinetic(IDockable dockable)
        {
            yield return new WaitForSeconds( .2f );

            foreach ( IDockable selfOrChild in dockable.GetGameObject().GetComponentsInChildren<IDockable>() )
            {
                selfOrChild.GetRigidbody().isKinematic = true;
            }
        }

        InventoryItem GetFreeInventorySpot(IDockable forDockable)
        {
            InventoryItem chosen = null;

            foreach ( InventoryItem spot in docked )
            {
                if ( spot.Docked == null )
                {
                    Vector3 dockablePos = forDockable.GetGameObject().transform.position;
                    if ( chosen == null || (
                            chosen != null && Vector3.Distance( chosen.Docker.transform.position, dockablePos ) >
                            Vector3.Distance( spot.Docker.transform.position, dockablePos )
                        ) )
                    {
                        chosen = spot;
                    }
                }
            }

            return chosen;
        }

        InventoryItem GetInventorySpotForDockable(IDockable dockable)
        {
            foreach ( InventoryItem spot in docked )
            {
                if ( spot.Docked == dockable )
                {
                    return spot;
                }
            }

            return null;
        }

        public void OnTriggerStay(Collider other)
        {
            IDockable dockable = other.gameObject.GetComponentInParent<IDockable>();
            if ( dockable != null )
            {
                //Debug.Log( "Entering inventory " + dockable.GetGameObject().name );
                dockable.CloseToInventory = true;
            }
        }

        public void OnTriggerExit(Collider other)
        {
            IDockable dockable = other.gameObject.GetComponentInParent<IDockable>();
            if ( dockable != null )
            {
                //Debug.Log( "Exiting inventory for " + dockable.GetGameObject().name );
                dockable.CloseToInventory = false;
            }
        }

        void Position()
        {
            transform.localPosition = new Vector3(
                Camera.main.transform.localPosition.x,
                Camera.main.transform.localPosition.y - .6f,
                Camera.main.transform.localPosition.z
            );

            move = true;
        }

        void Reposition()
        {
            // Only if camera moved locally more than 50cm 
            if ( Vector3.Distance( transform.localPosition, new Vector3(
                    Camera.main.transform.localPosition.x,
                    transform.localPosition.y,
                    Camera.main.transform.localPosition.z
                ) ) > .5f )
            {
                Hashtable moveTo = new();

                moveTo.Add( "position", new Vector3(
                    Camera.main.transform.localPosition.x,
                    Camera.main.transform.localPosition.y - .6f,
                    Camera.main.transform.localPosition.z
                ) );
                moveTo.Add( "islocal", true );
                moveTo.Add( "time", 1.5f );
                moveTo.Add( "easetype", iTween.EaseType.easeOutCubic );

                iTween.MoveTo( gameObject, moveTo );

                moveTime = -2f;
            }
        }

        void ScaleDockedItem(IDockable docked, Vector3 to)
        {
        }

        void Start()
        {
            CreateSpots();
            Invoke( "Position", .5f );
        }

        public void UnDock(IDockable dockable)
        {
            /* -    Unparent
             * -    Reset Scale, animated
             */

            InventoryItem item = GetInventorySpotForDockable( dockable );

            if ( docked != null )
            {
                GameObject docked = dockable.GetGameObject();
                Vector3 currentPos = docked.transform.position;
                docked.transform.parent = null; // right? Or does the grabbing do that?
                docked.transform.position = currentPos;
                dockable.Docked = false;
                item.Docked = null;
                ScaleDockedItem( dockable, dockable.GetGameObject().transform.localScale / ScaleFactor );
            }
        }

        void Update()
        {
            if ( move )
            {
                moveTime += Time.deltaTime;

                if ( moveTime >= 0 )
                {
                    Reposition();
                }
            }
        }
    }
}