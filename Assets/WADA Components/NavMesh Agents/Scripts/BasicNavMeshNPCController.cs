using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.AI;

namespace Wada
{
    [Serializable]
    public class AnimalSoundClips
    {
        public AudioClip[] SoundClips;
    }

    [Serializable]
    public class BasicNPCSounds : SerializableDictionary<AnimalSounds, AnimalSoundClips>
    {
    }

    [Serializable]
    public class BasicNPCSoundStrategy : SerializableDictionary<AnimalSounds, AnimalSoundStrategy>
    {
    }

    [Serializable]
    public class BasicNPCSoundOffsets : SerializableDictionary<AnimalSounds, float>
    {
    }

    public class BasicNavMeshNPCController : MonoBehaviour
    {
        public string StartAnimationTrigger = "";
        public bool InitializeOnNavMesh = false;
        public List<float> ClampForwardSpeed;
        public bool MoveWithAnimatedThrust = false;
        public float AnimatedThrustAcceleration = 1.5f;
        public AnimationCurve AccelerationCurve;
        public float AnimatedThrustDeceleration = 0.3f;
        public AnimationCurve DecelerationCurve;

        public BasicNPCSounds Sounds;
        public BasicNPCSoundStrategy SoundStrategies;
        public BasicNPCSoundOffsets SoundOffsets;
        public AudioSource Sound;
        float intendedVolume = 1;

        string latestAnimationTrigger;

        protected SoundEffect activeSound;
        protected AnimalSounds activeSoundType = AnimalSounds.None;
        protected Animator animator;
        protected NavMeshAgent navMeshAgent;
        protected bool walking = false;

        List<string> AnimationsWalk = new() { "walk", "walkwithscript", "walkwiththrust", "swim" };
        List<string> AnimationsWalk2 = new() { "walk2" };
        List<string> AnimationsFly = new() { "fly", "flywithscript", "flywithscript2" };
        List<string> AnimationsFly2 = new() { "fly2" };
        List<string> AnimationsIdle = new() { "idle", "stand" };
        List<string> AnimationsRevive = new() { "revive", "getup", "get up" };
        List<string> AnimationsTakeOff = new() { "takeoff", "liftoff", "start walk", "take off", "idletowalk" };
        List<string> AnimationsHalt = new() { "land", "halt", "landalternative", "walktoidle" };

        [SerializeField] float intendedSpeed; // private made public for debug mode only
        float thrustAccelTime;
        float thrustDecelTime;
        bool moveWithAnimatedThrust = false;

        float forward = 0f;
        float turn = 0f;

        bool muted;
        bool isAutoTurning = false;
        float autoTurnX = 0f;

        Vector2 smoothDeltaPosition = Vector2.zero;
        Vector2 velocity = Vector2.zero;

        protected void Awake()
        {
            animator = GetComponentInChildren<Animator>();
            navMeshAgent = GetComponent<NavMeshAgent>();
            navMeshAgent.updatePosition = false;

            intendedSpeed = navMeshAgent.speed;
            if ( Sound != null )
            {
                intendedVolume = Sound.volume;
            }

            if ( MoveWithAnimatedThrust )
            {
                navMeshAgent.speed = 0;
            }
        }


        void AnimateTurnValue(float to, float time)
        {
            Hashtable valueTo = new();

            valueTo.Add( "from", autoTurnX );
            valueTo.Add( "to", to );
            valueTo.Add( "time", time );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "SetTurnValue" );
            valueTo.Add( "onupdatetarget", gameObject );
            valueTo.Add( "oncomplete", "AnimateTurnValueBack" );

            Hashtable paramHashtable = new();
            paramHashtable.Add( "time", time );
            paramHashtable.Add( "startValue", autoTurnX );

            valueTo.Add( "oncompleteparams", paramHashtable );

            iTween.ValueTo( gameObject, valueTo );
        }

