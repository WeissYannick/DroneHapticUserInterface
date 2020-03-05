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
    /// Base Class of Drone Tracking. 
    /// The tracking implementations (e.g. with ViveTracker) should derive from this.
    /// </summary>
    public abstract class DHUI_DroneTracking_Base : MonoBehaviour
    {
        /// <summary>
        /// The virtual object, which should show the drone's real world position and rotation
        /// </summary>
        [Header("Base Setup")]
        [SerializeField][Tooltip("The virtual object, which should show the drone's real world position and rotation.")]
        protected Transform trackedTransform = null;

        #region Public Information

        /// <summary>
        /// Shows if the Tracking is working as expected.
        /// </summary>
        [Header("Base Information")]
        [Tooltip("Shows if the Tracking is working as expected.")]
        public bool trackingOK = false;
        
        /// <summary>
        /// The transform of the drone.
        /// </summary>
        public Transform droneTransform
        {
            get { return trackedTransform; }
        }

        /// <summary>
        /// Current Position of the drone.
        /// </summary>
        public Vector3 dronePosition
        {
            get { return trackedTransform.position; }
        }
        
        /// <summary>
        /// Current Forward-Vector of the drone.
        /// </summary>
        public Vector3 droneForward
        {
            get { return trackedTransform.forward; }
        }

        /// <summary>
        /// Public Getter for current Velocity of the drone.
        /// </summary>
        public Vector3 droneVelocity
        {
            get { return velocity; }
        }
        
        /// <summary>
        /// Current velocity of the drone. 
        /// </summary>
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