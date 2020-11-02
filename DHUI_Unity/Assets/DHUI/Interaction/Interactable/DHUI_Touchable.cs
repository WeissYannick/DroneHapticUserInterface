using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;
using DHUI.Core;

namespace DHUI
{
    public class DHUI_Touchable : MonoBehaviour
    {
        #region Classes/Structs/Enums

        [Serializable]
        public class DHUI_HoverEvent : UnityEvent<DHUI_HoverEventArgs> { }
        [Serializable]
        public class DHUI_TouchEvent : UnityEvent<DHUI_TouchEventArgs> { }

        public struct DHUI_HoverEventArgs
        {
            public Vector3 InteractorPhysicalPosition;
            public Vector3 InteractorVirtualPosition;
        }

        public struct DHUI_TouchEventArgs
        {
            public float TouchDuration;
        }

        public enum TouchableInternalStates
        {
            Inactive, Hover, NearTouch, Touch
        }

        public enum HapticRetargetingMode
        {
            NoRetargeting, PresetRetargeting, CenterToCenter, Encapsulate, ZOnly
        }
        #endregion Classes/Structs/Enums

        #region Inspector Fields

        #region Fields.Setup
        [Header("Touchable.Setup")]
        [SerializeField]
        protected DHUI_InteractionManager m_interactionManager = null;
        [SerializeField]
        protected Transform m_centerPoint = null;
        [SerializeField]
        protected List<Transform> m_touchableBounds = new List<Transform>();
        [SerializeField]
        protected Transform m_hapticRetargeting_virtualTarget = null;
        [SerializeField]
        protected Transform m_hapticRetargeting_physicalTarget = null;
        [SerializeField]
        protected Transform m_droneTargetPoint = null;
        #endregion Fields.Setup

        #region Fields.Events
        [Header("Touchable.Events")]
        public DHUI_HoverEvent OnHoverStart = null;
        public DHUI_HoverEvent OnHoverStay = null;
        public DHUI_HoverEvent OnHoverEnd = null;

        public DHUI_TouchEvent OnTouchStart = null;
        public DHUI_TouchEvent OnTouchStay = null;
        public DHUI_TouchEvent OnTouchEnd = null;

        public DHUI_HoverEvent OnRetractionEnd = null;
        #endregion Fields.Events

        #region Fields.Settings
        [Header("Touchable.Settings")]
        [SerializeField]
        protected bool _manualRegistering = false;
        [SerializeField]
        protected float _nearTouchThreshold = 0.1f;
        [SerializeField]
        protected float _touchThreshold = 0.05f;
        [SerializeField]
        protected float _touchCooldown = 0.5f;
        [SerializeField]
        protected float _maxRetractionTime = 2f;
        [SerializeField]
        protected float _initialPositioningDroneSpeed = 0.5f;
        [SerializeField]
        protected HapticRetargetingMode _hapticRetargetingMode = HapticRetargetingMode.NoRetargeting;
        [SerializeField]
        protected float _retargetingActivationDistance = 1;
        [SerializeField]
        protected float _maxRetargetingDistance = 1;
        #endregion Fields.Settings

        #endregion Inspector Fields

        #region Public Variables

        public Utils.MathPlane StaticContactPlane
        {
            get { return new Utils.MathPlane(m_centerPoint); }
        }

        public Vector3 CenterPoint
        {
            get { return m_centerPoint.position; }
        }

        public List<Vector3> TouchableBounds
        {
            get
            {
                List<Vector3> touchBoundPoints = new List<Vector3>();
                foreach (Transform t in m_touchableBounds)
                {
                    touchBoundPoints.Add(t.position);
                }
                return touchBoundPoints;
            }
        }

        public bool IsEnabled
        {
            get;
            private set;
        } = true;


        private TouchableInternalStates internal_touchableState;
        
