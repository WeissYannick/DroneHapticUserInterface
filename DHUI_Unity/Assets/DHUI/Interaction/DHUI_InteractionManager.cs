using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DHUI_InteractionManager : MonoBehaviour
{
    [SerializeField]
    private DHUI_Hand _leftHand = null;

    [SerializeField]
    private DHUI_Hand _rightHand = null;

    private enum ActiveHandState
    {
        Left, Right, Both, None
    }

    private ActiveHandState currentActiveHandState = ActiveHandState.None;

    [SerializeField]
    private DHUI_InteractionCenterPoint _interactionCenterPoint = null;
    
    private List<DHUI_Interactable_Base> hoveredInteractables = new List<DHUI_Interactable_Base>();

    private DHUI_Interactable_Base primaryHoveredInteractable = null;

    [SerializeField]
    private float minDistance_Hover = 0.5f;

    [SerializeField]
    private float minDistance_primaryHover = 0.3f;

    private void Start()
    {
        if (_interactionCenterPoint == null)
        {
            Debug.LogError("<b>DHUI</b> | InteractionManager | No DHUI_InteractionCenterPoint was set for 'Interaction Center Point'.");
            return;
        }
        _interactionCenterPoint.SetRadius(minDistance_Hover);
        _interactionCenterPoint.OnHovered.AddListener(OnInteractableHovered);
        _interactionCenterPoint.OnUnhovered.AddListener(OnInteractableUnhovered);
    }

    private void OnInteractableHovered(DHUI_Interactable_Base _interactable)
    {
        if (!hoveredInteractables.Contains(_interactable))
        {
            hoveredInteractables.Add(_interactable);
            _interactable.Hover_Start();
        }
    }
    private void OnInteractableUnhovered(DHUI_Interactable_Base _interactable)
    {
        if (hoveredInteractables.Contains(_interactable))
        {
            hoveredInteractables.Remove(_interactable);
            _interactable.Hover_End();
        }
    }

    private void Update()
    {
        UpdateHandState();
        UpdateInteractionCenterPoint();
        UpdatePrimaryHover();
    }
    
    private void UpdateHandState()
    {
        if (_leftHand.isActiveAndEnabled && _rightHand.isActiveAndEnabled)
        {
            currentActiveHandState = ActiveHandState.Both;
        }
        else if (_leftHand.isActiveAndEnabled)
        {
            currentActiveHandState = ActiveHandState.Left;
        }
        else if (_rightHand.isActiveAndEnabled)
        {
            currentActiveHandState = ActiveHandState.Right;
        }
        else
        {
            currentActiveHandState = ActiveHandState.None;
        }
    }
    
    private void UpdateInteractionCenterPoint()
    {
        if (_interactionCenterPoint == null)
        {
            return;
        }
        if (currentActiveHandState == ActiveHandState.Right || currentActiveHandState == ActiveHandState.Both)
        {
            _interactionCenterPoint.SetPosition(_rightHand.InteractionCenterPoint);
        }
        else if (currentActiveHandState == ActiveHandState.Left)
        {
            _interactionCenterPoint.SetPosition(_leftHand.InteractionCenterPoint);
        }
        else
        {
            // If no Hand is active. TODO
        }
    }

    private void UpdatePrimaryHover()
    {
        DHUI_Interactable_Base interactableWithLowestDistance = null;
        float lowestDistance = Mathf.Infinity;
        foreach (DHUI_Interactable_Base interactable in hoveredInteractables)
        {
            float distance = Vector3.Distance(_interactionCenterPoint.transform.position, interactable.transform.position);
            if (distance < lowestDistance && distance <= minDistance_primaryHover)
            {
                lowestDistance = distance;
                interactableWithLowestDistance = interactable;
            }
        }

        if (interactableWithLowestDistance != primaryHoveredInteractable)
        {
            Debug.Log(interactableWithLowestDistance?.gameObject.name);
            primaryHoveredInteractable?.PrimaryHover_End();
            interactableWithLowestDistance?.PrimaryHover_Start();
            primaryHoveredInteractable = interactableWithLowestDistance;
        }
    }
}
