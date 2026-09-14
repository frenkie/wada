using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using Oculus.Interaction;
using UnityEditor;
using UnityEngine.AI;
#endif

namespace Wada
{
    public class AnimalConfigurator : MonoBehaviour
    {
        public string AnimalName;
        public Ressurectable Animal;
        public AnimalType Type;
        public bool AttractsAttention = false;
        public AnimalEmbodiment Embodiment = AnimalEmbodiment.None;

        [Space( 10 )] [Header( "Containers" )] public GameObject AnimationController;
        public GameObject FlockerModel;
        public GameObject TouchCollider;

        [Space( 10 )] [Header( "Main Body Containers" )]
        public List<GameObject> BodySupport;

        public List<GameObject> BodyParts;


        [Space( 10 )] [Header( "Limb Configuration" )]
        public List<AnimalLimbConfiguration> Limbs;

        void Start()
        {
        }


        GameObject AddBounds(GameObject to)
        {
            GameObject bounds = new( "Bounds" );
            bounds.transform.parent = to.transform;
            bounds.transform.localPosition = Vector3.zero;
            BoxCollider boundingBox = bounds.AddComponent<BoxCollider>();
            boundingBox.isTrigger = true;

            return bounds;
        }

        BoxCollider AddCollider(GameObject to, PhysicMaterial physicMaterial)
        {
            GameObject sup = GameObject.CreatePrimitive( PrimitiveType.Cube );

            sup.name = to.name + "_Collider";
            sup.transform.parent = to.transform;
            sup.transform.localPosition = Vector3.zero;
            sup.layer = LayerMask.NameToLayer( "Animal" );

            BoxCollider box = sup.GetComponent<BoxCollider>();
            box.material = physicMaterial;

            sup.transform.localScale = Vector3.one * (1f / to.transform.localScale.magnitude);

            return box;
        }

        GameObject AddReattachment(GameObject to)
        {
            GameObject sup = new( "Reattachment Collider" );
            sup.transform.parent = to.transform;
            sup.transform.localPosition = Vector3.zero;
            sup.layer = LayerMask.NameToLayer( "ColliderForSockets" );

            SphereCollider sphere = sup.AddComponent<SphereCollider>();
            sphere.radius = .05f;

            Rigidbody rb = sup.AddComponent<Rigidbody>();
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.constraints = RigidbodyConstraints.FreezeAll;

            return sup;
        }

        LimbSocket AddSocket(string name, Transform container, Transform parent, Rigidbody animalRigidbody,
            LimbTypeMask allowedLimbs)
        {
            GameObject socket = new( "Socket " + name );
            socket.transform.parent = container;
            socket.transform.localPosition = Vector3.zero;
            socket.layer = LayerMask.NameToLayer( "ColliderForSockets" );

            GameObject visual = GameObject.CreatePrimitive( PrimitiveType.Sphere );
            DestroyImmediate( visual.GetComponent<SphereCollider>() );
            visual.name = "Sphere";
            visual.transform.parent = socket.transform;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one * .05f;

            SphereCollider collider = socket.AddComponent<SphereCollider>();
            collider.isTrigger = true;
            collider.radius = .66f;

            LimbSocket limbSocket = socket.AddComponent<LimbSocket>();
            limbSocket.JointRigidbody = animalRigidbody;
            limbSocket.ProximityRenderer = visual.GetComponent<MeshRenderer>();
            limbSocket.Parent = parent;
            limbSocket.AllowLimbs = allowedLimbs;

            return limbSocket;
        }

        GameObject AddSupport(GameObject to, PhysicMaterial physicMaterial)
        {
            GameObject sup = new( "Support" );
            sup.transform.parent = to.transform;
            sup.transform.localPosition = Vector3.zero;
            sup.layer = LayerMask.NameToLayer( "Animal" );

            BoxCollider box = sup.AddComponent<BoxCollider>();
            box.material = physicMaterial;

            return sup;
        }


#if UNITY_EDITOR
        public string GetAnimalAssetPath()
        {
            MonoScript script = MonoScript.FromMonoBehaviour( Animal );
            return AssetDatabase.GetAssetPath( script ).Replace( "/" + script.name + ".cs", "" );
        }

        public PhysicMaterial GetRoughMaterial()
        {
            return AssetDatabase.LoadAssetAtPath<PhysicMaterial>(
                "Assets/WADA Components/Animal Controllers/Materials/Rough.physicMaterial" );
        }

