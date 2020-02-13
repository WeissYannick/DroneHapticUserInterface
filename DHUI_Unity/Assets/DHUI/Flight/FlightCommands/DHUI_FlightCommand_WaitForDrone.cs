using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    public class DHUI_FlightCommand_WaitForDrone : DHUI_FlightCommand_Base
    {
        public override void UpdateCommand(out bool _finished)
        {
            _finished = DroneFinished();
        }
    }
}
