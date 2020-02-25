using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Diagnostics;

/// <summary>
/// This Namespace contains the Core features of DHUI – Drone Haptic User Interface.
/// These Scripts are essential for the system to work.
/// </summary>
namespace DHUI.Core
{
    /// <summary>
    /// This class controls the drone. 
    /// </summary>
    public class DHUI_DroneController : MonoBehaviour
    {
        #region Fields | Setup
        [Header("Setup")]       
        [SerializeField][Tooltip("The target object. This is necessary to always get the currently targeted position/orientation for the drone to fly to.")]
        private Transform _target = null;

        [SerializeField][Tooltip("This transform is controlled by this DroneController to serve as the virtual representation of the real drone.")]
        private Transform _virtualDrone = null;

        [SerializeField][Tooltip("The drone tracker holding information about the drone, e.g. real Position, Orientation and Velocity")]
        private DHUI_DroneTracking_Base _droneTracker = null;

        [SerializeField][Tooltip("The script handling the calculations of the PID-values for Throttle, Roll, Pitch and Yaw based on the drone's and target's transforms.")]
        private DHUI_PIDCalculation _PIDCalculator = null;

        [SerializeField][Tooltip("The script handling the output of the values to the serial.")]
        private DHUI_SerialOutput _serialOutput = null;

        [SerializeField][Tooltip("The script handling the emergency inputs (e.g. Emerency Hover, Emergency Land, Emergency ShutOff). This is required as a safety precaution.")]
        private DHUI_EmergencyInput_Base _emergencyInputs = null;

        #endregion Fields | Setup

        #region Fields | Calibration
        [Header("Calibration")]
        [Tooltip("Y-Coordinate/Height of the floor.")]
        public float _floorY = 0f;
        #endregion Fields | Calibration

        #region Fields | Thresholds
        [Header("Thresholds")]

        #region Fields | Thresholds | Tracking Lost
        [Tooltip("The amount of time (in ms) the tracking can be lost, before the drone starts to Emergency-Hover.")]
        public int _th_TrackingLost_MaxMsUntilHover = 1000;

        [Tooltip("The amount of time (in ms) the tracking can be lost, before the drone starts to Emergency-Land.")]
        public int _th_TrackingLost_MaxMsUntilLand = 5000;
        #endregion Fields | Thresholds | Tracking Lost

        #region Fields | Thresholds | Safety Thresholds
        [Tooltip("The threshold for minimum Centimeters above the floor, where it is save to fly to without the danger of crashing into the floor.")]
        public float _th_SaveFlying_MinCmFromFloor = 30f;
        
        [Tooltip("The maximum threshold in Centimeters above the floor, where it is still save to shut off the drone.")]
        public float _th_SaveShutDown_MaxCmFromFloor = 10f;

        [Tooltip("The maximum value the throttle is allowed to have for a save shut off.")]
        public float _th_SaveShutDown_MaxThrottle = 1200f;
        
        [Tooltip("The threshold of the maximum velocity the drone is allowed to have and still be save to shut off.")]
        public float _th_SaveShutDown_MaxVelocity = 2f;
        #endregion Fields | Thresholds | Safety Thresholds

        #region Fields | Thresholds | Trigger Distances for Regular Landings
        [Tooltip("The trageted distance in Centimeters above the floor, where we want to initiate the soft landing behaviour.")]
        public float _th_RegularLanding_InitLand_TargetCmFromFloor = 30f;
        
        [Tooltip("The targeted distance in Centimeters above the floor, where we want to initiate the shut off of the drone.")]
        public float _th_RegularLanding_ShutOff_TargetCmFromFloor = 2f;
        #endregion Fields | Thresholds | Trigger Distances for Regular Landings

        #endregion Fields | Thresholds

        #region Fields | Info
        [Header("Info")]
        [Tooltip("Public Information: Wether the tracking was lost for too long (beyond the defined threshold).")]
        public bool error_TrackingLost_Info = false;
        #endregion Fields | Info

