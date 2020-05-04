using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DHUI_Hand_DebugCanvas : MonoBehaviour
{
    public Transform lookAtTarget = null;

    public DHUI.DHUI_InteractionManager interactionManager = null;

    public Text activeHandState_text = null;
    public Text interactionPointMode_text = null;

    void Update()
    {
        transform.LookAt(Camera.main.transform);

        if (interactionManager.activeHandState == DHUI.DHUI_InteractionManager.ActiveHandState.None)
        {
            transform.GetChild(0).gameObject.SetActive(false);
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(true);
            activeHandState_text.text = interactionManager.activeHandState.ToString();
            interactionPointMode_text.text = interactionManager.mainHand.InteractionPointMode.ToString();
        }
    }
}
