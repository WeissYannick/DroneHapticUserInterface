using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DHUI.Core;

namespace DHUI
{
    public class DHUI_Interactable : MonoBehaviour
    {
        [Header("Members")]
        [SerializeField]
        private DHUI_InteractionManager m_interactionManager = null;
        [SerializeField]
        private DHUI_FlightController m_flightController = null;
        [SerializeField]
        private Transform m_contactCenterPoint = null;
        [Header("Advanced")]
        [SerializeField]
        private bool _manualRegistering = false;
        
        public Vector3 ContactCenterPoint
        {
            get { return m_contactCenterPoint.position; }
        }

        public bool IsDisabled
        {
            get;
            private set;
        }

        private void Start()
        {
            if (m_interactionManager == null)
            {
                m_interactionManager = FindObjectOfType<DHUI_InteractionManager>();
            }
            if (m_flightController == null)
            {
                m_flightController = FindObjectOfType<DHUI_FlightController>();
            }
            if (m_contactCenterPoint == null)
            {
                m_contactCenterPoint = transform;
            }

            if (!_manualRegistering)
            {
                Register();
            }
        }

        private void OnDestroy()
        {
            if (!_manualRegistering)
            {
                Deregister();
            }
        }

        public void Disable()
        {
            IsDisabled = true;
        }

        public void Register()
        {
            if (m_interactionManager == null)
            {
                m_interactionManager = FindObjectOfType<DHUI_InteractionManager>();
            }
            m_interactionManager?.RegisterInteractable(this);
        }   

        public void Deregister()
        {
            if (m_interactionManager == null)
            {
                m_interactionManager = FindObjectOfType<DHUI_InteractionManager>();
            }
            m_interactionManager?.DeregisterInteractable(this);
        }

        public void Hover_Start(DHUI_Hand _hand)
        {
            DHUI_FlightCommand_MoveTo cmd = new DHUI_FlightCommand_MoveTo(m_contactCenterPoint.position, m_contactCenterPoint.rotation);
            m_flightController.AddToFrontOfQueue(cmd, true, true);
            Debug.Log(gameObject.name + " | Hover_Start");
        }

        public void Hover_Stay(DHUI_Hand _hand)
        {
            transform.LookAt(_hand.Position);
        }

        public void Hover_End(DHUI_Hand _hand)
        {
            Debug.Log(gameObject.name + " | Hover_End");
        }
    }
}
