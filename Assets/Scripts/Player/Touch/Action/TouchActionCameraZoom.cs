using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Touch Input/Action/Camera Zoom")]
public class TouchActionCameraZoom : TouchAction
{
    public override void Action(Vector2 direction, float magnitude, TouchInputController controller)
    {
        controller.GetCameraController().ScrollZoom(magnitude);
    }
}
