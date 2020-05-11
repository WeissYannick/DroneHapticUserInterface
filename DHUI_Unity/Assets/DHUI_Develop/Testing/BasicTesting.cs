using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicTesting : MonoBehaviour
{
    DHUI.Utils.MathPlane mp = null;

    private void Start()
    {
        mp = new DHUI.Utils.MathPlane(transform);
    }

    private void Update()
    {
        if (mp != null)
        {
           // Debug.Log(mp.GetDistance(transform.position));

            Debug.Log(mp.GetProjectedPoint(transform.position));
        }
    }

}
