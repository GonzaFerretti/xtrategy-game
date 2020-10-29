using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float transitionTime;
    [SerializeField] Camera cam;
    [SerializeField] float moveSpeed;
    [SerializeField] float maxZoom;
    [SerializeField] float selectZoom;
    [SerializeField] float minZoom;
    [SerializeField] float zoomSensitivity;
    FollowEvent currentFollowEvent = null;

    public void SetFollowTarget(Transform target)
    {
        if (Physics.Raycast(transform.position,transform.forward, out RaycastHit hit, float.MaxValue, 1 << LayerMask.NameToLayer("GroundBase")))
        {
            Vector3 hitPoint = hit.transform.position;
            currentFollowEvent = new FollowEvent(target);
            StartCoroutine(FollowTarget(hitPoint));
        }
    }

    public void InterruptTargetFollow()
    {
        currentFollowEvent.shouldStop = true;
    }

    public void MoveCamera(Vector2 movementVector)
    {
        if (currentFollowEvent != null) InterruptTargetFollow();
        transform.position += new Vector3(movementVector.x,0, movementVector.y) * Time.deltaTime * moveSpeed;
    }

    IEnumerator FollowTarget(Vector3 groundPos)
    {
        float startTime = Time.time;
        Vector3 offset = groundPos - transform.position;
        Vector3 startPos = new Vector3(groundPos.x, cam.orthographicSize, groundPos.z);
        float currentTime = 0;

        while (currentTime < transitionTime)
        {
            if (currentFollowEvent.shouldStop)
            {
                break;
            }
            Vector3 targetPosition = new Vector3(currentFollowEvent.target.position.x, selectZoom, currentFollowEvent.target.position.z);
            Vector3 currentPosition = Vector3.Lerp(startPos, targetPosition, currentTime / transitionTime);
            transform.position = new Vector3(currentPosition.x - offset.x, transform.position.y, currentPosition.z - offset.z);
            cam.orthographicSize = currentPosition.y;
            currentTime = Time.time - startTime;
            yield return null;
        }
        currentFollowEvent = null;
    }

    public void ScrollZoom(float inputDelta)
    {
        if (currentFollowEvent != null) InterruptTargetFollow(); 
        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize + inputDelta * zoomSensitivity, minZoom, maxZoom);
    }
}
