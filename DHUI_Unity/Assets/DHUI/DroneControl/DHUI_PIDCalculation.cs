using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System;
using System.Threading;
using System.Diagnostics;

/// <summary>
/// This Namespace contains the Core features of DHUI – Drone Haptic User Interface.
/// These Scripts are essential for the system to work.
/// </summary>
namespace DHUI.Core
{
    /// <summary>
    /// This class handles the Calculations for the Output of Throttle, Roll, Pitch and Yaw of the drone using PID (Proportional-Integral-Derivative) equations.
    /// It requires the Transform of the Drone and the Transform of the Target and continuously calculates the error (distance) of these two.
    /// </summary>
    public class DHUI_PIDCalculation : MonoBehaviour
    {
        // Current Position of Drone
        private Vector3 currentVector = Vector3.zero;

        // Current Orientation of Drone
        private Vector3 currentVectorForward = Vector3.zero;

        // Current Velocity of Drone (according to SteamVR)
        private Vector3 currentVectorVelocity = Vector3.zero;

        // Current Position of Target
        private Vector3 targetVector = Vector3.zero;

        // Current Orientation of Target
        private Vector3 targetVectorForward = Vector3.zero;

        // Distance to Target (In local space of drone)
        private Vector3 distanceToTarget = Vector3.zero;

        // Thread handling the PID-Calculations at 90Hz
        private Thread PIDThread;

        // If PIDs should be currently calculated.
        private bool calculatePIDs = false;

        // Output-Array containing: Throttle, Roll, Pitch, Yaw
        private int[] outputValues = { 1000, 1500, 1500, 1500 };

        /// <summary>
        /// On Start: The PID-Thread is started.
        /// </summary>
        private void Start()
        {
            StartPIDLoop();
        }

        #region Public Methods
        /// <summary>
        /// Updates the current position of the drone and its target to adjust the trajectory accordingly.
        /// </summary>
        public void UpdateTrajectory(Vector3 _dronePosition, Vector3 _droneForward, Vector3 _droneVelocity, Vector3 _targetPosition, Vector3 _targetForward, Vector3 _distanceToTarget)
        {
            currentVector = _dronePosition;
            currentVectorForward = _droneForward;
            currentVectorVelocity = _droneVelocity;

            targetVector = _targetPosition;
            targetVectorForward = _targetForward;

            distanceToTarget = _distanceToTarget;
        }

        /// <summary>
        /// Determines wether the PIDs in the PID-Loop will be calculated. On when Flying, Off when Parked/Error/Emergency/etc.
        /// </summary>
        /// <param name="_calculationOn">Calculation: true = on, false = off.</param>
        public void ToggleCalculation(bool _calculationOn){
            calculatePIDs = _calculationOn;
        }

        /// <summary>
        /// Gets the calculated output-values of Throttle, Roll, Pitch and Yaw
        /// </summary>
        /// <returns></returns>
        public int[] GetAll()
        {
            return outputValues;
        }

        /// <summary>
        /// Gets the calculated output-value of Throttle.
        /// </summary>
        /// <returns>Throttle output-value.</returns>
        public int GetThrottle()
        {
            return outputValues[0];
        }

        /// <summary>
        /// Gets the calculated output-value of Roll.
        /// </summary>
        /// <returns>Roll output-value.</returns>
        public int GetRoll()
        {
            return outputValues[1];
        }

        /// <summary>
        /// Gets the calculated output-value of Pitch.
        /// </summary>
        /// <returns>Pitch output-value.</returns>
        public int GetPitch()
        {
            return outputValues[2];
        }

        /// <summary>
        /// Gets the calculated output-value of Yaw.
        /// </summary>
        /// <returns>Yaw output-value.</returns>
        public int GetYaw()
        {
            return outputValues[3];
        }
        #endregion Public Methods