        public void AnimateTurnValueBack(object cmpParams)
        {
            Hashtable hstbl = (Hashtable)cmpParams;

            Hashtable valueTo = new();

            valueTo.Add( "from", autoTurnX );
            valueTo.Add( "to", (float)hstbl["startValue"] );
            valueTo.Add( "time", (float)hstbl["time"] );
            valueTo.Add( "easetype", iTween.EaseType.linear );
            valueTo.Add( "onupdate", "SetTurnValue" );
            valueTo.Add( "onupdatetarget", gameObject );

            iTween.ValueTo( gameObject, valueTo );
        }

        void AnimateLatestTrigger()
        {
            if ( navMeshAgent.enabled )
            {
                navMeshAgent.enabled = false;
                walking = false;
            }

            animator.SetTrigger( latestAnimationTrigger );
        }

        SoundEffect CreateSoundEffectObject(AnimalSounds animalSound)
        {
            GameObject randomShot = new( "soundEffect" );
            randomShot.transform.parent = Sound.transform;
            randomShot.transform.localPosition = Vector3.zero;
            SoundEffect effect = randomShot.AddComponent<SoundEffect>();
            effect.DestroyAfterPlay = SoundStrategies[animalSound] != AnimalSoundStrategy.FireOnceAndLoop;
            effect.PlayOnStart = true;
            effect.FadeTime = 0.125f;
            effect.Loop = SoundStrategies[animalSound] == AnimalSoundStrategy.FireOnceAndLoop;
            effect.Audio = GetAnimalSoundsAsList( animalSound );
            effect.Spatial = Sound.spatialBlend == 1f;
            effect.RolloffMode = Sound.rolloffMode;
            effect.SpatialRange = new Vector2( Sound.minDistance, Sound.maxDistance );
            effect.Volume = Sound.rolloffMode == AudioRolloffMode.Linear ? intendedVolume : Sound.volume;
            effect.Muted = muted;

            if ( SoundOffsets.Contains( animalSound ) )
            {
                effect.TimeOffset = SoundOffsets[animalSound];
            }

            return effect;
        }

        public void EnableNavMesh()
        {
            navMeshAgent.enabled = true;
        }

        float easeOutCubic(float start, float end, float value)
        {
            value--;
            end -= start;
            return end * (value * value * value + 1) + start;
        }

        List<AudioClip> GetAnimalSoundsAsList(AnimalSounds animalSound)
        {
            List<AudioClip> animalSounds = new();
            foreach ( AudioClip clip in Sounds[animalSound].SoundClips )
            {
                animalSounds.Add( clip );
            }

            return animalSounds;
        }

        float GetClampedForward(float toClamp)
        {
            float clamped = toClamp;

            int index = 0;
            foreach ( float clamper in ClampForwardSpeed )
            {
                if ( toClamp <= clamper )
                {
                    float[] range = new[] { index == 0 ? 0 : ClampForwardSpeed[index - 1], clamper };
                    clamped = range[Mathf.Min( 1, Mathf.Abs( (int)Mathf.Round( toClamp / clamper ) ) )];
                    break;
                }
                else if ( index == ClampForwardSpeed.Count - 1 )
                {
                    clamped = ClampForwardSpeed[index];
                }

                index++;
            }

            return clamped;
        }

        public float GetIntendedSpeed()
        {
            return intendedSpeed;
        }

        public NavMeshAgent GetNavMeshAgent()
        {
            return navMeshAgent;
        }

        public void SetAutoTurning(bool to)
        {
            isAutoTurning = to;
            if ( to )
            {
                autoTurnX = 1; // todo smooth in
            }
            else
            {
                autoTurnX = 0; // todo smooth out
            }
        }

