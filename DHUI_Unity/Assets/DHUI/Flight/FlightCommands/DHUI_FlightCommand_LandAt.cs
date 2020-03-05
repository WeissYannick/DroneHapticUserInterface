using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    /// <summary>
    /// FlightCommand | 
    /// Lands drone at given position with given orientation.
    /// </summary>
    public class DHUI_FlightCommand_LandAt : DHUI_FlightCommand_BaseLand
    {
        /// <summary>
        /// Constructs a new instance of 'DHUI_FlightCommand_LandAt'. Requires a position and rotation to land in.
        /// </summary>
        /// <param name="_targetPosition">Position to land at.</param>
        /// <param name="_targetRotation">Orientation to land in.</param>
        public DHUI_FlightCommand_LandAt(Vector3 _targetPosition, Quaternion _targetRotation)
        {
            targetPosition = _targetPosition;
            targetRotation = _targetRotation;
        }
        /// <summary>
        /// Constructs a new instance of 'DHUI_FlightCommand_LandAt'. Requires a pose to land in.
        /// </summary>
        /// <param name="_targetPose">Pose (position & rotation) to land in .</param>
        public DHUI_FlightCommand_LandAt(Pose _targetPose)
        {
            targetPosition = _targetPose.position;
            targetRotation = _targetPose.rotation;
        }
        
    }
}