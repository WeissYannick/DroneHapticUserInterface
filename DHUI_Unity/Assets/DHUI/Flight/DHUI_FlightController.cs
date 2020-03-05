using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Namespace contains the Core features of DHUI – Drone Haptic User Interface.
/// These Scripts are essential for the system to work.
/// </summary>
namespace DHUI.Core
{
    /// <summary>
    /// This class controls the leader (= the target the drone is following) and therefore the flight and route of the drone. 
    /// It handles the behaviours of the leader/drone by processing commands from a queue in order (e.g. Fly to 'x' in 'y' seconds, then Hover for 'z' seconds, then ...)
    /// </summary>
    public class DHUI_FlightController : MonoBehaviour
    {
        #region Fields | Setup
        [Header("Setup")]
        [SerializeField][Tooltip("The target object we move. This serves as the leader to follow in the DroneController.")]
        private Transform _leader = null;

        [SerializeField][Tooltip("The controller handling and managing the drone.")]
        private DHUI_DroneController _droneController = null;
        #endregion Fields | Setup
        
        #region Fields | Info
        [Header("Info")]
        [Tooltip("Information about the current Flight state. This is only used for information and manually changing this does not effect anything.")]
        public FlightState currentFlightState_Info = FlightState.Parked;

        [Tooltip("Name of the currently processed command. This is only used for information and manually changing this does not effect anything.")]
        public string currentProcessedCommand_Info = "None";
                
        [Tooltip("Names of the commands currently in the queue. This is only used for information and manually changing this does not effect anything.")]
        public List<string> commandsInQueue_Info = new List<string>();
        
        #endregion Fields | Info

        #region Fields | Private
        /// <summary>
        /// Transform of the virtual representation of the drone from the DHUI_DroneController.
        /// </summary>
        private Transform virtualDrone = null;
        
        /// <summary>
        /// Current State of the Flight.
        /// </summary>
        private FlightState currentFlightState = FlightState.Parked;

        /// <summary>
        /// The List of Commands that will be processed next.  
        /// </summary>
        private List<DHUI_FlightCommand_Base> queuedCommands = new List<DHUI_FlightCommand_Base>();
        
        /// <summary>
        /// The Command currently processed by the controller. 
        /// </summary>
        private DHUI_FlightCommand_Base currentProcessedCommand = null;
        
        /// <summary>
        /// The Pose the drone is starting in (Last parked position/rotation before TakeOff).
        /// </summary>
        private Pose droneStartingPose = Pose.identity;
        #endregion Fields | Private

        #region Enums
        /// Possible States of the Flight:
        /// - Parked:       Drone (and Leader) is on the ground and idle/off.
        /// - TakeOff:      Drones just started flying after being in Parked-mode.
        /// - Flying:       Drone is flying in the air.
        /// - Drone:        Drone is targeting to land.
        /// - TouchDown:    Drone is in the process of landing and slowly reduced the throttle to touch down softly.
        public enum FlightState { Parked, Takeoff, Flying, Landing, TouchDown };
        #endregion Enums

        #region Methods | Start/Update

        /// <summary>
        /// On Start, we check if everything is set up correctly.
        /// </summary>
        private void Start()
        {
            if (_leader == null)
            {
                _leader = transform;
                Debug.Log("<b>DHUI</b> | FlightController | No Transform for Leader was set in Inspector -> Defaulting to this transform. (\"" + gameObject.name + "\")");
            }
            if (_droneController == null)
            {
                _droneController = FindObjectOfType<DHUI_DroneController>();

                if (_droneController != null)
                {
                    Debug.Log("<b>DHUI</b> | FlightController | No DHUI_DroneController was set in Inspector -> Found one in the scene on \"" + _droneController.gameObject.name + "\".");
                }
                else
                {
                    Debug.LogError("<b>DHUI</b> | FlightController | No FlightController was set in Inspector and none was found in the scene.");
                }
            }
            if (_droneController != null)
            {
                virtualDrone = _droneController.GetVirtualDrone();
            }
        }


        /// <summary>
        /// Every Update cycle we update the current command.
        /// </summary>
        private void Update()
        {
            UpdateCurrentCommand();
            UpdateInfo();
            
        }

        private void UpdateInfo()
        {
            currentFlightState_Info = currentFlightState;

            if (currentProcessedCommand != null)
            {
                currentProcessedCommand_Info = currentProcessedCommand.GetType().ToString();
            }
            else
            {
                currentProcessedCommand_Info = "None";
            }

            if (queuedCommands != null)
            {
                commandsInQueue_Info.Clear();
                foreach (DHUI_FlightCommand_Base cmd in queuedCommands)
                {
                    commandsInQueue_Info.Add(cmd.GetType().ToString());
                }
            }

        }

