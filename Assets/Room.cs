using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Room : MonoBehaviour
{
    public HashSet<Vector2> GridPoints = new HashSet<Vector2>();
    public int expand = 1;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        var bounds = new Bounds(transform.position, Vector3.zero);
        GridPoints.Clear();
        Vector3 localCenter;
        Vector3 center;
        Vector3 size;
        foreach (var render in GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(render.bounds);
        }
        /*foreach (var render in GetComponentsInChildren<Renderer>())
        {
            center = PositionToGrid(render.bounds.center);
            size = RoundToOdd(render.bounds.size);
            Gizmos.color = Color.blue;
            for (var x = 0; x < size.x; x++)
            {
                for (var z = 0; z < size.z; z++)
                {
                    var pixel = center - new Vector3(Mathf.Floor(size.x / 2), 0, Mathf.Floor(size.z / 2)) + new Vector3(x, 0, z);
                    GridPoints.Add(new Vector2(pixel.x - bounds.center.x, pixel.z - bounds.center.z));
                }
            }
        }
        foreach (var pixel in GridPoints)
        {
            Gizmos.DrawWireCube(pixel.ToVector3() + bounds.center, new Vector3(1, bounds.size.y, 1));
        }*/

        localCenter = transform.InverseTransformPoint(bounds.center);
        center = PositionToGrid(bounds.center);
        Debug.Log(PositionToGrid(localCenter));
        size = RoundToOdd(bounds.size);
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(center, size);
    }

    private Vector3 PositionToGrid(Vector3 input)
    {
        //input = RoundVector(input, 1000);
        input.x = (Mathf.Abs(input.x) < .5f ? 0 : Mathf.Round(Mathf.Abs(input.x) - .5f) + .5f) * Mathf.Sign(input.x);
        input.z = (Mathf.Abs(input.z) < .5f ? 0 : Mathf.Round(Mathf.Abs(input.z) - .5f) + .5f) * Mathf.Sign(input.z);
        Debug.Log(input.x);
        return input;
    }

    private Vector3 RoundToOdd(Vector3 input)
    {
        //input = RoundVector(input, 1000);
        input.x = 2 * Mathf.Ceil(input.x / 2) + 1;
        input.z = 2 * Mathf.Ceil(input.z / 2) + 1;
        Debug.Log(input.x);
        return input;
    }

    private Vector3 RoundVector(Vector3 input, int nth = 10)
    {
        return new Vector3(Mathf.Round(input.x * nth) / nth, input.y, Mathf.Round(input.z * nth) / nth);
    }
}
