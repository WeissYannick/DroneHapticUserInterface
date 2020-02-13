using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    public class DHUI_FlightCommand_LandAtStart : DHUI_FlightCommand_BaseLand
    {
       
        public override void StartCommand(DHUI_FlightController _flightController, Transform _leader, Transform _drone)
        {
            Pose startPose = _flightController.GetDroneStartingPose();
            targetPosition = startPose.position;
            targetRotation = startPose.rotation;

            base.StartCommand(_flightController, _leader, _drone);
        }

    }
}