        #region Fields | Private

        // Wether the Setup is correct and successful. Blocks the drone from starting (except with ForceSetDroneState).
        private bool setupCorrect = false;

        // Wether the drone is currently in emergency mode. This blocks any changes in behaviour while true.
        private bool emergencyMode = false;

        // Wether the tracking was lost for too long (beyond the defined threshold). This is used internally, while "error_trackingLost_info" is only used for public information.
        private bool error_trackingLost_internal = false;

        // Stopwatch used to time the tracking loss.
        private Stopwatch error_trackingLost_timer = new Stopwatch();
        
        /* TODO
        public bool error_noFlyZone
        {
            get { return error_noFlyZone_internal; }
        }
        private bool error_noFlyZone_internal = false;
        */

        // Current State the drone is in.
        private DroneState currentDroneState = DroneState.Off;

        // The current values that should be output to the drone
        // Channels:         Throttle, Roll, Pitch, Yaw, ----, ----, ----, ----)
        private int[] values = { 1000, 1500, 1500, 1500, 1000, 1000, 1000, 1000 };

        // The default/idle values of the channels.
        private readonly int[] defaultValues = { 1000, 1500, 1500, 1500, 1000, 1000, 1000, 1000 };

        #endregion Fields | Private

        #region Enums 
        /// <summary>
        /// The different states the drone can assume:
        /// Off : Drone will be idle/off. It will fall, if not parked.
        /// Follow: Default state while in flight. The drone will follow the target.
        /// Hover: The Drone will stand still / hover in the air.
        /// Land: The drone will slowly decrease its Throttle to softly land on floor.
        /// </summary>
        public enum DroneState { Off, Follow, Hover, Land }
        #endregion Enums

        #region Methods | Public

        /// <summary>
        /// Gets the current state of the drone.
        /// </summary>
        /// <returns>Current state.</returns>
        public DroneState GetDroneState()
        {
            return currentDroneState;
        }

        /// <summary>
        /// Tries to set the DroneState to the given value. This changes the behaviour and output to the actual drone.
        /// </summary>
        /// <param name="_newDroneState">New State we try to set it to.</param>
        /// <returns>Wether setting the state has worked.</returns>
        public bool TrySetDroneState(DroneState _newDroneState)
        {
            if (emergencyMode)
            {
                Debug.LogError("<b>DHUI</b> | DroneController | Tried to change drone state, but drone is in emergency mode.");
                return false;
            }
            if (!setupCorrect)
            {
                Debug.LogError("<b>DHUI</b> | DroneController | Tried to change drone state, but setup was not correct. Check other error-messages and make sure all required fields in the inspector are assigned.");
                return false;
            }
            // If we are already touching down to land (either by choice or due to error e.g. a long tracking loss), going back to 'Follow' is disallowed to ensure safety.
            else if (currentDroneState == DroneState.Land && _newDroneState == DroneState.Follow)
            {
                Debug.LogWarning("<b>DHUI</b> | DroneController | Tried to set DroneState to 'Follow' while drone is Landing. This is disallowed due to safety concerns.");
                return false;
            }
            // While the tracking is not working, don't allow the state to be changed to 'Follow'.
            else if (_newDroneState == DroneState.Follow && error_trackingLost_internal)
            {
                Debug.LogWarning("<b>DHUI</b> | DroneController | Tried to set DroneState to 'Follow' while drone has lost tracking.");
                return false;
            }
            // If we are currently up in the air (Following or Hovering), don't allow the drone to be shut off.
            else if ((currentDroneState == DroneState.Follow || currentDroneState == DroneState.Hover) && _newDroneState == DroneState.Off)
            {
                Debug.LogWarning("<b>DHUI</b> | DroneController | Tried to set DroneState to 'Off', but this is not allowed in current state: '" + currentDroneState + "', because the drone would fall to the ground.");
                return false;
            }
            else if (_newDroneState == DroneState.Off && !DroneSaveToTurnOff())
            {
                Debug.LogWarning("<b>DHUI</b> | DroneController | Tried to set DroneState to 'Off', but drone is not save to turn off now.");
                return false;
            }
            else if (currentDroneState == DroneState.Off && (_newDroneState == DroneState.Hover || _newDroneState == DroneState.Land))
            {
                Debug.LogWarning("<b>DHUI</b> | DroneController | Tried to set DroneState to '" + _newDroneState + "', but this is not allowed nor useful while drone is 'Off'.");
                return false;
            }
            currentDroneState = _newDroneState;
            return true;
        }

