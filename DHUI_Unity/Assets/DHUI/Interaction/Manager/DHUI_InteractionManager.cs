using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DHUI.Core;

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
        private DHUI_Hand m_leftRetargetedHand = null;
        [SerializeField]
        private DHUI_Hand m_rightRetargetedHand = null;
        [SerializeField]
        private Transform m_head = null;
        [SerializeField]
        private Transform m_physicalInteractionPoint = null;
        [SerializeField]
        private Transform m_virtualInteractionPoint = null;
        [SerializeField]
        private DHUI_DroneController m_droneController = null;
        [SerializeField]
        private DHUI_FlightController m_flightController = null;
        [SerializeField]
        private GameObject m_hapticRetargetingObject = null;

        private DHUI_IHapticRetargeting hapticRetargeting = null;

        [Header("Settings")]
        [SerializeField]
        private float _maxReachableDistance = 1;
        [SerializeField]
        private float _bodyDangerRadius = 0.3f;

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
        public DHUI_Hand mainRetargetedHand
        {
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

        public static string InteractorTag = "DHUI_Interactor";
        
        public DHUI_IHapticRetargeting HapticRetargeting
        {
            get {
                if (hapticRetargeting == null)
                {
                    hapticRetargeting = m_hapticRetargetingObject.GetComponent<DHUI_IHapticRetargeting>();
                }
                return hapticRetargeting; }
        }

        public DHUI_FlightController FlightController
        {
            get { return m_flightController; }
        }

        public DHUI_DroneController DroneController
        {
            get { return m_droneController; }
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
                    mainRetargetedHand = m_rightRetargetedHand;
                    m_physicalInteractionPoint.position = mainHand.Position;
                    m_virtualInteractionPoint.position = mainRetargetedHand.Position;
                    break;
                case ActiveHandState.Left:
                    mainHand = m_leftHand;
                    mainRetargetedHand = m_leftRetargetedHand;
                    m_physicalInteractionPoint.position = mainHand.Position;
                    m_virtualInteractionPoint.position = mainRetargetedHand.Position;
                    break;
                case ActiveHandState.None:
                default:
                    mainHand = null;
                    mainRetargetedHand = null;
                    m_physicalInteractionPoint.position = m_head.position;
                    m_virtualInteractionPoint.position = m_head.position;
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
            Vector2 bodyCenter2D = new Vector2(m_head.position.x, m_head.position.z);

            DHUI_Interactable closestInteractable = null;
            foreach (DHUI_Interactable interactable in registeredInteractables)
            {
                // Skip Disabled and Inactive Interactables
                if (!interactable.isActiveAndEnabled || interactable.IsDisabled)
                {
                    continue;
                }
                // Skip Interactables to close to the user's body.
                Vector2 interactableCenter2D = new Vector2(interactable.CenterPoint.x, interactable.CenterPoint.z);
                if (Vector2.Distance(bodyCenter2D, interactableCenter2D) <= _bodyDangerRadius && interactable != ActiveInteractable)
                {
                    continue;
                }

                // Get closest Interactable to the InteractionPoint
                float distance = Vector3.Distance(m_physicalInteractionPoint.position, interactable.CenterPoint);
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
            hover.InteractorPosition = m_physicalInteractionPoint.position;
            return hover;
        }

        #endregion HoverEvent
        
    }
}
