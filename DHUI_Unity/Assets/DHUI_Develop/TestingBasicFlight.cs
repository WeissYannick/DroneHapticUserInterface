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
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(transform.position, transform.rotation, 0.2f);
            controller.AddToFrontOfQueue(cmd,true,true);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            DHUI_FlightCommand_WaitForDrone cmd = new DHUI_FlightCommand_WaitForDrone();
            cmd.waitingTimeout = 20;
            controller.AddToFrontOfQueue(cmd,true,true);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            DHUI_FlightCommand_LandAtStart cmd = new DHUI_FlightCommand_LandAtStart();
            controller.AddToFrontOfQueue(cmd,true,true);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            DHUI_FlightCommand_LandHere cmd = new DHUI_FlightCommand_LandHere();
            controller.AddToFrontOfQueue(cmd,true,true);
        }
    }
}
