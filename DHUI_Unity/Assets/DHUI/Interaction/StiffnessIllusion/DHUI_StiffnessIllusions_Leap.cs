using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leap;
using Leap.Unity;

namespace DHUI
{
    public class DHUI_StiffnessIllusions_Leap : PostProcessProvider, DHUI_IStiffnessIllusion
    {
        private Vector3 displacementVector = Vector3.zero;

        public void SetDisplacementVector(Vector3 _displacementVector)
        {
            displacementVector = _displacementVector;
        }

        public void SetNoDisplacement()
        {
            displacementVector = Vector3.zero;
        }

        public override void ProcessFrame(ref Frame inputFrame)
        {
            foreach (var hand in inputFrame.Hands)
            {
                Vector3 newPosition = hand.PalmPosition.ToVector3() + displacementVector;
                hand.SetTransform(newPosition, hand.Rotation.ToQuaternion());
            }
        }
    }
}