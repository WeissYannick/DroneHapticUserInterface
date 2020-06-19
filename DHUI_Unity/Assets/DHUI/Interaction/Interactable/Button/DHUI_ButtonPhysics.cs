using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DHUI_ButtonPhysics : MonoBehaviour
{
    private Vector3 startPosition = Vector3.zero;
    private bool colliding = false;
    
    void Start()
    {
        startPosition = transform.localPosition;
    }
    
    void FixedUpdate()
    {
        if (colliding)
        {
            transform.Translate(Vector3.back * 0.01f, Space.Self);
        }
        else if (transform.localPosition.z < startPosition.z)
        {
            transform.Translate(Vector3.forward * 0.01f, Space.Self);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == DHUI.DHUI_InteractionManager.InteractorTag)
        {
            colliding = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == DHUI.DHUI_InteractionManager.InteractorTag)
        {
            colliding = false;
        }
    }
}
