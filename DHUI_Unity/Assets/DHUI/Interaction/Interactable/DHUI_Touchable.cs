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

        #region Fields

        #region Fields.Setup
        [Header("Touchable.Setup")]
        [SerializeField]
        protected DHUI_InteractionManager m_interactionManager = null;
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
        [SerializeField]
        protected float _maxRetargetingDistance = 1;
        #endregion Fields.Settings

        #region Fields.Points
        [Header("Touchable.Points")]
        [SerializeField]
        protected Transform m_centerPoint = null;
        [SerializeField]
        protected List<Transform> m_touchableBounds = new List<Transform>();
        #endregion Fields.Points

        #endregion Fields
        
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

        public bool IsDisabled
        {
            get;
            private set;
        }


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

        private float lastTouched = 0f;

        private float currentTouchDuration {
            get { return Time.time - lastTouched; }
        }


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

        public void Disable()
        {
            IsDisabled = true;
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

        #region OnHover

        public virtual void Hover_Start(DHUI_HoverEventArgs _hoverEventArgs)
        {
            OnHoverStart?.Invoke(_hoverEventArgs);
            SentDroneToInitialPosition();
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

        #region OnTouch

        public virtual void Touch_Start(DHUI_TouchEventArgs _touchEventArgs)
        {
            OnTouchStart?.Invoke(_touchEventArgs);
        }

        public virtual void Touch_Stay(DHUI_TouchEventArgs _touchEventArgs)
        {
            OnTouchStay?.Invoke(_touchEventArgs);
        }

        public virtual void Touch_End(DHUI_TouchEventArgs _touchEventArgs)
        {
            OnTouchEnd?.Invoke(_touchEventArgs);
        }

        protected DHUI_TouchEventArgs ConstructTouchEventArgs()
        {
            DHUI_TouchEventArgs args = new DHUI_TouchEventArgs();
            args.TouchDuration = currentTouchDuration;
            return args;
        }

        #endregion OnTouch

        protected void SentDroneToInitialPosition()
        {
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_centerPoint.position, m_centerPoint.rotation, _initialPositioningDroneSpeed);
            m_interactionManager.FlightController.AddToFrontOfQueue(cmd, true, true);
        }

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

        protected virtual void UpdateHapticRetargeting(DHUI_HoverEventArgs _hoverEvent)
        {

            //TODO
        }
        

    }

}