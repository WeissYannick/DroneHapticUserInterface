using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    public class DHUI_EmergencyInput_Keyboard : DHUI_EmergencyInput_Base
    {
        public KeyCode emergencyHoverKey = KeyCode.H;
        public bool emergencyHoverKey_doubleTab = false;
        private float emergencyHoverKey_lastPressed = 0;

        public KeyCode emergencyLandKey = KeyCode.L;
        public bool emergencyLandKey_doubleTab = false;
        private float emergencyLandKey_lastPressed = 0;

        public KeyCode emergencyShutOffKey = KeyCode.Space;
        public bool emergencyShutOffKey_doubleTab = true;
        private float emergencyShutOffKey_lastPressed = 0;
        
        public KeyCode continueKey = KeyCode.C;
        public bool continueKey_doubleTab = false;
        private float continueKey_lastPressed = 0;

        private float tabResetSeconds = 1f;

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