using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Namespace contains the Core features of DHUI – Drone Haptic User Interface.
/// These Scripts are essential for the system to work.
/// </summary>
namespace DHUI.Core
{
    /// <summary>
    /// This class handles emergency inputs that force the droneController to change it's behaviour.
    /// This is required (as a safety precaution) and can force the dorne to Hover, Land or ShutOff in emergencies.
    /// </summary>
    public abstract class DHUI_EmergencyInput_Base : MonoBehaviour
    {
        DHUI_DroneController controller = null;

        /// <summary>
        /// Sets up the Drone Controller Instance, which has control over the (real) drone. 
        /// </summary>
        /// <param name="_dc">Drone controller instance</param>
        public void Setup(DHUI_DroneController _dc)
        {
            controller = _dc;
        }

        /// <summary>
        /// Trigger the Emergency Hover of the drone.
        /// </summary>
        protected virtual void TriggerEmergencyHover()
        {
            Debug.LogWarning("<b>DHUI</b> | EmergencyInput | Emergency Hover was triggered by user!");
            controller.SetEmergencyMode(true);
            controller.ForceSetDroneState(DHUI_DroneController.DroneState.Hover);
        }

        /// <summary>
        /// Trigger the Emergency Land of the drone.
        /// </summary>
        protected virtual void TriggerEmergencyLand()
        {
            Debug.LogWarning("<b>DHUI</b> | EmergencyInput | Emergency Land was triggered by user!");
            controller.SetEmergencyMode(true);
            controller.ForceSetDroneState(DHUI_DroneController.DroneState.Land);
        }

        /// <summary>
        /// Trigger the Emergency ShutOff of the drone. This is very dangerous and should not be used lightly, since the drone will fall out of the air.
        /// </summary>
        protected virtual void TriggerEmergencyShutOff()
        {
            Debug.LogWarning("<b>DHUI</b> | EmergencyInput | Emergency ShutOff was triggered by user!");
            controller.SetEmergencyMode(true);
            controller.ForceSetDroneState(DHUI_DroneController.DroneState.Off);
        }

        /// <summary>
        /// Trigger the Emergency ShutOff of the drone. This is very dangerous and should not be used lightly, since the drone will fall out of the air.
        /// </summary>
        protected virtual void TriggerContinue()
        {
            Debug.LogWarning("<b>DHUI</b> | EmergencyInput | Continue was triggered by user!");
            controller.SetEmergencyMode(false);
        }
    }
}