        public TouchableInternalStates TouchableInternalState
        {
            get
            {
                return internal_touchableState;
            }
            protected set
            {
                if (internal_touchableState != value)
                {
                    if (value == TouchableInternalStates.Touch)
                    {
                        float currentTime = Time.time;
                        if (currentTime >= lastTouchedDown + _touchCooldown)
                        {
                            lastTouchedDown = Time.time;
                            TouchedState = true;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (internal_touchableState == TouchableInternalStates.Touch && value != internal_touchableState)
                    {
                        //handRetracting = true;
                        lastTouchedUp = Time.time;
                        TouchedState = false;
                    }

                    internal_touchableState = value;
                }
            }
        }

        private bool internal_touchedState = false;
        
        public bool TouchedState
        {
            get
            {
                return internal_touchedState;
            }
            protected set
            {
                if (value == internal_touchedState) return;
                if (value)
                {
                    Touch_Start(ConstructTouchEventArgs());
                }
                else
                {
                    Touch_End(ConstructTouchEventArgs());
                }

                internal_touchedState = value;
            }
        }

        #endregion Public Variables

        #region Private/Protected Variables
        
        protected float lastTouchedDown = 0f;

        protected float lastTouchedUp = 0f;

        protected float currentTouchDuration {
            get { return Time.time - lastTouchedDown; }
        }

        protected bool handRetracting = false;

        #endregion Private Variables

        #region MonoBehaviour-Methods

        protected virtual void Start()
        {
            if (m_interactionManager == null)
            {
                m_interactionManager = FindObjectOfType<DHUI_InteractionManager>();
            }
            if (m_centerPoint == null)
            {
                m_centerPoint = transform;
            }
        }

        protected void OnEnable()
        {
            if (!_manualRegistering)
            {
                Register();
            }
        }

        protected void OnDisable()
        {
            if (!_manualRegistering)
            {
                Deregister();
            }
        }

        protected void OnDestroy()
        {
            if (!_manualRegistering)
            {
                Deregister();
            }
        }

        #endregion MonoBehaviour-Methods

        #region Registering

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        public void Register()
        {
            if (m_interactionManager == null)
            {
                m_interactionManager = FindObjectOfType<DHUI_InteractionManager>();
            }
            m_interactionManager?.RegisterTouchable(this);
        }

        public void Deregister()
        {
            if (m_interactionManager == null)
            {
                m_interactionManager = FindObjectOfType<DHUI_InteractionManager>();
            }
            m_interactionManager?.DeregisterTouchable(this);
        }

        #endregion Registering

        #region Hover

        #region OnHover

        public virtual void Hover_Start(DHUI_HoverEventArgs _hoverEventArgs)
        {
            OnHoverStart?.Invoke(_hoverEventArgs);
            SentDroneToInitialPosition();
            SetupHapticRetargeting(_hoverEventArgs);
        }

        public virtual void Hover_Stay(DHUI_HoverEventArgs _hoverEventArgs)
        {
            OnHoverStay?.Invoke(_hoverEventArgs);
            UpdateTouchableStates(_hoverEventArgs);
            UpdateHandRetracting(_hoverEventArgs);
            UpdateHapticRetargeting(_hoverEventArgs);
            UpdateTouchStay();
        }

        public virtual void Hover_End(DHUI_HoverEventArgs _hoverEventArgs)
        {
            OnHoverEnd?.Invoke(_hoverEventArgs);
            TouchableInternalState = TouchableInternalStates.Inactive;
        }

        #endregion OnHover

        #region Drone Methods

        protected void SentDroneToInitialPosition()
        {
            if (m_droneTargetPoint == null)
            {
                m_droneTargetPoint = new GameObject("DroneTargetPoint").transform;
                m_droneTargetPoint.transform.parent = transform;
                m_droneTargetPoint.transform.position = m_centerPoint.position;
                m_droneTargetPoint.transform.rotation = m_centerPoint.rotation;
            }
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_droneTargetPoint.position, m_droneTargetPoint.rotation, _initialPositioningDroneSpeed);
            m_interactionManager.FlightController.AddToFrontOfQueue(cmd, true, true);
        }

        protected void UpdateDronePosition(float _speed = 0.5f)
        {
            if (m_droneTargetPoint == null) return;
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_droneTargetPoint.position, m_droneTargetPoint.rotation, _speed);
            m_interactionManager.FlightController.AddToFrontOfQueue(cmd, true, true);
        }

