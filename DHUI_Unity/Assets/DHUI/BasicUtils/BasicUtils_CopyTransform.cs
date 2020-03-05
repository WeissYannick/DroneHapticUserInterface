using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.BasicUtils
{
    /// <summary>
    /// Util for copying poses (position and rotation) from one Transform to another.
    /// </summary>
    public class BasicUtils_CopyTransform : MonoBehaviour
    {
        /// <summary>
        /// Transform to copy from.
        /// </summary>
        public Transform _origin = null;
        /// <summary>
        /// Transform to copy to.
        /// </summary>
        public Transform _target = null;

        /// <summary>
        /// Global: Target's global position and rotation will be set to Origin's global position and rotation.
        /// Local: Target's local position and rotation a will be set to Origins's local position and rotation.
        /// </summary>
        public enum CopyTransform_Modes
        {
            Global, Local
        }
        /// <summary>
        /// Selected Mode determines wether we copy local or global poses.
        /// </summary>
        public CopyTransform_Modes mode = CopyTransform_Modes.Global;

        /// <summary>
        /// Should positions be copied?
        /// </summary>
        public bool copyPosition = true;

        /// <summary>
        /// Should rotations be copied?
        /// </summary>
        public bool copyRotation = true;

        private void FixedUpdate()
        {
            if (mode == CopyTransform_Modes.Global)
            {
                if (copyPosition)
                    _target.position = _origin.position;
                if (copyRotation)
                    _target.rotation = _origin.rotation;
            }
            else
            {
                if (copyPosition)
                    _target.localPosition = _origin.localPosition;
                if (copyRotation)
                    _target.localRotation = _origin.localRotation;
            }
        }
    }
}
