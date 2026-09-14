using UnityEngine;

namespace Wada
{
    public class Flocker : Ressurectable
    {
        public FlockController FlockerController;

        FlockChild flockChild;

        bool landed;

        void Awake()
        {
            base.Awake();

            flockChild = GetComponent<FlockChild>();
            flockChild.enabled = false;
        }

        public void AddToFlocker()
        {
            if ( FlockerController != null )
            {
                FlockerController.AddChild( flockChild );
                flockChild.enabled = true;
            }

            //PlaySound( AnimalSounds.Fly );
        }

        protected void EnableFreeRoaming()
        {
            Debug.Log( "[Flocker] EnableFreeRoaming" );

            flockChild.AutomaticWaypoints = true;
            flockChild.CanLand = true;
            flockChild.Paused = false;
            flockChild._move = true;
            flockChild.SetWaypoint( flockChild.findWaypoint() );
            flockChild.Flap();
        }

        public bool IsPaused()
        {
            return !flockChild.enabled || (flockChild.enabled && flockChild.Paused);
        }

        protected override void OnEnterHeldMode()
        {
            /*
             * If bird is held, release it from any landing spot
             */
            // if ( flockChild._landing && flockChild._lander != null )
            // {
            //     flockChild._lander.ReleaseFlockChild();
            // }

            flockChild.Paused = true;
            //flockChild.GetAnimator().speed = 0;
        }

        protected override void OnExitHeldMode(bool lateSaveChoice = false)
        {
            if ( lateSaveChoice )
            {
                OnEnteredHeldMode -= OnEnterHeldMode;
                OnExitedHeldMode -= OnExitHeldMode;
                RemoveFromFlocker();
            }
            else
            {
                flockChild.Paused = false;
                if ( flockChild._landing && flockChild._lander != null )
                {
                    flockChild._lander.ReleaseFlockChild();
                }

                //flockChild.GetAnimator().speed = 1;
                // flockChild.SetWaypoint( flockChild.findWaypoint() );
                // flockChild._move = true;
                // flockChild.Flap();
            }
        }

        public void RemoveFromFlocker()
        {
            if ( FlockerController != null )
            {
                FlockerController.RemoveChild( flockChild );
                flockChild.enabled = false;
            }
        }

        protected override void SetFree()
        {
            Debug.Log( "[Flocker] SetFree" );

            AddToFlocker();

            Invoke( "EnableFreeRoaming", .2f );

            OnEnteredHeldMode += OnEnterHeldMode;
            OnExitedHeldMode += OnExitHeldMode;
        }

        public void SetWaypoint(Vector3 to, float delay = 0)
        {
            flockChild._wayPoint = to;
            flockChild.Paused = false;
            flockChild.Wander( delay );
        }

        public void SetWaypointDistanceOverride(float to)
        {
            flockChild.WaypointDistanceOverride = to;
        }

        public void Unstuck()
        {
            flockChild.Paused = false;
        }

        protected void Update()
        {
            base.Update();

            if ( flockChild._landing && flockChild._move == false && !landed )
            {
                landed = true;
            }
            else if ( !flockChild._landing && flockChild._move && landed )
            {
                landed = false;
            }
        }
    }
}