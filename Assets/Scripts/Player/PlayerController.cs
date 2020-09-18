using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class PlayerController : BaseController
{
    public bool GetObjectUnderMouse(out GameObject hitObject, int layerMaskOfObject)
    {
        hitObject = null;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector3 mousePosition = ray.origin;
        RaycastHit hit;
        Debug.DrawLine(ray.origin, ray.origin + ray.direction * 500f, Color.red, 5);
        bool hasHitObject = Physics.Raycast(ray, out hit, 500f, layerMaskOfObject);
        if (hasHitObject)
        {
            hitObject = hit.transform.gameObject;
        }
        return hasHitObject;
    }
}
