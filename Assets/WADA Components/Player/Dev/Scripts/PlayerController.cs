using System;
using System.Collections;
using UnityEngine;
using Oculus.Interaction;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class PlayerController : MonoBehaviour
    {
        static PlayerController instance;

        public GameObject FloorCenter;
        public FocusCone Focus;
        public Transform PlayerOrienter;
        public InventorySaveTarget InventorySaveTarget;
        public bool DebugPassthrough = false;

        OVRPassthroughLayer passthroughLayer;
        Rigidbody rigidbody;
        PlayerLocomotion locomotion;

        InteractorGroup leftHandInteractorGroup;
        InteractorGroup rightHandInteractorGroup;

        bool determineEyeHeight = false;
        float eyeHeightTimer;
        const float MAX_EYE_HEIGHT_TIMER = .5f;
        const float MAX_EMBODIMENT_TIME = 30f;
        float eyeHeight;

        Vector2 centerOffset;

        AnimalEmbodiment activeEmbodiment = AnimalEmbodiment.Normal;

        void Awake()
        {
            if ( instance != null && instance != this )
            {
                Debug.LogError( "PlayerController already instantiated" );
            }

            instance = this;
            passthroughLayer = GetComponent<OVRPassthroughLayer>();
            if ( !Application.isEditor || DebugPassthrough )
            {
                passthroughLayer.enabled = true;
            }

            rigidbody = GetComponent<Rigidbody>();
            locomotion = GetComponent<PlayerLocomotion>();

            leftHandInteractorGroup = locomotion.LeftHand.InteractorGroup;
            rightHandInteractorGroup = locomotion.RightHand.InteractorGroup;
        }

        public void AddAnimalToWorkshop(AnimalController animal)
        {
            Workshop.GetInstance().AddAnimal( animal );
        }

        public void AdjustPassthroughOpacity(float opacity)
        {
            if ( passthroughLayer != null && passthroughLayer.enabled && passthroughLayer.textureOpacity != opacity )
            {
                passthroughLayer.textureOpacity = opacity;
            }
        }

        // Needed for use in Signals
        public void AlignToTarget(Transform targetPos)
        {
            AlignToTarget( targetPos, true );
        }

        public void AlignToTarget(Transform targetPos, bool includeRotation)
        {
            if ( includeRotation )
            {
                Vector3 offsetAngle = Camera.main.transform.rotation.eulerAngles - PlayerOrienter.rotation.eulerAngles;
                offsetAngle.x = 0;
                offsetAngle.z = 0;

                Vector3 targetRot = targetPos.rotation.eulerAngles;
                targetRot.z = 0;

                PlayerOrienter.rotation = Quaternion.Euler( targetRot - offsetAngle );
            }

            Vector3 offsetPos = Camera.main.transform.position - PlayerOrienter.position;
            PlayerOrienter.position = targetPos.position - offsetPos;
        }

        public void AlignToTarget(Vector3 targetPos)
        {
            Vector3 offsetPos = Camera.main.transform.position - PlayerOrienter.position;
            PlayerOrienter.position = targetPos - offsetPos;
        }


        public void AlignToFloorTarget(Transform targetPos)
        {
            AlignToFloorTarget( targetPos, true );
        }

        public void AlignToFloorTarget(Transform targetPos, bool includeRotation)
        {
            if ( includeRotation )
            {
                Vector3 offsetAngle = Camera.main.transform.rotation.eulerAngles - transform.rotation.eulerAngles;
                offsetAngle.x = 0;
                offsetAngle.z = 0;

                Vector3 targetRot = targetPos.rotation.eulerAngles;
                targetRot.z = 0;

                transform.rotation = Quaternion.Euler( targetRot - offsetAngle );
            }

            Vector3 offsetPos = new Vector3(
                Camera.main.transform.position.x,
                transform.position.y,
                Camera.main.transform.position.z
            ) - transform.position;
            transform.position = targetPos.position - offsetPos;
        }

        public void AlignToCenterFloorTarget(Transform targetPos)
        {
            AlignToCenterFloorTarget( targetPos, true );
        }

        public void AlignToCenterFloorTarget(Transform targetPos, bool includeRotation)
        {
            if ( includeRotation )
            {
                Vector3 offsetAngle = Camera.main.transform.rotation.eulerAngles - transform.rotation.eulerAngles;
                offsetAngle.x = 0;
                offsetAngle.z = 0;

                Vector3 targetRot = targetPos.rotation.eulerAngles;
                targetRot.z = 0;

                transform.rotation = Quaternion.Euler( targetRot - offsetAngle );
            }

            Vector3 centerWorldOffset = GetCenterWorldOffset();
            Debug.Log( centerWorldOffset );

            if ( centerWorldOffset.magnitude > 0 )
            {
                centerWorldOffset.y = 0;
                transform.position = targetPos.position - centerWorldOffset;
            }
            else
            {
                transform.position = targetPos.position;
            }
        }

        void DetermineEyeHeight()
        {
            eyeHeightTimer = 0;
            determineEyeHeight = true;
        }

        void DeterminePivotCenter()
        {
            centerOffset = new Vector2(
                Camera.main.transform.localPosition.x,
                Camera.main.transform.localPosition.z
            );

            FloorCenter.transform.position = transform.TransformPoint( new Vector3(
                centerOffset.x,
                0,
                centerOffset.y
            ) );
        }

        public void DisableHandGrabsMomentarily()
        {
            Debug.Log( "[Player] DisableHandGrabsMomentarily" );
            locomotion.LeftHand.Interactor.DisableHandsMomentarily = true;
            locomotion.RightHand.Interactor.DisableHandsMomentarily = true;
            leftHandInteractorGroup.Disable();
            rightHandInteractorGroup.Disable();
        }

        public void EnableHandGrabs()
        {
            Debug.Log( "[Player] EnableHandGrabs" );
            leftHandInteractorGroup.Enable();
            rightHandInteractorGroup.Enable();
            locomotion.LeftHand.Interactor.DisableHandsMomentarily = false;
            locomotion.RightHand.Interactor.DisableHandsMomentarily = false;
            leftHandInteractorGroup.Unhover();
            rightHandInteractorGroup.Unhover();
        }

        public void EmbodyAnimal(AnimalEmbodiment newEmbodiment)
        {
            Debug.Log( "[Player] EmbodyAnimal" );

            // TODO for now the hard way, use loop through enum names
            if ( newEmbodiment != activeEmbodiment )
            {
                activeEmbodiment = newEmbodiment;
                GetLocomotion().LeftHand.SetActiveEmbodiment( newEmbodiment );
                GetLocomotion().RightHand.SetActiveEmbodiment( newEmbodiment );
            }

            CancelInvoke( "ReturnToNormalEmbodiment" );
            Invoke( "ReturnToNormalEmbodiment", MAX_EMBODIMENT_TIME );
        }

        public void EnterFocusMode()
        {
            locomotion.EnterFocusMode();
        }

        public void ExitFocusMode()
        {
            locomotion.ExitFocusMode();
        }

        public void EnterWorkshopMode()
        {
            locomotion.EnterWorkshopMode();
        }

        public void ExitWorkshopMode()
        {
            locomotion.ExitWorkshopMode();
        }

        public Vector3 GetCameraOffset()
        {
            return transform.InverseTransformPoint( Camera.main.transform.position );
        }

        /**
         * Is local, based on the correct forward of the
         */
        public Vector2 GetCenterOffset()
        {
            return centerOffset;
        }

        public Vector3 GetCenterWorldOffset()
        {
            return FloorCenter.transform.position - transform.position;
        }

        public Vector3 GetCenterPosition()
        {
            return FloorCenter.transform.position;
        }

        public static PlayerController GetInstance()
        {
            return instance;
        }

        public float GetEyeHeight()
        {
            if ( eyeHeight == 0 )
            {
                DetermineEyeHeight();
            }

            return eyeHeight;
        }

        public WadaHand GetGrabbingHandForInteractable(
            PointerInteractable<WadaTouchHandGrabInteractor, WadaTouchHandGrabInteractable> interactable)
        {
            if ( interactable.HasSelectingInteractor( locomotion.LeftHand.Interactor ) )
            {
                return locomotion.LeftHand;
            }
            else if ( interactable.HasSelectingInteractor( locomotion.RightHand.Interactor ) )
            {
                return locomotion.RightHand;
            }
            else
            {
                return null;
            }
        }

        public PlayerLocomotion GetLocomotion()
        {
            return locomotion;
        }

        public Vector3 GetLocation()
        {
            return Camera.main.transform.position;
        }

        public Rigidbody GetRigidbody()
        {
            return rigidbody;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        public void HidePasshtrough()
        {
            passthroughLayer.enabled = false;
        }

        public void ShowPassthrough()
        {
            if ( !Application.isEditor || DebugPassthrough )
            {
                passthroughLayer.enabled = true;
            }
        }

        public void ReturnToNormalEmbodiment()
        {
            Debug.Log( "[Player] Returning to normal embodiment" );
            activeEmbodiment = AnimalEmbodiment.Normal;

            GetLocomotion().LeftHand.SetActiveEmbodiment( activeEmbodiment );
            GetLocomotion().RightHand.SetActiveEmbodiment( activeEmbodiment );
        }

        public void Start()
        {
            Invoke( "DeterminePivotCenter", 1f ); // That should be save
            Invoke( "DetermineEyeHeight", 1.5f ); // That should be save
        }

        public void TestAllEmbodiments()
        {
            StartCoroutine( TestEmbodiments() );
        }

        IEnumerator TestEmbodiments()
        {
            string[] animalTypes = Enum.GetNames( typeof(AnimalEmbodiment) );

            for ( int i = 0; i < animalTypes.Length; i++ )
            {
                if ( animalTypes[i] != AnimalEmbodiment.Normal.ToString() )
                {
                    EmbodyAnimal( (AnimalEmbodiment)Enum.Parse( typeof(AnimalEmbodiment), animalTypes[i] ) );
                    yield return new WaitForSeconds( 7 );
                }
                else
                {
                    yield return new WaitForEndOfFrame();
                }
            }
        }

        void Update()
        {
            if ( determineEyeHeight )
            {
                // making sure Trackingspace can never have an influence 
                eyeHeight = MathF.Max( eyeHeight, Camera.main.transform.position.y - transform.position.y );
                eyeHeightTimer += Time.deltaTime;
                if ( eyeHeightTimer >= MAX_EYE_HEIGHT_TIMER )
                {
                    determineEyeHeight = false;
                }
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(PlayerController) )]
    public class PlayerControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            PlayerController myTarget = (PlayerController)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Test Embodiments" ) )
            {
                myTarget.TestAllEmbodiments();
            }
        }
    }
#endif
}