using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI
{
    public interface DHUI_IStiffnessIllusion
    {
        void SetDisplacementVector(Vector3 _displacementVector);
        void SetNoDisplacement();
    }
}