        #region PID Loop
        /// <summary>
        /// Starts a new Thread running the PID-Loop
        /// </summary>
        private void StartPIDLoop()
        {
            PIDThread = new Thread(PIDLoop);
            PIDThread.Start();
        }
        /// <summary>
        /// If this GameObject or Script gets disabled, the thread is aborted and closed safely.
        /// </summary>
        private void OnDisable()
        {
            PIDThread.Abort();
            PIDThread.Join();
        }
        /// <summary>
        /// If this GameObject or Script gets destroyed, the thread is aborted and closed safely.
        /// </summary>
        private void OnDestroy()
        {
            PIDThread.Abort();
            PIDThread.Join();
        }
        /// <summary>
        /// This Method runs the PID Calculations and outputs them to the "outputValues"-Array at 90Hz.
        /// It uses System.Diagnostics.Stopwatch to calculate the time deltas for PID. 
        /// </summary>
        private void PIDLoop()
        {
            Stopwatch sw = null;
            double deltaTime = 0;

            while (true)
            {
                if (sw != null)
                {
                    deltaTime = sw.Elapsed.TotalMilliseconds / 1000;
                }
                else
                {
                    deltaTime = 0.008;
                }
                sw = Stopwatch.StartNew();
                if (calculatePIDs)
                {
                    outputValues[0] = calculateThrottlePID(deltaTime);
                    outputValues[1] = calculateRollPID(deltaTime);
                    outputValues[2] = calculatePitchPID(deltaTime);
                    outputValues[3] = calculateYawPID();
                }
                Thread.Sleep(11);
                sw.Stop();
            }
        }
        #endregion PID Loop

        #region Throttle
        [Header("Throttle PIDs")]
        [SerializeField]
        private double throttle_KP = 0.66;
        [SerializeField]
        private double throttle_KI = 0.48;
        [SerializeField]
        private double throttle_KD = 0.5;   

        private const int throttle_lowerLimit = 1000;
        private const int throttle_upperLimit = 1550; // may be necessary to be adapted to more weight/other battery. theoretical max is 2000
        private const int throttle_integral_lowerLimit = -150;
        private const int throttle_integral_upperLimit = 150;

        private const double throttle_hover = 1450; // needs to be evaluated for a different drone / weight setup
        private const double throttle_smoothingFactor = 0.1;

        private double throttle_errorPrior = 0;
        private double throttle_integral = 0;
        private double throttle_lastDerivativeFiltered = 0;

        /// <summary>
        /// Calculates the Output of the Throttle using PID-Equations.
        /// </summary>
        /// <param name="deltaTime">Time since last Calculation</param>
        /// <returns></returns>
        private int calculateThrottlePID(double deltaTime)
        {
            // P
            float error = (targetVector.y - currentVector.y) * 100;

            // I
            throttle_integral = throttle_integral + (error * deltaTime);
            Clamp(ref throttle_integral, throttle_integral_lowerLimit, throttle_integral_upperLimit);
            
            // D
            double derivative = (error - throttle_errorPrior) / deltaTime;
            double derivativeFilteredThrottle = (1 - throttle_smoothingFactor) * throttle_lastDerivativeFiltered + throttle_smoothingFactor * derivative;
            throttle_lastDerivativeFiltered = derivativeFilteredThrottle;
            throttle_errorPrior = error;

            // Resulting Output
            double output = throttle_KP * error + throttle_KI * throttle_integral + throttle_KD * derivativeFilteredThrottle + throttle_hover;
            Clamp(ref output, throttle_lowerLimit, throttle_upperLimit);            
            return Convert.ToInt32(output);
        }

        #endregion Throttle

        #region Roll
        [Header("Roll PIDs")]
        [SerializeField]
        private double roll_KP = 1;
        [SerializeField]
        private double roll_KI = 0.4;
        [SerializeField]
        private double roll_KD = 3;

        private const double roll_integral_upperLimit = 100;
        private const double roll_integral_lowerLimit = -100;
        private const double roll_derivative_upperLimit = 200;
        private const double roll_derivative_lowerLimit = -200;
        private const double roll_upperLimit = 1600;
        private const double roll_lowerLimit = 1400;

        private const double roll_hover = 1500;
        private const double roll_smoothingFactor = 0.1;

        private double roll_integral = 0;
        private double roll_errorPrior = 0;
        private double roll_lastDerivativeFiltered = 0;

        /// <summary>
        /// Calculates the Output of the Roll using PID-Equations.
        /// </summary>
        /// <param name="deltaTime">Time since last Calculation</param>
        /// <returns></returns>
        private int calculateRollPID(double deltaTime)
        {
            // P
            double error = distanceToTarget.x * 100;
            
            // I
            roll_integral = roll_integral + (error * deltaTime);
            Clamp(ref roll_integral, roll_integral_lowerLimit, roll_integral_upperLimit);
            
            // D
            double derivativeRoll = (error - roll_errorPrior) / deltaTime;
            double derivativeFilteredRoll = (1 - roll_smoothingFactor) * roll_lastDerivativeFiltered + roll_smoothingFactor * derivativeRoll;
            if (roll_KD * derivativeFilteredRoll > roll_derivative_upperLimit)
                derivativeFilteredRoll = roll_derivative_upperLimit / roll_KD;
            if (roll_KD * derivativeFilteredRoll < roll_derivative_lowerLimit)
                derivativeFilteredRoll = roll_derivative_lowerLimit / roll_KD;
            roll_lastDerivativeFiltered = derivativeFilteredRoll;
            roll_errorPrior = error;

            // Resulting Output
            double output = roll_KP * (error - currentVectorVelocity.x) + roll_KI * roll_integral + roll_KD * derivativeFilteredRoll + roll_hover;
            Clamp(ref output, roll_lowerLimit, roll_upperLimit);
            return Convert.ToInt32(output);

        }
        #endregion Roll

