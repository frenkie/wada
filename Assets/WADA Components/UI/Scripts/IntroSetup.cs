using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Wada
{
    public class IntroSetup : MonoBehaviour
    {
        public SkyboxAdjuster Skybox;
        public float ShowDuration = 5f;
        public bool AllowRotation = true;

        float rotateThreshTime = 2f;
        float rotateTime;

        const float ROTATE_START_THRESHOLD = 25;
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
            rotateTime = rotateThreshTime;
            targetY = Mathf.Repeat( transform.eulerAngles.y + 180, 360 ) - 180;
            PositionOnce();
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

        public void FadeOut()
        {
            foreach ( FadeIntroSprite fader in GetComponentsInChildren<FadeIntroSprite>() )
            {
                fader.FadeOut();
            }

            Skybox.FadeOutIntro();
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

        void OnRotated()
        {
            rotating = false;
        }

        void PositionOnce()
        {
            Vector3 cam = PlayerController.GetInstance().GetLocation();

            transform.position = new Vector3(
                cam.x,
                cam.y,
                cam.z
            );
        }

        void Position()
        {
            Vector3 cam = PlayerController.GetInstance().GetLocation();

            transform.position = new Vector3(
                transform.position.x,
                cam.y,
                transform.position.z
            );
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

        void Start()
        {
            Invoke( "FadeOut", ShowDuration );
            Invoke( "PlayTimeline", ShowDuration - 1.5f ); // to play audio in time
        }

        void PlayTimeline()
        {
            GameEngine.GetInstance().ResumeTimeline();
        }

        void Update()
        {
            Position();

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