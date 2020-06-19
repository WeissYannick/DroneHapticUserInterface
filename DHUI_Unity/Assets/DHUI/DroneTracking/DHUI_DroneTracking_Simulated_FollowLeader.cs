using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DHUI.Core;

namespace DHUI
{
    /// <summary>
    /// This class simulates the Tracking of the Drone by following the leader.
    /// </summary>
    public class DHUI_DroneTracking_Simulated_FollowLeader : DHUI_DroneTracking_Base
    {
        [Header("Simulation Setup")]

        [Tooltip("The leader object controlled by the FlightController/FlightCommands. We will simply follow this object around with some speed and smoothing.")]
        public Transform _leaderToFollow = null;

        [Tooltip("The drone controller. This is needed to simulate ")]
        public DHUI_DroneController _droneController = null;

        [Tooltip("The speed with which we follow the leader.")]
        public float _followSpeed = 0.05f;

        [Tooltip("The speed with which we simulate landing.")]
        public float _landSpeed = 0.005f;

        [Tooltip("The speed with which we simulate free fall.")]
        public float _freeFallSpeed = 0.04f;

        [Tooltip("Turn this on to simulate tracking lost.")]
        public bool _simulateTrackingLost = false;

        [Tooltip("Turn this on to stop updating the simulated drone transform. This allows you to move it without it flying back towards the target.")]
        public bool _stopUpdatingDrone = false;

        /// <summary>
        /// When Updating this tracker, we will simply follow the leader around with Lerp.
        /// </summary>
        protected override void UpdateTracker()
        {
            trackingOK = !_simulateTrackingLost;

            if (_stopUpdatingDrone) return;

            switch (_droneController.GetDroneState())
            {
                case DHUI_DroneController.DroneState.Follow:
                    trackedTransform.position = Vector3.Lerp(trackedTransform.position, _leaderToFollow.position - _droneController.contactPointPositionOffset, _followSpeed);
                    trackedTransform.forward = Vector3.Lerp(trackedTransform.forward, Quaternion.Euler(0, -_droneController.contactPointRotationOffset, 0) * _leaderToFollow.forward, _followSpeed);
                    velocity = Vector3.zero;
                    break;
                case DHUI_DroneController.DroneState.Land:
                    if (trackedTransform.position.y > _droneController._floorY + _landSpeed)
                    {
                        trackedTransform.Translate(Vector3.down * _landSpeed);
                    }
                    else
                    {
                        trackedTransform.position = new Vector3(trackedTransform.position.x, _droneController._floorY, trackedTransform.position.z);
                    }
                    break;
                case DHUI_DroneController.DroneState.Off:
                    if (trackedTransform.position.y > _droneController._floorY + _freeFallSpeed)
                    {
                        trackedTransform.Translate(Vector3.down * _freeFallSpeed);
                    }
                    else
                    {
                        trackedTransform.position = new Vector3(trackedTransform.position.x, _droneController._floorY, trackedTransform.position.z);
                    }
                    break;
                case DHUI_DroneController.DroneState.Hover:
                    break;
                default:
                    break;
            }


        }

    }
}
