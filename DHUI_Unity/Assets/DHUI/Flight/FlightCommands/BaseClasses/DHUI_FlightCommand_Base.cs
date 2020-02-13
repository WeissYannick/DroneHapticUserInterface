using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    public abstract class DHUI_FlightCommand_Base
    {
        protected DHUI_FlightController flightController = null;
        protected Transform leader = null;
        protected Transform drone = null;
        protected float startTime = -1;
        
        // Distance threshold in Meters between the drone and the target, where we consider the drone to be finished moving to the point (only used when waitForDrone = true).
        private const float droneFinishedTrigger_distance = 0.05f;
        // Angle threshold in Degrees between the drone-YAxis and the target-YAxis, where we consider the drone to be finished moving to the point (only used when waitForDrone = true).
        private const float droneFinishedTrigger_angle = 5f;

        public virtual void StartCommand(DHUI_FlightController _flightController, Transform _leader, Transform _drone)
        {
            flightController = _flightController;
            leader = _leader;
            drone = _drone;
            startTime = Time.time;

            SetFlightModeAtCommandStart();
        }
        public virtual void UpdateCommand(out bool _finished)
        {
            _finished = true;
            Debug.LogWarning("<b>DHUI</b> | FlightCommand_Base | Update Command on Base-Class was called. This should always be overridden.");
        }

        protected virtual void SetFlightModeAtCommandStart()
        {
            flightController.TrySetFlightState(DHUI_FlightController.FlightState.Flying);
        }

        protected bool DroneFinished()
        {
            float degreeOffset = Mathf.Abs(drone.rotation.y - leader.rotation.y) * 180;
            return (Vector3.Distance(drone.position, leader.position) <= droneFinishedTrigger_distance && degreeOffset <= droneFinishedTrigger_angle);
        }
    }

}