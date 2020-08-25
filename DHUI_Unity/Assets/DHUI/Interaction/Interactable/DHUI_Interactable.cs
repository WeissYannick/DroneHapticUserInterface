using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DHUI.Core;
using System;
using UnityEngine.Events;

namespace DHUI
{
    public abstract class DHUI_Interactable : MonoBehaviour
    {
        
        [Header("Interactable.Setup")]
        [SerializeField]
        protected DHUI_InteractionManager m_interactionManager = null;
        [SerializeField]
        protected Transform m_centerPoint = null;
        [SerializeField]
        protected List<Transform> m_touchBounds = new List<Transform>();
        
        [Header("Interactable.Events")]
        public DHUI_InteractableHoverEvent OnHoverStart = null;
        public DHUI_InteractableHoverEvent OnHoverStay = null;
        public DHUI_InteractableHoverEvent OnHoverEnd = null;

        [Header("Interactable.Advanced")]
        [SerializeField]
        protected bool _manualRegistering = false;
        
        [Serializable]
        public class DHUI_InteractableHoverEvent : UnityEvent<DHUI_HoverEventArgs> { }
        
        public Vector3 CenterPoint
        {
            get { return m_centerPoint.position; }
        }

        public List<Vector3> TouchBounds
        {
            get {
                List<Vector3> touchBoundPoints = new List<Vector3>();
                foreach (Transform t in m_touchBounds)
                {
                    touchBoundPoints.Add(t.position);
                }
                return touchBoundPoints;
            }
        }
        
        public bool IsDisabled
        {
            get;
            private set;
        }

        protected virtual void Start()
        {
            if (m_interactionManager == null)
            {
                m_interactionManager = FindObjectOfType<DHUI_InteractionManager>();
            }
            if (m_centerPoint == null)
            {
                m_centerPoint = transform;
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
            //m_interactionManager?.RegisterInteractable(this);
        }   

        public void Deregister()
        {
            if (m_interactionManager == null)
            {
                m_interactionManager = FindObjectOfType<DHUI_InteractionManager>();
            }
            //m_interactionManager?.DeregisterInteractable(this);
        }

        public virtual void Hover_Start(DHUI_HoverEventArgs _hoverEvent)
        {
            OnHoverStart?.Invoke(_hoverEvent);
        }

        public virtual void Hover_Stay(DHUI_HoverEventArgs _hoverEvent)
        {

            OnHoverStay?.Invoke(_hoverEvent);
        }

        public virtual void Hover_End(DHUI_HoverEventArgs _hoverEvent)
        {

            OnHoverEnd?.Invoke(_hoverEvent);
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