        public void ConfigureAnimationAssets()
        {
            string animalScriptPath = GetAnimalAssetPath();
            string controllerToCopy = "";
            string controllerAfterCopy = animalScriptPath + "/" + AnimalName + ".controller";
            switch ( Type )
            {
                case AnimalType.Walker:
                    // if ( AttractsAttention )
                    // {
                    controllerToCopy = "Assets/WADA Components/Animal Fox/Fox.controller";
                    // }
                    // else
                    // {
                    //     controllerToCopy = "Assets/WADA Components/Animal Mole/Animations/Mole.controller";
                    // }

                    break;

                case AnimalType.Flyer:
                    // if ( AttractsAttention )
                    // {
                    //     controllerToCopy = "Assets/WADA Components/Animal Magpie/Animations/Magpie.controller";
                    // }
                    // else
                    // {
                    controllerToCopy = "Assets/WADA Components/Animal Pigeon/Animations/Pigeon.controller";
                    //}

                    break;
            }

            if ( controllerToCopy != "" )
            {
                if ( AssetDatabase.CopyAsset( controllerToCopy,
                        controllerAfterCopy ) )
                {
                    CreateAnimationController();
                    Debug.Log( "Done Creating Animation assets" );
                }
                else
                {
                    Debug.LogError( "Cant copy controller" );
                }
            }
        }

        public void ConfigureAnimalController()
        {
            PhysicMaterial rough = GetRoughMaterial();

            bool first = true;
            List<Collider> colliders = new();
            BoxCollider boundingBox = null;
            foreach ( GameObject part in BodyParts )
            {
                part.layer = LayerMask.NameToLayer( "Animal" );
                BoxCollider collider = AddCollider( part, rough );
                colliders.Add( collider );

                if ( first )
                {
                    first = false;
                    GameObject bounds = AddBounds( part );
                    boundingBox = bounds.GetComponent<BoxCollider>();
                }
            }

            GameObject[] supporters = new GameObject[BodySupport.Count];
            int supportIdx = 0;
            foreach ( GameObject support in BodySupport )
            {
                GameObject sup = new( "Body" );
                sup.transform.parent = support.transform;
                sup.transform.localPosition = Vector3.zero;
                sup.layer = LayerMask.NameToLayer( "Animal" );
                supporters[supportIdx] = sup;
                supportIdx++;

                BoxCollider box = sup.AddComponent<BoxCollider>();
                box.material = rough;
            }

            Grabbable grabbable = gameObject.AddComponent<Grabbable>();
            grabbable.enabled = false;

            PhysicsGrabbable grabbablePhysics = gameObject.AddComponent<PhysicsGrabbable>();
            grabbablePhysics.enabled = false;

            WadaTouchHandGrabInteractable grabInteractable = gameObject.AddComponent<WadaTouchHandGrabInteractable>();
            grabInteractable.enabled = false;
            grabInteractable.InjectOptionalPointableElement( grabbable );
            grabInteractable.InjectColliders( colliders );
            if ( boundingBox != null )
            {
                grabInteractable.InjectAllTouchHandGrabInteractable( boundingBox, colliders );
            }
            else
            {
                grabInteractable.InjectColliders( colliders );
            }

            AnimalController controller = gameObject.AddComponent<AnimalController>();

            controller.DefaultGodModeMovent = Type == AnimalType.Walker ? Animations.Walk : Animations.Fly;
            controller.SubBodyParts = BodyParts;
            controller.Support = supporters;

            Debug.Log( "Done configuring animal controller" );
        }

        public void ConfigureResurrection()
        {
            Rigidbody rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = 5;
            rb.useGravity = false;
            rb.isKinematic = true;
            gameObject.layer = LayerMask.NameToLayer( "Animal" );

            if ( AttractsAttention )
            {
                SphereCollider attraction = gameObject.AddComponent<SphereCollider>();
                attraction.isTrigger = true;
            }

            NavMeshAgent agent = gameObject.AddComponent<NavMeshAgent>();
            agent.enabled = false;

            if ( Type == AnimalType.Flyer )
            {
                Animal.WalkAfterRessurection = false;

                FlockChild flockChild = gameObject.AddComponent<FlockChild>();
                flockChild._model = FlockerModel;
                flockChild._modelT = FlockerModel.transform;
                flockChild._thisT = transform;
                flockChild.AlignWithWaypointFromStart = false;
                flockChild.SetRandomScaleAtStart = false;
                flockChild._speed = 1.0f;
            }

            GameObject toucher = new( "Collidert Touch" );
            toucher.transform.parent = TouchCollider.transform;
            toucher.transform.localPosition = Vector3.zero;
            toucher.layer = LayerMask.NameToLayer( "Animal" );
            toucher.tag = "PassOnTouch";

            Rigidbody touchRb = toucher.AddComponent<Rigidbody>();
            touchRb.useGravity = false;
            touchRb.isKinematic = true;

            CapsuleCollider touchCaps = toucher.AddComponent<CapsuleCollider>();
            touchCaps.isTrigger = true;

            PassOnTouch passOnTouch = toucher.AddComponent<PassOnTouch>();
            passOnTouch.TouchReceiver = gameObject;

            InfoPlaqueData infoData = ScriptableObject.CreateInstance<InfoPlaqueData>();
            infoData.name = AnimalName;
            AssetDatabase.CreateAsset( infoData, GetAnimalAssetPath() + "/InfoData.asset" );

            Animal.AnimalInfo = infoData;

            Debug.Log( "Done configuring resurrection" );
        }

