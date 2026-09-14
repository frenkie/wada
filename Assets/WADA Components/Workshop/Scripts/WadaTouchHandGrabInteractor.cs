/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using UnityEngine;
using UnityEngine.Assertions;
using Oculus.Interaction;
using Oculus.Interaction.Input;
using Oculus.Interaction.PoseDetection;
using UnityEngine.Serialization;

namespace Wada
{
    /// <summary>
    /// TouchHandGrabInteractor provides a hand-specific grab interaction model
    /// where selection begins when finger tips overlap with an associated interactable.
    /// Upon selection, the distance between the fingers and thumb is cached and is used for
    /// determining the point of release: when fingers are outside of the cached distance.
    /// </summary>
    public class
        WadaTouchHandGrabInteractor : PointerInteractor<WadaTouchHandGrabInteractor, WadaTouchHandGrabInteractable>
    {
        [SerializeField] [Interface( typeof(IHand) )]
        UnityEngine.Object _hand;

        IHand Hand { get; set; }

        [SerializeField] [Interface( typeof(IHand) )]
        UnityEngine.Object _openHand;

        IHand OpenHand { get; set; }

        [SerializeField] [Interface( typeof(IHandSphereMap) )]
        UnityEngine.Object _handSphereMap;

        protected IHandSphereMap HandSphereMap;

        [SerializeField] Transform _hoverLocation;

        [SerializeField] Transform _grabLocation;

        [SerializeField] float _minHoverDistance = 0.05f;

        [SerializeField] float _curlDeltaThreshold = 3f;

        [SerializeField] float _curlTimeThreshold = 0.05f;

        [SerializeField] [Min( 1 )] int _iterations = 10;

        [SerializeField] [Interface( typeof(IActiveState) )] [Optional]
        UnityEngine.Object _grabPrerequisite;

        public event Action WhenFingerLocked = delegate() { };

        Vector3 _saveOffset = Vector3.zero;

        Vector3 GrabOffset = Vector3.zero;
        Vector3 GrabPosition => _grabLocation.position;
        Quaternion GrabRotation => _grabLocation.rotation;

        protected IActiveState GrabPrerequisite = null;

        protected bool grabActive = false;
        protected float ungrabTime = 0;
        protected const float ungrabThreshold = 0.03f;

        public bool DisableHandsMomentarily = false;

        class FingerStatus
        {
            public bool Locked = false;
            public bool Selecting = false;
            public HandJointId[] Joints;
            public Pose[] LocalJoints;
            public float CurlValueAtLock = 0f;
            public float Timer = 0f;
        }

        FingerStatus[] _fingerStatuses;
        TouchShadowHand _touchShadowHand;
        ShadowHand _fromShadow;
        ShadowHand _toShadow;
        ShadowHand _openShadow;
        Func<float> _timeProvider;
        bool _firstSelect = false;
        float _previousTime;
        float _deltaTime;

        protected override void Awake()
        {
            base.Awake();
            Hand = _hand as IHand;
            OpenHand = _openHand as IHand;
            HandSphereMap = _handSphereMap as IHandSphereMap;
            GrabPrerequisite = _grabPrerequisite as IActiveState;

            _fingerStatuses = new FingerStatus[Constants.NUM_FINGERS];
            for ( int i = 0; i < Constants.NUM_FINGERS; i++ )
            {
                int[] jointIndices = FingersMetadata.FINGER_TO_JOINT_INDEX[i];
                HandJointId[] joints = new HandJointId[jointIndices.Length];
                for ( int j = 0; j < jointIndices.Length; j++ )
                {
                    joints[j] = FingersMetadata.HAND_JOINT_IDS[jointIndices[j]];
                }

                _fingerStatuses[i] = new FingerStatus()
                {
                    Joints = joints,
                    LocalJoints = new Pose[joints.Length]
                };
            }

            _timeProvider = () => Time.time;
        }

        protected override void Start()
        {
            base.Start();
            this.AssertField( _hoverLocation, nameof(_hoverLocation) );
            this.AssertField( _grabLocation, nameof(_grabLocation) );
            this.AssertField( Hand, nameof(Hand) );
            this.AssertField( OpenHand, nameof(OpenHand) );
            this.AssertField( HandSphereMap, nameof(HandSphereMap) );
            this.AssertIsTrue( _iterations > 0,
                $"{AssertUtils.Nicify( nameof(_iterations) )} must be bigger than {0}." );

            _touchShadowHand = new TouchShadowHand( HandSphereMap, Hand.Handedness, _iterations );
            _fromShadow = new ShadowHand();
            _toShadow = new ShadowHand();
            _openShadow = new ShadowHand();
            _fromShadow.FromHand( Hand );
            _toShadow.FromHand( Hand );
            _previousTime = _timeProvider();
            _deltaTime = 0;
        }

        public bool IsFingerLocked(HandFinger finger)
        {
            if ( State == InteractorState.Select && _selectedInteractable == null )
            {
                return false;
            }

            return _fingerStatuses[(int)finger].Locked;
        }

        public Pose[] GetFingerJoints(HandFinger finger)
        {
            return _fingerStatuses[(int)finger].LocalJoints;
        }

        protected override void DoPreprocess()
        {
            base.DoPreprocess();
            _toShadow.FromHand( Hand );

            float currentTime = _timeProvider();
            _deltaTime = _timeProvider() - _previousTime;
            _previousTime = currentTime;

            if ( GrabPrerequisite != null )
            {
                if ( GrabPrerequisite.Active )
                {
                    ungrabTime = 0;
                    grabActive = true;
                }
                else
                {
                    if ( grabActive != ungrabTime <= ungrabThreshold )
                    {
                        grabActive = ungrabTime <= ungrabThreshold;
                    }

                    ungrabTime += _deltaTime;
                }
            }
        }

        protected override void DoPostprocess()
        {
            if ( State != InteractorState.Select && _interactable != null )
            {
                _fromShadow.FromHand( Hand );
            }
            else
            {
                _fromShadow.FromHandRoot( Hand );
                for ( int j = 0; j < Constants.NUM_FINGERS; j++ )
                {
                    FingerStatus fingerStatus = _fingerStatuses[j];
                    if ( !fingerStatus.Locked )
                    {
                        for ( int i = 0; i < fingerStatus.Joints.Length; i++ )
                        {
                            HandJointId jointId = fingerStatus.Joints[i];
                            if ( Hand.GetJointPoseLocal( jointId, out Pose localPose ) )
                            {
                                _fromShadow.SetLocalPose( jointId, localPose );
                            }
                        }
                    }
                }
            }

            base.DoPostprocess();
        }

        protected override bool ComputeShouldSelect()
        {
            return HandStatusSelecting();
        }


        protected override bool ComputeShouldUnselect()
        {
            return !HandStatusSelecting();
        }

        protected override void DoHoverUpdate()
        {
            WadaTouchHandGrabInteractable closestInteractable = _interactable;
            if ( closestInteractable == null || State == InteractorState.Disabled )
            {
                return;
            }

            TouchShadowHand.GrabTouchInfo output = new();

            _touchShadowHand.GrabTouch( _fromShadow, _toShadow, closestInteractable.ColliderGroup,
                false, output );
            if ( !output.grabbing )
            {
                _touchShadowHand.GrabTouch( _fromShadow, _toShadow, closestInteractable.ColliderGroup,
                    true, output );
            }

            if ( !output.grabbing )
            {
                return;
            }

            _touchShadowHand.SetShadowRootFromHands( _fromShadow, _toShadow, output.grabT );
            // for ( int i = 0; i < _fingerStatuses.Length; i++ )
            // {
            //     FingerStatus fingerStatus = _fingerStatuses[i];
            //     ComputeNewTouching( i, _interactable.ColliderGroup, output.offset );
            //
            //     // We are overlapping at start, try pushout
            //     if ( output.grabbingFingers[i] && !_fingerStatuses[i].Locked )
            //     {
            //         _openShadow.FromHand( OpenHand, OpenHand.Handedness != Hand.Handedness );
            //         if ( !_touchShadowHand.PushoutFinger( i, _fromShadow, _openShadow,
            //                 _interactable.ColliderGroup, output.offset ) )
            //         {
            //             continue;
            //         }
            //
            //         // Save the pre-touching locations
            //         for ( int j = 0; j < fingerStatus.Joints.Length; j++ )
            //         {
            //             HandJointId jointId = fingerStatus.Joints[j];
            //             _fromShadow.SetLocalPose( jointId, _touchShadowHand.ShadowHand.GetLocalPose( jointId ) );
            //         }
            //
            //         ComputeNewTouching( i, _interactable.ColliderGroup, output.offset );
            //     }
            // }

            if ( !HandStatusSelecting() )
            {
                ClearFingerLockStatuses();
            }
            else
            {
                GrabOffset = Vector3.zero;
                _saveOffset = Quaternion.Inverse( GrabRotation ) * output.offset;
                _firstSelect = true;
            }

            WhenFingerLocked();
        }

        bool MeetsGrabPrerequisite()
        {
            if ( !DisableHandsMomentarily && (GrabPrerequisite == null || grabActive) )
            {
                return true;
            }

            return false;
        }

        bool HandStatusSelecting()
        {
            return MeetsGrabPrerequisite();
            // return MeetsGrabPrerequisite() &&
            //        _fingerStatuses[0].Selecting &&
            //        (_fingerStatuses[1].Selecting ||
            //         _fingerStatuses[2].Selecting ||
            //         _fingerStatuses[3].Selecting ||
            //         _fingerStatuses[4].Selecting);
        }

        // Given a touch hand root, conform fingers from _fromShadow to _toShadow until touching
        void ComputeNewTouching(int idx, ColliderGroup colliderGroup, Vector3 offset)
        {
            FingerStatus fingerStatus = _fingerStatuses[idx];

            // Ignore locked
            if ( fingerStatus.Locked )
            {
                return;
            }

            _touchShadowHand.SetShadowFingerFrom( idx, _fromShadow );

            // Check if finger is starting within
            if ( _touchShadowHand.CheckFingerTouch( idx, 0, colliderGroup, offset, null ) )
            {
                return;
            }

            if ( !_touchShadowHand.GrabConformFinger( idx, _fromShadow, _toShadow, colliderGroup, offset ) )
            {
                return;
            }

            // We are touching, lock the finger
            fingerStatus.Locked = true;
            fingerStatus.Selecting = true;
            fingerStatus.Timer = 0f;

            // Save the locked joints
            _touchShadowHand.GetJointsFromShadow( fingerStatus.Joints, fingerStatus.LocalJoints, true );

            // Save the curl value of the conformed finger
            Pose[] worldPoses = new Pose[fingerStatus.Joints.Length];
            for ( int i = 0; i < fingerStatus.Joints.Length; i++ )
            {
                worldPoses[i] = _touchShadowHand.ShadowHand.GetWorldPose( fingerStatus.Joints[i] );
            }

            fingerStatus.CurlValueAtLock = FingerShapes.PosesListCurlValue( worldPoses );

            // Save the touching locations
            for ( int i = 0; i < fingerStatus.Joints.Length; i++ )
            {
                HandJointId jointId = fingerStatus.Joints[i];
                _fromShadow.SetLocalPose( jointId, _touchShadowHand.ShadowHand.GetLocalPose( jointId ) );
            }
        }

        void ComputeNewRelease(int idx, ColliderGroup colliderGroup, Vector3 offset)
        {
            FingerStatus fingerStatus = _fingerStatuses[idx];
            // Ignore unlocked
            if ( !fingerStatus.Locked )
            {
                return;
            }

            Pose[] worldPoses = new Pose[fingerStatus.Joints.Length];
            for ( int i = 0; i < fingerStatus.Joints.Length; i++ )
            {
                worldPoses[i] = _toShadow.GetWorldPose( fingerStatus.Joints[i] );
            }

            // If not curling out, return
            float toCurl = FingerShapes.PosesListCurlValue( worldPoses );
            if ( toCurl >= fingerStatus.CurlValueAtLock - _curlDeltaThreshold )
            {
                fingerStatus.Timer = 0f;
                return;
            }

            // Check if finger releases
            if ( !_touchShadowHand.GrabReleaseFinger( idx, _fromShadow, _toShadow, colliderGroup, offset ) )
            {
                fingerStatus.Timer = 0f;
                return;
            }

            fingerStatus.Timer += _deltaTime;
            if ( fingerStatus.Timer < _curlTimeThreshold )
            {
                return;
            }

            // If so, unlock
            fingerStatus.Locked = false;
            fingerStatus.Selecting = false;
        }

        protected override void DoSelectUpdate()
        {
            if ( _firstSelect )
            {
                GrabOffset = _saveOffset;
                _saveOffset = Vector3.zero;
                _firstSelect = false;
                return;
            }

            WadaTouchHandGrabInteractable interactable = _selectedInteractable;
            if ( interactable == null )
            {
                for ( int i = 0; i < _fingerStatuses.Length; i++ )
                {
                    FingerStatus fingerStatus = _fingerStatuses[i];
                    if ( !fingerStatus.Locked )
                    {
                        continue;
                    }

                    fingerStatus.Selecting = true;
                    fingerStatus.Locked = false;
                    fingerStatus.Timer = 0f;

                    Pose[] worldPoses = new Pose[fingerStatus.Joints.Length];
                    for ( int j = 0; j < fingerStatus.Joints.Length; j++ )
                    {
                        worldPoses[j] = _toShadow.GetWorldPose( fingerStatus.Joints[j] );
                    }

                    fingerStatus.CurlValueAtLock = FingerShapes.PosesListCurlValue( worldPoses );
                }

                for ( int i = 0; i < _fingerStatuses.Length; i++ )
                {
                    FingerStatus fingerStatus = _fingerStatuses[i];
                    if ( !fingerStatus.Selecting )
                    {
                        continue;
                    }

                    Pose[] worldPoses = new Pose[fingerStatus.Joints.Length];
                    for ( int j = 0; j < fingerStatus.Joints.Length; j++ )
                    {
                        worldPoses[j] = _toShadow.GetWorldPose( fingerStatus.Joints[j] );
                    }

                    float curlValue = FingerShapes.PosesListCurlValue( worldPoses );
                    if ( curlValue >= fingerStatus.CurlValueAtLock - _curlDeltaThreshold )
                    {
                        fingerStatus.Timer = 0f;
                        continue;
                    }

                    fingerStatus.Timer += _deltaTime;
                    if ( fingerStatus.Timer < _curlTimeThreshold )
                    {
                        return;
                    }

                    fingerStatus.Selecting = false;
                }

                return;
            }

            _touchShadowHand.ShadowHand.Copy( _fromShadow );

            _touchShadowHand.SetShadowRootFromHand( _fromShadow );
            if ( MeetsGrabPrerequisite() )
            {
                for ( int i = 0; i < _fingerStatuses.Length; i++ )
                {
                    if ( _fingerStatuses[i].Locked )
                    {
                        ComputeNewRelease( i, interactable.ColliderGroup, Vector3.zero );
                    }
                    else
                    {
                        //ComputeNewTouching( i, interactable.ColliderGroup, Vector3.zero );
                    }
                }
            }

            WhenFingerLocked();
        }

        public override void Unselect()
        {
            if ( !ShouldUnselect )
            {
                base.Unselect();
                return;
            }

            ClearFingerLockStatuses();

            GrabOffset = Vector3.zero;

            WhenFingerLocked();

            base.Unselect();
        }

        void ClearFingerLockStatuses()
        {
            for ( int i = 0; i < _fingerStatuses.Length; i++ )
            {
                _fingerStatuses[i].Locked = false;
                _fingerStatuses[i].Selecting = false;
            }
        }

        protected override WadaTouchHandGrabInteractable ComputeCandidate()
        {
            WadaTouchHandGrabInteractable closest = null;
            float minSqrDist = float.MaxValue;
            foreach ( WadaTouchHandGrabInteractable interactable in WadaTouchHandGrabInteractable.Registry
                         .List() )
            {
                if ( interactable.enabled && interactable.ColliderGroup != null &&
                     interactable.ColliderGroup.Colliders != null )
                {
                    foreach ( Collider collider in interactable.ColliderGroup.Colliders )
                    {
                        if ( collider.enabled )
                        {
                            Vector3 closestPoint = collider.ClosestPoint( _hoverLocation.position );
                            float sqrDist = (closestPoint - _hoverLocation.position).sqrMagnitude;
                            if ( sqrDist < minSqrDist && sqrDist < _minHoverDistance * _minHoverDistance )
                            {
                                minSqrDist = sqrDist;
                                closest = interactable;
                            }
                        }
                    }
                }
            }

            return closest;
        }

        protected override Pose ComputePointerPose()
        {
            return new Pose( GrabPosition + GrabRotation * GrabOffset, GrabRotation );
        }

        #region Inject

        public void InjectAllTouchHandGrabInteractor(
            IHand hand,
            IHand openHand,
            IHandSphereMap handSphereMap,
            Transform hoverLocation,
            Transform grabLocation)
        {
            InjectHand( hand );
            InjectOpenHand( openHand );
            InjectHandSphereMap( handSphereMap );
            InjectHoverLocation( hoverLocation );
            InjectGrabLocation( grabLocation );
        }

        public void InjectHand(IHand hand)
        {
            Hand = hand;
            _hand = hand as UnityEngine.Object;
        }

        public void InjectOpenHand(IHand openHand)
        {
            OpenHand = openHand;
            _openHand = openHand as UnityEngine.Object;
        }

        public void InjectHandSphereMap(IHandSphereMap handSphereMap)
        {
            HandSphereMap = handSphereMap;
            _handSphereMap = handSphereMap as UnityEngine.Object;
        }

        public void InjectHoverLocation(Transform hoverLocation)
        {
            _hoverLocation = hoverLocation;
        }

        public void InjectGrabLocation(Transform grabLocation)
        {
            _grabLocation = grabLocation;
        }

        public void InjectOptionalGrabPrerequisite(IActiveState grabPrerequisite)
        {
            GrabPrerequisite = grabPrerequisite;
            _grabPrerequisite = grabPrerequisite as UnityEngine.Object;
        }

        public void InjectOptionalMinHoverDistance(float minHoverDistance)
        {
            _minHoverDistance = minHoverDistance;
        }

        public void InjectOptionalCurlDeltaThreshold(float threshold)
        {
            _curlDeltaThreshold = threshold;
        }

        public void InjectOptionalCurlTimeThreshold(float seconds)
        {
            _curlTimeThreshold = seconds;
        }

        public void InjectOptionalIterations(int iterations)
        {
            _iterations = iterations;
        }

        public void InjectOptionalTimeProvider(Func<float> timeProvider)
        {
            _timeProvider = timeProvider;
        }

        #endregion
    }
}