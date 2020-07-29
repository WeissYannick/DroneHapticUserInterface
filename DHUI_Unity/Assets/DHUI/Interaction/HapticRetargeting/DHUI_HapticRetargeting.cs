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
                Vector3 currentPhysicalVector = transform.InverseTransformPoint(physicalTarget.position) - transform.InverseTransformPoint(physicalHand.position);

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
                    virtualHand.position = transform.TransformPoint(transform.InverseTransformPoint(physicalHand.position) + Vector3.Lerp(Vector3.zero, transform.InverseTransformPoint(virtualTarget.position) - transform.InverseTransformPoint(physicalTarget.position), retargetingCurve.Evaluate(1 - step)));
                }
            }
            else
            {
                virtualHand.position = physicalHand.position;

                if (Vector3.Distance(transform.TransformPoint(physicalHand.position), transform.TransformPoint(physicalTarget.position)) < activationDistance)
                {
                    StartRetargeting();
                }
            }


        }

        public void StartRetargeting()
        {
            startingPosition = transform.TransformPoint(physicalHand.position);
            physicalVector = transform.TransformPoint(physicalTarget.position) - startingPosition;
            virtualVector = transform.TransformPoint(virtualTarget.position) - startingPosition;
            retargetingOn = true;
        }

    }
}