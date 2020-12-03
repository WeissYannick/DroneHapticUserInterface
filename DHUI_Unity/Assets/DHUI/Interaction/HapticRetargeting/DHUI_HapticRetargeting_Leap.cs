using System.Collections;
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

        protected Vector3 virtualVector = Vector3.zero;

        private bool retargetingOn = false;

        private bool retargetingEnabled = false;

        private bool retargetingHold = false;

        private bool retargetingLocked = false;

        public AnimationCurve retargetingCurve = null;

        private Vector3 retargetingVector = Vector3.zero;

        private Vector3 currentVirtualTargetPos = Vector3.zero;

        private Vector3 currentPhysicalTargetPos = Vector3.zero;

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

        public void HoldRetargeting()
        {
            retargetingHold = true;
        }

        public void UnholdRetargeting()
        {
            retargetingHold = false;
        }

        public void LockTargetPositions()
        {
            retargetingLocked = true;
        }

        public void UnlockTargetPositions()
        {
            retargetingLocked = false;
        }

        #endregion IHapticRetargeting

        #region PostProcessProvider

        public override void ProcessFrame(ref Frame inputFrame)
        {
            if (retargetingEnabled && !retargetingLocked && physicalTarget != null && virtualTarget != null)
            {
                currentVirtualTargetPos = virtualTarget.position;
                currentPhysicalTargetPos = physicalTarget.position;
            }

            foreach (var hand in inputFrame.Hands)
            {
                Vector3 newPosition = hand.PalmPosition.ToVector3();
                Vector3 physicalHandPosition = transform.InverseTransformPoint(newPosition);

                if (!retargetingEnabled)
                {
                    hand.SetTransform(hand.PalmPosition.ToVector3(), hand.Rotation.ToQuaternion());
                    return;
                }
                else { 
                    if (retargetingHold)
                    {
                        newPosition = transform.TransformPoint(physicalHandPosition + retargetingVector);
                    }
                    else if (retargetingOn)
                    {
                        Vector3 currentPhysicalVector = transform.InverseTransformPoint(currentPhysicalTargetPos) - physicalHandPosition;
                        Vector3 physicalVector = transform.TransformPoint(currentPhysicalTargetPos) - startingPosition;

                        float step = 0;
                        step = currentPhysicalVector.magnitude / physicalVector.magnitude;
                
                        if (step > 1)
                        {
                            retargetingOn = false;
                        }
                        else
                        {
                            Vector3 newRetargetingVector = Vector3.Lerp(Vector3.zero, transform.InverseTransformPoint(currentVirtualTargetPos) - transform.InverseTransformPoint(currentPhysicalTargetPos), retargetingCurve.Evaluate(1 - step));
                            if (newRetargetingVector.magnitude > retargetingVector.magnitude)
                            {
                                retargetingVector = newRetargetingVector;
                            }
                            else
                            {
                                retargetingVector = retargetingVector / (retargetingVector.magnitude / newRetargetingVector.magnitude);
                            }
                            newPosition = transform.TransformPoint(physicalHandPosition + retargetingVector);
                        }
                    }
                    else
                    {
                        if (Vector3.Distance(newPosition, transform.TransformPoint(currentPhysicalTargetPos)) < activationDistance)
                        {
                            startingPosition = transform.TransformPoint(physicalHandPosition);
                            virtualVector = transform.TransformPoint(currentVirtualTargetPos) - startingPosition;
                            retargetingOn = true;
                        }
                    }

                    hand.SetTransform(newPosition, hand.Rotation.ToQuaternion());
                }

            }
        }

        #endregion PostProcessProvider

    }
}