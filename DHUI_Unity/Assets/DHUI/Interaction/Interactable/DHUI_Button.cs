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
        private Transform m_buttonPressValue_point1;
        [SerializeField]
        private Transform m_buttonPressValue_point2;

        [Header("Button.GeneralSettings")]
        [SerializeField]
        private float _hover_touchThreshold = 0.1f;
        [SerializeField]
        private GlobalLocalMode _activationDistance_mode = GlobalLocalMode.Local;
        [SerializeField]
        private float _activationDistance_threshold = 0.15f;
        [SerializeField]
        private float _activationCooldown = 0.5f;

        [Header("Button.DroneSettings")]
        [SerializeField]
        private float _droneSpeed_initialPositioning = 1f;

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
        /// Current Distance between the Button ('ContactPlane') and Hand ('_hoverEvent.InteractorPosition').
        /// </summary>
        protected float hover_distance = 0f;

        /// <summary>
        /// Projected Point of the Hand ('_hoverEvent.InteractorPosition') on the Button ('ContactPlane')
        /// </summary>
        protected Vector3 hover_projectedPoint = Vector3.zero;

        /// <summary>
        /// Current Distance between 'm_buttonPressValue_point1' and 'm_buttonPressValue_point2'. This represents the current value of the button press.
        /// </summary>
        protected float currentActivationDistance = 0f;

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

        #endregion Private

        #endregion Fields

        #region Methods
        
        #region Hover

        public override void Hover_Start(DHUI_HoverEventArgs _hoverEvent)
        {
            base.Hover_Start(_hoverEvent);
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_contactCenterPoint.position, m_contactCenterPoint.rotation, _droneSpeed_initialPositioning);
            m_flightController.AddToFrontOfQueue(cmd, true, true);
        }

        public override void Hover_End(DHUI_HoverEventArgs _hoverEvent)
        {
            base.Hover_End(_hoverEvent);

            ButtonInternalState = ButtonInternalStates.Inactive;
        }

        public override void Hover_Stay(DHUI_HoverEventArgs _hoverEvent)
        {
            base.Hover_Stay(_hoverEvent);

            // Calculating Hover Information
            Vector3 interactorPos = _hoverEvent.InteractorPosition;
            hover_distance = Mathf.Abs(ContactPlane.GetDistance(interactorPos));
            hover_projectedPoint = ContactPlane.GetProjectedPoint(interactorPos);

            // Calculating Activation Information
            if (_activationDistance_mode == GlobalLocalMode.Global)
            {
                currentActivationDistance = Vector3.Distance(m_buttonPressValue_point1.position, m_buttonPressValue_point2.position);
            }
            else
            {
                currentActivationDistance = Vector3.Distance(m_buttonPressValue_point1.localPosition, m_buttonPressValue_point2.localPosition);
            }
            
            //Switching States
            if (ContactPlane.PointInFrontOfPlane(interactorPos))
            {
                if (hover_distance < _hover_touchThreshold)
                {
                    ButtonInternalState = ButtonInternalStates.Touched;
                }
                else
                {
                    ButtonInternalState = ButtonInternalStates.Hovered;
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

        #endregion

        #region Activation

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