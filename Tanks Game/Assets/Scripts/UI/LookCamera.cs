using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookCamera : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Camera.main)
        {
            Camera cameraToLookAt = Camera.main;
            transform.LookAt(cameraToLookAt.transform);
            transform.rotation = Quaternion.LookRotation(cameraToLookAt.transform.forward);
        }
    }
}
