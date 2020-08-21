using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DHUI.Core;
using UnityEngine.Events;
using System;

namespace DHUI
{
    public class DHUI_Button : DHUI_Interactable
    {
        #region Fields

        #region Inspector Setup
        [Header("Button.Setup")]
        [SerializeField]
        protected Transform m_centerPoint_StaticPart = null;
        [SerializeField]
        protected Transform m_centerPoint_MovingPart = null;
        [SerializeField]
        protected Transform m_buttonPressValue_point1;
        [SerializeField]
        protected Transform m_buttonPressValue_point2;
        [SerializeField]
        protected Transform m_droneTargetPoint = null;

        [Header("Button.GeneralSettings")]
        [SerializeField]
        protected bool _hover_setDroneTargetToHoverPoint = false;
        [SerializeField]
        protected float _hover_touchThreshold = 0.1f;
        [SerializeField]
        protected GlobalLocalMode _activationDistance_mode = GlobalLocalMode.Global;
        [SerializeField]
        protected float _activationDistance_threshold = 0.15f;
        [SerializeField]
        protected float _activationCooldown = 0.5f;

        [Header("Button.DroneSettings")]
        [SerializeField]
        protected float _droneSpeed = 2f;
        [SerializeField][Range(0,2)]
        protected float _droneResistance = 1f;
        [SerializeField]
        protected bool _lockDroneXYWhilePressed = true;


        [Header("Button.HapticRetargetingSettings")]
        [SerializeField]
        protected HapticRetargetingMode _hapticRetargetingMode = HapticRetargetingMode.NoRetargeting;
        [SerializeField]
        protected float _retargetingActivationDistance = 1;

        #endregion Inspector Setup

        #region Inspector Events
        [Serializable]
        public class DHUI_ButtonActivationEvent : UnityEvent<DHUI_ButtonActivationEventArgs> { }

        [Header("Button.Events")]
        public DHUI_ButtonActivationEvent OnActivationStart = null;
        public DHUI_ButtonActivationEvent OnActivationStay = null;
        public DHUI_ButtonActivationEvent OnActivationEnd = null;

        #endregion Inspector Events

        #region Enums

        public enum ButtonInternalStates
        {
            Inactive, Hovered, Touched, Pressed, Activated
        }

        public enum GlobalLocalMode
        {
            Global, Local
        }

        public enum HapticRetargetingMode
        {
            NoRetargeting, CenterToCenter
        }

        #endregion Enums

        #region Structs

        public struct DHUI_ButtonActivationEventArgs
        {
            public float activationDuration;
        }

        #endregion Structs

        #region States

