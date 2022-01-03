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
        [SerializeField]
        private GameObject m_stiffnessIllusionObject = null;

        private DHUI_IHapticRetargeting hapticRetargeting = null;
        private DHUI_IStiffnessIllusion stiffnessIllusion = null;

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

        private HashSet<DHUI_Touchable> registeredTouchables = new HashSet<DHUI_Touchable>();

        private DHUI_Touchable internal_activeTouchable;
        public DHUI_Touchable ActiveTouchable
        {
            get { return internal_activeTouchable; }
            private set
            {
                if (internal_activeTouchable != value)
                {
                    if (internal_activeTouchable != null)
                    {
                        internal_activeTouchable.Hover_End(GenerateHoverEventArgs());
                    }
                    if (value != null)
                    {
                        value.Hover_Start(GenerateHoverEventArgs());
                    }
                    internal_activeTouchable = value;
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

        public DHUI_IStiffnessIllusion StiffnessIllusion
        {
            get
            {
                if (stiffnessIllusion == null)
                {
                    stiffnessIllusion = m_stiffnessIllusionObject.GetComponent<DHUI_IStiffnessIllusion>();
                }
                return stiffnessIllusion;
            }
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
            UpdateTouchables();
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

        #region Touchables

        #region Touchables.Registering

        public void RegisterTouchable(DHUI_Touchable _touchable)
        {
            registeredTouchables.Add(_touchable);
        }

        public void DeregisterTouchable(DHUI_Touchable _touchable)
        {
            registeredTouchables.Remove(_touchable);
        }

        private void ClearRegisteredTouchables()
        {
            registeredTouchables.Clear();
        }

        #endregion Touchables.Registering

        #region Touchables.Updating

        private void UpdateTouchables() {
            
            CheckClosestTouchable();
            UpdateCurrentActiveTouchable();
        }

        private void CheckClosestTouchable()
        {
            float closestDistance = float.MaxValue;
            Vector2 bodyCenter2D = new Vector2(m_head.position.x, m_head.position.z);

            DHUI_Touchable closestTouchable = null;
            foreach (DHUI_Touchable touchable in registeredTouchables)
            {
                // Skip Disabled and Inactive Touchables
                if (!touchable.isActiveAndEnabled || !touchable.IsEnabled)
                {
                    continue;
                }
                // Skip Touchables to close to the user's body.
                Vector2 touchableCenter2D = new Vector2(touchable.CenterPoint.x, touchable.CenterPoint.z);
                if (Vector2.Distance(bodyCenter2D, touchableCenter2D) <= _bodyDangerRadius && touchable != ActiveTouchable)
                {
                    continue;
                }

                // Get closest Touchable to the InteractionPoint
                float distance = Vector3.Distance(m_physicalInteractionPoint.position, touchable.CenterPoint);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTouchable = touchable;
                }
            }

            if (closestDistance <= _maxReachableDistance)
            {
                ActiveTouchable = closestTouchable;
            }
            else
            {
                ActiveTouchable = null;
            }
        }

        private void UpdateCurrentActiveTouchable()
        {
            ActiveTouchable?.Hover_Stay(GenerateHoverEventArgs());
        }

        #endregion Touchables.Updating

        #endregion Touchables

        #region HoverEvent
        
        private DHUI_Touchable.DHUI_HoverEventArgs GenerateHoverEventArgs()
        {
            DHUI_Touchable.DHUI_HoverEventArgs hover = new DHUI_Touchable.DHUI_HoverEventArgs();
            hover.InteractorPhysicalPosition = m_physicalInteractionPoint.position;
            hover.InteractorVirtualPosition = m_virtualInteractionPoint.position;
            return hover;
        }

        #endregion HoverEvent
        
    }
}
