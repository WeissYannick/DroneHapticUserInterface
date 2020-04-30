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
        protected DHUI_InteractionManager m_interactionManager = null;
        [SerializeField]
        protected DHUI_FlightController m_flightController = null;
        [SerializeField]
        protected Transform m_contactCenterPoint = null;
        [Header("Advanced")]
        [SerializeField]
        protected bool _manualRegistering = false;
        
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

        public virtual void Hover_Start(DHUI_Hand _hand)
        {
            
        }

        public virtual void Hover_Stay(DHUI_Hand _hand)
        {
            
        }

        public virtual void Hover_End(DHUI_Hand _hand)
        {
            
        }
    }
}
