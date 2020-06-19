using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DHUI_DroneFollow : MonoBehaviour
{
    public DHUI.Core.DHUI_DroneController m_droneController = null;
    public Transform m_restingTransform = null;

    public bool zValueOnly = true;

    private bool followDrone = false;

    private void FixedUpdate()
    {
        if (followDrone)
        {
            if (zValueOnly)
            {
                Vector3 pos = transform.localPosition;
                pos.z = transform.parent.InverseTransformPoint(m_droneController.contactPointTransform.position).z;
                transform.localPosition = pos;
            }
            else
            {
                transform.position = m_droneController.contactPointTransform.position;
                transform.rotation = m_droneController.contactPointTransform.rotation;
            }
        }
        else
        {
            transform.position = m_restingTransform.position;
            transform.rotation = m_restingTransform.rotation;
        }
    }

    public void SetFollowDrone(bool _val)
    {
        followDrone = _val;
    }

}
