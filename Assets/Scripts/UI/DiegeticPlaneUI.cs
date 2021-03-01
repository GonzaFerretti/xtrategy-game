using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiegeticPlaneUI : MonoBehaviour
{
    public void UpdateFacingDirection(CameraController cam)
    {
        transform.forward = cam.mainCam.transform.forward;
    }

    public void Start()
    {
        FindObjectOfType<CameraController>().OnCameraPositionChanged += UpdateFacingDirection;
    }

    public void OnDestroy()
    {
        var camController = FindObjectOfType<CameraController>();
        try
        {
            if (camController)
            {
                camController.OnCameraPositionChanged -= UpdateFacingDirection;
            }
        }
        catch { }
    }
}