        /// <summary>
        /// Forces the DroneState to set to the given value. This should only be used for emergency situations e.g. Emergency Hover or Emergency Shut-Down.
        /// </summary>
        /// <param name="droneState">Drone state to set it to.</param>
        public void ForceSetDroneState(DroneState _droneState)
        {
            currentDroneState = _droneState;
        }

        /// <summary>
        /// Sets the emergencyMode, which will block all changes to the drone's behaviour (except with ForceSetDroneState) if 'true'.
        /// </summary>
        /// <param name="_emergency"></param>
        public void SetEmergencyMode(bool _emergency)
        {
            Debug.LogWarning("<b>DHUI</b> | DroneController | EmergencyMode was set to '" + _emergency + "'.");
            emergencyMode = _emergency;
        }

        /// <summary>
        /// Returns the transform of the virtual representation of the drone.
        /// </summary>
        /// <returns>Transform of virtual drone object.</returns>
        public Transform GetVirtualDrone()
        {
            return _virtualDrone;
        }
        
        /// <summary>
        /// Tells us, wether the drone is save to turn off. This returns false if the drone is too high in the air, the throttle ouput or velocity is too high or we have an error and don't know.
        /// </summary>
        /// <returns>If the Drone is save to turn off (True = Save, False = Unsave).</returns>
        public bool DroneSaveToTurnOff()
        {
            bool save = false;
            if (!error_trackingLost_internal && values[0] <= _th_SaveShutDown_MaxThrottle && _droneTracker.dronePosition.y <= (_floorY + _th_SaveShutDown_MaxCmFromFloor/100) && _droneTracker.droneVelocity.magnitude <= _th_SaveShutDown_MaxVelocity)
            {
                save = true;
            }
            return save;
        }
        #endregion Methods | Public

