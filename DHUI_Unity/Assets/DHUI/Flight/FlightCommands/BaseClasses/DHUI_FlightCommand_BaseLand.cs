using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    /// <summary>
    /// This is the Base-Class of all regular Landing-Commands. This should not be used directly, derived classes only.
    /// </summary>
    public abstract class DHUI_FlightCommand_BaseLand : DHUI_FlightCommand_Base
    {
        #region Must Override

        /// <summary>
        /// Target Position to land at.
        /// </summary>
        protected Vector3 targetPosition = Vector3.zero;
        /// <summary>
        /// Target Orientation to land in.
        /// </summary>
        protected Quaternion targetRotation = Quaternion.identity;

        #endregion Must Override

        #region May Override

        /// <summary>
        /// Speed to the first target, where we then initiate TouchDown.
        /// </summary>
        public float landingSpeed = 0.2f;

        /// <summary>
        /// Amount of time we wait for the drone to reach the first target, before we timeout and init TouchDown anyways.
        /// </summary>
        public float waitingTimeout = 3f;

        /// <summary>
        /// Wether the landing will be on the floor. Landing on a table needs additional measures, since we can't just always go downwards.
        /// </summary>
        public bool floorLanding = true;

        #endregion May Override

        #region Private Fields

        /// <summary>
        /// Position of leader at start of this command.
        /// </summary>
        private Vector3 startPosition = Vector3.zero;

        /// <summary>
        /// Orientation of leader at start of this command.
        /// </summary>
        private Quaternion startRotation = Quaternion.identity;
        
        /// <summary>
        /// First target to move to, where we then wait and init TouchDown.
        /// </summary>
        private Pose target_TouchDownTrigger = Pose.identity;

        /// <summary>
        /// Internal states of landing-proceure:
        /// 1. Move to a Target Position (above the landing point).
        /// 2. Wait for Drone to reach it.
        /// 3. Initiate TouchDown (Soft landing).
        /// 4. Shut off Drone.
        /// 5. Finished landing.
        /// </summary>
        private enum LandingState { MoveTo, WaitForDrone, TouchDown, ShutOff, Done }

        /// <summary>
        /// Current internal state of landing-procedure
        /// </summary>
        private LandingState currentState = LandingState.MoveTo;

        /// <summary>
        /// Height where we want to init TouchDown. Will be set at Start of Command.
        /// </summary>
        private float touchDownHeight = -1;

        /// <summary>
        /// Height where we want to init ShutOff. Will be set at Start of Command.
        /// </summary>
        private float shutOffHeight = -1;

        /// <summary>
        /// Time for the leader to reach the first target, calculated by the distance at start and given speed.
        /// </summary>
        private float time = 0f;

        /// <summary>
        /// Time of starting to wait for drone to reach first target.
        /// </summary>
        private float waitingStartTime = 0f;

        #endregion Private Fields

        #region Overrides of FlightCommand-Base
        public override void StartCommand(DHUI_FlightController _flightController, Transform _leader, Transform _drone)
        {
            base.StartCommand(_flightController, _leader, _drone);

            startPosition = _leader.position;
            startRotation = _leader.rotation;
            
            // If we are landing on the floor, the targeted positions are the given position projected onto the floor.
            if (floorLanding)
            {
                targetPosition.y = _flightController.GetFloorHeight();
            }

            touchDownHeight = targetPosition.y + _flightController.GetTriggerHeight_TouchDown();
            shutOffHeight = targetPosition.y + flightController.GetTriggerHeight_ShutOff();
            

            target_TouchDownTrigger.rotation = targetRotation;
            target_TouchDownTrigger.position = targetPosition;
            target_TouchDownTrigger.position.y = touchDownHeight;
            
            time = Vector3.Distance(startPosition, target_TouchDownTrigger.position) / landingSpeed;

            currentState = LandingState.MoveTo;
        }

        protected override void SetFlightModeAtCommandStart()
        {
            flightController.TrySetFlightState(DHUI_FlightController.FlightState.Landing);
        }

        public override void UpdateCommand(out bool _finished)
        {
            _finished = false;

            switch (currentState)
            {
                case LandingState.MoveTo:
                    Move();
                    break;
                case LandingState.WaitForDrone:
                    Wait();
                    break;
                case LandingState.TouchDown:
                    TouchDown();
                    break;
                case LandingState.ShutOff:
                    ShutOff();
                    break;
                case LandingState.Done:
                    _finished = true;
                    return;
                default:
                    break;
            }
        }
        #endregion Overrides of FlightCommand-Base

        #region Internal Update-Logic
        
        private void Move()
        {
            if (drone.position.y <= touchDownHeight && floorLanding)
            {
                currentState = LandingState.TouchDown;
                return;
            }

            float timeSinceStart = Time.time - startTime;
            float fraction = timeSinceStart / time;
            
            if (fraction < 1)
            {
                leader.position = Vector3.Lerp(startPosition, target_TouchDownTrigger.position, fraction);
                leader.rotation = Quaternion.Slerp(startRotation, target_TouchDownTrigger.rotation, fraction);
            }
            else
            {
                leader.position = target_TouchDownTrigger.position;
                leader.rotation = target_TouchDownTrigger.rotation;
                currentState = LandingState.WaitForDrone;
                waitingStartTime = Time.time;
            }
        }

        private void Wait()
        {
            if (DroneReachedTargetTranslation() || (drone.position.y <= touchDownHeight && floorLanding) || Time.time >= waitingStartTime + waitingTimeout)
            {
                currentState = LandingState.TouchDown;
            }
        }

        private void TouchDown()
        {
            flightController.TrySetFlightState(DHUI_FlightController.FlightState.TouchDown);
            leader.position = drone.position;
            if (drone.position.y <= shutOffHeight)
            {
                currentState = LandingState.ShutOff;
            }
        }
        private void ShutOff()
        {
            leader.position = drone.position;
            DHUI_FlightController.FlightState newState = flightController.TrySetFlightState(DHUI_FlightController.FlightState.Parked);
            if (newState == DHUI_FlightController.FlightState.Parked)
            {
                currentState = LandingState.Done;
            }
            else
            {
                Debug.LogWarning("<b>DHUI</b> | FlightCommand_LandAt | Trying to Park drone, but shut off gets rejected. See other warnings and errors.");
            }
        }
        #endregion Internal Update-Logic
    }
}