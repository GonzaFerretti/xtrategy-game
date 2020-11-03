using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Touch Input/Action/Move Camera")]
public class TouchActionMoveCamera : TouchAction
{
    public override void Action(Vector2 direction, float magnitude)
    {
        Camera.main.GetComponent<CameraController>().MoveCamera(-direction);
    }
}
