using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Namespace contains the Core features of DHUI – Drone Haptic User Interface.
/// These Scripts are essential for the system to work.
/// </summary>
namespace DHUI.Core
{
    public class DHUI_DroneTracking_Base : MonoBehaviour
    {
        [Header("Base Setup")]
        [SerializeField][Tooltip("The virtual object, which should show the drone's real world position and rotation.")]
        protected Transform trackedTransform = null;

        #region Public Information
        [Header("Base Information")]
        [Tooltip("Shows if the Tracking is working as expected.")]
        public bool trackingOK = false;
        
        public Transform droneTransform
        {
            get { return trackedTransform; }
        }
        // Current Position of the drone.
        public Vector3 dronePosition
        {
            get { return trackedTransform.position; }
        }

        // Current Forward-Vector of the drone.
        public Vector3 droneForward
        {
            get { return trackedTransform.forward; }
        }

        // Current Velocity of the drone.
        public Vector3 droneVelocity
        {
            get { return velocity; }
        }
        
        // Current velocity of the drone.
        protected Vector3 velocity = Vector3.zero;

        /// <summary>
        /// Calculates the current distance-vector from the real position of the drone and the given target in local space of the drone.
        /// </summary>
        /// <param name="_target">Target position</param>
        /// <returns>Distance-Vector to target from local space of drone.</returns>
        public Vector3 droneDistanceToTarget(Vector3 _target)
        {
            return trackedTransform.InverseTransformDirection(_target - dronePosition);
        }
        #endregion Public Information

        #region Internal
        /// <summary>
        /// On Start: Check the 'trackedTransform' and default it to 'this.transform' if not set.
        /// </summary>
        private void Start()
        {
            if (trackedTransform == null) trackedTransform = transform;
        }
        /// <summary>
        /// Update the Tracker in every Update.
        /// </summary>
        private void Update()
        {
            UpdateTracker();
        }
        #endregion Internal

        #region Virtual Methods
        /// <summary>
        /// UpdateTracker-Method, which should be overridden by functionality that updates the virtual drone-tracker.
        /// </summary>
        protected virtual void UpdateTracker()
        {
            trackingOK = false;
        }

        #endregion
    }
}