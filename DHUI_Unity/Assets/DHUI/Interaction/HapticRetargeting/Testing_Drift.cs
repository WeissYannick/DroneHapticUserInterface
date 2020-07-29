using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testing_Drift : MonoBehaviour
{
    Vector3 currentDriftVector = Vector3.zero;

    private float lastTimeUpdated = 0;
    private float updateInterval = 3;

    void Update()
    {
        if (Time.time > lastTimeUpdated + updateInterval)
        {
            lastTimeUpdated = Time.time;

            currentDriftVector = new Vector3(Random.value - 0.5f, Random.value  - 0.5f, Random.value - 0.5f);
        }

        transform.Translate(currentDriftVector * 0.0003f);
    }
}
