using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI
{
    public class DHUI_Hand_Leap : DHUI_Hand
    {
        [SerializeField]
        private Leap.Unity.RiggedHand m_leapRiggedHand = null;
       

        private void Start()
        {
            if (m_leapRiggedHand == null)
            {
                m_leapRiggedHand = GetComponent<Leap.Unity.RiggedHand>();
                if (m_leapRiggedHand == null)
                {
                    Debug.LogError("<b>DHUI</b> | DHUI_Hand_Leap | No Leap.Unity.RiggedHand-Component was set in Inspector or found on GameObject '" + gameObject.name + "'.");
                }
            }
        }

        protected override void UpdateHandStatus()
        {
            IsActive = m_leapRiggedHand.IsTracked;

            Vector3 indexTip = m_leapRiggedHand.fingers[1].GetTipPosition();
            Vector3 middleTip = m_leapRiggedHand.fingers[2].GetTipPosition();
            if (Vector3.Distance(indexTip, middleTip) < 0.05f)
            {
                InteractionPointMode = InteractionPointModes.Palm;
                Position = m_leapRiggedHand.GetPalmPosition();
            }
            else
            {
                InteractionPointMode = InteractionPointModes.Index;
                Position = m_leapRiggedHand.fingers[1].GetTipPosition();
            }
        }
    }
}

