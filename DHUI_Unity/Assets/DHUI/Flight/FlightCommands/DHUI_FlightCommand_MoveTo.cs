using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    /// <summary>
    /// FlightCommand | 
    /// Makes the drone move to a new pose.
    /// </summary>
    public class DHUI_FlightCommand_MoveTo : DHUI_FlightCommand_Base
    {
        /// <summary>
        /// Position to move to.
        /// </summary>
        public Vector3 targetPosition = Vector3.zero;

        /// <summary>
        /// Rotation to move to.
        /// </summary>
        public Quaternion targetRotation = Quaternion.identity;

        /// <summary>
        /// Speed (in m/s) used to calculate time to move to the new target.
        /// </summary>
        public float speed = 0.5f;

        /// <summary>
        /// Wether the movement animation shoudl smooth in and out.
        /// </summary>
        public bool smoothInOut = false;

        /// <summary>
        /// Wether we will wait for the drone to catch up to the leader before we consider the command finished.
        /// </summary>
        public bool waitForDrone = false;

        /// <summary>
        /// If 'waitForDrone' is true, how long will we wait, until we timeout and consider the command finished.
        /// </summary>
        public float waitingTimeout = 3f;

        /// <summary>
        /// Starting position of the leader.
        /// </summary>
        protected Vector3 startPosition = Vector3.zero;

        /// <summary>
        /// Starting orientation of the leader.
        /// </summary>
        protected Quaternion startRotation = Quaternion.identity;

        /// <summary>
        /// Time to move to the new target.
        /// </summary>
        protected float time = 5;

        /// <summary>
        /// Wether Time was set manually.
        /// </summary>
        private bool manualTimeSet = false;

        /// <summary>
        /// Constructs a new MoveTo-Command.
        /// </summary>
        /// <param name="_targetPosition">Position to move to.</param>
        /// <param name="_targetRotation">Orientation to rotate to.</param>
        /// <param name="_time">Amount of time for the translation and rotation.</param>
        public DHUI_FlightCommand_MoveTo(Vector3 _targetPosition, Quaternion _targetRotation, float _speed = 0.5f)
        {
            targetPosition = _targetPosition;
            targetRotation = _targetRotation;
            
            speed = _speed;
        }

        /// <summary>
        /// Sets the time to fly to target manually. Speed and Distance at the Start of the Command will therefore not be used to calculate the time.
        /// </summary>
        /// <param name="_time">Time to fly to target.</param>
        public void SetTimeManually(float _time)
        {
            time = _time;
            manualTimeSet = true;
        }
        
        public override void StartCommand(DHUI_FlightController _flightController, Transform _leader, Transform _drone)
        {
            base.StartCommand(_flightController, _leader, _drone);

            startPosition = leader.position;
            startRotation = leader.rotation;

            if (!manualTimeSet)
            {
                if (speed <= 0)
                {
                    time = 0;
                }
                else
                {
                    time = Vector3.Distance(startPosition, targetPosition) / speed;
                }
            }
        }

        public override void UpdateCommand(out bool _finished)
        {
            bool leaderFinished = UpdateLeader();
            _finished = false;
            if (leaderFinished)
            {
                if (!waitForDrone)
                {
                    _finished = true;
                }
                else if (DroneReachedTarget() || Time.time >= startTime + time + waitingTimeout)
                {
                    _finished = true;
                }
            }

        }

        /// <summary>
        /// Animates the leader towards the given target position & rotation in the given time.
        /// </summary>
        /// <returns></returns>
        private bool UpdateLeader()
        {
            // Set the pose to the target pose instantly if time is 0 or less.
            if (time <= 0)
            {
                SetPoseToEnd();
                return true;
            }
            else
            {
                float timeSinceStart = Time.time - startTime;
                float fraction = timeSinceStart / time;
                
                if (fraction < 1)
                {
                    if (smoothInOut)
                    {
                        Move_SmoothInOut(fraction);
                    }
                    else
                    {
                        Move_Linear(fraction);
                    }
                    return false;
                }
                else
                {
                    SetPoseToEnd();
                    return true;
                }
            }
        }

        /// <summary>
        /// Lerps/Slerps between the starting and target position/rotation.
        /// </summary>
        /// <param name="fraction">Fraction of total animation in the range 0-1.</param>
        private void Move_Linear(float fraction)
        {
            leader.position = Vector3.Lerp(startPosition, targetPosition, fraction);
            leader.rotation = Quaternion.Slerp(startRotation, targetRotation, fraction);
        }

        /// <summary>
        /// Lerps/Slerps between the starting and target position/rotation with smoothing at beginning & end.
        /// </summary>
        /// <param name="fraction">Fraction of total animation in the range 0-1.</param>
        private void Move_SmoothInOut(float fraction)
        {
            float smoothStep = Mathf.SmoothStep(0f, 1f, fraction);
            leader.position = Vector3.Lerp(startPosition, targetPosition, smoothStep);
            leader.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothStep);
        }

        /// <summary>
        /// Sets the leaders pose to the target pose.
        /// </summary>
        private void SetPoseToEnd()
        {
            leader.position = targetPosition;
            leader.rotation = targetRotation;
        }
        
    }
}