        #endregion Drone Methods

        #region States

        protected virtual void UpdateTouchableStates(DHUI_HoverEventArgs _hoverEvent)
        {
            float dist = StaticContactPlane.GetDistance(_hoverEvent.InteractorVirtualPosition);

            if (dist <= _touchThreshold)
            {
                TouchableInternalState = TouchableInternalStates.Touch;
            }
            else if (dist <= _nearTouchThreshold)
            {
                TouchableInternalState = TouchableInternalStates.NearTouch;
            }
            else
            {
                TouchableInternalState = TouchableInternalStates.Hover;
            }

        }

        protected void UpdateHandRetracting(DHUI_HoverEventArgs _hoverEventArgs)
        {
            if (handRetracting)
            {
                float dist = StaticContactPlane.GetDistance(_hoverEventArgs.InteractorPhysicalPosition);
                if (internal_touchableState != TouchableInternalStates.Touch && ((Time.time >= lastTouchedUp + _maxRetractionTime) || dist >= _retargetingActivationDistance))
                {
                    handRetracting = false;
                    OnRetractionEnd?.Invoke(_hoverEventArgs);
                }
            }
        }

        #endregion States


        #region Haptic Retargeting

        protected virtual void SetupHapticRetargeting(DHUI_HoverEventArgs _hoverEvent)
        {
            m_interactionManager.HapticRetargeting?.SetActivationDistance(_retargetingActivationDistance);
            m_interactionManager.HapticRetargeting?.SetTargets(m_hapticRetargeting_virtualTarget, m_hapticRetargeting_physicalTarget);
            m_interactionManager.HapticRetargeting?.EnableRetargeting();
        }

        protected virtual void UpdateHapticRetargeting(DHUI_HoverEventArgs _hoverEvent)
        {
            if (TouchableInternalState == TouchableInternalStates.Touch)
            {
                m_interactionManager.HapticRetargeting?.HoldRetargeting();
            }
            else
            {
                m_interactionManager.HapticRetargeting?.UnholdRetargeting();
            }

            if (handRetracting)
            {
                m_interactionManager.HapticRetargeting?.LockTargetPositions();
            }
            else
            {
                m_interactionManager.HapticRetargeting?.UnlockTargetPositions();
            }

            switch (_hapticRetargetingMode)
            {
                case HapticRetargetingMode.PresetRetargeting:
                    break;
                case HapticRetargetingMode.CenterToCenter:
                    HapticRetargeting_CenterToCenter();
                    break;
                case HapticRetargetingMode.Encapsulate:
                    HapticRetargeting_Encapsulate();
                    break;
                case HapticRetargetingMode.ZOnly:
                    HapticRetargeting_ZOnly();
                    break;
                case HapticRetargetingMode.NoRetargeting:
                default:
                    HapticRetargeting_NoRetargeting();
                    break;
            }
            if (!handRetracting && TouchableInternalState != TouchableInternalStates.Touch && Vector3.Distance(m_hapticRetargeting_virtualTarget.position, m_hapticRetargeting_physicalTarget.position) > _maxRetargetingDistance && _hapticRetargetingMode != HapticRetargetingMode.NoRetargeting && !TouchedState)
            {
                OverMaxRetargetingDistance();
                return;
            }
            else
            {
                m_interactionManager.HapticRetargeting?.EnableRetargeting();
            }

        }
        
        protected virtual void OverMaxRetargetingDistance()
        {
            // TODO: What to do if retargeting targets are too far apart?
            // -> Idea: Hold out on retargeting and give feedback to user to wait
            m_interactionManager.HapticRetargeting?.DisableRetargeting();
            Debug.LogWarning("<b> DHUI </b> | Touchable | HapticRetargeting-Targets are too far apart. Wait for drone to be closer to the virtual Object.");
        }

