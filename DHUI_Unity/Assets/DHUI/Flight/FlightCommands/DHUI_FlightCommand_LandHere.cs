using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    public class DHUI_FlightCommand_LandHere : DHUI_FlightCommand_BaseLand
    {
        public bool landAtDroneInsteadOfLeader = false;

        public DHUI_FlightCommand_LandHere(bool _landAtDroneInsteadOfLeader = false)
        {
            landAtDroneInsteadOfLeader = _landAtDroneInsteadOfLeader;
        }
        
        public override void StartCommand(DHUI_FlightController _flightController, Transform _leader, Transform _drone)
        {
            if (landAtDroneInsteadOfLeader)
            {
                _leader.position = _drone.position;
                _leader.rotation = _drone.rotation;
            }
            targetPosition = _leader.position;
            targetRotation = _leader.rotation;
            base.StartCommand(_flightController, _leader, _drone);
        }
        
    }
}