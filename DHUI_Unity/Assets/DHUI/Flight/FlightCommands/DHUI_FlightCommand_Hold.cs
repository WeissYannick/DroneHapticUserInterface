using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    /// <summary>
    /// FlightCommand | 
    /// Makes the drone hold its current position.
    /// If it is moved, it will try to push back.
    /// </summary>
    public class DHUI_FlightCommand_Hold : DHUI_FlightCommand_Base
    {
        /// <summary>
        /// Position to hold. This will be the leader's position when this command is starting.
        /// </summary>
        private Vector3 targetPosition = Vector3.zero;

        /// <summary>
        /// Orientation to hold. This will be the leader's orientation when this command is starting.
        /// </summary>
        private Quaternion targetRotation = Quaternion.identity;

        /// <summary>
        /// Duration of this command.
        /// </summary>
        public double duration;

        
        public override void StartCommand(DHUI_FlightController _flightController, Transform _leader, Transform _drone)
        {
            base.StartCommand(_flightController, _leader, _drone);

            targetPosition = leader.position;
            targetRotation = leader.rotation;
        }

        /// <summary>
        /// While this command is being updated, the leader will stay in the targeted pose, resulting in the drone trying to hold its pose.
        /// </summary>
        /// <param name="_finished">Wether the command is finished (duration reached).</param>
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