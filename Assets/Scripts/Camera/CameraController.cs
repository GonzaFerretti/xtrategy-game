using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float transitionTime;
    [SerializeField] Camera cam;
    [SerializeField] float moveSpeed;
    FollowEvent currentFollowEvent = null;

    public void SetFollowTarget(Transform target)
    {
        currentFollowEvent = new FollowEvent(target);
        StartCoroutine(FollowTarget());
    }

    public void InterruptTargetFollow()
    {
        currentFollowEvent.shouldStop = true;
    }

    public void MoveCamera(Vector2 movementVector)
    {
        if (currentFollowEvent != null) InterruptTargetFollow();
        transform.position += new Vector3(movementVector.x,0,movementVector.y) * Time.deltaTime * moveSpeed;
    }

    IEnumerator FollowTarget()
    {
        float startTime = Time.time;
        float currentTime = 0;
        Vector3 startPos = new Vector3(transform.position.x, cam.orthographicSize, transform.position.z);

        while (currentTime < transitionTime)
        {
            if (currentFollowEvent.shouldStop)
            {
                break;
            }
            Vector3 targetPosition = new Vector3(currentFollowEvent.target.position.x, 2, currentFollowEvent.target.position.z);
            Vector3 currentPosition = Vector3.Slerp(startPos, targetPosition, currentTime / transitionTime);
            transform.position = new Vector3(currentPosition.x, transform.position.y, currentPosition.z);
            //cam.orthographicSize = currentPosition.y;
            currentTime = Time.time - startTime;
            yield return null;
        }
        currentFollowEvent = null;
    }
}