        public AnimalSounds GetAnimalSoundFromAnimationName(string name)
        {
            string parseable = name.ToLower();

            if ( AnimationsWalk.Contains( parseable ) )
            {
                return AnimalSounds.Walk;
            }

            if ( AnimationsWalk2.Contains( parseable ) )
            {
                return AnimalSounds.Walk2;
            }

            if ( AnimationsFly.Contains( parseable ) )
            {
                return AnimalSounds.Fly;
            }

            if ( AnimationsFly2.Contains( parseable ) )
            {
                return AnimalSounds.Fly2;
            }

            if ( AnimationsIdle.Contains( parseable ) )
            {
                return AnimalSounds.Idle;
            }

            if ( AnimationsRevive.Contains( parseable ) )
            {
                return AnimalSounds.Revive;
            }

            if ( AnimationsTakeOff.Contains( parseable ) )
            {
                return AnimalSounds.TakeOff;
            }

            if ( AnimationsHalt.Contains( parseable ) )
            {
                return AnimalSounds.Halt;
            }

            return AnimalSounds.None;
        }

        public bool GetAutoTurning()
        {
            return isAutoTurning;
        }

        public void KillSound()
        {
            if ( Sound != null && activeSound != null && activeSoundType != AnimalSounds.None &&
                 SoundStrategies.ContainsKey( activeSoundType ) &&
                 SoundStrategies[activeSoundType] == AnimalSoundStrategy.FireOnceAndLoop )
            {
                activeSound.FadeOutAndKill();
            }
        }

        public void Mute()
        {
            muted = true;
            if ( Sound != null )
            {
                Sound.volume = 0;
            }

            if ( activeSound != null )
            {
                activeSound.Mute();
            }
        }

        public void NavMeshAgentInitialize()
        {
            if ( navMeshAgent != null )
            {
                EnableNavMesh();

                SetOnNavMesh();

                transform.position = new Vector3(
                    transform.position.x,
                    navMeshAgent.gameObject.transform.position.y,
                    transform.position.z
                );

                navMeshAgent.enabled = false;
            }
        }

        public void OnAnimatorMove()
        {
            // Update position to agent position
            if ( navMeshAgent.enabled )
            {
                transform.position = navMeshAgent.nextPosition;
            }
            else if ( !isAutoTurning )
            {
                animator.ApplyBuiltinRootMotion();
            }
        }

        public void OnAnimatedSound(AnimalSounds animalSound)
        {
            PlaySound( animalSound );
        }

        public void OnAnimatedThrust()
        {
            thrustAccelTime = 0;
            thrustDecelTime = 0;
            moveWithAnimatedThrust = true;
        }

        void OnTriggerEnter(Collider collider)
        {
            // Debug.Log( "[BasicNavMeshNPCController] OnTriggerEnter " + collider.gameObject.name );

            if ( collider.gameObject.layer == LayerMask.NameToLayer( "Interactable" ) )
            {
                Trigger trigger = collider.gameObject.GetComponent<Trigger>();
                if ( trigger != null && trigger.CanTriggerFor( gameObject ) )
                {
                    trigger.gameObject.SetActive( false );
                    SetTrigger( trigger.TriggerKey, trigger.Delay );
                }
                else
                {
                    Debug.Log( "[BasicNavMeshNPCController] No Trigger component found" );
                }
            }

            if ( collider.gameObject.layer == LayerMask.NameToLayer( "ScriptInteractable" ) )
            {
                // For now simple methods without arguments

                ScriptTrigger trigger = collider.gameObject.GetComponent<ScriptTrigger>();
                if ( trigger != null && trigger.CanTriggerFor( gameObject ) )
                {
                    trigger.gameObject.SetActive( false );
                    Invoke( trigger.TriggerMethod, trigger.Delay );
                }
                else
                {
                    Debug.Log( "[BasicNavMeshNPCController] No Trigger component found" );
                }
            }

            if ( collider.gameObject.layer == LayerMask.NameToLayer( "NavigationInteractable" ) )
            {
                NavigationTrigger trigger = collider.gameObject.GetComponent<NavigationTrigger>();
                if ( trigger.CanTriggerFor( gameObject ) )
                {
                    trigger.gameObject.SetActive( false );
                    SetMoveTarget( trigger.Target );
                }
            }
        }

