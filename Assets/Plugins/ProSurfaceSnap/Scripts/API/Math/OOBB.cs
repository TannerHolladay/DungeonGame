﻿#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace PSS
{
    public struct OOBB
    {
        private Vector3 _size;
        private Vector3 _center;
        private Quaternion _rotation;
        private bool _isValid;

        public bool IsValid { get { return _isValid; } }
        public Vector3 Center { get { return _center; } set { _center = value; } }
        public Vector3 Size { get { return _size; } set { _size = value; } }
        public Vector3 Extents { get { return Size * 0.5f; } }
        public Quaternion Rotation { get { return _rotation; } set { _rotation = value; } }
        public Matrix4x4 RotationMatrix { get { return Matrix4x4.TRS(Vector3.zero, _rotation, Vector3.one); } }
        public Vector3 Right { get { return _rotation * Vector3.right; } }
        public Vector3 Up { get { return _rotation * Vector3.up; } }
        public Vector3 Look { get { return _rotation * Vector3.forward; } }

        public OOBB(Vector3 center, Vector3 size)
        {
            _center = center;
            _size = size;
            _rotation = Quaternion.identity;
            _isValid = true;
        }

        public OOBB(Vector3 center, Vector3 size, Quaternion rotation)
        {
            _center = center;
            _size = size;
            _rotation = rotation;
            _isValid = true;
        }

        public OOBB(Vector3 center, Quaternion rotation)
        {
            _center = center;
            _size = Vector3.zero;
            _rotation = rotation;
            _isValid = true;
        }

        public OOBB(Quaternion rotation)
        {
            _center = Vector3.zero;
            _size = Vector3.zero;
            _rotation = rotation;
            _isValid = true;
        }

        public OOBB(Bounds bounds, Quaternion rotation)
        {
            _center = bounds.center;
            _size = bounds.size;
            _rotation = rotation;
            _isValid = true;
        }

        public OOBB(AABB aabb)
        {
            _center = aabb.Center;
            _size = aabb.Size;
            _rotation = Quaternion.identity;
            _isValid = true;
        }

        public OOBB(AABB aabb, Quaternion rotation)
        {
            _center = aabb.Center;
            _size = aabb.Size;
            _rotation = rotation;
            _isValid = true;
        }

        public OOBB(AABB modelSpaceAABB, Transform worldTransform)
        {
            _size = Vector3.Scale(modelSpaceAABB.Size, worldTransform.lossyScale);
            _center = worldTransform.TransformPoint(modelSpaceAABB.Center);
            _rotation = worldTransform.rotation;
            _isValid = true;
        }

        public OOBB(OOBB copy)
        {
            _size = copy._size;
            _center = copy._center;
            _rotation = copy._rotation;
            _isValid = copy._isValid;
        }

        public OOBB(IEnumerable<Vector3> points, Quaternion rotation)
        {
            var pointList = new List<Vector3>(points);
            _center = pointList[0];

            _size = Vector3.zero;
            _rotation = rotation;
            _isValid = true;

            for (int ptIndex = 1; ptIndex < pointList.Count; ++ptIndex)
                Encapsulate(pointList[ptIndex]);
        }

        public static OOBB GetInvalid()
        {
            return new OOBB();
        }

        public void Inflate(float amount)
        {
            Size += Vector3Ex.FromValue(amount);
        }

        public Matrix4x4 GetUnitBoxTransform()
        {
            if (!_isValid) return Matrix4x4.identity;
            return Matrix4x4.TRS(Center, Rotation, Size);
        }

        public List<Vector3> GetCornerPoints()
        {
            return BoxMath.CalcBoxCornerPoints(_center, _size, _rotation);
        }

        public List<Vector3> GetCenterAndCornerPoints()
        {
            List<Vector3> centerAndCorners = GetCornerPoints();
            centerAndCorners.Add(Center);

            return centerAndCorners;
        }

        public void Encapsulate(OOBB otherOOBB)
        {
            var otherPts = BoxMath.CalcBoxCornerPoints(otherOOBB.Center, otherOOBB.Size, otherOOBB.Rotation);

            Matrix4x4 transformMtx = Matrix4x4.TRS(Center, Rotation, Vector3.one);
            var modelPts = transformMtx.inverse.TransformPoints(otherPts);

            AABB modelAABB = new AABB(Vector3.zero, Size);
            modelAABB.Encapsulate(modelPts);

            Center = (Rotation * modelAABB.Center) + Center;
            Size = modelAABB.Size;
        }

        public void Encapsulate(Vector3 point)
        {
            Matrix4x4 transformMtx = Matrix4x4.TRS(Center, Rotation, Vector3.one);
            var modelPt = transformMtx.inverse.MultiplyPoint(point);

            AABB modelAABB = new AABB(Vector3.zero, Size);
            modelAABB.Encapsulate(modelPt);

            Center = (Rotation * modelAABB.Center) + Center;
            Size = modelAABB.Size;
        }

        /// <summary>
        /// Assuming 'pointOnFace' is a point which resides on one of the OOBB's faces,
        /// the method will return the normal of this face.
        /// </summary>
        public Vector3 GetPointFaceNormal(Vector3 pointOnFace)
        {
            Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, _rotation, Vector3.one);
            Vector3[] boxAxes = rotationMatrix.GetNormalizedAxes();
            Vector3 extents = Extents;

            Vector3 fromCenterToPt = pointOnFace - _center;
            float smallestDiff = float.MaxValue;
            float bestAxisSign = -1.0f;
            Vector3 bestAxis = Vector3.zero;

            for(int axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                Vector3 axis = boxAxes[axisIndex];
                float projection = Vector3.Dot(fromCenterToPt, axis);

                float diff = Mathf.Abs(Mathf.Abs(projection) - extents[axisIndex]);
                if(diff < smallestDiff)
                {
                    smallestDiff = diff;
                    bestAxis = axis;                   
                    bestAxisSign = Mathf.Sign(projection);
                }
            }

            return Vector3.Normalize(bestAxis * bestAxisSign);
        }

        public bool IntersectsOOBB(OOBB otherOOBB)
        {
            return BoxMath.BoxIntersectsBox(_center, _size, _rotation, otherOOBB.Center, otherOOBB.Size, otherOOBB.Rotation);
        }

        public Vector3 GetClosestPoint(Vector3 point)
        {
            return BoxMath.CalcBoxPtClosestToPt(point, _center, _size, _rotation);
        }

        public bool IntersectsSphere(Sphere sphere)
        {
            Vector3 closestPt = GetClosestPoint(sphere.Center);
            return sphere.ContainsPoint(closestPt);
        }
    }
}
#endif