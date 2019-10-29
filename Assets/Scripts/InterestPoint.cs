using System;
using System.Collections;
using System.Collections.Generic;
using Characters.Scripts;
using ProBuilder2.Common;
using UnityEditor;
using UnityEngine;

public class InterestPoint : MonoBehaviour
{
    public float radius;
    [Range(0, 20)]
    public float distance;
    [HideInInspector]
    public float currentDistance;

    [HideInInspector]
    public float rotation;

    public float rotationOffset;


    private void OnDrawGizmos()
    {
        Handles.ArrowHandleCap(0, transform.position, Quaternion.AngleAxis(transform.rotation.eulerAngles.y + rotationOffset - 180, Vector3.up), 1, EventType.Repaint);
        Handles.DrawWireDisc(transform.position, Vector3.up, radius);
        var color = Color.blue;
        Handles.color = color;
        Handles.DrawWireDisc(transform.position, Vector3.up, radius + distance);

    }

    public float GetPercent(Vector3 pos)
    {
        rotation = transform.rotation.eulerAngles.y + rotationOffset - 180;
        currentDistance = (transform.position - pos).Flatten().magnitude;
        return 1 - Mathf.Clamp(currentDistance / distance, 0, 1);
    }

    // Start is called before the first frame update
    private void Awake()
    {
        rotation = transform.rotation.eulerAngles.y + rotationOffset;
        PlayerCamera.InterestPoints.Add(this);
    }
}
