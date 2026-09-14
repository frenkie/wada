using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Wada
{
    public class Ressurectable : BasicNavMeshTargetsNPCController, ITouchReceiver
    {
        public static bool DisableAllRevivals;
        public static event Action<Ressurectable> OnResurrect = delegate { };
        public static event Action<Ressurectable> OnRelease = delegate { };

        public static List<Ressurectable> AllRessurectables = new();

        public event Action OnEnteredHeldMode = delegate { };
        public event Action<bool> OnExitedHeldMode = delegate { };

        public AnimalEmbodiment EmbodimentType = AnimalEmbodiment.None;

        public bool UseDisplayMode = true;
        public bool CanAttractAttention = true;
        public bool WalkAfterRessurection = true;
        public float GetUpTime = 2f;
        public float OnDisplayScaleFactor = .3f;
        public float OnDisplayMoveFactor = .7f;
        public float OnDisplayPlateMoveFactor = -.2f;
        public float OnDisplayInfoMoveFactor = 1f;
        public InfoPlaqueData AnimalInfo;
        public bool AllowTouch = true;

        float displayModeTransformTime;

        protected bool resurrected = false;
        bool allowAnimalFlipOverOnGrab = true;
        bool readyToBeReleasedInChoiceMode = false;
        bool allowChoiceMode = false;
        bool inChoiceMode = false;
        bool touched = false; // when a hand is in the touch area
        float ungrabbedTime = 0;
        Vector3 grabbedOffset;

        const float
            UNGRABBED_THRESHOLD =
                .05f; // debouncer between grab and release to determing hand track glitch or actual release

        bool grabbed;
        bool firstGrabbed;

        bool allowHeldMode;
        bool inHeldMode; // for eating released / godmode animals
        bool inReleasedMode;
        bool navMeshWasEnabled;
        Vector3 heldModePreviousPosition;

        bool alertUnsaveable;

        WadaHand rightHandTouching;
        WadaHand leftHandTouching;
        WadaHand handToFollow;

        Vector3 followVelocity = Vector3.zero;
        InventorySaveTarget inventorySave;
        Vector3 warpToOnRelease;
        Vector3 startPosition;
        Quaternion startRotation;
        float startRotationFollowerHand;

        Vector3 scaleAtStart;
        Vector3 displayPos;

        bool movePlayerIntoFocus = false;
        float movePlayerIntoFocusTime;
        const float FOCUS_DISTANCE = .9f;
        const float MAX_MOVE_PLAYER_INTO_FOCUS_TIME = 2f;
        Vector3 playerFocusTarget;

        GameObject platter;
        GameObject focusLight;
        InfoPlaque plaque;

        Collider attentionCollider;

        protected void Start()
        {
            if ( CanAttractAttention )
            {
                attentionCollider = GetComponent<Collider>();
            }

            AllRessurectables.Add( this );

            // Disables base Start if called
            OnRelease += OnReleased;
            OnResurrect += OnRessurected;
        }

        IEnumerator AlertUnsaveable(bool special = false)
        {
            InstructionManager.GetInstance().Play( AudioInstructionType.NotForSave, special );
            yield return new WaitForSeconds( 15 );
            alertUnsaveable = false;
        }

        IEnumerator AlertBagFull(bool special = false)
        {
            InstructionManager.GetInstance().Play( AudioInstructionType.BagFull, special );
            yield return new WaitForSeconds( 15 );
            alertUnsaveable = false;
        }

        public void CancelChoice()
        {
            // go back to platter?
        }

        public void ChooseInventory()
        {
            if ( inChoiceMode )
            {
                AnimalController ac = GetComponent<AnimalController>();
                bool workshopIsFull = Workshop.GetInstance().IsFull();
                bool unsaveable = ac == null || (ac != null && !ac.enabled);
                if ( unsaveable || workshopIsFull )
                {
                    if ( !alertUnsaveable )
                    {
                        alertUnsaveable = true;

                        // Fade out any tutorial hands if they didnt finish themselves already
                        if ( NudgeManager.GetInstance().HasListenToNudge( NudgeType.ReviveChoice ) )
                        {
                            Debug.Log( "Clearing stuff!" );
                            InstructionManager.GetInstance()
                                .DisableSpecialQueueParsing(); // to prevent a clearance triggering a play of a queued one

                            InstructionManager.GetInstance()
                                .CancelInstruction( AudioInstructionType.FirstTouchWellDone );
                            InstructionManager.GetInstance().CancelInstruction( AudioInstructionType.FirstGrab );
                            InstructionManager.GetInstance().CancelInstruction( AudioInstructionType.KeepOrSetFree );

                            InstructionManager.GetInstance().EnableSpecialQueueParsing();
                        }

                        if ( unsaveable )
                        {
                            StartCoroutine( AlertUnsaveable( true ) );
                        }
                        else // if workshopIsFull
                        {
                            StartCoroutine( AlertBagFull( true ) );
                        }
                    }

                    return;
                }

                DenyChoiceMode();
                ExitChoiceMode();

                // reset scale
                transform.localScale = scaleAtStart;

                // for now on the fly; TODO convert all animals to grabbables
                PlayerController.GetInstance().AddAnimalToWorkshop( ac );

                GameEngine.GetInstance().MakeReviveChoice( RevivalModus.Saved );

                ExitDisplayMode();

                OnRelease.Invoke( this );
            }
        }

        public void ChooseRelease()
        {
            if ( inChoiceMode )
            {
                displayModeTransformTime = .1f; // FAST!

                if ( WalkAfterRessurection )
                {
                    RaycastHit navhittie;
                    if ( Physics.Raycast( transform.position, -Vector3.up, out navhittie, 20f,
                            LayerMask.GetMask( "Navigation", "ColliderForAnimals" ) ) )
                    {
                        warpToOnRelease = navhittie.point;
                    }
                    else
                    {
                        Debug.Log( "Couldnt find a warp position" );
                        warpToOnRelease = startPosition;
                    }
                }

                DenyChoiceMode();
                ExitChoiceMode();
                GameEngine.GetInstance().MakeReviveChoice( RevivalModus.Released );
                TransformToReleasePosition();
                ExitDisplayMode();

                Invoke( "AllowHeldMode", .3f ); // let it fall first just to be sure

                if ( WalkAfterRessurection )
                {
                    readyToBeReleasedInChoiceMode = true;
                    Rigidbody rb = GetComponent<Rigidbody>();
                    if ( rb != null )
                    {
                        rb.useGravity = true;
                        rb.isKinematic = false;
                    }
                }
                else
                {
                    // Set it freeeeee
                    Invoke( "SetFree", displayModeTransformTime );
                }
            }

            OnRelease( this );
        }

        void AllowChoiceMode()
        {
            allowChoiceMode = true;
        }

        void DenyChoiceMode()
        {
            allowChoiceMode = false;
        }

        public void AllowHeldMode()
        {
            Debug.Log( "AllowHeldMode for " + gameObject.name );
            allowHeldMode = true;
        }

        public void DisallowHeldMode()
        {
            Debug.Log( "DisallowHeldMode for " + gameObject.name );
            allowHeldMode = false;
        }

        public void EnterChoiceMode()
        {
            inChoiceMode = true;
        }

        void ExitChoiceMode()
        {
            inChoiceMode = false;
            grabbed = false;
            handToFollow = null;
        }

        public void EnterHeldMode()
        {
            Debug.Log( "[Ressurectable] EnterHeldMode " + gameObject.name );
            inHeldMode = true;
            if ( handToFollow != null )
            {
                handToFollow.SetHeldModeObject( gameObject );
            }

            /*
             * Disable moving of the player so we can't carry animals to other locations
             * and preferably don't drop them on a non navmesh position
             */
            PlayerController.GetInstance().EnterFocusMode();

            OnEnteredHeldMode.Invoke();
        }

        public void ExitHeldMode()
        {
            inHeldMode = false;
            if ( handToFollow != null )
            {
                handToFollow.UnSetHeldModeObject();
                handToFollow = null;
            }

            grabbed = false;

            PlayerController.GetInstance().ExitFocusMode();

            OnExitedHeldMode.Invoke( false );
        }

        void EnterDisplayMode()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
            displayModeTransformTime = GetUpTime / 2;

            inventorySave = PlayerController.GetInstance().InventorySaveTarget;

            // TODO determine display pos based on eyeheight player
            displayPos = transform.position + new Vector3( 0,
                Math.Max( .8f, OnDisplayMoveFactor + PlayerController.GetInstance().GetEyeHeight() - .5f ), 0 );
            TransformToDisplayMode();

            EnterFocusMode();
            Invoke( "ShowDisplayPlatter", GetUpTime / 4 * 2 );
            Invoke( "ShowInfoPlaque", GetUpTime );
            Invoke( "AllowChoiceMode", GetUpTime ); // prevent triggering too fast
        }

        void ExitDisplayMode()
        {
            ExitFocusMode();
            HideDisplayPlatter();
            HideInfoPlaque();
        }

        public void EnterFocusMode()
        {
            PlayerController.GetInstance().EnterFocusMode();
            MovePlayerIntoFocus();

            // light cone on subject
            // dim surround light (check how that works with unlit shaders, maybe create a custom one
            //      - overlays a darkning Turquoise color

            foreach ( Renderer renderer in GetComponentsInChildren<Renderer>() )
            {
                Material mat = renderer.material;
                mat.SetInt( "_Exclude_From_Focus", 1 );
                renderer.material = mat;
            }

            Vector3 halfPos = Vector3.Lerp(
                transform.position, playerFocusTarget, .5f
            );
            halfPos.y = transform.position.y + 3;

            Vector3 lookAtT = transform.position -
                              halfPos;

            focusLight =
                Instantiate( GameEngine.GetInstance().FocusLight,
                    halfPos,
                    Quaternion.LookRotation( lookAtT ) );

            GameEngine.GetInstance().EnterFocussedEnvironment();
        }

        public void EnableTouch()
        {
            AllowTouch = true;
        }

        public void ExitFocusMode()
        {
            // Todo animate this?
            foreach ( Renderer renderer in GetComponentsInChildren<Renderer>() )
            {
                Material mat = renderer.material;
                mat.SetInt( "_Exclude_From_Focus", 0 );
                renderer.material = mat;
            }

            Destroy( focusLight );

            PlayerController.GetInstance().ExitFocusMode();
            GameEngine.GetInstance().ExitFocussedEnvironment();
        }

        void HideDisplayPlatter()
        {
            Hashtable scaleParams = new();

            scaleParams.Add( "scale", Vector3.zero );
            scaleParams.Add( "time", GetUpTime / 4 );
            scaleParams.Add( "oncomplete", "OnHidePlatter" );

            iTween.ScaleTo( platter, scaleParams );
        }

        void HideInfoPlaque()
        {
            plaque.Hide();
        }

        public bool IsResurrected()
        {
            return resurrected;
        }

        public bool InHeldMode()
        {
            return inHeldMode;
        }

        protected void LateUpdate()
        {
            base.LateUpdate();

            if ( (inChoiceMode || inHeldMode) && handToFollow != null )
            {
                Transform follower = handToFollow.GetFollowerTransform();

                // Make sure animal doesn't follow the hand when it rotates too much palm upwards.
                if ( !allowAnimalFlipOverOnGrab && handToFollow.GetUp() <= .2f
                                                && transform.rotation != startRotation )
                {
                    transform.rotation = Quaternion.Slerp( transform.rotation, startRotation,
                        Time.deltaTime * 10 );
                }
                else if ( transform.rotation != follower.rotation )
                {
                    transform.rotation = Quaternion.Slerp( transform.rotation, follower.rotation,
                        Time.deltaTime * 40 );
                }

                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    follower.position, ref followVelocity, .04f );
            }
        }

        void MovePlayerIntoFocus()
        {
            /*
             * Player Controller should have start position height of the animal start position
             *
             * If Camera is >= x distance from the Player Controller floor center
             *      move the player controller to the x and z of the animal
             *
             * Save the expected position of the camera so animal details can rotate to it correctly
             */


            Vector3 cam = PlayerController.GetInstance().GetLocation();
            Transform player = PlayerController.GetInstance().GetTransform();
            Vector3 centerFloorPos = PlayerController.GetInstance().GetCenterPosition();
            Vector3 centerWorldOffset = PlayerController.GetInstance().GetCenterWorldOffset();

            Vector3 moveControllerTo = player.position;

            // if player is nowhere near or camera is off center
            if ( Vector2.Distance(
                    new Vector2( cam.x, cam.z ),
                    new Vector2( centerFloorPos.x, centerFloorPos.z )
                ) >= .35f || Vector3.Distance( centerFloorPos,
                    startPosition ) > 5 )
            {
                // move the controller to the animal pos making the floor center the middle
                moveControllerTo = startPosition - new Vector3( centerWorldOffset.x, 0, centerWorldOffset.z );
                Debug.Log( "[Ressurrectable] move animal to center for focus" );
            }
            else
            {
                // keep its horizontal position but move it to the same floor height
                moveControllerTo.y = startPosition.y;
                Debug.Log( "[Ressurrectable] move animal to side for focus" );
            }

            // Vector3 leftOfAnimal = transform.position + -transform.right * FOCUS_DISTANCE;
            // Vector3 rightOfAnimal = transform.position + transform.right * FOCUS_DISTANCE;


            Vector3 camOffset = PlayerController.GetInstance().GetCameraOffset();
            playerFocusTarget = moveControllerTo + camOffset;

            if ( Vector3.Distance( player.position,
                    moveControllerTo ) > 0.05f )
            {
                Hashtable moveTo = new();

                moveTo.Add( "position", moveControllerTo );
                moveTo.Add( "time",
                    Vector3.Distance( player.position,
                        moveControllerTo ) / 0.6f ); // let's see if this makes people nauseous
                moveTo.Add( "easetype", iTween.EaseType.easeOutCubic );

                iTween.MoveTo( PlayerController.GetInstance().gameObject, moveTo );
            }
        }

        public void OnAttention(bool state)
        {
            animator.SetBool( "Attention", state );
        }

        protected virtual void OnEnterHeldMode()
        {
            CancelInvoke( "WalkToRandomTarget" );
            if ( WalkAfterRessurection )
            {
                heldModePreviousPosition = transform.position;
                navMeshWasEnabled = true;
                navMeshAgent.enabled = false;
            }

            animator.speed = 0;
        }

        protected virtual void OnExitHeldMode(bool lateSaveChoice = false)
        {
            if ( !lateSaveChoice )
            {
                if ( navMeshWasEnabled )
                {
                    heldModePreviousPosition = transform.position;
                    navMeshAgent.enabled = true;
                    if ( navMeshAgent.Warp( heldModePreviousPosition ) )
                    {
                        WalkToRandomTarget();
                    }
                }

                animator.speed = 1;
            }
        }

        void OnHidePlatter()
        {
            Destroy( platter );
        }

        public virtual void OnReleased(Ressurectable resurrectable)
        {
            if ( resurrectable != this )
            {
                Unmute();
            }
        }

        public virtual void OnRessurected(Ressurectable resurrectable)
        {
            if ( resurrectable != this )
            {
                Mute();
            }
        }

        public virtual void OnTouch(WadaHand hand)
        {
            if ( AllowTouch && !DisableAllRevivals )
            {
                touched = true;
                if ( !resurrected )
                {
                    if ( attentionCollider != null && CanAttractAttention )
                    {
                        attentionCollider.enabled = false;
                        attentionCollider.isTrigger = false; //to be sure it doesn't hit the trigger
                    }

                    resurrected = true;

                    animator.SetTrigger( "GetUp" );
                    //PlaySound( AnimalSounds.Revive );


                    if ( UseDisplayMode )
                    {
                        OnResurrect( this );

                        EnterDisplayMode();
                    }

                    // here to make sure we are in special/focussed visual/audio mode
                    GameEngine.GetInstance().RevivedAnimal( this );
                }
                else if ( hand != null && (allowChoiceMode || allowHeldMode) )
                {
                    switch ( hand.Type )
                    {
                        case OVRInput.Controller.LHand:
                            leftHandTouching = hand;
                            break;
                        case OVRInput.Controller.RHand:
                            rightHandTouching = hand;
                            break;
                    }
                }
            }
        }

        public virtual void OnLeaveTouch(WadaHand hand)
        {
            if ( AllowTouch )
            {
                touched = false;
                switch ( hand.Type )
                {
                    case OVRInput.Controller.LHand:
                        leftHandTouching = null;
                        break;
                    case OVRInput.Controller.RHand:
                        rightHandTouching = null;
                        break;
                }
            }
        }

        void OnTriggerExit(Collider other)
        {
            if ( !resurrected && CanAttractAttention && other.gameObject.tag == "Hand" )
            {
                // OnAttention( false );
            }
        }

        void OnTriggerStay(Collider other)
        {
            if ( !resurrected && CanAttractAttention && other.gameObject.tag == "Hand" )
            {
                // OnAttention( true );
            }
        }

        public void SetResurrected(bool to)
        {
            resurrected = true;
        }

        void ShowDisplayPlatter()
        {
            platter =
                Instantiate( GameEngine.GetInstance().PlatterPrefab,
                    displayPos + new Vector3( 0, OnDisplayPlateMoveFactor, 0 ),
                    Quaternion.identity );
            Vector3 scale = platter.transform.localScale;
            platter.transform.localScale = Vector3.zero;

            Hashtable scaleParams = new();

            scaleParams.Add( "scale", scale );
            scaleParams.Add( "time", GetUpTime / 4 );

            iTween.ScaleTo( platter, scaleParams );
        }


        void ShowInfoPlaque()
        {
            GameObject info =
                Instantiate( GameEngine.GetInstance().InfoPrefab,
                    displayPos + new Vector3( 0, OnDisplayInfoMoveFactor - .2f, 0 ),
                    Quaternion.identity );

            plaque = info.GetComponent<InfoPlaque>();

            if ( AnimalInfo != null )
            {
                plaque.ParseData( AnimalInfo );
            }
            else
            {
                plaque.ParseData( GameEngine.GetInstance().DummyData );
            }

            Invoke( "ShowPlaque", .1f );
        }

        void ShowPlaque()
        {
            plaque.Show( playerFocusTarget );
        }

        void TransformToDisplayMode()
        {
            scaleAtStart = transform.localScale;

            Hashtable scale = new();

            scale.Add( "scale", OnDisplayScaleFactor * scaleAtStart );
            scale.Add( "time", displayModeTransformTime );

            iTween.ScaleTo( gameObject, scale );

            Hashtable rotate = new();

            rotate.Add( "x", 0 );
            rotate.Add( "y", transform.eulerAngles.y );
            rotate.Add( "z", 0 );
            rotate.Add( "time", displayModeTransformTime );

            iTween.RotateTo( gameObject, rotate );

            Hashtable moveTo = new();

            moveTo.Add( "position", displayPos );
            moveTo.Add( "time", displayModeTransformTime );
            moveTo.Add( "easetype", iTween.EaseType.easeOutCubic );

            iTween.MoveTo( gameObject, moveTo );
        }

        protected virtual void SetFree()
        {
            if ( WalkAfterRessurection )
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if ( rb != null )
                {
                    rb.isKinematic = true;
                    rb.useGravity = false;
                    rb.velocity = Vector3.zero;
                    rb.angularVelocity = Vector3.zero;
                }

                navMeshAgent.enabled = true;
                if ( navMeshAgent.Warp( warpToOnRelease ) )
                {
                    WalkToRandomTarget();

                    inReleasedMode = true;
                    OnEnteredHeldMode += OnEnterHeldMode;
                    OnExitedHeldMode += OnExitHeldMode;
                }
                else
                {
                    Debug.LogError( "[Resurrectable] Weird! Can't set " + gameObject.name + " free?" );
                }
            }
            else
            {
                // else birds have to override this function
                Debug.LogError( "[Resurrectable] Flocker should override this method " );
            }
        }

        void TransformToReleasePosition()
        {
            Hashtable scale = new();

            scale.Add( "scale", scaleAtStart );
            scale.Add( "time", displayModeTransformTime );

            iTween.ScaleTo( gameObject, scale );

            Hashtable rotate = new();

            rotate.Add( "x", 0 );
            rotate.Add( "y", transform.eulerAngles.y );
            rotate.Add( "z", 0 );
            rotate.Add( "time", displayModeTransformTime );

            iTween.RotateTo( gameObject, rotate );
        }

        protected void Update()
        {
            base.Update();

            if ( readyToBeReleasedInChoiceMode )
            {
                if ( transform.position.y < warpToOnRelease.y || transform.position.y - warpToOnRelease.y <= .1f )
                {
                    readyToBeReleasedInChoiceMode = false;
                    SetFree();
                }
            }

            if ( resurrected )
            {
                AnimatorClipInfo[] clips = animator.GetNextAnimatorClipInfo( 0 );
                if ( clips.Length == 0 )
                {
                    clips = animator.GetCurrentAnimatorClipInfo( 0 );
                }

                if ( clips.Length > 0 )
                {
                    AnimalSounds newSoundType = GetAnimalSoundFromAnimationName( clips[0].clip.name );

                    if ( newSoundType != AnimalSounds.None )
                    {
                        if ( activeSoundType != newSoundType )
                        {
                            if ( activeSound != null && activeSoundType == AnimalSounds.Revive &&
                                 newSoundType == AnimalSounds.Halt )
                            {
                                activeSound = null;
                                activeSoundType =
                                    AnimalSounds.Halt;
                            }
                            else if ( SoundStrategies.ContainsKey( newSoundType ) &&
                                      SoundStrategies[newSoundType] != AnimalSoundStrategy.FireEveryTime )
                            {
                                Debug.Log( gameObject.name + " is going to play " + newSoundType.ToString() );

                                // only play it when it's not supposed to be played by the animation itself,
                                // which is the FireEveryTime strategy
                                PlaySound( newSoundType );
                            }
                            else if ( activeSound != null && activeSoundType != AnimalSounds.None &&
                                      SoundStrategies.ContainsKey( activeSoundType ) &&
                                      SoundStrategies[activeSoundType] == AnimalSoundStrategy.FireOnceAndLoop )
                            {
                                Debug.Log( gameObject.name + " wants to play " + newSoundType.ToString() +
                                           ", killing loop " +
                                           activeSoundType );

                                activeSound.FadeOutAndKill();
                                activeSound = null;
                                activeSoundType = AnimalSounds.None;
                            }
                            else if ( activeSound != null && activeSoundType != AnimalSounds.None &&
                                      SoundStrategies.ContainsKey( activeSoundType ) &&
                                      SoundStrategies[activeSoundType] == AnimalSoundStrategy.FireOnce )
                            {
                                activeSound = null;
                                activeSoundType =
                                    AnimalSounds.None;
                            }
                        }
                    }
                    else if ( activeSoundType != AnimalSounds.None && activeSound != null )
                    {
                        if ( SoundStrategies.ContainsKey( activeSoundType ) &&
                             SoundStrategies[activeSoundType] == AnimalSoundStrategy.FireOnceAndLoop )
                        {
                            Debug.Log( gameObject.name + " kill current looped sound " + activeSoundType );
                            activeSound.FadeOutAndKill();
                        }

                        activeSoundType = AnimalSounds.None;
                        activeSound = null;
                    }
                }
            }

            /*
             *  TODO: Refactor towards the following when we are going to allow montaging
             *
             * If inChoiceMode
             *  if ! grabbed but grabbing
             *      grabbed=true
             *      connect to hand
             *  if grabbed but not grabbing and not grabbing time >= threshold (for debouncing)
             *      grabbed = false
             *      release + exit choice mode
             *  or if close to inventory save
             *      grabbed = false
             *      save and hide + exit choice mode
             *
             *
             *  Create An AnimalController
             *  Limb or parts that are hand interactable (our extension)
             *  send out signals to Animal Controller (if an attached limb) that it's grabbed or released
             *  Ressurectable registers to Animal Controller
             *
             *      HandGrabInteractable extension
             *          has a switch to not follow the hand, just registers grab
             *
             *      Animal Controller
             *          with grabbed state
             *          and delegates for onGrab onRelease
             *
             *      Resurrectable registers to onGrab onRelease
             *      de registers after the choice mode release
             */
            if ( allowChoiceMode && touched && !inChoiceMode )
            {
                if ( rightHandTouching != null && rightHandTouching.InClenchedPose() )
                {
                    handToFollow = rightHandTouching;
                    handToFollow.SetFollower( transform );
                    ungrabbedTime = 0;
                    grabbed = true;
                    EnterChoiceMode();
                }
                else if ( leftHandTouching != null && leftHandTouching.InClenchedPose() )
                {
                    handToFollow = leftHandTouching;
                    handToFollow.SetFollower( transform );
                    ungrabbedTime = 0;
                    grabbed = true;
                    EnterChoiceMode();
                }

                if ( grabbed && !firstGrabbed )
                {
                    Debug.Log( "[Ressurectable] Grab " + gameObject.name );
                    firstGrabbed = true;
                    GameEngine.GetInstance().GrabbedAnimal();
                }
            }
            else if ( allowHeldMode && touched && !inHeldMode )
            {
                if ( rightHandTouching != null && rightHandTouching.InClenchedPose() )
                {
                    handToFollow = rightHandTouching;
                    handToFollow.SetFollower( transform );
                    ungrabbedTime = 0;
                    grabbed = true;
                    EnterHeldMode();
                }
                else if ( leftHandTouching != null && leftHandTouching.InClenchedPose() )
                {
                    handToFollow = leftHandTouching;
                    handToFollow.SetFollower( transform );
                    ungrabbedTime = 0;
                    grabbed = true;
                    EnterHeldMode();
                }
            }

            if ( grabbed && (inChoiceMode || inHeldMode) && handToFollow != null )
            {
                if ( handToFollow.InClenchedPose() )
                {
                    // re-establish grab is true for when we had a hand detecting glitch
                    ungrabbedTime = 0;
                    grabbed = true;

                    if ( inChoiceMode && inventorySave.HasActiveHand() &&
                         inventorySave.GetActiveHand() == handToFollow )
                    {
                        ChooseInventory();
                    }
                    else if ( inHeldMode && inventorySave.HasActiveHand() &&
                              inventorySave.GetActiveHand() == handToFollow )
                    {
                        AnimalController ac = GetComponent<AnimalController>();
                        bool workshopIsFull = Workshop.GetInstance().IsFull();
                        bool unsaveable = !(ac != null && ac.enabled && !ac.InGodMode());
                        if ( !unsaveable && !workshopIsFull )
                        {
                            // for now not montaged items

                            DisallowHeldMode();
                            inHeldMode = false;
                            grabbed = false;
                            if ( handToFollow != null )
                            {
                                handToFollow.UnSetHeldModeObject();
                                handToFollow = null;
                            }

                            if ( inReleasedMode )
                            {
                                inReleasedMode = false;
                                OnEnteredHeldMode -= OnEnterHeldMode;
                                OnExitedHeldMode -= OnExitHeldMode;
                            }
                            else
                            {
                                OnExitedHeldMode.Invoke( true );
                            }

                            PlayerController.GetInstance().ExitFocusMode();

                            PlayerController.GetInstance().AddAnimalToWorkshop( ac );
                            GameEngine.GetInstance().MakeReviveChoiceLater( RevivalModus.Saved );
                        }
                        else if ( !alertUnsaveable )
                        {
                            alertUnsaveable = true;
                            if ( unsaveable )
                            {
                                StartCoroutine( AlertUnsaveable() );
                            }
                            else // if ( workshopIsFull )
                            {
                                StartCoroutine( AlertBagFull() );
                            }
                        }
                    }
                }
                else if ( grabbed && !handToFollow.InClenchedPose() )
                {
                    if ( ungrabbedTime >= UNGRABBED_THRESHOLD )
                    {
                        grabbed = false;

                        if ( inChoiceMode )
                        {
                            // if inside the inventory target
                            if ( inventorySave.HasActiveHand() && inventorySave.GetActiveHand() == handToFollow )
                            {
                                ChooseInventory();
                            }
                            else
                            {
                                ChooseRelease();
                            }
                        }
                        else if ( inHeldMode )
                        {
                            ExitHeldMode();
                        }
                    }

                    ungrabbedTime += Time.deltaTime;
                }
            }
            else if ( !grabbed && (inChoiceMode || inHeldMode) && handToFollow != null )
            {
                // Is this the weird bug where the animal is stuck to a hand without a choice somehow?
                Debug.Log( "Animal stuck on a hand?" );
                if ( inChoiceMode )
                {
                    ChooseRelease();
                }
                else if ( inHeldMode )
                {
                    ExitHeldMode();
                }
            }


            if ( movePlayerIntoFocus )
            {
                // determine player root position based on intended cam endpoint
            }
        }
    }


