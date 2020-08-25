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
            NoRetargeting, CenterToCenter, Encapsulate
        }

        #endregion Classes & Structs

        #region Inspector Fields

        #region Fields.Setup
        [Header("Touchable.Setup")]
        [SerializeField]
        protected DHUI_InteractionManager m_interactionManager = null;
        [SerializeField]
        protected Transform m_centerPoint = null;
        [SerializeField]
        protected List<Transform> m_touchableBounds = new List<Transform>();
        #endregion Fields.Setup

        #region Fields.Events
        [Header("Touchable.Events")]
        public DHUI_HoverEvent OnHoverStart = null;
        public DHUI_HoverEvent OnHoverStay = null;
        public DHUI_HoverEvent OnHoverEnd = null;

        public DHUI_TouchEvent OnTouchStart = null;
        public DHUI_TouchEvent OnTouchStay = null;
        public DHUI_TouchEvent OnTouchEnd = null;
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
        protected float _initialPositioningDroneSpeed = 0.5f;
        [SerializeField]
        protected HapticRetargetingMode _hapticRetargetingMode = HapticRetargetingMode.NoRetargeting;
        [SerializeField]
        protected float _retargetingActivationDistance = 1;
        /*
        [SerializeField]
        protected float _maxRetargetingDistance = 1;*/
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
                        if (currentTime >= lastTouched + _touchCooldown)
                        {
                            lastTouched = currentTime;
                            TouchedState = true;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (internal_touchableState == TouchableInternalStates.Touch && value != internal_touchableState)
                    {
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

        #region Private Variables

        private float lastTouched = 0f;

        private float currentTouchDuration {
            get { return Time.time - lastTouched; }
        }

        private bool hapticRetargetingActive = false;

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
            UpdateHapticRetargeting(_hoverEventArgs);
            if (TouchedState)
            {
                Touch_Stay(ConstructTouchEventArgs());
            }
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
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_centerPoint.position, m_centerPoint.rotation, _initialPositioningDroneSpeed);
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

        #endregion States

        #region Haptic Retargeting

        protected virtual void SetupHapticRetargeting(DHUI_HoverEventArgs _hoverEvent)
        {
            switch (_hapticRetargetingMode)
            {
                case HapticRetargetingMode.CenterToCenter:
                    HapticRetargeting_CenterToCenter();
                    break;
                case HapticRetargetingMode.Encapsulate:
                    HapticRetargeting_Encapsulate();
                    break;
                case HapticRetargetingMode.NoRetargeting:
                default:
                    HapticRetargeting_NoRetargeting();
                    break;
            }
        }

        protected virtual void HapticRetargeting_NoRetargeting()
        {
            m_interactionManager.HapticRetargeting?.DisableRetargeting();
        }

        protected virtual void HapticRetargeting_CenterToCenter()
        {
            m_interactionManager.HapticRetargeting?.SetActivationDistance(_retargetingActivationDistance);
            m_interactionManager.HapticRetargeting?.SetTargets(m_centerPoint, m_interactionManager.DroneController.contactPointTransform);
            m_interactionManager.HapticRetargeting?.EnableRetargeting();
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
                // TODO: Retarget Z only, since xy are inside the touchable surface of the drone
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

                m_interactionManager.HapticRetargeting?.SetActivationDistance(_retargetingActivationDistance);
                m_interactionManager.HapticRetargeting?.SetTargets(m_touchableBounds[closestDronePointIndex], droneBoundingBox[closestDronePointIndex]);
                m_interactionManager.HapticRetargeting?.EnableRetargeting();
            }

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

        protected DHUI_TouchEventArgs ConstructTouchEventArgs()
        {
            DHUI_TouchEventArgs args = new DHUI_TouchEventArgs();
            args.TouchDuration = currentTouchDuration;
            return args;
        }

        #endregion Touch
        
    }
}