        public void PauseSound()
        {
            if ( activeSound != null )
            {
                activeSound.Pause();
            }
        }

        public void PlaySound(AnimalSounds animalSound)
        {
            if ( Sound != null )
            {
                /*
                 * if there is an active sound
                 *  and it's looping (as in not self destructive), FadeItOut (and destroy?)
                 *
                 * if animalSound is of type loop and activeSound not exist
                 *      create
                 */
                if ( animalSound != activeSoundType && activeSound != null && activeSoundType != AnimalSounds.None &&
                     SoundStrategies.ContainsKey( activeSoundType ) &&
                     SoundStrategies[activeSoundType] == AnimalSoundStrategy.FireOnceAndLoop )
                {
                    activeSound.FadeOutAndKill();
                }

                if ( SoundStrategies.ContainsKey( animalSound ) )
                {
                    switch ( SoundStrategies[animalSound] )
                    {
                        case AnimalSoundStrategy.FireOnce:
                        case AnimalSoundStrategy.FireOnceAndLoop:
                            if ( activeSoundType != animalSound )
                            {
                                activeSoundType = animalSound;
                                activeSound = CreateSoundEffectObject( animalSound );
                            }

                            break;

                        case AnimalSoundStrategy.FireEveryTime:
                            activeSoundType = animalSound;
                            activeSound = CreateSoundEffectObject( animalSound );
                            break;
                    }
                }
                else
                {
                    activeSoundType = animalSound;
                    //?
                    activeSound = null;
                }
            }
        }

        public void ResumeSound()
        {
            if ( activeSound != null && activeSound.IsPaused() )
            {
                activeSound.Resume();
            }
        }

        public void SetMoveTarget(GameObject to)
        {
            RaycastHit hit;
            if ( Physics.Raycast( to.transform.position, -Vector3.up, out hit, 20f,
                    LayerMask.GetMask( "Navigation", "ColliderForAnimals" ) ) )
            {
                if ( !navMeshAgent.enabled )
                {
                    EnableNavMesh();
                }

                if ( !navMeshAgent.isOnNavMesh )
                {
                    SetOnNavMesh();
                }

                if ( navMeshAgent.SetDestination( hit.point ) )
                {
                    // Debug.Log( "Destination for " + gameObject.name + " " + hit.point );

                    walking = true;
                    if ( latestAnimationTrigger != null )
                    {
                        animator.ResetTrigger( latestAnimationTrigger );
                    }

                    navMeshAgent.speed = 0;
                    latestAnimationTrigger = "Walk";
                    animator.SetTrigger( latestAnimationTrigger );

                    // TODO transition to animation state checker
                    //PlaySound( AnimalSounds.Walk );
                }
                else
                {
                    Debug.Log( "Couldn't set destination" );
                }
            }
            else
            {
                Debug.Log( "Target not on a navmesh for " + gameObject.name );
            }
        }

        public void SetOnNavMesh()
        {
            NavMeshHit navhittie;
            if ( NavMesh.SamplePosition( transform.position, out navhittie, 10f, NavMesh.AllAreas ) )
            {
                if ( navMeshAgent.Warp( navhittie.position ) )
                {
                    Debug.Log( "Warped!" );
                }
                else
                {
                    Debug.Log( "Not warped!" );
                }
            }
            else
            {
                Debug.Log( "Hmm, no nav areas?" );
            }
        }

        void Start()
        {
            if ( InitializeOnNavMesh )
            {
                NavMeshAgentInitialize();
            }

            if ( StartAnimationTrigger != "" )
            {
                SetTrigger( StartAnimationTrigger );
            }
        }

        public void SetTurnValue(float to)
        {
            autoTurnX = to;
        }

