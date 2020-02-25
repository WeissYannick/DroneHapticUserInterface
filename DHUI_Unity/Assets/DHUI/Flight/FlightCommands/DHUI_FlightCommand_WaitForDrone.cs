using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    public class DHUI_FlightCommand_WaitForDrone : DHUI_FlightCommand_Base
    {
        public float waitingTimeout = 5f;

        public override void StartCommand(DHUI_FlightController _flightController, Transform _leader, Transform _drone)
        {
            base.StartCommand(_flightController, _leader, _drone);
        }

        public override void UpdateCommand(out bool _finished)
        {
            if (DroneReachedTarget() || Time.time >= startTime + waitingTimeout)
            {
                _finished = true;
            }
            else
            {
                _finished = false;
            }
        }
    }
}
