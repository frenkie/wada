using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Wada
{
    public class BasicNavMeshTargetsNPCController : BasicNavMeshNPCController
    {
        public GameObject[] Targets;
        public float BrakeDistance = .5f;
        public Vector2 WaitInterval = new( 3f, 8f );

        GameObject target;

        void Start()
        {
            Debug.Log( "BasicNavMeshTargetsNPCController started for " + gameObject.name );
            Invoke( "WalkToRandomTarget", 1f );
        }

        public void WalkToRandomTarget()
        {
            SetMoveTarget( GetRandomTarget() );
        }

        public GameObject GetRandomTarget()
        {
            // GameObject[] targets = Array.FindAll( Targets, t => t != null &&
            //                                                     (target == null ||
            //                                                      (target != null && t.name != target.name)) );

            GameObject[] targets = Array.FindAll( Targets, t => t != null );

            if ( targets.Length == 0 )
            {
                Targets = GameEngine.GetInstance().TestMontageTargets;
            }

            // filter any active target
            targets = Array.FindAll( Targets, t => target == null ||
                                                   (target != null && t.name != target.name) );

            if ( targets.Length > 0 )
            {
                target = GetRandomTarget( targets );
            }
            // else should not be possible, then we have no animal targets and no general ones (left)

            return target;
        }

        GameObject GetRandomTarget(GameObject[] from)
        {
            return from[(int)Mathf.Round( Random.Range( 0, from.Length - 1 ) )];
        }

        protected void LateUpdate()
        {
            if ( walking && navMeshAgent.enabled )
            {
                if ( !navMeshAgent.pathPending )
                {
                    if ( navMeshAgent.remainingDistance <= BrakeDistance )
                    {
                        Debug.Log( "[BasicNavMeshTargetsNPCController] reached target, getting next one" );

                        walking = false;
                        SetTrigger( "Idle" );
                        //PlaySound( AnimalSounds.Idle );

                        float waitFor = BrakeDistance;
                        if ( Vector2.Distance( Vector2.zero, WaitInterval ) != 0 )
                        {
                            waitFor += Random.Range( WaitInterval.x, WaitInterval.y );
                        }

                        Invoke( "WalkToRandomTarget", waitFor );
                    }
                }
            }
        }
    }
}