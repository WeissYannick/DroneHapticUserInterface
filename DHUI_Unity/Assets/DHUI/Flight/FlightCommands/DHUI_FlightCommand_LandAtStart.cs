using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    /// <summary>
    /// FlightCommand | 
    /// Lands drone in the position and rotation it started in (last take-off).
    /// </summary>
    public class DHUI_FlightCommand_LandAtStart : DHUI_FlightCommand_BaseLand
    {
        /// <summary>
        /// When this command is started, it gets the Drones last takeOff pose and sets it as the landing target.
        /// </summary>
        public override void StartCommand(DHUI_FlightController _flightController, Transform _leader, Transform _drone)
        {
            Pose startPose = _flightController.GetDroneStartingPose();
            targetPosition = startPose.position;
            targetRotation = startPose.rotation;

            base.StartCommand(_flightController, _leader, _drone);
        }

    }
}