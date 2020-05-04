using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI
{
    public class DHUI_Hand : MonoBehaviour
    {
        public bool IsActive
        {
            get; protected set;
        }

        public Vector3 Position
        {
            get; protected set;
        }

        public InteractionPointModes InteractionPointMode
        {
            get; protected set;
        }


        private void FixedUpdate()
        {
            UpdateHandStatus();
        }

        protected virtual void UpdateHandStatus()
        {
        }

        
        public enum InteractionPointModes
        {
            Index, Palm
        }
    }
}
