using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DHUI.Core;

namespace DHUI
{
    public abstract class DHUI_Interactable : MonoBehaviour
    {
        [Header("Interactable.Setup")]
        [SerializeField]
        protected DHUI_InteractionManager m_interactionManager = null;
        [SerializeField]
        protected DHUI_FlightController m_flightController = null;
        [SerializeField]
        protected Transform m_contactCenterPoint = null;
        [Header("Interactable.Advanced")]
        [SerializeField]
        protected bool _manualRegistering = false;
        
        public Utils.MathPlane ContactPlane
        {
            get { return new Utils.MathPlane(m_contactCenterPoint); }
        }

        public Vector3 ContactCenterPoint
        {
            get { return m_contactCenterPoint.position; }
        }

        public bool IsDisabled
        {
            get;
            private set;
        }

        protected void Start()
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

        protected void OnDestroy()
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

        public virtual void Hover_Start(DHUI_HoverEventArgs _hoverEvent)
        {
            
        }

        public virtual void Hover_Stay(DHUI_HoverEventArgs _hoverEvent)
        {
            
        }

        public virtual void Hover_End(DHUI_HoverEventArgs _hoverEvent)
        {
            
        }
    }


    /// <summary>
    /// Event passed to Hover_Start, Hover_Stay and Hover_End
    /// </summary>
    public struct DHUI_HoverEventArgs
    {
        public Vector3 InteractorPosition;
    }
}
