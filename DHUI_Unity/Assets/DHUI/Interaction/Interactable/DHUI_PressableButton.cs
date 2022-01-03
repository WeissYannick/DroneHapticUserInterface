using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

namespace DHUI
{
    public class DHUI_PressableButton : DHUI_Touchable
    {
        #region Classes/Structs/Enums

        [Serializable]
        public class DHUI_ButtonActivationEvent : UnityEvent<DHUI_ButtonActivationEventArgs> { }

        public enum GlobalLocalMode
        {
            Global, Local
        }
        public struct DHUI_ButtonActivationEventArgs
        {
            public float activationDuration;
        }

        #endregion Classes/Structs/Enums

        #region Inspector Fields

        [Header("Button.Setup")]
        [SerializeField]
        protected Transform m_buttonPressValue_point1;
        [SerializeField]
        protected Transform m_buttonPressValue_point2;

        [Header("Button.Settings")]
        [SerializeField]
        protected GlobalLocalMode _activationDistance_mode = GlobalLocalMode.Global;
        [SerializeField]
        protected float _activationDistance_threshold = 0.15f;
        [SerializeField]
        protected float _activationCooldown = 0.5f;
        [SerializeField]
        protected float _droneResistance = 1f;

        [Header("Button.StiffnessIllusion")]
        [SerializeField]
        protected bool _stiffnessIllusionActive = false;
        [SerializeField]    // cdr = Control/Display Ratio
        protected AnimationCurve _cdrCurve = null;
        [SerializeField]
        protected float _cdrMaxDistance_threshold = 0.2f;
        [SerializeField]
        protected float _cdrMultiplier = 0.5f;


        [Header("Button.Events")]
        public DHUI_ButtonActivationEvent OnActivationStart = null;
        public DHUI_ButtonActivationEvent OnActivationStay = null;
        public DHUI_ButtonActivationEvent OnActivationEnd = null;

        #endregion Inspector Fields
        
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
                    float currentTime = Time.time;
                    if (currentTime >= lastActivated + _activationCooldown)
                    {
                        lastActivated = currentTime;
                        Activation_Start(ConstructActivationEventArgs());
                    }
                    else
                    {
                        return;
                    }
                }
                else
                {
                    Activation_End(ConstructActivationEventArgs());
                }

                internal_buttonActivationState = value;
            }
        }

        /// <summary>
        /// Current Distance between 'm_buttonPressValue_point1' and 'm_buttonPressValue_point2'. This represents the current value of the button press.
        /// </summary>
        protected float currentActivationDistance = 0f;
        
        private float lastActivated = 0f;

        private float currentActivationDuration
        {
            get { return Time.time - lastActivated; }
        }

        private void Update()
        {
            UpdateActivationCalculations();
            UpdateActivationStay();
        }

        protected override void Touch_Stay(DHUI_TouchEventArgs _touchEventArgs)
        {
            base.Touch_Stay(_touchEventArgs);
            ApplyDroneResistance();
        }

        protected override void Touch_End(DHUI_TouchEventArgs _touchEventArgs)
        {
            base.Touch_End(_touchEventArgs);
            ResetDroneTarget();
        }

        protected virtual void UpdateActivationCalculations()
        {
            if (_activationDistance_mode == GlobalLocalMode.Global)
            {
                currentActivationDistance = Vector3.Distance(m_buttonPressValue_point1.position, m_buttonPressValue_point2.position);
            }
            else
            {
                currentActivationDistance = Vector3.Distance(m_buttonPressValue_point1.localPosition, m_buttonPressValue_point2.localPosition);
            }

            if (_stiffnessIllusionActive && TouchedState)
            {
                UpdateStiffnessIllusion(currentActivationDistance);
            }

            if (currentActivationDistance > _activationDistance_threshold)
            {
                ButtonActivationState = true;
            }
            else
            {
                ButtonActivationState = false;
            }
        }

        protected void UpdateStiffnessIllusion(float _currentDistance)
        {
            float step = _currentDistance / _cdrMaxDistance_threshold;
            if (step < 0) step = 0;
            else if (step > 1) step = 1;

            Vector3 displacementDirection = (m_buttonPressValue_point2.position - m_buttonPressValue_point1.position).normalized;
            float displacementMagnitude = _cdrCurve.Evaluate(step) * _cdrMultiplier;

            Vector3 displacementVector = displacementDirection * displacementMagnitude;
            m_interactionManager.StiffnessIllusion?.SetDisplacementVector(displacementVector);

        }

        protected void ApplyDroneResistance()
        {
            Vector3 calculatedPos = m_centerPoint.position + m_centerPoint.forward * currentActivationDistance * (_droneResistance - 1);
            m_droneTargetPoint.position = calculatedPos;
            UpdateDronePosition(0);
        }

        protected void ResetDroneTarget()
        {
            m_droneTargetPoint.position = m_centerPoint.position;
            UpdateDronePosition(0);
        }

        #region OnActivation

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

        protected void UpdateActivationStay()
        {
            if (ButtonActivationState)
            {
                Activation_Stay(ConstructActivationEventArgs());
            }
        }

        #endregion OnActivation
    }

}