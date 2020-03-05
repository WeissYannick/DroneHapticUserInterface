using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    /// <summary>
    /// FlightCommand | 
    /// Waits for the drone to reach the leader (distance and angle lower than thresholds).
    /// </summary>
    public class DHUI_FlightCommand_WaitForDrone : DHUI_FlightCommand_Base
    {
        /// <summary>
        /// Timeout for waiting (in seconds). If this threshold is reached, we consider the command finished.
        /// </summary>
        public float waitingTimeout = 5f;
        
        public override void StartCommand(DHUI_FlightController _flightController, Transform _leader, Transform _drone)
        {
            base.StartCommand(_flightController, _leader, _drone);
        }

        /// <summary>
        /// While the command is being updated:
        /// We wait for the drone to reach the target position and rotation or time runs out.
        /// </summary>
        /// <param name="_finished">Shows wether command is finished.</param>
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
