using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    public abstract class DHUI_FlightCommand_BaseLand : DHUI_FlightCommand_Base
    {
        protected Vector3 targetPosition = Vector3.zero;
        protected Quaternion targetRotation = Quaternion.identity;

        protected Vector3 startPosition = Vector3.zero;
        protected Quaternion startRotation = Quaternion.identity;

        protected Pose target_TouchDownTrigger = Pose.identity;

        protected float landingSpeed = 0.2f;
        private float time = 0f;

        private enum LandingState { MoveTo, WaitForDrone, TouchDown, ShutOff, Done }
        private LandingState currentState = LandingState.MoveTo;

        private float touchDownHeight = 0f;
        private float shutOffHeight = 0f;
        
        public override void StartCommand(DHUI_FlightController _flightController, Transform _leader, Transform _drone)
        {
            base.StartCommand(_flightController, _leader, _drone);

            startPosition = _leader.position;
            startRotation = _leader.rotation;

            touchDownHeight = _flightController.GetTriggerHeight_TouchDown();

            target_TouchDownTrigger.rotation = targetRotation;
            target_TouchDownTrigger.position = targetPosition;
            target_TouchDownTrigger.position.y = touchDownHeight;

            shutOffHeight = flightController.GetTriggerHeight_ShutOff();

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

        private void Move()
        {
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
            }
        }

        private void Wait()
        {
            if (DroneFinished())
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

    }
}