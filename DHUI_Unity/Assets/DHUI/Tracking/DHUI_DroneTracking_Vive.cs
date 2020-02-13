using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using DHUI.Core;

/// <summary>
/// This Namespace contains the Core features of DHUI – Drone Haptic User Interface.
/// These Scripts are essential for the system to work.
/// </summary>
namespace DHUI.VR
{
    /// <summary>
    /// This class handles the Tracking of the Drone with a Vive Tracker.
    /// It checks the Status of the VR-System and -Tracker and provides the current Tracking-Status, Position, Rotation and Velocity of the drone.
    /// </summary>
    public class DHUI_DroneTracking_Vive : DHUI_DroneTracking_Base
    {
        // ActionPose of the tracker attached to the drone.
        private SteamVR_Action_Pose droneActionPose = null;

        // Index of the tracker attached to the drone.
        private uint droneTrackerIndex = 0;
        
        /// <summary>
        /// Checks the Tracker-Status and Updates the Position, Rotation and Velocity.
        /// </summary>
        protected override void UpdateTracker()
        {
            // Setting 'trackingOK' to false (In case we have an Error and return before the end of the method).
            trackingOK = false;
            // Check if OpenVR-System is found correctly.
            if (OpenVR.System == null)
            {
                Debug.LogError("<b>DHUI</b> | DroneTracking_Vive | OpenVR-System was not found.");
                return;
            }
            // This should only be 'null' in the beginning and filled with the ActionPose of the VRTracker in the following iterations.
            if (droneActionPose == null)
            {
                // Check if there is an Active VRTracker.
                if (SteamVR_Input._default.VRTracker.GetActive(SteamVR_Input_Sources.Any))
                {
                    droneActionPose = SteamVR_Input._default.VRTracker;
                    droneTrackerIndex = droneActionPose.GetDeviceIndex(SteamVR_Input_Sources.Any);
                }
                else
                {
                    Debug.LogError("<b>DHUI</b> | DroneTracking_Vive | No Active VR-Tracker was found.");
                    return;
                }
            }

            // Copy Position and Rotation to the 'virtualTrackerTransform' and save the Velocity.
            virtualTrackerTransform.position = droneActionPose.GetLocalPosition(SteamVR_Input_Sources.Any);
            virtualTrackerTransform.rotation = droneActionPose.GetLocalRotation(SteamVR_Input_Sources.Any) * Quaternion.Euler(-90, 90, 90);
            velocity = droneActionPose.GetVelocity(SteamVR_Input_Sources.Any);

            // Calculate the current pose for our device. [See https://github.com/ValveSoftware/openvr/wiki/IVRSystem::GetDeviceToAbsoluteTrackingPose]
            TrackedDevicePose_t[] trackedDevicePoses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];
            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0.0f, trackedDevicePoses);
            TrackedDevicePose_t droneTrackedDevicePose = trackedDevicePoses[droneTrackerIndex];

            // If the 'TrackedDevicePose' of the drone is valid, connected and running ok, we can set the 'trackingOK' to true.
            if (droneTrackedDevicePose.bPoseIsValid && droneTrackedDevicePose.bDeviceIsConnected && droneTrackedDevicePose.eTrackingResult == ETrackingResult.Running_OK)
            {
                trackingOK = true;
            }
        }

    }
}

