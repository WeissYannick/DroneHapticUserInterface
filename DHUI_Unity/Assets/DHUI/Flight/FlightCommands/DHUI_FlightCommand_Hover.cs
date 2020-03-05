using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    /// <summary>
    /// FlightCommand | 
    /// Makes the drone hover freely in the air. If it is moved, it does not try to push back.
    /// This behaviour will result in drifting.
    /// </summary>
    public class DHUI_FlightCommand_Hover : DHUI_FlightCommand_Base
    {
        /// <summary>
        /// Duration of hover.
        /// </summary>
        public float duration;

        /// <summary>
        /// While this command is not finished, the leader's pose is copying the drone's current pose. 
        /// </summary>
        /// <param name="_finished">Shows wether the command is finished (duration reached).</param>
        public override void UpdateCommand(out bool _finished)
        {
            if (Time.time >= startTime + duration)
            {
                _finished = true;
            }
            else
            {
                leader.position = drone.position;
                leader.rotation = drone.rotation;
                _finished = false;
            }
        }
    }
}