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

        #region Events
        public struct DHUI_ButtonEventArgs
        {

        }
        [Serializable]
        public class DHUI_ButtonEvent : UnityEvent<DHUI_ButtonEventArgs> { }

        [Header("Button.Events")]
        public DHUI_ButtonEvent OnActivated = null;
        
        #endregion Events

        #region Enums

        public enum ButtonState
        {
            Inactive, Hovered, Touched, Pressed, Activated
        }

        public enum GlobalLocalMode
        {
            Global, Local
        }

        #endregion Enums

        private ButtonState internal_buttonState;
        /// <summary>
        /// Current State of the button.
        /// </summary>
        public ButtonState CurrentButtonState
        {
            get
            {
                return internal_buttonState;
            }
            protected set
            {
                if (internal_buttonState != value)
                {
                    if (value == ButtonState.Activated)
                    {
                        float currentTime = Time.time;
                        if (currentTime >= lastActivationTime + _activationCooldown)
                        {
                            lastActivationTime = currentTime;
                            OnActivated?.Invoke(new DHUI_ButtonEventArgs());
                        }
                        else
                        {
                            return;
                        }
                    }

                    internal_buttonState = value;
                }
            }
        }

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

        /// <summary>
        /// Time of last button activation. Used to implement Activation-Cooldown.
        /// </summary>
        private float lastActivationTime = 0f;


        public override void Hover_Start(DHUI_HoverEvent _hoverEvent)
        {
            base.Hover_Start(_hoverEvent);
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_contactCenterPoint.position, m_contactCenterPoint.rotation, _droneSpeed_initialPositioning);
            m_flightController.AddToFrontOfQueue(cmd, true, true);
        }

        public override void Hover_End(DHUI_HoverEvent _hoverEvent)
        {
            base.Hover_End(_hoverEvent);

            CurrentButtonState = ButtonState.Inactive;
        }

        public override void Hover_Stay(DHUI_HoverEvent _hoverEvent)
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
                    CurrentButtonState = ButtonState.Touched;
                }
                else
                {
                    CurrentButtonState = ButtonState.Hovered;
                }
            }
            else
            {
                if (currentActivationDistance >= _activationDistance_threshold)
                {
                    CurrentButtonState = ButtonState.Activated;
                }
                else
                {
                    CurrentButtonState = ButtonState.Pressed;
                }

            }
        }
    }

}