        private ButtonInternalStates internal_buttonState;
        /// <summary>
        /// Current Internal State of the button.
        /// </summary>
        public ButtonInternalStates ButtonInternalState
        {
            get
            {
                return internal_buttonState;
            }
            protected set
            {
                if (internal_buttonState != value)
                {
                    if (value == ButtonInternalStates.Activated)
                    {
                        float currentTime = Time.time;
                        if (currentTime >= lastActivationTime + _activationCooldown)
                        {
                            lastActivationTime = currentTime;
                            ButtonActivationState = true;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else if (internal_buttonState == ButtonInternalStates.Activated && value != internal_buttonState)
                    {
                        ButtonActivationState = false;
                    }

                    internal_buttonState = value;
                }
            }
        }

        private bool internal_buttonActivationState = false;

        /// <summary>
        /// Current Activation State of the button. (true -> Activated, false -> Not activated).
        /// </summary>
        public bool ButtonActivationState
        {
            get
            {
                return internal_buttonActivationState;
            }
            protected set
            {
                if (value == internal_buttonActivationState) return;
                if (value)
                {
                    currentActivationDuration = 0;
                    Activation_Start(ConstructActivationEventArgs());
                }
                else
                {
                    Activation_End(ConstructActivationEventArgs());
                }

                internal_buttonActivationState = value;
            }
        }

        #endregion States

        #region Protected
        
        /// <summary>
        /// Current Distance between 'm_buttonPressValue_point1' and 'm_buttonPressValue_point2'. This represents the current value of the button press.
        /// </summary>
        protected float currentActivationDistance = 0f;

        /// <summary>
        /// Contact plane of the static part of the button.
        /// </summary>
        protected Utils.MathPlane ContactPlane_StaticPart
        {
            get { return new Utils.MathPlane(m_centerPoint_StaticPart); }
        }
        /// <summary>
        /// Contact plane of the moving Part of the button.
        /// </summary>
        protected Utils.MathPlane ContactPlane_MovingPart
        {
            get { return new Utils.MathPlane(m_centerPoint_MovingPart); }
        }
        /// <summary>
        /// Current Distance between the Button ('ContactPlane') and Hand ('_hoverEvent.InteractorPosition').
        /// </summary>
        protected float hover_distance_staticPart = 0f;

        /// <summary>
        /// Current Distance between the Button's MovingPart ('ContactPlane_MovingPart') and Hand ('_hoverEvent.InteractorPosition').
        /// </summary>
        protected float hover_distance_movingPart = 0f;

        /// <summary>
        /// Projected Point of the Hand ('_hoverEvent.InteractorPosition') on the Button ('ContactPlane')
        /// </summary>
        protected Vector3 hover_projectedPoint_staticPart = Vector3.zero;
        
        /// <summary>
        /// Projected Point of the Hand ('_hoverEvent.InteractorPosition') on the Button's Moving Part ('ContactPlane_MovingPart')
        /// </summary>
        protected Vector3 hover_projectedPoint_movingPart = Vector3.zero;
        
        #endregion Protected

        #region Private
        /// <summary>
        /// Time of last button activation. Used to implement Activation-Cooldown.
        /// </summary>
        private float lastActivationTime = 0f;

        /// <summary>
        /// Duration of current activation/ duration of button being pressed (in seconds).
        /// </summary>
        private float currentActivationDuration = 0f;

        /// <summary>
        /// Saved last Position of the Interactor. Used when drone's XY should get locked while button is pressed.
        /// </summary>
        private Vector3 lastInteractorPos = Vector3.zero;

        #endregion Private

        #endregion Fields

        #region Methods
        
        #region Hover

        public override void Hover_Start(DHUI_HoverEventArgs _hoverEvent)
        {
            base.Hover_Start(_hoverEvent);
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_centerPoint_StaticPart.position, m_centerPoint_StaticPart.rotation, _droneSpeed);
            m_interactionManager.FlightController.AddToFrontOfQueue(cmd, true, true);
            
            UpdateHapticRetargeting(_hoverEvent);
        }

        public override void Hover_End(DHUI_HoverEventArgs _hoverEvent)
        {
            base.Hover_End(_hoverEvent);
            ButtonInternalState = ButtonInternalStates.Inactive;
        }

        public override void Hover_Stay(DHUI_HoverEventArgs _hoverEvent)
        {
            base.Hover_Stay(_hoverEvent);
            
            UpdateHoverCalculations(_hoverEvent);
            UpdateActivationCalculations(_hoverEvent);
            UpdateState(_hoverEvent);
            
            UpdateDroneTargetPoint();
            UpdateFlightController();
        }

        protected virtual void UpdateHoverCalculations(DHUI_HoverEventArgs _hoverEvent)
        {
            Vector3 interactorPos = _hoverEvent.InteractorPosition;

            if (_lockDroneXYWhilePressed && (ButtonInternalState == ButtonInternalStates.Pressed || ButtonInternalState == ButtonInternalStates.Activated))
            {
                interactorPos = lastInteractorPos;
            }

            hover_distance_staticPart = Mathf.Abs(ContactPlane_StaticPart.GetDistance(interactorPos));
            hover_projectedPoint_staticPart = ContactPlane_StaticPart.GetProjectedPoint(interactorPos);
            hover_distance_movingPart = Mathf.Abs(ContactPlane_MovingPart.GetDistance(interactorPos));
            hover_projectedPoint_movingPart = ContactPlane_MovingPart.GetProjectedPoint(interactorPos);

            lastInteractorPos = interactorPos;
        }
        
        protected virtual void UpdateState(DHUI_HoverEventArgs _hoverEvent)
        {
            if (ContactPlane_StaticPart.PointInFrontOfPlane(_hoverEvent.InteractorPosition))
            {
                if (hover_distance_staticPart < _hover_touchThreshold)
                {
                    ButtonInternalState = ButtonInternalStates.Touched;
                    m_interactionManager.HapticRetargeting?.HoldRetargeting();
                }
                else
                {
                    ButtonInternalState = ButtonInternalStates.Hovered;

                    m_interactionManager.HapticRetargeting?.UnholdRetargeting();
                }
            }
            else
            {
                if (currentActivationDistance >= _activationDistance_threshold)
                {
                    ButtonInternalState = ButtonInternalStates.Activated;
                }
                else
                {
                    ButtonInternalState = ButtonInternalStates.Pressed;
                }
            }
            
        }

        protected virtual void UpdateDroneTargetPoint()
        {
            if (!_hover_setDroneTargetToHoverPoint) return;
            Vector3 calculatedPos = hover_projectedPoint_movingPart + (hover_projectedPoint_staticPart - hover_projectedPoint_movingPart) * _droneResistance;

            float minX = CenterPoint.x - transform.localScale.x * 0.5f;
            float maxX = CenterPoint.x + transform.localScale.x * 0.5f;
            float minY = CenterPoint.y - transform.localScale.y * 0.5f;
            float maxY = CenterPoint.y + transform.localScale.y * 0.5f;
            if (calculatedPos.x < minX) calculatedPos.x = minX;
            if (calculatedPos.x > maxX) calculatedPos.x = maxX;
            if (calculatedPos.y < minY) calculatedPos.y = minY;
            if (calculatedPos.y > maxY) calculatedPos.y = maxY;

            m_droneTargetPoint.transform.position = calculatedPos;
        }

        protected virtual void UpdateFlightController()
        {
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_droneTargetPoint.position, m_droneTargetPoint.rotation, _droneSpeed);
            m_interactionManager.FlightController.AddToFrontOfQueue(cmd, true, true);
        }

