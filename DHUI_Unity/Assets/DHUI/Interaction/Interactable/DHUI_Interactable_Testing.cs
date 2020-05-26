using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DHUI;
using DHUI.Core;
using UnityEngine.Events;
using UnityEngine.UI;

public class DHUI_Interactable_Testing : DHUI_Interactable
{
    [SerializeField]
    private Transform m_hoverInformationObject = null;
    [SerializeField]
    private Transform m_hoverDistanceInformationObject = null;
    [SerializeField]
    private Transform m_movingPart = null;
    [SerializeField]
    private Transform m_staticPart = null;
    [SerializeField]
    private GameObject m_debugCanvas = null;
    [SerializeField]
    private List<Text> m_debugTexts = new List<Text>();
    [SerializeField]
    private float _activationDistance = 0.3f;
    [SerializeField]
    private string buttonMode = "";
    [SerializeField]
    private Transform m_pressedDistanceMovingObject = null;
    [SerializeField]
    private Transform m_pressedDistanceStaticObject = null;

    private ModeTesting mode;
    private enum ModeTesting
    {
        Inactive, Hovered, Touched, Pressed, Activated, Released
    }

    public UnityEvent OnActivated = null;

    public override void Hover_Start(DHUI_HoverEventArgs _hand)
    {
        DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_centerPoint.position, m_centerPoint.rotation);
        m_flightController.AddToFrontOfQueue(cmd, true, true);

        SetDebugText(0, buttonMode);
        SetDebugText(5, _activationDistance.ToString());
    }

    public override void Hover_Stay(DHUI_HoverEventArgs _hand)
    {
        if (m_debugCanvas != null)
            m_debugCanvas.SetActive(true);


        float dist = Mathf.Abs(_hand.InteractorPosition.z - CenterPoint.z);
        SetDebugText(2, dist.ToString());
        if (mode == ModeTesting.Activated || mode == ModeTesting.Released)
        {
            SetMode(ModeTesting.Released);
        }
        else if (Vector3.Angle(_hand.InteractorPosition - CenterPoint, m_centerPoint.forward) > 90)
        {
            
            if (dist < 0.01f)
            {
                SetMode(ModeTesting.Touched);
            }
            else
            {
                SetMode(ModeTesting.Hovered);
            }
            m_hoverInformationObject.gameObject.SetActive(true);
        }
        else
        {
            SetMode(ModeTesting.Pressed);
            m_hoverInformationObject.gameObject.SetActive(false);
        }

        float x = _hand.InteractorPosition.x;
        float y = _hand.InteractorPosition.y;

        float minX = CenterPoint.x - transform.localScale.x * 0.5f;
        float maxX = CenterPoint.x + transform.localScale.x * 0.5f;
        float minY = CenterPoint.y - transform.localScale.y * 0.5f;
        float maxY = CenterPoint.y + transform.localScale.y * 0.5f;
        if (x < minX) x = minX;
        if (x > maxX) x = maxX;
        if (y < minY) y = minY;
        if (y > maxY) y = maxY;
        
        float z = CenterPoint.z;
        m_hoverInformationObject.position = new Vector3(x,y,z);
        
        SetDebugText(3, "X: " + ((x - CenterPoint.x)/ (transform.localScale.x/2)).ToString("F2") + "                   Y: " +  ((y - CenterPoint.y) / (transform.localScale.y / 2)).ToString("F2"));

        DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_hoverInformationObject.position, m_hoverInformationObject.rotation);
        m_flightController.AddToFrontOfQueue(cmd, true, true);

        if (m_hoverDistanceInformationObject != null)
        {
            if (dist > 0.2f) dist = 0.2f;
            m_hoverDistanceInformationObject.localScale = new Vector3(dist * 15, dist * 15, 1);
        }

        if (m_movingPart != null && m_staticPart != null && _activationDistance > 0)
        {
            float pressDist = Vector3.Distance(m_pressedDistanceMovingObject.position, m_pressedDistanceStaticObject.position);
            SetDebugText(4, pressDist.ToString("F2"));
            if (pressDist > _activationDistance)
            {
                SetMode(ModeTesting.Activated);
                OnActivated?.Invoke();
            }
        }
    }

    public override void Hover_End(DHUI_HoverEventArgs _e)
    {
        m_hoverInformationObject.gameObject.SetActive(false);
        if (m_debugCanvas != null)
            m_debugCanvas.SetActive(false);

        SetMode(ModeTesting.Inactive);
    }

    public void SetCenterPoint(Transform transform)
    {
        m_centerPoint = transform;
    }

    private void SetDebugText(int _index, string _string)
    {
        if (_index < m_debugTexts.Count)
        {
            m_debugTexts[_index].text = _string;
        }
    }

    private void SetMode(ModeTesting _mode)
    {
        mode = _mode;
        SetDebugText(1, mode.ToString());
    }
}
