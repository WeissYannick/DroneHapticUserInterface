using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DHUI
{
    public interface DHUI_IHapticRetargeting
    {
        void SetActivationDistance(float activationDistance);
        void SetTargets(Transform virtualTarget, Transform physicalTarget);
        void EnableRetargeting();
        void DisableRetargeting();
        void HoldRetargeting();
        void UnholdRetargeting();
        void LockTargetPositions();
        void UnlockTargetPositions();
    }
}
