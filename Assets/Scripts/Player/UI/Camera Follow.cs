using UnityEngine;
using System;
using System.Collections;

public class CameraFollow : MonoBehaviour
{
public Transform target;
public Vector3 offset;
public float smoothSpeed;

    void LateUpdate()
    {
        Vector3 desiredPosition = target.position + offset;
        desiredPosition.z = transform.position.z;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}
