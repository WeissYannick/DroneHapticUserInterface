using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DHUI;
using DHUI.Core;

public class DHUI_Interactable_Testing : DHUI_Interactable
{
    [SerializeField]
    private Transform m_hoverInformationObject = null;

    public override void Hover_Start(DHUI_Hand _hand)
    {
        DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_contactCenterPoint.position, m_contactCenterPoint.rotation);
        m_flightController.AddToFrontOfQueue(cmd, true, true);
    }

    public override void Hover_Stay(DHUI_Hand _hand)
    {
        if (_hand == null) return;
        float dist = Vector3.Distance(_hand.Position,ContactCenterPoint);

        float x = _hand.Position.x;
        float y = _hand.Position.y;
        
        float z = ContactCenterPoint.z;
        
        m_hoverInformationObject.position = new Vector3(x,y,z);
        GetComponentInChildren<Renderer>().material.color = new Color(dist, dist, dist);
    }

    public override void Hover_End(DHUI_Hand _hand)
    {
        GetComponentInChildren<Renderer>().material.color = new Color(1, 1, 1);
    }
}