        public void SetRotateTarget(Transform rotateTarget)
        {
            Quaternion target_rot = Quaternion.LookRotation( rotateTarget.position - transform.position );

            float turnValue = Quaternion.Angle( transform.rotation, target_rot ) < 0 ? -.5f : .5f;
            float turnTime = Quaternion.Angle( transform.rotation, target_rot ) * .02f;

            AnimateTurnValue( turnValue, turnTime / 2 );
            isAutoTurning = true;

            Hashtable rotateParams = new();

            rotateParams.Add( "rotation", new Vector3(
                transform.eulerAngles.x,
                target_rot.eulerAngles.y,
                transform.eulerAngles.z
            ) );
            rotateParams.Add( "time", turnTime );
            rotateParams.Add( "oncomplete", "OnRotatedToTarget" );
            rotateParams.Add( "easetype", iTween.EaseType.easeInOutQuad );

            iTween.RotateTo( gameObject, rotateParams );
        }

        public void OnRotatedToTarget()
        {
            isAutoTurning = false;
            autoTurnX = 0f;
        }

        public void SetAnimator(Animator to)
        {
            animator = to;
        }

        public void SetRotateTargetToPlayer()
        {
            if ( navMeshAgent.enabled )
            {
                navMeshAgent.enabled = false;
                walking = false;
            }

            SetRotateTarget( Camera.main.transform );
        }

        public void SetTrigger(string to)
        {
            SetTrigger( to, 0f );
        }

        public void SetTrigger(string to, float delay)
        {
            if ( latestAnimationTrigger != null )
            {
                animator.ResetTrigger( latestAnimationTrigger );
            }

            latestAnimationTrigger = to;
            Invoke( "AnimateLatestTrigger", delay );
        }

        public void Unmute()
        {
            muted = false;
            if ( Sound != null )
            {
                Sound.volume = intendedVolume;
            }

            if ( activeSound != null )
            {
                activeSound.Unmute();
            }
        }

        protected void Update()
        {
            if ( navMeshAgent.enabled && MoveWithAnimatedThrust )
            {
                // animation determines speed

                // TODO take into account start speed for both acceleration and deceleration?

                if ( moveWithAnimatedThrust )
                {
                    navMeshAgent.speed = intendedSpeed * AccelerationCurve.Evaluate(
                        thrustAccelTime / AnimatedThrustAcceleration );

                    if ( thrustAccelTime >= AnimatedThrustAcceleration )
                    {
                        moveWithAnimatedThrust = false;
                    }

                    thrustAccelTime += Time.deltaTime;
                }
                else if ( navMeshAgent.speed > 0 )
                {
                    navMeshAgent.speed = intendedSpeed * DecelerationCurve.Evaluate(
                        thrustDecelTime / AnimatedThrustDeceleration );

                    thrustDecelTime += Time.deltaTime;
                }
            }
            else
            {
                // Navmesh agent determines speed

                Vector3 worldDeltaPosition = navMeshAgent.nextPosition - transform.position;

                // Map 'worldDeltaPosition' to local space
                float dx = Vector3.Dot( transform.right, worldDeltaPosition );
                float dy = Vector3.Dot( transform.forward, worldDeltaPosition );
                Vector2 deltaPosition = new( dx, dy );

                // Low-pass filter the deltaMove
                float smooth = Mathf.Min( 1.0f, Time.deltaTime / 0.15f );
                smoothDeltaPosition = Vector2.Lerp( smoothDeltaPosition, deltaPosition, smooth );

                // Update velocity if time advances
                if ( Time.deltaTime > 1e-5f )
                {
                    velocity = deltaPosition / Time.deltaTime;
                }

                bool shouldMove = velocity.magnitude > 0.5f && navMeshAgent.remainingDistance > navMeshAgent.radius;

                // veloctiy x should be the angle difference

                if ( isAutoTurning )
                {
                    animator.SetFloat( "Turn", autoTurnX );
                }
                else
                {
                    animator.SetFloat( "Turn", velocity.x );
                }

                if ( ClampForwardSpeed.Count > 0 )
                {
                    animator.SetFloat( "Forward", GetClampedForward( velocity.y ) );
                }
                else
                {
                    animator.SetFloat( "Forward", velocity.y );
                }
            }
        }
    }
}