        protected virtual void HapticRetargeting_NoRetargeting()
        {
            m_interactionManager.HapticRetargeting?.DisableRetargeting();
        }

        protected virtual void HapticRetargeting_PresetRetargeting()
        {
            // We do nothing here for now.
        }

        protected virtual void HapticRetargeting_CenterToCenter()
        {
            m_hapticRetargeting_virtualTarget.position = m_centerPoint.position;
            m_hapticRetargeting_physicalTarget.position = m_interactionManager.DroneController.contactPointTransform.position;
        }

        protected virtual void HapticRetargeting_Encapsulate()
        {
            List<Transform> droneBoundingBox = m_interactionManager.DroneController.contactFaceBoundingBox;
            float closestDronePointDist = float.MaxValue;
            int closestDronePointIndex = 0;

            List<Vector3> droneProjectedPoints = new List<Vector3>();
            bool inside = true;
            foreach (Transform t in droneBoundingBox)
            {
                droneProjectedPoints.Add(StaticContactPlane.GetProjectedPoint(t.position));
            }
            foreach (Transform t in m_touchableBounds)
            {
                Vector3 touchablePP = StaticContactPlane.GetProjectedPoint(t.position);
                if (touchablePP.x > droneProjectedPoints[0].x && touchablePP.y < droneProjectedPoints[0].y && touchablePP.x < droneProjectedPoints[droneProjectedPoints.Count - 1].x && touchablePP.y > droneProjectedPoints[droneProjectedPoints.Count - 1].y)
                {
                    continue;
                }
                else
                {
                    inside = false;
                    break;
                }
            }
            if (inside)
            {
                HapticRetargeting_ZOnly();
            }
            else
            {
                for (int dronePointCounter = 0; dronePointCounter < droneBoundingBox.Count; dronePointCounter++)
                {
                    float dist = Vector3.Distance(CenterPoint, droneBoundingBox[dronePointCounter].position);
                    if (dist < closestDronePointDist)
                    {
                        closestDronePointIndex = dronePointCounter;
                        closestDronePointDist = dist;
                    }
                }
                
                m_hapticRetargeting_virtualTarget.position = m_touchableBounds[closestDronePointIndex].position;
                m_hapticRetargeting_physicalTarget.position = droneBoundingBox[closestDronePointIndex].position;
            }

        }
        
        protected virtual void HapticRetargeting_ZOnly()
        {
            m_hapticRetargeting_virtualTarget.position = m_centerPoint.position;
            m_hapticRetargeting_physicalTarget.position = m_centerPoint.position + m_interactionManager.DroneController.contactPointTransform.position - StaticContactPlane.GetProjectedPoint(m_interactionManager.DroneController.contactPointTransform.position);

        }


        #endregion Haptic Retargeting

        #endregion Hover

        #region Touch

        protected virtual void Touch_Start(DHUI_TouchEventArgs _touchEventArgs)
        {
            OnTouchStart?.Invoke(_touchEventArgs);
        }

        protected virtual void Touch_Stay(DHUI_TouchEventArgs _touchEventArgs)
        {
            OnTouchStay?.Invoke(_touchEventArgs);
        }

        protected virtual void Touch_End(DHUI_TouchEventArgs _touchEventArgs)
        {
            OnTouchEnd?.Invoke(_touchEventArgs);
        }

        protected void UpdateTouchStay()
        {
            if (TouchedState)
            {
                Touch_Stay(ConstructTouchEventArgs());
            }
        }

        protected DHUI_TouchEventArgs ConstructTouchEventArgs()
        {
            DHUI_TouchEventArgs args = new DHUI_TouchEventArgs();
            args.TouchDuration = currentTouchDuration;
            return args;
        }

        #endregion Touch
        
    }
}