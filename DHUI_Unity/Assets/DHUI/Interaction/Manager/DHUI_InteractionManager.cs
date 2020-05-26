using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI
{
    public class DHUI_InteractionManager : MonoBehaviour
    {
        #region Fields

        [Header("Members")]
        [SerializeField]
        private DHUI_Hand m_leftHand = null;
        [SerializeField]
        private DHUI_Hand m_rightHand = null;
        [SerializeField]
        private Transform m_head = null;
        [SerializeField]
        private Transform m_interactionPoint = null;
        [Header("Settings")]
        [SerializeField]
        private float _maxReachableDistance = 1;

        public enum ActiveHandState {
            None, Left, Right, Both
        }
        public ActiveHandState activeHandState {
            private set;
            get;
        } = ActiveHandState.None;
        public DHUI_Hand mainHand {
            private set;
            get;
        } = null;

        private HashSet<DHUI_Interactable> registeredInteractables = new HashSet<DHUI_Interactable>();

        private DHUI_Interactable internal_activeInteractable;
        public DHUI_Interactable ActiveInteractable
        {
            get { return internal_activeInteractable; }
            private set
            {
                if (internal_activeInteractable != value)
                {
                    if (internal_activeInteractable != null)
                    {
                        internal_activeInteractable.Hover_End(GenerateHoverEventArgs());
                    }
                    if (value != null)
                    {
                        value.Hover_Start(GenerateHoverEventArgs());
                    }
                    internal_activeInteractable = value;
                }
            }
        }

        #endregion Fields

        #region UpdateLoop

        private void FixedUpdate()
        {
            UpdateHands();
            UpdateInteractables();
        }

        #endregion UpdateLoop

        #region Hands

        private void UpdateHands()
        {
            UpdateActiveHandState();
            UpdateMainHand();
        }

        private void UpdateActiveHandState()
        {
            if (m_rightHand != null && m_rightHand.isActiveAndEnabled && m_rightHand.IsActive)
            {
                if (m_leftHand != null && m_leftHand.isActiveAndEnabled && m_leftHand.IsActive)
                {
                    activeHandState = ActiveHandState.Both;
                }
                else
                {
                    activeHandState = ActiveHandState.Right;
                }
            }
            else if (m_leftHand != null && m_leftHand.isActiveAndEnabled && m_leftHand.IsActive)
            {
                activeHandState = ActiveHandState.Left;
            }
            else
            {
                activeHandState = ActiveHandState.None;
            }
        }

        private void UpdateMainHand()
        {
            switch (activeHandState)
            {
                case ActiveHandState.Both:
                case ActiveHandState.Right:
                    mainHand = m_rightHand;
                    m_interactionPoint.position = mainHand.Position;
                    break;
                case ActiveHandState.Left:
                    mainHand = m_leftHand;
                    m_interactionPoint.position = mainHand.Position;
                    break;
                case ActiveHandState.None:
                default:
                    mainHand = null;
                    m_interactionPoint.position = m_head.position;
                    break;
            }
        }

        #endregion Hands

        #region Interactables

        #region Interactables.Registering

        public void RegisterInteractable(DHUI_Interactable _interactable)
        {
            registeredInteractables.Add(_interactable);
        }

        public void DeregisterInteractable(DHUI_Interactable _interactable)
        {
            registeredInteractables.Remove(_interactable);
        }

        private void ClearRegisteredInteractables()
        {
            registeredInteractables.Clear();
        }

        #endregion Interactables.Registering

        #region Interactables.Updating

        private void UpdateInteractables() {

            CheckClosestInteractable();
            UpdateCurrentActiveInteractable();
        }

        private void CheckClosestInteractable()
        {
            float closestDistance = float.MaxValue;
            DHUI_Interactable closestInteractable = null;
            foreach (DHUI_Interactable interactable in registeredInteractables)
            {
                if (!interactable.isActiveAndEnabled || interactable.IsDisabled)
                {
                    continue;
                }

                float distance = Vector3.Distance(m_interactionPoint.position, interactable.CenterPoint);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }

            if (closestDistance <= _maxReachableDistance)
            {
                ActiveInteractable = closestInteractable;
            }
            else
            {
                ActiveInteractable = null;
            }
        }

        private void UpdateCurrentActiveInteractable()
        {
            ActiveInteractable?.Hover_Stay(GenerateHoverEventArgs());
        }

        #endregion Interactables.Updating

        #endregion Interactables

        #region HoverEvent
        
        private DHUI_HoverEventArgs GenerateHoverEventArgs()
        {
            DHUI_HoverEventArgs hover = new DHUI_HoverEventArgs();
            hover.InteractorPosition = m_interactionPoint.position;
            return hover;
        }

        #endregion HoverEvent
    }
}
