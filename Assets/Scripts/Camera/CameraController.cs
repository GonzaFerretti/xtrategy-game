using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] float minZoomMovementMultiplier;
    [SerializeField] float maxZoomMovementMultiplier;
    FollowEvent currentFollowEvent = null;


    [SerializeField] float minX;
    [SerializeField] float maxX;

    [SerializeField] float minZ;
    [SerializeField] float maxZ;


    public void Start()
    {
        if (Application.isEditor)
        {
            zoomSensitivity = zoomSensitivity * 100f;
            moveSpeed = moveSpeed * 100f;
        }
    }

    public void SetFollowTarget(Transform target)
    {
        if (Physics.Raycast(transform.position,transform.forward, out RaycastHit hit, float.MaxValue, 1 << LayerMask.NameToLayer("GroundBase")))
        {
            if (currentFollowEvent != null) InterruptTargetFollow();
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
        float currentZoomPercentage = Mathf.InverseLerp(minZoom,maxZoom, cam.orthographicSize);
        float zoomMultiplier = Mathf.Lerp(minZoomMovementMultiplier, maxZoomMovementMultiplier, currentZoomPercentage);
        transform.position += LimitVectorToBoundaries(new Vector3(movementVector.x,0, movementVector.y) * Time.deltaTime * moveSpeed * zoomMultiplier);
    }

    Vector3 LimitVectorToBoundaries(Vector3 baseVector)
    {
        return baseVector;
        //return new Vector3(Mathf.Clamp(baseVector.x, minX, maxX), baseVector.y, Mathf.Clamp(baseVector.z, minZ, maxZ));
    }

    IEnumerator FollowTarget(Vector3 groundPos)
    {
        float startTime = Time.time;
        Vector3 offset = groundPos - transform.position;
        Vector3 startPos = new Vector3(groundPos.x, cam.orthographicSize, groundPos.z);
        float currentTime = 0;

        while (currentTime < transitionTime)
        {
            if (currentFollowEvent == null) break;
            if (currentFollowEvent.shouldStop)
            {
                break;
            }
            Vector3 targetPosition = new Vector3(currentFollowEvent.target.position.x, selectZoom, currentFollowEvent.target.position.z);
            Vector3 currentPosition = Vector3.Lerp(startPos, targetPosition, currentTime / transitionTime);
            transform.position = LimitVectorToBoundaries(new Vector3(currentPosition.x - offset.x, transform.position.y, currentPosition.z - offset.z));
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
