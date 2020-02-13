using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    public class DHUI_FlightCommand_Copy : DHUI_FlightCommand_Base
    {
        public Transform targetTransform = null;
        public double duration = 5;

        public override void UpdateCommand(out bool _finished)
        {
            if (Time.time >= startTime + duration)
            {
                _finished = true;
            }
            else
            {
                if (leader != null && targetTransform != null)
                {
                    leader.position = targetTransform.position;
                    leader.rotation = targetTransform.rotation;
                }
                _finished = false;
            }
        }
    }
}