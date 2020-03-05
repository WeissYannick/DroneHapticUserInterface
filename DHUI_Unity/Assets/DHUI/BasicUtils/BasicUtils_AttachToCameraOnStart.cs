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
        [SerializeField][Tooltip("Offset for the position while/after being attached to the Camera.")]
        private Vector3 positionOffset = Vector3.zero;
        [SerializeField][Tooltip("Offset for the rotation while/after being attached to the Camera.")]
        private Quaternion rotationOffset = Quaternion.identity;

        void Start()
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

        private void SetParentAndPose(Transform _camTransform)
        {
            transform.parent = _camTransform;
            transform.localPosition = Vector3.zero + positionOffset;
            transform.localRotation = Quaternion.identity * rotationOffset;
            transform.localScale = Vector3.one;
        }
    
    }
}