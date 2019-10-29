#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace PSS
{
    public static class Vector2Ex
    {
        public static Vector2 Abs(this Vector2 v)
        {
            return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
        }

        public static float AbsDot(this Vector2 v1, Vector2 v2)
        {
            return Mathf.Abs(Vector2.Dot(v1, v2));
        }

        public static Vector3 ToVector3(this Vector2 vec, float z = 0.0f)
        {
            return new Vector3(vec.x, vec.y, z);
        }

        public static Vector2 GetNormal(this Vector2 vec)
        {
            return (new Vector2(-vec.y, vec.x)).normalized;
        }

        public static Vector2 FromValue(float value)
        {
            return new Vector2(value, value);
        }

        public static float GetDistanceToSegment(this Vector2 point, Vector2 point0, Vector2 point1)
        {
            Vector2 segmentDir = (point1 - point0);
            float segmentLength = segmentDir.magnitude;
            segmentDir.Normalize();

            Vector2 fromStartToPt = (point - point0);

            float projection = Vector2.Dot(segmentDir, fromStartToPt);

            if (projection >= 0.0f && projection <= segmentLength)
                return ((point0 + segmentDir * projection) - point).magnitude;

            if (projection < 0.0f) return fromStartToPt.magnitude;
            return (point1 - point).magnitude;
        }

        public static int GetPointClosestToPoint(List<Vector2> points, Vector2 pt)
        {
            float minDistSq = float.MaxValue;
            int closestPtIndex = -1;

            for (int ptIndex = 0; ptIndex < points.Count; ++ptIndex)
            {
                Vector2 point = points[ptIndex];

                float distSq = (point - pt).sqrMagnitude;
                if (distSq < minDistSq)
                {
                    minDistSq = distSq;
                    closestPtIndex = ptIndex;
                }
            }

            return closestPtIndex;
        }
    }
}
#endif