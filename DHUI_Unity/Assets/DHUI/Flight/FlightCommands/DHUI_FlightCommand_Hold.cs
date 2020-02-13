using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    public class DHUI_FlightCommand_Hold : DHUI_FlightCommand_Base
    {
        private Vector3 targetPosition = Vector3.zero;
        private Quaternion targetRotation = Quaternion.identity;
        public double duration;

        public override void StartCommand(DHUI_FlightController _flightController, Transform _leader, Transform _drone)
        {
            base.StartCommand(_flightController, _leader, _drone);

            targetPosition = leader.position;
            targetRotation = leader.rotation;
        }

        public override void UpdateCommand(out bool _finished)
        {
            if (Time.time >= startTime + duration)
            {
                _finished = true;
            }
            else
            {
                if (leader != null)
                {
                    leader.position = targetPosition;
                    leader.rotation = targetRotation;
                }
                _finished = false;
            }
        }
    }
}