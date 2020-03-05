using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    /// <summary>
    /// FlightCommand | 
    /// Lands drone at its current position.
    /// </summary>
    public class DHUI_FlightCommand_LandHere : DHUI_FlightCommand_BaseLand
    {
        /// <summary>
        /// Wether the drone should land at the leader's or drone's current position/rotation.
        /// If set to false (= default), we land at the leader's current pose.
        /// If set to true, we land at the drone's current pose instead.
        /// </summary>
        public bool landAtDroneInsteadOfLeader = false;

        public DHUI_FlightCommand_LandHere(bool _landAtDroneInsteadOfLeader = false)
        {
            landAtDroneInsteadOfLeader = _landAtDroneInsteadOfLeader;
        }
        
        /// <summary>
        /// When this Command is started, we set the landing pose to be the current pose (of leader or drone).
        /// </summary>
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