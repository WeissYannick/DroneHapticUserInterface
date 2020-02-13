using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    public class DHUI_FlightCommand_Hover : DHUI_FlightCommand_Base
    {
        public float duration;

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