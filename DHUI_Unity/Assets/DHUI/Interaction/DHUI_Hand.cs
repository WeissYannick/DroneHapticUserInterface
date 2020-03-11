using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DHUI_Hand : MonoBehaviour
{
    [SerializeField]
    private bool isLeft = false;
    [SerializeField]
    private Transform rootPoint = null;
    [SerializeField]
    private Transform interactionCenterPoint = null;

    public Vector3 InteractionCenterPoint
    {
        get { return interactionCenterPoint.position; }
    }
}
