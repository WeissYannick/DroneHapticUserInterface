using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = UnityEngine.Debug;
using System.Diagnostics;
using System;

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
        [Header("Members")]
        [SerializeField] [Tooltip("The target object. This is necessary to always get the currently targeted position/orientation for the drone to fly to.")]
        private Transform _target = null;

        [SerializeField] [Tooltip("This transform is controlled by this DroneController to serve as the virtual representation of the real drone.")]
        private Transform _virtualDrone = null;

        [SerializeField] [Tooltip("The drone tracker holding information about the drone, e.g. real Position, Orientation and Velocity")]
        private DHUI_DroneTracking_Base _droneTracker = null;

        [SerializeField] [Tooltip("The script handling the calculations of the PID-values for Throttle, Roll, Pitch and Yaw based on the drone's and target's transforms.")]
        private DHUI_PIDCalculation _PIDCalculator = null;

        [SerializeField] [Tooltip("The script handling the output of the values to control the drone.")]
        private DHUI_Output _output = null;

        [SerializeField] [Tooltip("The script handling the emergency inputs (e.g. Emergency Hover, Emergency Land, Emergency ShutOff). This is required as a safety precaution.")]
        private DHUI_EmergencyInput_Base _emergencyInputs = null;
        
        [SerializeField][Tooltip("Transform of the center point of the drone.")]
        private Transform _dronePoint_center = null;

        [SerializeField][Tooltip("Transform of the center point of the front face of the drone.")]
        private Transform _dronePoint_frontFace = null;

        [SerializeField][Tooltip("Transforms of the bounding box points of the front face of the drone.")]
        private List<Transform> _dronePoint_frontFaceBoundingPoints = new List<Transform>();

        [SerializeField][Tooltip("Transform of the center point of the back face of the drone.")]
        private Transform _dronePoint_backFace = null;

        [SerializeField][Tooltip("Transforms of the bounding box points of the back face of the drone.")]
        private List<Transform> _dronePoint_backFaceBoundingPoints = new List<Transform>();

        [SerializeField][Tooltip("Transform of the center point of the left face of the drone.")]
        private Transform _dronePoint_leftFace = null;
        
        [SerializeField][Tooltip("Transforms of the bounding box points of the left face of the drone.")]
        private List<Transform> _dronePoint_leftFaceBoundingPoints = new List<Transform>();

        [SerializeField][Tooltip("Transform of the center point of the right face of the drone.")]
        private Transform _dronePoint_rightFace = null;

        [SerializeField][Tooltip("Transforms of the bounding box points of the right face of the drone.")]
        private List<Transform> _dronePoint_rightFaceBoundingPoints = new List<Transform>();
        #endregion Fields | Setup

        #region Fields | Public Settings
        [Header("Public Settings")]
        /// <summary>
        /// The currently selected face of the drone as active face. This will determine the contactPointOffset and theresfore which face will be positioned on the target.
        /// For regular use (without direct touch interaction), "Center"-mode is recommended, resulting in the drone's center to be positioned at the target location.
        /// For use with direct touch interaction, on of the faces other than "Center" should be selected, to require the drone to position this face on the target location, rather than its center. 
        /// </summary>
        [Tooltip("The currently selected face of the drone as active face. This will determine the contactPointOffset and theresfore which face will be positioned on the target." 
        + "For regular use (without direct touch interaction), 'Center'-mode is recommended, resulting in the drone's center to be positioned at the target location." 
        + "For use with direct touch interaction, on of the faces other than 'Center' should be selected, to require the drone to position this face on the target location, rather than its center.")]
        public DroneActiveFaceOptions currentActiveFace = DroneActiveFaceOptions.Center;

        #endregion Fields | Settings

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

        [Tooltip("Information about current State/Mode of the drone.")]
        public DroneState droneState_Info = DroneState.Off;

        [Tooltip("Information about wether the drone is currently in emergency mode.")]
        public bool emergencyMode_Info = false;

        [Tooltip("Public Information: Wether the tracking was lost for too long (beyond the defined threshold).")]
        public bool error_TrackingLost_Info = false;

        #endregion Fields | Info

        #region Fields | Private

        /// <summary>
        /// Wether the Setup is correct and successful. Blocks the drone from starting (except with ForceSetDroneState).
        /// </summary>
        private bool setupCorrect = false;

        /// <summary>
        /// Wether the drone is currently in emergency mode. This blocks any changes in behaviour while true.
        /// </summary>
        private bool emergencyMode = false;

        /// <summary>
        /// Wether the tracking was lost for too long (beyond the defined threshold). This is used internally, while "error_trackingLost_info" is only used for public information.
        /// </summary>
        private bool error_trackingLost_internal = false;

        /// <summary>
        /// Stopwatch used to time the tracking loss.
        /// </summary>
        private Stopwatch error_trackingLost_timer = new Stopwatch();

        /* TODO
        public bool error_noFlyZone
        {
            get { return error_noFlyZone_internal; }
        }
        private bool error_noFlyZone_internal = false;
        */

        /// <summary>
        /// Current State the drone is in.
        /// </summary>
        private DroneState currentDroneState = DroneState.Off;

        /// <summary>
        /// The current values that should be output to the drone
        /// Channels:         Throttle, Roll, Pitch, Yaw, ----, ----, ----, ----)
        /// </summary>
        private int[] values = { 1000, 1500, 1500, 1500, 1000, 1000, 1000, 1000 };

        /// <summary>
        /// The default/idle values of the channels.
        /// </summary>
        private readonly int[] defaultValues = { 1000, 1500, 1500, 1500, 1000, 1000, 1000, 1000 };
        
        /// <summary>
        /// The offset of the drone's center (position of '_virtualDrone') to the actual contact plane/point (position of the center point of the currently active Face).
        /// </summary>
        public Vector3 contactPointPositionOffset
        {
            get
            {
                Transform contactPoint = null;
                switch (currentActiveFace)
                {
                    case DroneActiveFaceOptions.Center:
                        contactPoint = _dronePoint_center;
                        break;
                    case DroneActiveFaceOptions.Front:
                        contactPoint = _dronePoint_frontFace;
                        break;
                    case DroneActiveFaceOptions.Back:
                        contactPoint = _dronePoint_backFace;
                        break;
                    case DroneActiveFaceOptions.Left:
                        contactPoint = _dronePoint_leftFace;
                        break;
                    case DroneActiveFaceOptions.Right:
                        contactPoint = _dronePoint_rightFace;
                        break;
                    default:
                        break;
                }

                if (contactPoint == null || _virtualDrone == null) return Vector3.zero;

                return _virtualDrone.transform.TransformVector(_virtualDrone.transform.InverseTransformPoint(contactPoint.transform.position) - _virtualDrone.transform.InverseTransformPoint(_virtualDrone.transform.position));
            }
        }
        /// <summary>
        /// The rotation offset (angle around Y-Axis) of the drone's center (rotation of '_virtualDrone') to the actual contact plane/point (rotation of the center point of the currently active Face).
        /// </summary>
        public float contactPointRotationOffset
        {
            get
            {
                Transform contactPoint = null;
                switch (currentActiveFace)
                {
                    case DroneActiveFaceOptions.Center:
                        contactPoint = _dronePoint_center;
                        break;
                    case DroneActiveFaceOptions.Front:
                        contactPoint = _dronePoint_frontFace;
                        break;
                    case DroneActiveFaceOptions.Back:
                        contactPoint = _dronePoint_backFace;
                        break;
                    case DroneActiveFaceOptions.Left:
                        contactPoint = _dronePoint_leftFace;
                        break;
                    case DroneActiveFaceOptions.Right:
                        contactPoint = _dronePoint_rightFace;
                        break;
                    default:
                        break;
                }

                if (contactPoint == null || _virtualDrone == null) return 0;
                                
                return contactPoint.eulerAngles.y - _virtualDrone.eulerAngles.y;
            }
        }

        /// <summary>
        /// The transform of the actual contact plane/point (transform of the center point of the currently active Face).
        /// </summary>
        public Transform contactPointTransform
        {
            get
            {
                Transform contactPoint = null;
                switch (currentActiveFace)
                {
                    case DroneActiveFaceOptions.Center:
                        contactPoint = _dronePoint_center;
                        break;
                    case DroneActiveFaceOptions.Front:
                        contactPoint = _dronePoint_frontFace;
                        break;
                    case DroneActiveFaceOptions.Back:
                        contactPoint = _dronePoint_backFace;
                        break;
                    case DroneActiveFaceOptions.Left:
                        contactPoint = _dronePoint_leftFace;
                        break;
                    case DroneActiveFaceOptions.Right:
                        contactPoint = _dronePoint_rightFace;
                        break;
                    default:
                        break;
                }
                return contactPoint;
            }
        }

        /// <summary>
        /// The transforms of the bounding box points of the currently active Face.
        /// </summary>
        public List<Transform> contactFaceBoundingBox
        {
            get
            {
                List<Transform> boundingBoxPoints = new List<Transform>();
                switch (currentActiveFace)
                {
                    case DroneActiveFaceOptions.Center:
                        boundingBoxPoints = _dronePoint_frontFaceBoundingPoints;
                        break;
                    case DroneActiveFaceOptions.Front:
                        boundingBoxPoints = _dronePoint_frontFaceBoundingPoints;
                        break;
                    case DroneActiveFaceOptions.Back:
                        boundingBoxPoints = _dronePoint_backFaceBoundingPoints;
                        break;
                    case DroneActiveFaceOptions.Left:
                        boundingBoxPoints = _dronePoint_leftFaceBoundingPoints;
                        break;
                    case DroneActiveFaceOptions.Right:
                        boundingBoxPoints = _dronePoint_rightFaceBoundingPoints;
                        break;
                    default:
                        break;
                }
                return boundingBoxPoints;
            }
        }

        /// <summary>
        /// Wether "Roll" Output Value is manually being overridden from outside.
        /// </summary>
        private bool overridingRoll = false;

        /// <summary>
        /// Value used to override "Roll" Output.
        /// </summary>
        private int overridingRollValue = 1500;

        /// <summary>
        /// Wether "Pitch" Output Value is manually being overridden from outside.
        /// </summary>
        private bool overridingPitch = false;

        /// <summary>
        /// Value used to override "Pitch" Output.
        /// </summary>
        private int overridingPitchValue = 1500;
        /// <summary>
        /// Wether "Yaw" Output Value is manually being overridden from outside.
        /// </summary>
        private bool overridingYaw = false;

        /// <summary>
        /// Value used to override "Yaw" Output.
        /// </summary>
        private int overridingYawValue = 1500;

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

        /// <summary>
        /// Possible Options for the Active Face of the drone. The Active Face will be the face that will be adjusted to ft the target point.
        /// In practice this will determine which face of the drone will be facing towards the user. 
        /// This is seperate from the target points rotation, meaning we can achieve the drone rotating around one of its faces and not just its center.
        /// e.g. Option "Center" means, the drone will position its center point on the target point.
        /// e.g. Option "Front" means, the drone will position the center point of the front face on the target point.
        /// e.g. Option "Left" means, the drone will posisition the center point of the left face of the drone on the target point.
        /// </summary>
        public enum DroneActiveFaceOptions { Center, Front, Back, Left, Right }
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

        /// <summary>
        /// Sets the current active face of the drone. 
        /// </summary>
        /// <param name="_newActiveFace">The chosen mode/option for the active face.</param>
        public void SetCurrentActiveFace(DroneActiveFaceOptions _newActiveFace)
        {
            currentActiveFace = _newActiveFace;
        }

        /// <summary>
        /// Do NOT use this, unless you know what you are doing.
        /// Starts the Override of the "Roll" Output value with the given value. 
        /// Needs to be Ended by EndOverrideRoll() for the override to stop.
        /// </summary>
        /// <param name="_val">Value overriding the "Roll" Output value.</param>
        public void Advanced_StartOverrideRoll(int _val)
        {
            overridingRollValue = _val;
            overridingRoll = true;
        }

        /// <summary>
        /// Ends the manual override of the "Roll" Output value.
        /// </summary>
        public void Advanced_EndOverrideRoll()
        {
            overridingRoll = false;
        }
        /// <summary>
        /// Do NOT use this, unless you know what you are doing.
        /// Starts the Override of the "Pitch" Output value with the given value. 
        /// Needs to be Ended by EndOverridePitch() for the override to stop.
        /// </summary>
        /// <param name="_val">Value overriding the "Pitch" Output value.</param>
        public void Advanced_StartOverridePitch(int _val)
        {
            overridingPitchValue = _val;
            overridingPitch = true;
        }

        /// <summary>
        /// Ends the manual override of the "Pitch" Output value.
        /// </summary>
        public void Advanced_EndOverridePitch()
        {
            overridingPitch = false;
        }
        /// <summary>
        /// Do NOT use this, unless you know what you are doing.
        /// Starts the Override of the "Yaw" Output value with the given value. 
        /// Needs to be Ended by EndOverrideYaw() for the override to stop.
        /// </summary>
        /// <param name="_val">Value overriding the "Yaw" Output value.</param>
        public void Advanced_StartOverrideYaw(int _val)
        {
            overridingYawValue = _val;
            overridingYaw = true;
        }

        /// <summary>
        /// Ends the manual override of the "Yaw" Output value.
        /// </summary>
        public void Advanced_EndOverrideYaw()
        {
            overridingYaw = false;
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
            if (_output == null)
            {
                _output = FindObjectOfType<DHUI_Output>();
                if (_output != null)
                {
                    Debug.Log("<b>DHUI</b> | DroneController | No DHUI_Output was set in Inspector -> Found one in the scene on \"" + _output.gameObject.name + "\".");
                }
                else
                {
                    Debug.LogError("<b>DHUI</b> | DroneController | No DHUI_Output was set in Inspector and none was found in the scene.");
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
            

            if (_target != null && _droneTracker != null && _output != null && _PIDCalculator != null && _emergencyInputs != null)
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
            UpdateInfo();

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
                        error_trackingLost_internal = true;
                        ForceSetDroneState(DroneState.Land);
                        SetEmergencyMode(true);
                    }
                    else if (error_trackingLost_timer.ElapsedMilliseconds > _th_TrackingLost_MaxMsUntilHover && _th_TrackingLost_MaxMsUntilHover >= 0)
                    {
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
        /// Updates information for users (publicly accessable and visible in Unity-Inspector).
        /// </summary>
        private void UpdateInfo()
        {
            droneState_Info = currentDroneState;
            emergencyMode_Info = emergencyMode;
            error_TrackingLost_Info = error_trackingLost_internal;
        }

        /// <summary>
        /// Sets the output values, which should be written to the serial.
        /// </summary>
        private void WriteOutput()
        {
            _output.SetValues(values);
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
            values[1] = overridingRoll? overridingRollValue : _PIDCalculator.GetRoll();
            values[2] = overridingPitch? overridingPitchValue : _PIDCalculator.GetPitch();
            values[3] = overridingYaw? overridingYawValue : _PIDCalculator.GetYaw();
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
            if (_droneTracker.dronePosition.y <= (_floorY + _th_SaveShutDown_MaxCmFromFloor / 100) && values[0] >= 1004)
            {
                values[0] -= 4;
            }
            else if (values[0] >= 1001) {
                values[0] -= 1;
            }
            values[1] = _PIDCalculator.GetRoll();
            values[2] = _PIDCalculator.GetPitch();
            values[3] = _PIDCalculator.GetYaw();

            if (values[0] <= 1000)
            {
                ForceSetDroneState(DroneState.Off);
            }
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
                    Vector3 targetPos = _target.position - contactPointPositionOffset;
                    Vector3 targetForward = Quaternion.Euler(0, -contactPointRotationOffset, 0) * _target.forward;
                    _PIDCalculator.UpdateTrajectory(_droneTracker.dronePosition, _droneTracker.droneForward, _droneTracker.droneVelocity, targetPos, targetForward, _droneTracker.droneDistanceToTarget(targetPos));
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