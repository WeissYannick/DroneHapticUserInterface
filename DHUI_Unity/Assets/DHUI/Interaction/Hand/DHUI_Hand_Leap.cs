using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;

namespace DHUI
{
    public class DHUI_Hand_Leap : DHUI_Hand
    {
        [SerializeField]
        private HandModelBase m_leapRiggedHand = null;
       

        private void Start()
        {
            if (m_leapRiggedHand == null)
            {
                m_leapRiggedHand = GetComponent<HandModelBase>();
                if (m_leapRiggedHand == null)
                {
                    Debug.LogError("<b>DHUI</b> | DHUI_Hand_Leap | No Leap.Unity.HandModelBase-Component was set in Inspector or found on GameObject '" + gameObject.name + "'.");
                }
            }
        }

        protected override void UpdateHandStatus()
        {
            IsActive = m_leapRiggedHand.IsTracked;
            Leap.Hand hand = m_leapRiggedHand.GetLeapHand();

            if (!IsActive || hand == null) return;
            
            Vector3 indexTip = hand.Fingers[1].TipPosition.ToVector3();
            Vector3 middleTip = hand.Fingers[2].TipPosition.ToVector3();
            if (Vector3.Distance(indexTip, middleTip) < 0.05f)
            {
                InteractionPointMode = InteractionPointModes.Palm;
                Position = hand.PalmPosition.ToVector3();
            }
            else
            {
                InteractionPointMode = InteractionPointModes.Index;
                Position = hand.Fingers[1].TipPosition.ToVector3();
            }
        }
    }
}

