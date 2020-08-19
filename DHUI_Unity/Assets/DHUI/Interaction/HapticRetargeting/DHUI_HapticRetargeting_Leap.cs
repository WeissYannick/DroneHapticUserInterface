﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap.Unity;
using Leap;

namespace DHUI
{
    public class DHUI_HapticRetargeting_Leap : PostProcessProvider, DHUI_IHapticRetargeting
    {
        [SerializeField]
        protected Transform virtualTarget = null;
        [SerializeField]
        protected Transform physicalTarget = null;
        [SerializeField]
        protected float activationDistance = 1;


        protected Vector3 startingPosition = Vector3.zero;
        protected Vector3 physicalVector = Vector3.zero;
        protected Vector3 virtualVector = Vector3.zero;

        private bool retargetingOn = false;

        private bool retargetingEnabled = false;

        public AnimationCurve retargetingCurve = null;

        #region IHapticRetargeting

        public void SetActivationDistance(float _activationDistance)
        {
            activationDistance = _activationDistance;
        }
        public void SetTargets(Transform _virtualTarget, Transform _physicalTarget)
        {
            virtualTarget = _virtualTarget;
            physicalTarget = _physicalTarget;
        }
        public void EnableRetargeting()
        {
            retargetingEnabled = true;
        }

        public void DisableRetargeting()
        {
            retargetingEnabled = false;
        }

        #endregion IHapticRetargeting

        #region PostProcessProvider

        public override void ProcessFrame(ref Frame inputFrame)
        {
            if (physicalTarget == null || virtualTarget == null || !retargetingEnabled) return;

            foreach (var hand in inputFrame.Hands)
            {
                Vector3 physicalHandPosition = transform.InverseTransformPoint(hand.PalmPosition.ToVector3());
                Vector3 newPosition = transform.TransformPoint(physicalHandPosition);

                if (retargetingOn)
                {
                    Vector3 currentPhysicalVector = transform.InverseTransformPoint(physicalTarget.position) - physicalHandPosition;

                    float step = 0;
                    step = currentPhysicalVector.magnitude / physicalVector.magnitude;
                

                    if (step > 1)
                    {

                        retargetingOn = false;
                    }
                    else
                    {
                        newPosition = transform.TransformPoint(physicalHandPosition + Vector3.Lerp(Vector3.zero, transform.InverseTransformPoint(virtualTarget.position) - transform.InverseTransformPoint(physicalTarget.position), retargetingCurve.Evaluate(1 - step)));
                    }
                }
                else
                {
                    if (Vector3.Distance(newPosition, transform.TransformPoint(physicalTarget.position)) < activationDistance)
                    {
                        startingPosition = transform.TransformPoint(physicalHandPosition);
                        physicalVector = transform.TransformPoint(physicalTarget.position) - startingPosition;
                        virtualVector = transform.TransformPoint(virtualTarget.position) - startingPosition;
                        retargetingOn = true;
                    }
                }

                hand.SetTransform(newPosition,hand.Rotation.ToQuaternion());
            }
        }

        #endregion PostProcessProvider

    }
}