        #endregion Methods | Start/Update

        #region Methods | Public

        /// <summary>
        /// Tries to set the current Flight State and adjusts the Drone State accordingly.
        /// We don't necessarily set the FlightState to the one that is put in, but check some other information as well. 
        /// (E.g. If the drone was parked, it should not go right into 'Flying' but 'TakeOff' first, and save some valuable information, like the last parked position, if we need to land there).
        /// This method should be used with great caution, since it has control over the real drone's behaviour.
        /// </summary>
        /// <param name="_flightState">The FlightState we try to set it to.</param>
        /// <returns>The actual FlightState we set.</returns>
        public FlightState TrySetFlightState(FlightState _flightState)
        {
            DHUI_DroneController.DroneState lastDroneState = _droneController.GetDroneState();
            switch (_flightState)
            {
                case FlightState.Flying:
                    // If we are currently parked and now start to fly, set the state to 'TakeOff' and save the Pose of where the drone was parked.
                    if (_droneController.TrySetDroneState(DHUI_DroneController.DroneState.Follow))
                    {
                        if (currentFlightState == FlightState.Parked || lastDroneState == DHUI_DroneController.DroneState.Off)
                        {
                            currentFlightState = FlightState.Takeoff;
                            droneStartingPose = new Pose(virtualDrone.position, virtualDrone.rotation);
                            _leader.position = virtualDrone.position;
                            _leader.rotation = virtualDrone.rotation;
                            
                        }
                        // If we are not parked, we can just set the state to 'Flying'
                        else
                        {
                            currentFlightState = FlightState.Flying;
                        }
                        // Set the drone to follow.
                    }
                    break;
                case FlightState.Landing:
                    // We can only start the landing procedure, if we are currently in the air ('Flying' or 'Takeoff').
                    if (currentFlightState == FlightState.Flying || currentFlightState == FlightState.Takeoff)
                    {
                        currentFlightState = FlightState.Landing;
                    }
                    else
                    {
                        Debug.LogError("<b>DHUI</b> | Flight Controller | Tried to initiate 'Landing', while drone was not in a fly-mode. It is in: '" +  currentFlightState + "'");
                    }
                    break;
                case FlightState.TouchDown:
                    // We should only be able to perform the 'TouchDown' if we are already in 'Landing'. 
                    // This may be changed, if we need to be able to TouchDown from everywhere, but it is saver this way. 
                    // (Note: When an error occurs, like 'Tracking Loss', or the user uses an 'Emergency Off'-switch, the drone controller will handle this himself. This controller only needs to handle successful flights.)
                    if (currentFlightState == FlightState.Landing || currentFlightState == FlightState.TouchDown)
                    {
                        if (_droneController.TrySetDroneState(DHUI_DroneController.DroneState.Land))
                        {
                            currentFlightState = FlightState.TouchDown;
                        }
                    }
                    else
                    {
                        Debug.LogError("<b>DHUI</b> | Flight Controller | Tried to initiate TouchDown, while drone was not in landing-mode. It is in: '" + currentFlightState + "'");
                    }
                    break;
                case FlightState.Parked:
                    // Only set the state to parked, when we already touched down.
                    if (currentFlightState == FlightState.TouchDown)
                    {
                        currentFlightState = FlightState.Parked;
                    }
                    else
                    {
                        Debug.LogError("<b>DHUI</b> | Flight Controller | Tried to turn off drone, while drone was not in TouchDown-mode. It is in: '" + currentFlightState + "'");
                    }
                    break;
                default:
                    break;
            }
            return currentFlightState;
        }

        /// <summary>
        /// Gets the height of the floor.
        /// </summary>
        /// <returns>Height of floor in Meters.</returns>
        public float GetFloorHeight()
        {
            return _droneController._floorY;
        }

        /// <summary>
        /// Gets the height at which the TouchDown should be initiated when landing.
        /// </summary>
        /// <returns>The height threshold (Y-Value) we want the TouchDown-Procedure to begin.</returns>
        public float GetTriggerHeight_TouchDown()
        {
            return (_droneController._th_RegularLanding_InitLand_TargetCmFromFloor / 100);
        }

        /// <summary>
        /// Gets the height at which the ShutOff should be initiated when landing.
        /// </summary>
        /// <returns>The height threshold (Y-Value) we want the Parking/ShutOff to begin.</returns>
        public float GetTriggerHeight_ShutOff()
        {
            return (_droneController._th_RegularLanding_ShutOff_TargetCmFromFloor / 100);
        }

        /// <summary>
        /// Gets the pose of the drone when it started the current flight (= Switched from 'Parked' to 'TakeOff').
        /// </summary>
        /// <returns>Pose (position + rotation) of the drone's starting point.</returns>
        public Pose GetDroneStartingPose()
        {
            return droneStartingPose;
        }

