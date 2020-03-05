using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    /// <summary>
    /// This class handles emergency inputs by keyboards.
    /// </summary>
    public class DHUI_EmergencyInput_Keyboard : DHUI_EmergencyInput_Base
    {
        
        [Tooltip("Key which forces the drone to change to Hover-Mode.")]
        public KeyCode emergencyHoverKey = KeyCode.H;
        [Tooltip("Wether the Hover-Key has to be double pressed to activate its function.")]
        public bool emergencyHoverKey_doubleTab = false;
        private float emergencyHoverKey_lastPressed = 0;
        
        [Tooltip("Key which forces the drone to change to Landing-Mode.")]
        public KeyCode emergencyLandKey = KeyCode.L;
        [Tooltip("Wether the Landing-Key has to be double pressed to activate its function.")]
        public bool emergencyLandKey_doubleTab = false;
        private float emergencyLandKey_lastPressed = 0;
        
        [Tooltip("Key which forces the drone to immediately ShutOff.")]
        public KeyCode emergencyShutOffKey = KeyCode.Space;
        [Tooltip("Wether the ShutOff-Key has to be double pressed to activate its function.")]
        public bool emergencyShutOffKey_doubleTab = true;
        private float emergencyShutOffKey_lastPressed = 0;
        
        [Tooltip("This key has to be pressed after an emergency input, before the drone will continue processing commands.")]
        public KeyCode continueKey = KeyCode.C;
        [Tooltip("Wether the Continue-Key has to be double pressed to activate its function.")]
        public bool continueKey_doubleTab = false;
        private float continueKey_lastPressed = 0;

        /// <summary>
        /// Threshold (in seconds) below which two presses of the same key count as a doubleTab.
        /// </summary>
        private float tabResetSeconds = 1f;

        /// <summary>
        /// Checks if the given keys are pressed down and triggers the respective base functions.
        /// </summary>
        void Update()
        {
            if (Input.GetKeyDown(emergencyHoverKey)){
                if (emergencyHoverKey_doubleTab)
                {
                    if (Time.time <= emergencyHoverKey_lastPressed + tabResetSeconds)
                    {
                        TriggerEmergencyHover();
                    }
                    emergencyHoverKey_lastPressed = Time.time;
                }
                else
                {
                    TriggerEmergencyHover();
                }
            }
            if (Input.GetKeyDown(emergencyLandKey))
            {
                if (emergencyLandKey_doubleTab)
                {
                    if (Time.time <= emergencyLandKey_lastPressed + tabResetSeconds)
                    {
                        TriggerEmergencyLand();
                    }
                    emergencyLandKey_lastPressed = Time.time;
                }
                else
                {
                    TriggerEmergencyLand();
                }
            }
            if (Input.GetKeyDown(emergencyShutOffKey))
            {
                if (emergencyShutOffKey_doubleTab)
                {
                    if (Time.time <= emergencyShutOffKey_lastPressed + tabResetSeconds)
                    {
                        TriggerEmergencyShutOff();
                    }
                    emergencyShutOffKey_lastPressed = Time.time;
                }
                else
                {
                    TriggerEmergencyShutOff();
                }
            }
            if (Input.GetKeyDown(continueKey))
            {
                if (continueKey_doubleTab)
                {
                    if (Time.time <= continueKey_lastPressed + tabResetSeconds)
                    {
                        TriggerContinue();
                    }
                    continueKey_lastPressed = Time.time;
                }
                else
                {
                    TriggerContinue();
                }
            }
        }
    }

}