using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    public class DHUI_FlightCommand_LandAt : DHUI_FlightCommand_BaseLand
    {
        public DHUI_FlightCommand_LandAt(Vector3 _targetPosition, Quaternion _targetRotation)
        {
            targetPosition = _targetPosition;
            targetRotation = _targetRotation;
        }
        
    }
}