        #endregion Methods | Public

        #region Methods | Process Commands

        /// <summary>
        /// Jump to the next Command in the Queue and start processing it.
        /// </summary>
        private void ProcessNext()
        {
            if (queuedCommands != null && queuedCommands.Count > 0)
            {
                currentProcessedCommand = queuedCommands[0];
                queuedCommands.RemoveAt(0);
                StartCurrentCommand();
            }
            else
            {
                currentProcessedCommand = null;
            }
        }

        /// <summary>
        /// Starts the new command we want to process now.
        /// </summary>
        private void StartCurrentCommand()
        {
            currentProcessedCommand.StartCommand(this, _leader, virtualDrone);
        }

        /// <summary>
        /// Updates the command we are currently processing.
        /// If it is null or finished, we continue to the next one.
        /// </summary>
        private void UpdateCurrentCommand() {
            if (currentProcessedCommand == null)
            {
                ProcessNext();
                return;
            }

            bool finished;
            currentProcessedCommand.UpdateCommand(out finished);
            if (finished)
            {
                ProcessNext();
            }
        }
        #endregion Methods | Process Commands

        #region Methods | Queue Actions

        /// <summary>
        /// Add a Command to the Back of the Queue
        /// </summary>
        /// <param name="_command">Command to Add.</param>
        public void AddToBackOfQueue(DHUI_FlightCommand_Base _command)
        {
            queuedCommands.Add(_command);
        }
        /// <summary>
        /// Add an Array of Commands to the Back of the Queue
        /// </summary>
        /// <param name="_commands">Commands to add.</param>
        public void AddToBackOfQueue(DHUI_FlightCommand_Base[] _commands)
        {
            queuedCommands.AddRange(_commands);
        }
        /// <summary>
        /// Add a List of Commands to the Back of the Queue
        /// </summary>
        /// <param name="_commands">Commands to add.</param>
        public void AddToBackOfQueue(List<DHUI_FlightCommand_Base> _commands)
        {
            queuedCommands.AddRange(_commands);
        }

        /// <summary>
        /// Add a Command to the Front of the Queue.
        /// </summary>
        /// <param name="_command">The Command to Add.</param>
        /// <param name="startImmediately">If the Command should start immediately (= true) or wait for the current Command to end (= false). Default is 'false'.</param>
        /// <param name="breakPreviousQueue">If the rest of the Queue should be cleared (= true) or should continue as is after the new command finished (= false). Default is 'false'.</param>
        public void AddToFrontOfQueue(DHUI_FlightCommand_Base _command, bool startImmediately = false, bool breakPreviousQueue = false)
        {
            if (breakPreviousQueue)
            {
                ClearQueue();
            }
            queuedCommands.Insert(0, _command);

            if (startImmediately)
            {
                ProcessNext();
            }
        }
        /// <summary>
        /// Add an Array of Commands to the Front of the Queue.
        /// </summary>
        /// <param name="_command">The Commands to Add.</param>
        /// <param name="startImmediately">If the Commands should start immediately (= true) or wait for the current Command to end (= false). Default is 'false'.</param>
        /// <param name="breakPreviousQueue">If the rest of the Queue should be cleared (= true) or should continue as is after the new commands finished (= false). Default is 'false'.</param>
        public void AddToFrontOfQueue(DHUI_FlightCommand_Base[] _commands, bool startImmediately = false, bool breakPreviousQueue = false)
        {
            if (breakPreviousQueue)
            {
                ClearQueue();
            }

            queuedCommands.InsertRange(0, _commands);

            if (startImmediately)
            {
                ProcessNext();
            }
        }
        /// <summary>
        /// Add a List of Commands to the Front of the Queue.
        /// </summary>
        /// <param name="_command">The Commands to Add.</param>
        /// <param name="startImmediately">If the Commands should start immediately (= true) or wait for the current Command to end (= false). Default is 'false'.</param>
        /// <param name="breakPreviousQueue">If the rest of the Queue should be cleared (= true) or should continue as is after the new commands finished (= false). Default is 'false'.</param>
        public void AddToFrontOfQueue(List<DHUI_FlightCommand_Base> _commands, bool startImmediately = false, bool breakPreviousQueue = false)
        {
            if (breakPreviousQueue)
            {
                ClearQueue();
            }

            queuedCommands.InsertRange(0, _commands);

            if (startImmediately)
            {
                ProcessNext();
            }
        }

        /// <summary>
        /// Clears the Queue of Commands.
        /// </summary>
        public void ClearQueue()
        {
            queuedCommands.Clear();
        }

        #endregion Methods | Queue Actions

    }
}