using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicUtils_CopyTransform : MonoBehaviour
{
    public Transform _origin = null;
    public Transform _target = null;

    public enum CopyTransform_Modes
    {
        Global, Local
    }
    public CopyTransform_Modes mode = CopyTransform_Modes.Global;
    public bool copyPosition = true;
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
