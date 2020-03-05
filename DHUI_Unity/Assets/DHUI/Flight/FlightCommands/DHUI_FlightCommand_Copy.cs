using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    /// <summary>
    /// FlightCommand | 
    /// Makes the drone follow a given transform for a specific duration.
    /// </summary>
    public class DHUI_FlightCommand_Copy : DHUI_FlightCommand_Base
    {
        /// <summary>
        /// Transform for the drone to follow.
        /// </summary>
        public Transform targetTransform = null;

        /// <summary>
        /// Duration of the drone following the transform.
        /// </summary>
        public double duration = 5;

        /// <summary>
        /// While this command is being updated, the leader will continuously copy the targeted transform's position & rotation.
        /// </summary>
        /// <param name="_finished">Wether the command is finished (= duration was reached).</param>
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