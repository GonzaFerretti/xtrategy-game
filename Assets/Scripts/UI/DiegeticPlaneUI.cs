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
        var cam = FindObjectOfType<CameraController>();
        cam.OnCameraPositionChanged += UpdateFacingDirection;
        transform.forward = cam.mainCam.transform.forward;
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