        #region Methods | Start/Update
        /// <summary>
        /// On Start: Check if everything necessary is set up correctly.
        /// </summary>
        private void Start()
        {
            if (_target == null)
            {
                DHUI_FlightController flightController = FindObjectOfType<DHUI_FlightController>(); 
                if (flightController != null)
                {
                    _target = flightController.transform;
                    Debug.Log("<b>DHUI</b> | DroneController | No Transform for Target was set in Inspector -> Defaulting to transform of DHUI_FlightController. (\"" + _target.gameObject.name + "\")");
                }
                else
                {
                    Debug.LogError("<b>DHUI</b> | DroneController | No Transform for Target was set in Inspector and no DHUI_FlightController could be found in Scene. Please set a Target in the Unity Inspector.");
                }
            }
            if (_virtualDrone == null)
            {
                _virtualDrone = transform;
                Debug.Log("<b>DHUI</b> | DroneController | No Transform for VirtualDrone was set in Inspector -> Defaulting to this transform. (\"" + gameObject.name + "\")");
            }
            if (_droneTracker == null)
            {
                _droneTracker = FindObjectOfType<DHUI_DroneTracking_Base>();
                if (_droneTracker != null)
                {
                    Debug.Log("<b>DHUI</b> | DroneController | No DHUI_DroneTracking_Base was set in Inspector -> Found one in the scene on \""+ _droneTracker.gameObject.name + "\".");
                }
                else
                {
                    Debug.LogError("<b>DHUI</b> | DroneController | No DHUI_DroneTracking_Base was set in Inspector and none was found in the scene.");
                }
            }
            if (_PIDCalculator == null)
            {
                _PIDCalculator = FindObjectOfType<DHUI_PIDCalculation>();
                if (_PIDCalculator != null)
                {
                    Debug.Log("<b>DHUI</b> | DroneController | No DHUI_PIDCalculation was set in Inspector -> Found one in the scene on \"" + _PIDCalculator.gameObject.name + "\".");
                }
                else
                {
                    Debug.LogError("<b>DHUI</b> | DroneController | No DHUI_PIDCalculation was set in Inspector and none was found in the scene.");
                }
            }
            if (_serialOutput == null)
            {
                _serialOutput = FindObjectOfType<DHUI_SerialOutput>();
                if (_serialOutput != null)
                {
                    Debug.Log("<b>DHUI</b> | DroneController | No DHUI_SerialOutput was set in Inspector -> Found one in the scene on \"" + _serialOutput.gameObject.name + "\".");
                }
                else
                {
                    Debug.LogError("<b>DHUI</b> | DroneController | No DHUI_SerialOutput was set in Inspector and none was found in the scene.");
                }
            }
            if (_emergencyInputs == null)
            {
                _emergencyInputs = FindObjectOfType<DHUI_EmergencyInput_Base>();
                if (_emergencyInputs != null)
                {
                    Debug.Log("<b>DHUI</b> | DroneController | No DHUI_EmergencyInput was set in Inspector -> Found one in the scene on \"" + _emergencyInputs.gameObject.name + "\".");
                }
                else
                {
                    Debug.LogError("<b>DHUI</b> | DroneController | No DHUI_EmergencyInput_Base was set in Inspector and none was found in the scene.");
                }
            }
            _emergencyInputs.Setup(this);

            if (_target != null && _droneTracker != null && _serialOutput != null && _PIDCalculator != null && _emergencyInputs != null)
            {
                setupCorrect = true;
            }
        }

        /// <summary>
        /// Update-Loop:
        /// 1.) Check for any errors
        /// 2.) Update virtual representation of drone
        /// 3.) Control drone behaviour
        /// 4.) Write Output values
        /// </summary>
        private void Update()
        {
            CheckUp();
            UpdateVirtualDrone();

            switch (currentDroneState)
            {
                case DroneState.Follow:
                    DroneFollow();
                    break;
                case DroneState.Hover:
                    DroneHover();
                    break;
                case DroneState.Land:
                    DroneLand();
                    break;
                case DroneState.Off:
                default:
                    DroneOff();
                    break;
            }

            WriteOutput();
        }
        #endregion Methods | Start/Update

        #region Methods | Checking
        /// <summary>
        /// Check if any errors are occuring.
        /// </summary>
        private void CheckUp()
        {
            CheckTracking();
        }

        /// <summary>
        /// Check if tracking is working fine. If the tracking is lost beyond the defined thresholds, the drone will Hover and/or Land and set an error-flag internally.
        /// </summary>
        private void CheckTracking()
        {
            if (_droneTracker.trackingOK)
            {
                if (error_trackingLost_timer != null && error_trackingLost_timer.IsRunning)
                {
                    Debug.LogWarning("<b>DHUI</b> | DroneController | Lost Tracking for " + error_trackingLost_timer.ElapsedMilliseconds + "ms.");
                    error_trackingLost_timer.Stop();
                    error_TrackingLost_Info = false;
                    error_trackingLost_internal = false;
                }
            }
            else
            {
                if (error_trackingLost_timer == null) error_trackingLost_timer = new Stopwatch();
                if (!error_trackingLost_timer.IsRunning)
                {
                    error_trackingLost_timer.Start();
                }
                else
                {
                    if (error_trackingLost_timer.ElapsedMilliseconds > _th_TrackingLost_MaxMsUntilLand && _th_TrackingLost_MaxMsUntilLand >= 0)
                    {
                        error_TrackingLost_Info = true;
                        error_trackingLost_internal = true;
                        ForceSetDroneState(DroneState.Land);
                        SetEmergencyMode(true);
                    }
                    else if (error_trackingLost_timer.ElapsedMilliseconds > _th_TrackingLost_MaxMsUntilHover && _th_TrackingLost_MaxMsUntilHover >= 0)
                    {
                        error_TrackingLost_Info = true;
                        error_trackingLost_internal = true;
                        ForceSetDroneState(DroneState.Hover);
                    }
                }
            }
        }
        #endregion Methods | Checking

