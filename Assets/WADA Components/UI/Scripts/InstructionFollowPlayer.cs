using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Wada
{
    public class InstructionFollowPlayer : MonoBehaviour
    {
        static InstructionFollowPlayer instance;

        public List<GameObject> Instructions;

        public bool AllowRotation = false;

        float rotateThreshTime = 2f;
        float rotateTime;

        const float ROTATE_START_THRESHOLD = 58;
        const float ROTATE_END_THRESHOLD = 5;
        bool rotating = false;
        const float MAX_SPEED = 1.3f;

        const float TOTAL_DIFF = ROTATE_END_THRESHOLD - ROTATE_START_THRESHOLD;
        const float HALF_DIFF = TOTAL_DIFF / 2;

        float targetY;
        float idleTime;
        const float MAX_IDLE_TIME = .1f;
        const float MAX_IDLE_RANGE = 10;

        void Awake()
        {
            if ( instance != null && instance != this )
            {
                Debug.LogError( "[InstructionFollowPlayer] Already initiated!" );
            }

            instance = this;

            rotateTime = rotateThreshTime;
        }

        float EaseInOutQuad(float start, float end, float value)
        {
            value /= .5f;
            end -= start;
            if ( value < 1 )
            {
                return end * 0.5f * value * value + start;
            }

            value--;
            return -end * 0.5f * (value * (value - 2) - 1) + start;
        }

        float GetPlayerAngleDiff()
        {
            float angleInRangeCam = Mathf.Repeat( Camera.main.transform.eulerAngles.y + 180, 360 ) - 180;
            float angleInRange = Mathf.Repeat( transform.eulerAngles.y + 180, 360 ) - 180;

            return MathF.Abs( angleInRangeCam - angleInRange );
        }

        float GetTargetAngleDiff()
        {
            float angleInRangeCam = Mathf.Repeat( Camera.main.transform.eulerAngles.y + 180, 360 ) - 180;
            float angleInRange = Mathf.Repeat( targetY + 180, 360 ) - 180;

            return MathF.Abs( angleInRangeCam - angleInRange );
        }

        public void HideInstructions()
        {
            CancelInvoke( "HideInstructions" );
            AllowRotation = false;

            foreach ( GameObject i in Instructions )
            {
                i.SetActive( false );
            }
        }

        public static InstructionFollowPlayer GetInstance()
        {
            return instance;
        }

        void OnRotated()
        {
            rotating = false;
        }

        void Rotate()
        {
            targetY = Camera.main.transform.eulerAngles.y;

            iTween.Stop( gameObject );

            Hashtable rotateParams = new();

            rotateParams.Add( "y", targetY );
            rotateParams.Add( "time", GetPlayerAngleDiff() / 50 );
            rotateParams.Add( "oncomplete", "OnRotated" );
            rotateParams.Add( "easetype", iTween.EaseType.easeInOutQuart );

            iTween.RotateTo( gameObject, rotateParams );
        }


        /**
         * @param hideAfter Float, either > 0 or 0 for indefinite
         */
        public void ShowInstruction(GameObject instruction, bool hideOthers = true, float hideAfter = 15f)
        {
            AlignToPlayer();

            foreach ( GameObject i in Instructions )
            {
                if ( hideOthers )
                {
                    i.SetActive( i == instruction );
                }
                else if ( i == instruction )
                {
                    i.SetActive( true );
                }
            }

            AllowRotation = true;

            if ( hideAfter > 0 )
            {
                Invoke( "HideInstructions", hideAfter );
            }
        }

        void AlignToPlayer()
        {
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                Camera.main.transform.eulerAngles.y,
                transform.eulerAngles.z
            );

            targetY = Mathf.Repeat( transform.eulerAngles.y + 180, 360 ) - 180;

            Vector3 playerCam = PlayerController.GetInstance().GetLocation();

            transform.position = new Vector3(
                playerCam.x,
                Mathf.Max( 1, playerCam.y ),
                playerCam.z
            );
        }

        void Start()
        {
            AlignToPlayer();
        }

        void Update()
        {
            if ( AllowRotation )
            {
                if ( GetPlayerAngleDiff() > ROTATE_START_THRESHOLD && GetTargetAngleDiff() >
                    MAX_IDLE_RANGE )
                {
                    Rotate();
                }
            }
        }
    }
}