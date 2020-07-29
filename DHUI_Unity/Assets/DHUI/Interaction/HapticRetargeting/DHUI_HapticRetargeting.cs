using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI
{
    public class DHUI_HapticRetargeting : MonoBehaviour
    {
        [SerializeField]
        protected Transform virtualHand = null;

        [SerializeField]
        protected Transform virtualTarget = null;

        [SerializeField]
        protected Transform physicalHand = null;

        [SerializeField]
        protected Transform physicalTarget = null;

        [SerializeField]
        protected float activationDistance = 1;

        protected Vector3 startingPosition = Vector3.zero;
        protected Vector3 physicalVector = Vector3.zero;
        protected Vector3 virtualVector = Vector3.zero;

        private bool retargetingOn = false;

        public AnimationCurve retargetingCurve = null;

        public bool onlyUseZForStep = false;

        protected void Update()
        {

            if (retargetingOn)
            {
                Vector3 currentPhysicalVector = physicalTarget.localPosition - physicalHand.localPosition;

                float step = 0;
                if (onlyUseZForStep)
                {
                    step = Mathf.Abs(currentPhysicalVector.z / physicalVector.z);
                }
                else
                {
                    step = currentPhysicalVector.magnitude / physicalVector.magnitude;
                }

                if (step > 1) {

                    retargetingOn = false;
                }
                else
                {
                    virtualHand.localPosition = physicalHand.localPosition + Vector3.Lerp(Vector3.zero, virtualTarget.localPosition - physicalTarget.localPosition, retargetingCurve.Evaluate(1 - step));
                }
            }
            else
            {
                virtualHand.localPosition = physicalHand.localPosition;

                if (Vector3.Distance(physicalHand.localPosition, physicalTarget.localPosition) < activationDistance)
                {
                    StartRetargeting();
                }
            }


        }

        public void StartRetargeting()
        {
            startingPosition = physicalHand.localPosition;
            physicalVector = physicalTarget.localPosition - startingPosition;
            virtualVector = virtualTarget.localPosition - startingPosition;
            retargetingOn = true;
            
        }

    }
}