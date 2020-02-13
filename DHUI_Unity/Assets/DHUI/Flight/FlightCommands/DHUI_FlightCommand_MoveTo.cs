using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    public class DHUI_FlightCommand_MoveTo : DHUI_FlightCommand_Base
    {
        public Vector3 targetPosition = Vector3.zero;
        public Quaternion targetRotation = Quaternion.identity;
        public float time = 5;
        public bool smoothInOut = false;
        public bool waitForDrone = false;
        
        protected Vector3 startPosition = Vector3.zero;
        protected Quaternion startRotation = Quaternion.identity;


        public override void StartCommand(DHUI_FlightController _flightController, Transform _leader, Transform _drone)
        {
            base.StartCommand(_flightController, _leader, _drone);

            startPosition = leader.position;
            startRotation = leader.rotation;
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
                else if (DroneFinished())
                {
                    _finished = true;
                }
            }

        }


        private bool UpdateLeader()
        {
            if (time <= 0)
            {
                SetPositionToEnd();
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
                    SetPositionToEnd();
                    return true;
                }
            }
        }

        private void Move_Linear(float fraction)
        {
            leader.position = Vector3.Lerp(startPosition, targetPosition, fraction);
            leader.rotation = Quaternion.Slerp(startRotation, targetRotation, fraction);
        }

        private void Move_SmoothInOut(float fraction)
        {
            float smoothStep = Mathf.SmoothStep(0f, 1f, fraction);
            leader.position = Vector3.Lerp(startPosition, targetPosition, smoothStep);
            leader.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothStep);
        }

        private void SetPositionToEnd()
        {
            leader.position = targetPosition;
            leader.rotation = targetRotation;
        }
        
    }
}