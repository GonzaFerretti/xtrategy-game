using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public delegate void CameraUpdateDelegate(CameraController cam);
    public delegate void CameraTargetFollowEndDelegate();

    [SerializeField] float transitionTime;
    public Camera mainCam;
    [SerializeField] float moveSpeed;
    [SerializeField] float maxZoom;
    [SerializeField] float selectZoom;
    [SerializeField] float minZoom;
    [SerializeField] float zoomSensitivity;
    [SerializeField] float minZoomMovementMultiplier;
    [SerializeField] float maxZoomMovementMultiplier;
    FollowEvent currentFollowEvent = null;

    public CameraUpdateDelegate OnCameraPositionChanged;
    public CameraTargetFollowEndDelegate OnCameraTargetFollowEnd;

    public bool lockUserMovement = false;

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
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, float.MaxValue, 1 << LayerMask.NameToLayer("GroundBase")))
        {
            if (currentFollowEvent != null) InterruptTargetFollow();
            Vector3 hitPoint = hit.transform.position;
            currentFollowEvent = new FollowEvent(target);
            StartCoroutine(FollowTarget(hitPoint));
        }
        else
        {
            OnCameraTargetFollowEnd.Invoke();
        }
    }

    public void InterruptTargetFollow()
    {
        currentFollowEvent.shouldStop = true;
    }

    public void MoveCameraByVector(Vector2 movementVector)
    {
        if (!lockUserMovement)
        {
            if (currentFollowEvent != null) InterruptTargetFollow();
            float currentZoomPercentage = Mathf.InverseLerp(minZoom, maxZoom, mainCam.orthographicSize);
            float zoomMultiplier = Mathf.Lerp(minZoomMovementMultiplier, maxZoomMovementMultiplier, currentZoomPercentage);

            Vector3 desiredPosition = transform.position + new Vector3(movementVector.x, 0, movementVector.y) * Time.deltaTime * moveSpeed * zoomMultiplier;

            MoveCamera(desiredPosition);
        }
    }

    Vector3 LimitVectorToBoundaries(Vector3 baseVector)
    {
        return new Vector3(Mathf.Clamp(baseVector.x, minX, maxX), baseVector.y, Mathf.Clamp(baseVector.z, minZ, maxZ));
    }

    IEnumerator FollowTarget(Vector3 groundPos)
    {
        float startTime = Time.time;
        Vector3 offset = groundPos - transform.position;
        Vector3 startPos = new Vector3(groundPos.x, mainCam.orthographicSize, groundPos.z);
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

            Vector3 desiredPosition = new Vector3(currentPosition.x - offset.x, transform.position.y, currentPosition.z - offset.z);

            MoveCamera(desiredPosition);

            mainCam.orthographicSize = currentPosition.y;
            currentTime = Time.time - startTime;
            yield return null;
        }
        currentFollowEvent = null;
        if (OnCameraTargetFollowEnd != null)
            OnCameraTargetFollowEnd.Invoke();
    }

    void MoveCamera(Vector3 desiredPosition)
    {
        Vector3 clampedPosition = LimitVectorToBoundaries(desiredPosition);

        if (transform.position != clampedPosition)
        {
            OnCameraPositionChanged.Invoke(this);
            transform.position = clampedPosition;
        }
    }

    public void ScrollZoom(float inputDelta)
    {
        if (currentFollowEvent != null) InterruptTargetFollow();
        mainCam.orthographicSize = Mathf.Clamp(mainCam.orthographicSize + inputDelta * zoomSensitivity, minZoom, maxZoom);
    }
}
