using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DHUI.Core;
using UnityEngine.Events;
using System;

namespace DHUI
{
    public class DHUI_Toggle : DHUI_Button
    {
        [Header("Toggle.Setup")]
        [SerializeField]
        protected GameObject m_objectToMoveForLock = null;
        [SerializeField]
        protected Transform m_unlocked_position = null;
        [SerializeField]
        protected Transform m_lockedIn_position = null;

        [Header("Toggle.Settings")]
        [SerializeField]
        protected float _lockIn_threshold = 0.2f;

        [Serializable]
        public class DHUI_ToggleLockingEvent : UnityEvent { }
        [Header("Toggle.Events")]
        public DHUI_ToggleLockingEvent OnToggleLocked = null;
        public DHUI_ToggleLockingEvent OnToggleUnlocked = null;

        private bool lockInProcess = false;
        private bool lockOutProcess = false;
        private bool lockedIn = false;

        protected override void Start()
        {
            base.Start();
        }


        protected override void UpdateState(DHUI_HoverEventArgs _hoverEvent)
        {
            base.UpdateState(_hoverEvent);

            
            if (currentActivationDistance >= _lockIn_threshold)
            {
                if (!lockedIn && !lockOutProcess)
                {
                    lockInProcess = true;
                    lockedIn = true;
                    LockToggle();
                }
                else if (lockedIn && !lockInProcess)
                {
                    lockOutProcess = true;
                    lockedIn = false;
                    UnlockToggle();
                }
            }
            else
            {
                lockInProcess = false;
                lockOutProcess = false;
            }

        }

        protected virtual void LockToggle()
        {
            Debug.Log("Toggle locked");
            m_objectToMoveForLock.transform.position = m_lockedIn_position.position;
            OnToggleLocked?.Invoke();
        }

        protected virtual void UnlockToggle()
        {
            Debug.Log("Toggle unlocked");
            m_objectToMoveForLock.transform.position = m_unlocked_position.position;
            OnToggleUnlocked?.Invoke();
        }
    }

}