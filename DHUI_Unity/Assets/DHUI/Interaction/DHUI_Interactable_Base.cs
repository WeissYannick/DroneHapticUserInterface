using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DHUI.Core;

public class DHUI_Interactable_Base : MonoBehaviour
{
    public UnityEvent Event_Hover_Start = null;
    public UnityEvent Event_Hover_End = null;
    public UnityEvent Event_PrimaryHover_Start = null;
    public UnityEvent Event_PrimaryHover_End = null;

    public DHUI_FlightController _flightController = null;

    public Transform _primaryHoverDronePose = null;

    private bool hovered = false;
    private bool primaryHovered = false;
    

    public void Hover_Start()
    {
        Event_Hover_Start?.Invoke();
        hovered = true;
    }

    public void Hover_End()
    {
        Event_Hover_End?.Invoke();
        hovered = false;
    }

    public void PrimaryHover_Start()
    {
        DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(_primaryHoverDronePose.position, _primaryHoverDronePose.rotation);
        _flightController?.AddToFrontOfQueue(cmd, true, true);
        cmd.time = 2;
        Event_PrimaryHover_Start?.Invoke();
        primaryHovered = true;
    }

    public void PrimaryHover_End()
    {
        Event_PrimaryHover_End?.Invoke();
        primaryHovered = false;
    }
}
