using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DHUI.Core;
using System;
using UnityEngine.Events;

namespace DHUI
{
    public class DHUI_TiltSwitch : DHUI_Interactable
    {
        [Header("TiltSwitch.Setup")]
        [SerializeField]
        private Transform m_movingPart = null;
        [SerializeField]
        private Transform m_center = null;
        
        [Header("TiltSwitch.Settings")]
        [SerializeField]
        private float _activationThreshold = 0f;
        [SerializeField]
        private bool _invertActivation = false;
        [SerializeField]
        private float _droneSpeed = 2f;
        [SerializeField]
        private float _tiltRange_max = 0.2f;
        [SerializeField]
        private float _tiltRange_min = -0.2f;

        #region Inspector Events
        [Serializable]
        public class DHUI_TiltSwitchEvent : UnityEvent { }
        
        [Header("TiltSwitch.Events")]
        public DHUI_TiltSwitchEvent OnActivationStart = null;
        public DHUI_TiltSwitchEvent OnActivationStay = null;
        public DHUI_TiltSwitchEvent OnActivationEnd = null;

        #endregion Inspector Events

        private bool internal_switchState = false;

        /// <summary>
        /// Current Activation State of the switch. (true -> Activated/, false -> Not activated).
        /// </summary>
        public bool SwitchState
        {
            get
            {
                return internal_switchState;
            }
            protected set
            {
                if (value == internal_switchState) return;
                if (value)
                {
                    Activation_Start();
                }
                else
                {
                    Activation_End();
                }

                internal_switchState = value;
            }
        }

        #region Hover

        public override void Hover_Start(DHUI_HoverEventArgs _hoverEvent)
        {
            base.Hover_Start(_hoverEvent);
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_center.position, m_center.rotation, _droneSpeed);
            m_interactionManager.FlightController.AddToFrontOfQueue(cmd, true, true);
        }

        public override void Hover_End(DHUI_HoverEventArgs _hoverEvent)
        {
            base.Hover_End(_hoverEvent);
        }

        public override void Hover_Stay(DHUI_HoverEventArgs _hoverEvent)
        {
            base.Hover_Stay(_hoverEvent);

            if (_invertActivation)
            {
                SwitchState = m_movingPart.transform.localRotation.y > _activationThreshold;
            }
            else
            {
                SwitchState = m_movingPart.transform.localRotation.y < _activationThreshold;
            }
            
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_center.position, m_center.rotation, _droneSpeed);
            m_interactionManager.FlightController.AddToFrontOfQueue(cmd, true, true);

            if (SwitchState)
            {
                Activation_Stay();
            }
        }

        #endregion Hover

        #region Activation

        public virtual void Activation_Start()
        {
            OnActivationStart?.Invoke();
        }

        public virtual void Activation_End()
        {
            OnActivationEnd?.Invoke();
        }

        public virtual void Activation_Stay()
        {
            OnActivationStay?.Invoke();
        }
        
        #endregion Activation

        private void Update()
        {
            Quaternion localRot = m_movingPart.transform.localRotation;
            if (localRot.y > _tiltRange_max)
            {
                localRot.y = _tiltRange_max;
                m_movingPart.transform.localRotation = localRot;
            }
            else if (localRot.y < _tiltRange_min)
            {
                localRot.y = _tiltRange_min;
                m_movingPart.transform.localRotation = localRot;
            }

        }
    }

}