        #region Pitch
        [Header("Pitch PIDs")]
        [SerializeField]
        private double pitch_KP = 1;
        [SerializeField]
        private double pitch_KI = 0.4;
        [SerializeField]
        private double pitch_KD = 3;

        private const double pitch_integral_upperLimit = 100;
        private const double pitch_integral_lowerLimit = -100;
        private const double pitch_derivative_upperLimit = 200;
        private const double pitch_derivative_lowerLimit = -200;
        private const double pitch_upperLimit = 1600;
        private const double pitch_lowerLimit = 1400;

        private const double pitch_hover = 1500;
        private const double pitch_smoothingFactor = 0.1;

        private double pitch_integral = 0;
        private double pitch_errorPrior = 0;
        private double pitch_lastDerivativeFiltered = 0;

        /// <summary>
        /// Calculates the Output of the Pitch using PID-Equations.
        /// </summary>
        /// <param name="deltaTime">Time since last Calculation</param>
        /// <returns></returns>
        private int calculatePitchPID(double deltaTime)
        {
            // P
            double error = distanceToTarget.z * 100;

            // I
            pitch_integral = pitch_integral + (error * deltaTime);
            Clamp(ref pitch_integral, pitch_integral_lowerLimit, pitch_integral_upperLimit);

            // D
            double derivativePitch = (error - pitch_errorPrior) / deltaTime;
            double derivativeFilteredPitch = (1 - pitch_smoothingFactor) * pitch_lastDerivativeFiltered + pitch_smoothingFactor * derivativePitch;
            if (pitch_KD * derivativeFilteredPitch > pitch_derivative_upperLimit)
                derivativeFilteredPitch = pitch_derivative_upperLimit / pitch_KD;
            if (pitch_KD * derivativeFilteredPitch < pitch_derivative_lowerLimit)
                derivativeFilteredPitch = pitch_derivative_lowerLimit / pitch_KD;
            pitch_lastDerivativeFiltered = derivativeFilteredPitch;
            pitch_errorPrior = error;

            // Resulting Output
            double output = pitch_KP * (error - currentVectorVelocity.z) + pitch_KI * pitch_integral + pitch_KD * derivativeFilteredPitch + pitch_hover;
            Clamp(ref output, pitch_lowerLimit, pitch_upperLimit);
            return Convert.ToInt32(output);

        }
        #endregion Pitch

        #region Yaw
        [Header("Yaw PIDs")]
        [SerializeField]
        private double yaw_KP = 5;

        private const double yaw_hover = 1500;
        private const double yaw_lowerLimit = 1300;
        private const double yaw_upperLimit = 1700;

        /// <summary>
        /// Calculates the Output of the Yaw. It only uses a Proportional equation, because overshoot is minimal (drone does not need to tilt to yaw).
        /// </summary>
        /// <returns></returns>
        private int calculateYawPID()
        {
            double errorAngle = Vector3.Angle(currentVectorForward, targetVectorForward);
            Vector3 cross = Vector3.Cross(currentVectorForward, targetVectorForward);
            if (cross.y < 0)
            {
                errorAngle = -errorAngle;
            }

            double output = yaw_KP * errorAngle + yaw_hover;
            Clamp(ref output, yaw_lowerLimit, yaw_upperLimit);
            return Convert.ToInt16(output);
        }
        #endregion Yaw

        #region Math Utilities
        /// <summary>
        /// Clamps value to the range of min to max.
        /// </summary>
        /// <param name="value">Value that should be clamped</param>
        /// <param name="min">Lower Limit</param>
        /// <param name="max">Upper Limit</param>
        private static void Clamp(ref double value, double min, double max)
        {
            // Check if limits where switched
            if (min > max)
            {
                double tmp_min = min;
                min = max;
                max = tmp_min;
            }
            // Check Lower Limit
            if (value <= min) value = min;
            // Check Upper Limit
            else if (value >= max) value = max;
        }

        #endregion Math Utilities
    }

}