using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DHUI.Core
{
    public class DHUI_Interactor : MonoBehaviour
    {
        [SerializeField]
        private Transform _hoverCenterPoint = null;

        private List<DHUI_Interactable_Base> hoveredInteractables = new List<DHUI_Interactable_Base>();
        private DHUI_Interactable_Base primaryHoveredInteractable = null;

        private void Start()
        {
            
        }

    }
}