        #region Methods | Output
        /// <summary>
        /// Sets the Transform of the virtual drone to the transform of the real tracked drone.  
        /// </summary>
        private void UpdateVirtualDrone()
        {
            _virtualDrone.position = _droneTracker.droneTransform.position;
            _virtualDrone.rotation = _droneTracker.droneTransform.rotation;
        }

        /// <summary>
        /// Sets the output values, which should be written to the serial.
        /// </summary>
        private void WriteOutput()
        {
            _serialOutput.SetValues(values);
        }
        #endregion Methods | Output

        #region Methods | Drone Behaviours
        /// <summary>
        /// Drone follows the Target. This is default state while in flight.
        /// </summary>
        private void DroneFollow()
        {
            UpdatePID(true, true);

            values[0] = _PIDCalculator.GetThrottle();
            values[1] = _PIDCalculator.GetRoll();
            values[2] = _PIDCalculator.GetPitch();
            values[3] = _PIDCalculator.GetYaw();
        }

        /// <summary>
        /// Drone hovers at its current location.
        /// </summary>
        private void DroneHover()
        {
            UpdatePID(true, false);

            values[0] = _PIDCalculator.GetThrottle();
            values[1] = _PIDCalculator.GetRoll();
            values[2] = _PIDCalculator.GetPitch();
            values[3] = _PIDCalculator.GetYaw();
        }

        /// <summary>
        /// Drone slowly reduces throttle to softly land at its current position.
        /// </summary>
        private void DroneLand()
        {
            UpdatePID(true, false);
            
            if (values[0] > 1001) {
                values[0] -= 1;
            }
            values[1] = _PIDCalculator.GetRoll();
            values[2] = _PIDCalculator.GetPitch();
            values[3] = _PIDCalculator.GetYaw();
        }

        /// <summary>
        /// Drone will be shut-off/ set to idle. 
        /// Warning: While in flight the drone will fall.  
        /// </summary>
        private void DroneOff()
        {
            UpdatePID(false);
            ResetValues();
        }

        /// <summary>
        /// Update the values needed by the PID Equations to calculate their output.
        /// </summary>
        /// <param name="_calculatePIDs">Wether PIDs should be calculated at all.</param>
        /// <param name="followTarget">Wether the drone should follow the target. If set to false, the PID error will be 0 -> drone will stop moving/hover.</param>
        private void UpdatePID(bool _calculatePIDs, bool followTarget = true)
        {
            _PIDCalculator.ToggleCalculation(_calculatePIDs);            
            if (_calculatePIDs)
            {
                if (followTarget)
                {
                    _PIDCalculator.UpdateTrajectory(_droneTracker.dronePosition, _droneTracker.droneForward, _droneTracker.droneVelocity, _target.position, _target.forward, _droneTracker.droneDistanceToTarget(_target.position));
                }
                else
                {
                    _PIDCalculator.UpdateTrajectory(_droneTracker.dronePosition, _droneTracker.droneForward, _droneTracker.droneVelocity, _droneTracker.dronePosition, _droneTracker.droneForward, Vector3.zero);
                }
            }
        }

        /// <summary>
        /// Reset Output-Values to default/idle.
        /// </summary>
        private void ResetValues()
        {
            defaultValues.CopyTo(values,0);
        }
        #endregion Methods | Drone Behaviours

    }

}