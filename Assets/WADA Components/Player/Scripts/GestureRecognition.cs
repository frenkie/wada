using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public enum GestureHandAxis
    {
        down,
        backward,
        forward,
        left,
        right,
        up
    }

    /**
     *
     */
    [RequireComponent( typeof(SoundEffect) )]
    public class GestureRecognition : MonoBehaviour
    {
        public float PoseTimeThreshold = 3;
        public bool MatchLeftHand = false;
        public bool MatchRightHand = false;
        public bool Active = true;
        public bool KeepHandsClose = false;
        public float Vicinity = .12f;
        public float IconTopOffset = .25f;

        public GestureHandAxis RightHandForwardAxis;

        public CanvasGroup Canvas;
        public Image ProgressImage;

        Vector3 targetPos;

        [SerializeField] public UnityEvent whenMatched;

        [SerializeField] UnityEvent whenUnmatched;

        public UnityEvent WhenMatched => whenMatched;
        public UnityEvent WhenUnmatched => whenUnmatched;

        const float VICINITY_HANDS = .12f;
        bool leftHandMatch = false;
        bool rightHandMatch = false;
        float poseTime;

        bool updatePosition = false;

        SoundEffect soundEffect;
        bool matchedIn = false; // a toggle for sounds

        Vector3 velocity = Vector3.zero;

        public void Activate()
        {
            Active = true;
        }

        public bool AreHandsCloseEnough()
        {
            if ( KeepHandsClose )
            {
                float distance = Vector3.Distance(
                    PlayerController.GetInstance().GetLocomotion().LeftHand.transform.position,
                    PlayerController.GetInstance().GetLocomotion().RightHand.transform.position
                );
                return distance <= Vicinity;
            }

            return true;
        }

        void Awake()
        {
            Canvas.alpha = 0;
            targetPos = transform.position;
        }

        public void Deactivate()
        {
            Active = false;
        }

        Vector3 GetLoaderPosition()
        {
            return Vector3.Lerp(
                PlayerController.GetInstance().GetLocomotion().LeftHand.transform.position,
                PlayerController.GetInstance().GetLocomotion().RightHand.transform.position,
                .5f
            );
        }


        Vector3 GetLoaderForward()
        {
            WadaHand right = PlayerController.GetInstance().GetLocomotion().RightHand;
            WadaHand left = PlayerController.GetInstance().GetLocomotion().LeftHand;

            Vector3 total = Vector3.zero;

            switch ( RightHandForwardAxis )
            {
                case GestureHandAxis.backward:
                    total += -right.transform.forward;
                    total += left.transform.forward;
                    break;

                case GestureHandAxis.forward:
                    total += right.transform.forward;
                    total += -left.transform.forward;
                    break;

                case GestureHandAxis.right:
                    total += right.transform.right;
                    total += -left.transform.right;
                    break;

                case GestureHandAxis.left:
                    total += -right.transform.right;
                    total += left.transform.right;
                    break;

                case GestureHandAxis.up:
                    total += right.transform.up;
                    total += -left.transform.up;
                    break;

                case GestureHandAxis.down:
                    total += -right.transform.up;
                    total += left.transform.up;
                    break;
            }

            return total / 2;
        }


        void LateUpdate()
        {
            /*
             * Position the canvas above the hand location
             *
             * Slowly follow the hands, but speedy enough
             *
             * Rotate to camera plane
             */
            Vector3 middle = GetLoaderPosition();
            middle.y += IconTopOffset;

            if ( Active && Vector3.Distance( middle, targetPos ) > .05f )
            {
                targetPos = middle;
                Canvas.transform.localPosition = GetLoaderForward() * .25f;
            }

            if ( Active )
            {
                transform.position =
                    Vector3.SmoothDamp( transform.position, targetPos, ref velocity, .3f );

                Vector3 target = PlayerController.GetInstance().GetLocation(); // is camera
                target.y = Canvas.transform.position.y;
                Vector3 lookAtT = target -
                                  Canvas.transform.position;


                if ( lookAtT != Vector3.zero )
                {
                    Canvas.transform.eulerAngles = new Vector3(
                        Canvas.transform.eulerAngles.x,
                        Quaternion.LookRotation( lookAtT ).eulerAngles.y,
                        Canvas.transform.eulerAngles.z
                    );
                }
            }
        }

        public void MatchedLeftHand()
        {
            leftHandMatch = true;
            // Debug.Log( "[GestureRecognition] matched left hand" );
        }

        public void MatchedRightHand()
        {
            rightHandMatch = true;
            // Debug.Log( "[GestureRecognition] matched right hand" );
        }

        void Start()
        {
            soundEffect = GetComponent<SoundEffect>();
        }

        public void ToggleActivation()
        {
            if ( Active )
            {
                Deactivate();
            }
            else
            {
                Activate();
            }
        }

        void ToggleState()
        {
            matchedIn = !matchedIn;
        }

        public void UnmatchedLeftHand()
        {
            leftHandMatch = false;
            // Debug.Log( "[GestureRecognition] unmatched left hand" );
        }

        public void UnmatchedRightHand()
        {
            rightHandMatch = false;
            // Debug.Log( "[GestureRecognition] unmatched right hand" );
        }


        void Update()
        {
            if ( Active )
            {
                if ( (MatchLeftHand && MatchRightHand && leftHandMatch && rightHandMatch && AreHandsCloseEnough()) ||
                     (MatchLeftHand && !MatchRightHand && leftHandMatch) ||
                     (MatchRightHand && !MatchLeftHand && rightHandMatch)
                   )
                {
                    if ( poseTime <= 0 && poseTime + Time.deltaTime > 0 )
                    {
                        transform.position = GetLoaderPosition(); // get close to the new target
                        soundEffect.StopFading();
                        soundEffect.SetVolume( 1 );
                        soundEffect.Play( matchedIn ? 1 : 0 );
                    }

                    poseTime += Time.deltaTime;

                    UpdateProgress();

                    if ( poseTime >= PoseTimeThreshold )
                    {
                        // Debug.Log( "[GestureRecognition] recognized long enough" );
                        whenMatched.Invoke();
                        ToggleState();
                        poseTime = -2f; // to not activate it immediately again
                        UpdateProgress();
                    }
                }
                else
                {
                    if ( poseTime > .1f * PoseTimeThreshold )
                    {
                        whenUnmatched.Invoke();
                        soundEffect.FadeOut();
                    }

                    poseTime = 0;
                    UpdateProgress();
                }
            }
        }

        void UpdateProgress()
        {
            if ( poseTime >= .1f * PoseTimeThreshold )
            {
                Canvas.alpha = 1;
                ProgressImage.fillAmount = MathF.Min( poseTime / PoseTimeThreshold, 1 );
            }
            else
            {
                ProgressImage.fillAmount = 0;
                Canvas.alpha = 0;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor( typeof(GestureRecognition) )]
    public class GestureRecognitionEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            GestureRecognition myTarget = (GestureRecognition)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Test" ) )
            {
                myTarget.whenMatched.Invoke();
            }
        }
    }
#endif
}