        void CreateAnimationController()
        {
            Animator animator = AnimationController.AddComponent<Animator>();
            BasicNavMeshNPCControllerOnAnimatorPassThrough passThrough =
                AnimationController.AddComponent<BasicNavMeshNPCControllerOnAnimatorPassThrough>();

            animator.runtimeAnimatorController =
                AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>( GetAnimalAssetPath() + "/" + AnimalName +
                                                                          ".controller" );

            passThrough.Controller = Animal;

            EditorUtility.SetDirty( AnimationController );
        }

        public void ConfigureLimbs()
        {
            PhysicMaterial rough = GetRoughMaterial();
            Rigidbody animalRigidbody = gameObject.GetComponent<Rigidbody>();

            foreach ( AnimalLimbConfiguration limb in Limbs )
            {
                // Allow Empty sockets
                LimbSocket socket = AddSocket( limb.Name, limb.SocketParent.transform, AnimationController.transform,
                    animalRigidbody, limb.AllowedLimbsForSocket );

                if ( limb.BodyParts is { Count: > 0 } )
                {
                    List<Collider> colliders = new();
                    foreach ( GameObject part in limb.BodyParts )
                    {
                        part.layer = LayerMask.NameToLayer( "Animal" );
                        BoxCollider collider = AddCollider( part, rough );
                        colliders.Add( collider );
                    }

                    GameObject limbRoot = limb.BodyParts.First();

                    Rigidbody limbRigidbody = limbRoot.AddComponent<Rigidbody>();
                    limbRigidbody.useGravity = true;
                    limbRigidbody.isKinematic = true;

                    CharacterJoint joint = limbRoot.AddComponent<CharacterJoint>();
                    joint.autoConfigureConnectedAnchor = false;

                    GameObject bounds = AddBounds( limbRoot );
                    MakeGrabbable( limbRoot, colliders, bounds.GetComponent<BoxCollider>() );


                    // Add limb script here, because it requires the above components and would otherwise generate them
                    Limb limbScript = limbRoot.AddComponent<Limb>();
                    limbScript.Type = limb.Type;
                    limbScript.EmbodimentType = Embodiment;
                    limbScript.GodmodeMultiplier = 9;
                    limbScript.DefaultGodModeMovent = Type == AnimalType.Walker ? Animations.Walk : Animations.Fly;

                    limbScript.Socket = socket;
                    socket.ConnectedLimb = limbScript;

                    GameObject reattach = AddReattachment( limbRoot );
                    limbScript.ReattachCollider = reattach;
                    limbScript.SubBodyParts =
                        limb.BodyParts.FindAll( o => o != limbRoot ); // should be in the right order TODO doublecheck

                    if ( limb.FootSupport )
                    {
                        limbScript.FootSupport = AddSupport( limb.BodyParts.Last(), rough );
                    }
                    else if ( limb.FlySupport )
                    {
                        limbScript.FlySupport = AddSupport( limb.BodyParts.Last(), rough );
                    }
                }
            }

            Debug.Log( "Done configuring limbs" );
        }

        void MakeGrabbable(GameObject grab, List<Collider> colliders, Collider bounds)
        {
            Grabbable grabbable = grab.AddComponent<Grabbable>();
            grabbable.enabled = false;

            PhysicsGrabbable grabbablePhysics = grab.AddComponent<PhysicsGrabbable>();
            grabbablePhysics.enabled = false;

            WadaTouchHandGrabInteractable grabInteractable = grab.AddComponent<WadaTouchHandGrabInteractable>();
            grabInteractable.enabled = false;
            grabInteractable.InjectOptionalPointableElement( grabbable );
            grabInteractable.InjectColliders( colliders );
            if ( bounds != null )
            {
                grabInteractable.InjectAllTouchHandGrabInteractable( bounds, colliders );
            }
            else
            {
                grabInteractable.InjectColliders( colliders );
            }
        }
#endif

        public void DisableSimpleTestColliders()
        {
            foreach ( BoxCollider collide in GetComponentsInChildren<BoxCollider>() )
            {
                if ( collide.name.Contains( "_Collider" ) )
                {
                    collide.enabled = false;
                    DestroyImmediate( collide.GetComponent<MeshRenderer>() );
                    DestroyImmediate( collide.GetComponent<MeshFilter>() );
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(AnimalConfigurator) )]
    public class AnimalConfiguratorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AnimalConfigurator myTarget = (AnimalConfigurator)target;

            DrawDefaultInspector();

            if ( myTarget.AnimalName != "" && myTarget.Animal != null )
            {
                if ( GUILayout.Button( "Configure Animations" ) )
                {
                    myTarget.ConfigureAnimationAssets();
                }

                if ( GUILayout.Button( "Configure Resurrection" ) )
                {
                    myTarget.ConfigureResurrection();
                }

                if ( GUILayout.Button( "Configure Animal Controller" ) )
                {
                    myTarget.ConfigureAnimalController();
                }

                if ( GUILayout.Button( "Configure Limbs" ) )
                {
                    myTarget.ConfigureLimbs();
                }

                if ( GUILayout.Button( "Cleanup Collider Visuals" ) )
                {
                    myTarget.DisableSimpleTestColliders();
                }
            }
        }
    }
#endif
}