#if UNITY_EDITOR
    [CustomEditor( typeof(Ressurectable) )]
    public class RessurectableEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            Ressurectable myTarget = (Ressurectable)target;

            DrawDefaultInspector();

            if ( GUILayout.Button( "Get Up" ) )
            {
                myTarget.OnTouch( null );
            }

            if ( GUILayout.Button( "Store" ) )
            {
                myTarget.EnterChoiceMode();
                myTarget.ChooseInventory();
            }

            if ( GUILayout.Button( "Release" ) )
            {
                myTarget.EnterChoiceMode();
                myTarget.ChooseRelease();
            }

            if ( GUILayout.Button( "EnterHeldMode" ) )
            {
                myTarget.EnterHeldMode();
            }

            if ( GUILayout.Button( "ExitHeldMode" ) )
            {
                myTarget.ExitHeldMode();
            }

            if ( GUILayout.Button( "CopyDownEmbodimentType" ) )
            {
                if ( myTarget.EmbodimentType != AnimalEmbodiment.None )
                {
                    foreach ( Limb limb in myTarget.gameObject.GetComponentsInChildren<Limb>() )
                    {
                        limb.EmbodimentType = myTarget.EmbodimentType;
                        EditorUtility.SetDirty( limb );
                    }
                }
            }
        }
    }
#endif
}