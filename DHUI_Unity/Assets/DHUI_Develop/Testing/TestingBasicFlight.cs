using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DHUI.Core;

public class TestingBasicFlight : MonoBehaviour
{

    public DHUI_FlightController controller;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo();
            cmd.targetPosition = transform.position;
            cmd.targetRotation = transform.rotation;
            cmd.time = 0;
            controller.AddToBackOfQueue(cmd);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            DHUI_FlightCommand_Hold cmd = new DHUI_FlightCommand_Hold();
            cmd.duration = 20;
            controller.AddToBackOfQueue(cmd);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            DHUI_FlightCommand_Hover cmd = new DHUI_FlightCommand_Hover();
            cmd.duration = 20;
            controller.AddToBackOfQueue(cmd);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            DHUI_FlightCommand_LandHere cmd = new DHUI_FlightCommand_LandHere();
            controller.AddToBackOfQueue(cmd);
        }
    }
}
