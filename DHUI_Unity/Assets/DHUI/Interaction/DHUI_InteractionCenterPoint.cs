using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class DHUI_InteractionCenterPoint : MonoBehaviour
{
    [Serializable]
    public class InteractableEvent : UnityEvent<DHUI_Interactable_Base> { }
    public InteractableEvent OnHovered = null;
    public InteractableEvent OnUnhovered = null;

    public void SetRadius(float _r)
    {
        this.GetComponent<SphereCollider>().radius = _r;
    }

    public void SetPosition(Vector3 _position)
    {
        transform.position = _position;
    }

    private void OnTriggerEnter(Collider other)
    {
        DHUI_Interactable_Base interactable = other.GetComponent<DHUI_Interactable_Base>();
        if (interactable != null)
        {
            OnHovered?.Invoke(interactable);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        DHUI_Interactable_Base interactable = other.GetComponent<DHUI_Interactable_Base>();
        if (interactable != null)
        {
            OnUnhovered?.Invoke(interactable);
        }
    }
}
