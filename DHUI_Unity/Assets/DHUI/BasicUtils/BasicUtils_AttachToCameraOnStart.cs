using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.BasicUtils
{
    /// <summary>
    /// Util to Attach a Transform to the main camera on start.
    /// This is useful for prefabs/modules, which should be drag
    /// </summary>
    public class BasicUtils_AttachToCameraOnStart : MonoBehaviour
    {
        /// <summary>
        /// Offset for the position while/after being attached to the Camera.
        /// </summary>
        [SerializeField][Tooltip("Offset for the position while/after being attached to the Camera.")]
        private Vector3 positionOffset = Vector3.zero;

        /// <summary>
        /// Offset for the rotation while/after being attached to the Camera.
        /// </summary>
        [SerializeField][Tooltip("Offset for the rotation while/after being attached to the Camera.")]
        private Quaternion rotationOffset = Quaternion.identity;

        /// <summary>
        /// On Start, we look for the main camera. If there is none set, we take the first camera which is enabled and active in the scene.
        /// We then set this transform to be a child of the camera-transform, with our defined offsets.
        /// </summary>
        private void Start()
        {
            if (Camera.main != null)
            {
                SetParentAndPose(Camera.main.transform);
            }
            else
            {
                Camera[] camsInScene = FindObjectsOfType<Camera>();
                foreach (Camera cam in camsInScene)
                {
                    if (cam.isActiveAndEnabled)
                    {
                        SetParentAndPose(cam.transform);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the parent of this.transform to the given transform and sets the position/rotation to the given offsets.
        /// </summary>
        /// <param name="_camTransform">Transform of the Camera.</param>
        private void SetParentAndPose(Transform _camTransform)
        {
            transform.parent = _camTransform;
            transform.localPosition = Vector3.zero + positionOffset;
            transform.localRotation = Quaternion.identity * rotationOffset;
            transform.localScale = Vector3.one;
        }
    
    }
}