        #endregion

        #region HapticRetargeting
        
        protected virtual void UpdateHapticRetargeting(DHUI_HoverEventArgs _hoverEvent)
        {
            switch (_hapticRetargetingMode)
            {
                case HapticRetargetingMode.CenterToCenter:
                    HapticRetargeting_CenterToCenter();
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
            m_interactionManager.HapticRetargeting?.SetTargets(m_centerPoint_StaticPart, m_interactionManager.DroneController.contactPointTransform);
            m_interactionManager.HapticRetargeting?.EnableRetargeting();
        }

        #endregion HapticRetargeting

        #region Activation
        
        protected virtual void UpdateActivationCalculations(DHUI_HoverEventArgs _hoverEvent)
        {
            if (_activationDistance_mode == GlobalLocalMode.Global)
            {
                currentActivationDistance = Vector3.Distance(m_buttonPressValue_point1.position, m_buttonPressValue_point2.position);
            }
            else
            {
                currentActivationDistance = Vector3.Distance(m_buttonPressValue_point1.localPosition, m_buttonPressValue_point2.localPosition);
            }
        }

        public virtual void Activation_Start(DHUI_ButtonActivationEventArgs _buttonActivationEventArgs)
        {
            OnActivationStart?.Invoke(_buttonActivationEventArgs);
        }

        public virtual void Activation_End(DHUI_ButtonActivationEventArgs _buttonActivationEventArgs)
        {
            OnActivationEnd?.Invoke(_buttonActivationEventArgs);
        }

        public virtual void Activation_Stay(DHUI_ButtonActivationEventArgs _buttonActivationEventArgs)
        {
            OnActivationStay?.Invoke(_buttonActivationEventArgs);
        }

        protected DHUI_ButtonActivationEventArgs ConstructActivationEventArgs()
        {
            DHUI_ButtonActivationEventArgs args = new DHUI_ButtonActivationEventArgs();
            args.activationDuration = currentActivationDuration;
            return args;
        }

        protected void UpdateActivation()
        {
            if (ButtonActivationState)
            {
                currentActivationDuration = Time.time - lastActivationTime;
                Activation_Stay(ConstructActivationEventArgs());
            }
        }

        #endregion Activation
        
        protected void Update()
        {
            UpdateActivation();
        }

        #endregion Methods
    }

}