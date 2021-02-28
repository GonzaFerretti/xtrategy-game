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
        FindObjectOfType<CameraController>().OnCameraPositionChanged -= UpdateFacingDirection;
    }
}
