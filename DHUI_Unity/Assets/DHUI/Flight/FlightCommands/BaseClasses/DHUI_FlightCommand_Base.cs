using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    /// <summary>
    /// Base Class of all FlightCommands. This should never be used directly.
    /// </summary>
    public abstract class DHUI_FlightCommand_Base
    {
        /// <summary>
        /// The active FlightController-Instance. 
        /// </summary>
        protected DHUI_FlightController flightController = null;
        /// <summary>
        /// The leader-object, which is controlled by FlightCommands.
        /// </summary>
        protected Transform leader = null;
        /// <summary>
        /// The drone-object, which is necessary to check the drone's current position, rotation, etc.
        /// </summary>
        protected Transform drone = null;
        /// <summary>
        /// Time of starting the Command (Set in StartCommand-Method).
        /// </summary>
        protected float startTime = -1;
        /// <summary>
        /// Default value of Distance threshold in Meters between the drone and the target, where we consider the drone to be finished moving to the point (only used when waitForDrone = true and distance not given to method).
        /// </summary>
        protected float droneFinishedTrigger_distance_default = 0.1f;
        /// <summary>
        /// Default value of Angle threshold in Degrees between the drone-YAxis and the target-YAxis, where we consider the drone to be finished moving to the point (only used when waitForDrone = true and angle not given to method).
        /// </summary>
        protected float droneFinishedTrigger_angle_default = 5f;

        /// <summary>
        /// Starts the current Command and sets up necessary members.
        /// </summary>
        /// <param name="_flightController">The FlightController-instance handling the commands.</param>
        /// <param name="_leader">The leader-object we control.</param>
        /// <param name="_drone">The drone-object.</param>
        public virtual void StartCommand(DHUI_FlightController _flightController, Transform _leader, Transform _drone)
        {
            flightController = _flightController;
            leader = _leader;
            drone = _drone;
            startTime = Time.time;

            SetFlightModeAtCommandStart();
        }

        /// <summary>
        /// Updates the current Command. 
        /// </summary>
        /// <param name="_finished">Wether the Command finished (e.g. duration reached or drone reached target) and can be Exited.</param>
        public virtual void UpdateCommand(out bool _finished)
        {
            _finished = true;
            Debug.LogWarning("<b>DHUI</b> | FlightCommand_Base | Update Command on Base-Class was called. This should always be overridden.");
        }

        /// <summary>
        /// Tries to set the Flight mode of the FlightController. This is always activated at the Start of a new Command, and may be overriden e.g. when initiating landing instead.
        /// </summary>
        protected virtual void SetFlightModeAtCommandStart()
        {
            flightController.TrySetFlightState(DHUI_FlightController.FlightState.Flying);
        }

        /// <summary>
        /// Wether the Drone reached the target position and rotation (both are using default distance thresholds).
        /// </summary>
        /// <returns></returns>
        protected bool DroneReachedTarget()
        {
            return (DroneReachedTargetTranslation() && DroneReachedTargetRotation());
        }

        /// <summary>
        /// Wether the Drone reached its target translation. (Using a threshold the distance has to be below of)
        /// </summary>
        /// <param name="_droneFinishedTrigger_distance">Threshold the distance has to be below to count as target reached. If none is given, a default value will be used.</param>
        /// <returns></returns>
        protected bool DroneReachedTargetTranslation(float _droneFinishedTrigger_distance = -1)
        {
            // If _droneFinishedTrigger_distance is not set or set below 0 -> use default value.
            if (_droneFinishedTrigger_distance < 0)
            {
                _droneFinishedTrigger_distance = droneFinishedTrigger_distance_default;
            }

            return (Vector3.Distance(drone.position, leader.position) <= _droneFinishedTrigger_distance);
        }

        /// <summary>
        /// Wether the Drone reached its target rotation. (Using a threshold the angle has to be below of)
        /// </summary>
        /// <param name="_droneFinishedTrigger_angle">Threshold the angle has to be below to count as target reached. If none is given, a default value will be used.</param>
        /// <returns></returns>
        protected bool DroneReachedTargetRotation(float _droneFinishedTrigger_angle = -1)
        {
            // If _droneFinishedTrigger_angle is not set or set below 0 -> use default value.
            if (_droneFinishedTrigger_angle < 0)
            {
                _droneFinishedTrigger_angle = droneFinishedTrigger_angle_default;
            }
            float degreeOffset = Mathf.Abs(drone.rotation.y - leader.rotation.y) * 180;
            return (degreeOffset <= _droneFinishedTrigger_angle);
        }
        
    }

}