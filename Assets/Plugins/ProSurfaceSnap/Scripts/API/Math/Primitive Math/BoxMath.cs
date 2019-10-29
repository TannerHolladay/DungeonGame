#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace PSS
{
    public enum BoxCorner
    {
        FrontTopLeft = 0,
        FrontTopRight,
        FrontBottomRight,
        FrontBottomLeft,
        BackTopLeft,
        BackTopRight,
        BackBottomRight,
        BackBottomLeft
    }

    public enum BoxFace
    {
        Front = 0,
        Back,
        Left,
        Right,
        Bottom,
        Top
    }

    public struct BoxFaceDesc
    {
        public BoxFace Face;
        public Plane Plane;
        public Vector3 Center;
    }

    public static class BoxMath
    {
        private static List<BoxFace> _allBoxFaces = new List<BoxFace>();

        static BoxMath()
        {
            _allBoxFaces.Add(BoxFace.Front);
            _allBoxFaces.Add(BoxFace.Back);
            _allBoxFaces.Add(BoxFace.Left);
            _allBoxFaces.Add(BoxFace.Right);
            _allBoxFaces.Add(BoxFace.Bottom);
            _allBoxFaces.Add(BoxFace.Top);
        }

        public static List<BoxFace> AllBoxFaces { get { return new List<BoxFace>(_allBoxFaces); } }

        public static int GetFaceAxisIndex(BoxFace face)
        {
            if (face == BoxFace.Top || face == BoxFace.Bottom) return 1;
            if (face == BoxFace.Left || face == BoxFace.Right) return 0;
            if (face == BoxFace.Back || face == BoxFace.Front) return 2;

            return -1;
        }

        public static BoxFaceDesc GetFaceClosestToPoint(Vector3 point, Vector3 boxCenter, Vector3 boxSize, Quaternion boxRotation)
        {
            float minDist = float.MaxValue;
            Plane bestPlane = new Plane();
            BoxFace bestFace = BoxFace.Front;
            var allFaces = AllBoxFaces;

            foreach(var face in allFaces)
            {
                Plane facePlane = BoxMath.CalcBoxFacePlane(boxCenter, boxSize, boxRotation, face);
                float dist = facePlane.GetAbsDistanceToPoint(point);
                if(dist < minDist)
                {
                    bestFace = face;
                    bestPlane = facePlane;
                    minDist = dist;
                }
            }

            return new BoxFaceDesc()
            {
                Face = bestFace,
                Plane = bestPlane,
                Center = BoxMath.CalcBoxFaceCenter(boxCenter, boxSize, boxRotation, bestFace)
            };
        }

        public static List<Vector3> CalcFaceMidSegmentPoints(Vector3 boxCenter, Vector3 boxSize, Quaternion boxRotation, BoxFace boxFace)
        {
            var faceCorners = CalcFaceCorners(boxCenter, boxSize, boxRotation, boxFace);
            var midPts = new List<Vector3>(4);

            for (int cornerIndex = 0; cornerIndex < faceCorners.Count; ++cornerIndex)
            {
                var pt0 = faceCorners[cornerIndex];
                var pt1 = faceCorners[(cornerIndex + 1) % faceCorners.Count];
                midPts.Add((pt0 + pt1) * 0.5f);
            }

            return midPts;
        }

        public static List<Vector3> CalcFaceCorners(Vector3 boxCenter, Vector3 boxSize, Quaternion boxRotation, BoxFace boxFace)
        {
            var boxCorners = CalcBoxCornerPoints(boxCenter, boxSize, boxRotation);
            switch (boxFace)
            {
                case BoxFace.Back:

                    return new List<Vector3>() 
                    { 
                        boxCorners[(int)BoxCorner.BackBottomLeft],
                        boxCorners[(int)BoxCorner.BackTopLeft],
                        boxCorners[(int)BoxCorner.BackTopRight],
                        boxCorners[(int)BoxCorner.BackBottomRight]
                    };

                case BoxFace.Front:

                    return new List<Vector3>() 
                    { 
                        boxCorners[(int)BoxCorner.FrontBottomLeft],
                        boxCorners[(int)BoxCorner.FrontTopLeft],
                        boxCorners[(int)BoxCorner.FrontTopRight],
                        boxCorners[(int)BoxCorner.FrontBottomRight]
                    };

                case BoxFace.Left:

                    return new List<Vector3>() 
                    { 
                        boxCorners[(int)BoxCorner.BackTopRight],
                        boxCorners[(int)BoxCorner.FrontTopLeft],
                        boxCorners[(int)BoxCorner.FrontBottomLeft],
                        boxCorners[(int)BoxCorner.BackBottomRight]
                    };

                case BoxFace.Right:

                    return new List<Vector3>() 
                    { 
                        boxCorners[(int)BoxCorner.FrontBottomRight],
                        boxCorners[(int)BoxCorner.FrontTopRight],
                        boxCorners[(int)BoxCorner.BackTopLeft],
                        boxCorners[(int)BoxCorner.BackBottomLeft]
                    };

                case BoxFace.Bottom:

                    return new List<Vector3>() 
                    { 
                        boxCorners[(int)BoxCorner.FrontBottomRight],
                        boxCorners[(int)BoxCorner.FrontBottomLeft],
                        boxCorners[(int)BoxCorner.BackBottomRight],
                        boxCorners[(int)BoxCorner.BackBottomLeft]
                    };

                case BoxFace.Top:

                    return new List<Vector3>() 
                    { 
                        boxCorners[(int)BoxCorner.FrontTopRight],
                        boxCorners[(int)BoxCorner.FrontTopLeft],
                        boxCorners[(int)BoxCorner.BackTopRight],
                        boxCorners[(int)BoxCorner.BackTopLeft]
                    };

                default:

                    return new List<Vector3>();
            }
        }

        public static BoxFace GetMostAlignedFace(Vector3 boxCenter, Vector3 boxSize, Quaternion boxRotation, Vector3 direction)
        {
            var allFaces = AllBoxFaces;
            int bestFaceIndex = 0;
            float bestScore = Vector3.Dot(direction, CalcBoxFaceNormal(boxCenter, boxSize, boxRotation, allFaces[0]));

            for (int faceIndex = 1; faceIndex < allFaces.Count; ++faceIndex)
            {
                float score = Vector3.Dot(direction, CalcBoxFaceNormal(boxCenter, boxSize, boxRotation, allFaces[faceIndex]));
                if (score > bestScore)
                {
                    bestScore = score;
                    bestFaceIndex = faceIndex;
                }
            }

            return allFaces[bestFaceIndex];
        }

        public static Plane CalcBoxFacePlane(Vector3 boxCenter, Vector3 boxSize, Quaternion boxRotation, BoxFace boxFace)
        {
            Vector3 extents = boxSize * 0.5f;
            Vector3 rightAxis = boxRotation * Vector3.right;
            Vector3 upAxis = boxRotation * Vector3.up;
            Vector3 lookAxis = boxRotation * Vector3.forward;

            switch (boxFace)
            {
                case BoxFace.Front:

                    return new Plane(-lookAxis, boxCenter - lookAxis * extents.z);

                case BoxFace.Back:

                    return new Plane(lookAxis, boxCenter + lookAxis * extents.z);

                case BoxFace.Left:

                    return new Plane(-rightAxis, boxCenter - rightAxis * extents.x);

                case BoxFace.Right:

                    return new Plane(rightAxis, boxCenter + rightAxis * extents.x);

                case BoxFace.Bottom:

                    return new Plane(-upAxis, boxCenter - upAxis * extents.y);

                default:

                    return new Plane(upAxis, boxCenter + upAxis * extents.y);
            }
        }

        public static Vector3 CalcBoxFaceNormal(Vector3 boxCenter, Vector3 boxSize, Quaternion boxRotation, BoxFace boxFace)
        {
            Vector3 rightAxis = boxRotation * Vector3.right;
            Vector3 upAxis = boxRotation * Vector3.up;
            Vector3 lookAxis = boxRotation * Vector3.forward;

            switch (boxFace)
            {
                case BoxFace.Front:

                    return -lookAxis;

                case BoxFace.Back:

                    return lookAxis;

                case BoxFace.Left:

                    return -rightAxis;

                case BoxFace.Right:

                    return rightAxis;

                case BoxFace.Bottom:

                    return -upAxis;

                default:

                    return upAxis;
            }
        }

        public static Vector3 CalcBoxFaceCenter(Vector3 boxCenter, Vector3 boxSize, Quaternion boxRotation, BoxFace boxFace)
        {
            Vector3 extents = boxSize * 0.5f;
            Vector3 rightAxis = boxRotation * Vector3.right;
            Vector3 upAxis = boxRotation * Vector3.up;
            Vector3 lookAxis = boxRotation * Vector3.forward;

            switch(boxFace)
            {
                case BoxFace.Front:

                    return boxCenter - lookAxis * extents.z;

                case BoxFace.Back:

                    return boxCenter + lookAxis * extents.z;

                case BoxFace.Left:

                    return boxCenter - rightAxis * extents.x;

                case BoxFace.Right:

                    return boxCenter + rightAxis * extents.x;

                case BoxFace.Bottom:

                    return boxCenter - upAxis * extents.y;

                default:

                    return boxCenter + upAxis * extents.y;
            }
        }

        public static List<Vector3> CalcBoxCornerPoints(Vector3 boxCenter, Vector3 boxSize, Quaternion boxRotation)
        {
            Vector3 extents = boxSize * 0.5f;
            Vector3 rightAxis = boxRotation * Vector3.right;
            Vector3 upAxis = boxRotation * Vector3.up;
            Vector3 lookAxis = boxRotation * Vector3.forward;

            var cornerPoints = new Vector3[8];
            Vector3 faceCenter = boxCenter - lookAxis * extents.z;
            cornerPoints[(int)BoxCorner.FrontTopLeft] = faceCenter - rightAxis * extents.x + upAxis * extents.y;
            cornerPoints[(int)BoxCorner.FrontTopRight] = faceCenter + rightAxis * extents.x + upAxis * extents.y;
            cornerPoints[(int)BoxCorner.FrontBottomRight] = faceCenter + rightAxis * extents.x - upAxis * extents.y;
            cornerPoints[(int)BoxCorner.FrontBottomLeft] = faceCenter - rightAxis * extents.x - upAxis * extents.y;

            faceCenter = boxCenter + lookAxis * extents.z;
            cornerPoints[(int)BoxCorner.BackTopLeft] = faceCenter + rightAxis * extents.x + upAxis * extents.y;
            cornerPoints[(int)BoxCorner.BackTopRight] = faceCenter - rightAxis * extents.x + upAxis * extents.y;
            cornerPoints[(int)BoxCorner.BackBottomRight] = faceCenter - rightAxis * extents.x - upAxis * extents.y;
            cornerPoints[(int)BoxCorner.BackBottomLeft] = faceCenter + rightAxis * extents.x - upAxis * extents.y;

            return new List<Vector3>(cornerPoints);
        }

        public static void TransformBox(Vector3 boxCenter, Vector3 boxSize, Matrix4x4 transformMatrix, out Vector3 newBoxCenter, out Vector3 newBoxSize)
        {
            Vector3 rightAxis = transformMatrix.GetColumn(0);
            Vector3 upAxis = transformMatrix.GetColumn(1);
            Vector3 lookAxis = transformMatrix.GetColumn(2);

            Vector3 extents = boxSize * 0.5f;
            Vector3 newExtentsRight = rightAxis * extents.x;
            Vector3 newExtentsUp = upAxis * extents.y;
            Vector3 newExtentsLook = lookAxis * extents.z;

            float newExtentX = Mathf.Abs(newExtentsRight.x) + Mathf.Abs(newExtentsUp.x) + Mathf.Abs(newExtentsLook.x);
            float newExtentY = Mathf.Abs(newExtentsRight.y) + Mathf.Abs(newExtentsUp.y) + Mathf.Abs(newExtentsLook.y);
            float newExtentZ = Mathf.Abs(newExtentsRight.z) + Mathf.Abs(newExtentsUp.z) + Mathf.Abs(newExtentsLook.z);

            newBoxCenter = transformMatrix.MultiplyPoint(boxCenter);
            newBoxSize = new Vector3(newExtentX, newExtentY, newExtentZ) * 2.0f;
        }

        #region BoxIntersectsBox Heap Alloc
        private static Vector3[] A = new Vector3[3];
        private static Vector3[] B = new Vector3[3];
        private static float[,] R = new float[3, 3];
        private static float[,] absR = new float[3, 3];
        #endregion
        public static bool BoxIntersectsBox(Vector3 center0, Vector3 size0, Quaternion rotation0, Vector3 center1, Vector3 size1, Quaternion rotation1)
        {
            A[0] = rotation0 * Vector3.right;
            A[1] = rotation0 * Vector3.up;
            A[2] = rotation0 * Vector3.forward;

            B[0] = rotation1 * Vector3.right;
            B[1] = rotation1 * Vector3.up;
            B[2] = rotation1 * Vector3.forward;

            // Note: We're using column major matrices.
            for (int row = 0; row < 3; ++row)
            {
                for (int column = 0; column < 3; ++column)
                {
                    R[row, column] = Vector3.Dot(A[row], B[column]);
                }
            }

            Vector3 extents = size0 * 0.5f;
            Vector3 AEx = new Vector3(extents.x, extents.y, extents.z);
            extents = size1 * 0.5f;
            Vector3 BEx = new Vector3(extents.x, extents.y, extents.z);

            // Construct absolute rotation error matrix to account for cases when 2 local axes are parallel
            const float epsilon = 1e-4f;
            for (int row = 0; row < 3; ++row)
            {
                for (int column = 0; column < 3; ++column)
                {
                    absR[row, column] = Mathf.Abs(R[row, column]) + epsilon;
                }
            }

            Vector3 trVector = center1 - center0;
            Vector3 t = new Vector3(Vector3.Dot(trVector, A[0]), Vector3.Dot(trVector, A[1]), Vector3.Dot(trVector, A[2]));

            // Test extents projection on box A local axes (A0, A1, A2)
            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                float bExtents = BEx[0] * absR[axisIndex, 0] + BEx[1] * absR[axisIndex, 1] + BEx[2] * absR[axisIndex, 2];
                if (Mathf.Abs(t[axisIndex]) > AEx[axisIndex] + bExtents) return false;
            }

            // Test extents projection on box B local axes (B0, B1, B2)
            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                float aExtents = AEx[0] * absR[0, axisIndex] + AEx[1] * absR[1, axisIndex] + AEx[2] * absR[2, axisIndex];
                if (Mathf.Abs(t[0] * R[0, axisIndex] +
                              t[1] * R[1, axisIndex] +
                              t[2] * R[2, axisIndex]) > aExtents + BEx[axisIndex]) return false;
            }

            // Test axis A0 x B0
            float ra = AEx[1] * absR[2, 0] + AEx[2] * absR[1, 0];
            float rb = BEx[1] * absR[0, 2] + BEx[2] * absR[0, 1];
            if (Mathf.Abs(t[2] * R[1, 0] - t[1] * R[2, 0]) > ra + rb) return false;

            // Test axis A0 x B1
            ra = AEx[1] * absR[2, 1] + AEx[2] * absR[1, 1];
            rb = BEx[0] * absR[0, 2] + BEx[2] * absR[0, 0];
            if (Mathf.Abs(t[2] * R[1, 1] - t[1] * R[2, 1]) > ra + rb) return false;

            // Test axis A0 x B2
            ra = AEx[1] * absR[2, 2] + AEx[2] * absR[1, 2];
            rb = BEx[0] * absR[0, 1] + BEx[1] * absR[0, 0];
            if (Mathf.Abs(t[2] * R[1, 2] - t[1] * R[2, 2]) > ra + rb) return false;

            // Test axis A1 x B0
            ra = AEx[0] * absR[2, 0] + AEx[2] * absR[0, 0];
            rb = BEx[1] * absR[1, 2] + BEx[2] * absR[1, 1];
            if (Mathf.Abs(t[0] * R[2, 0] - t[2] * R[0, 0]) > ra + rb) return false;

            // Test axis A1 x B1
            ra = AEx[0] * absR[2, 1] + AEx[2] * absR[0, 1];
            rb = BEx[0] * absR[1, 2] + BEx[2] * absR[1, 0];
            if (Mathf.Abs(t[0] * R[2, 1] - t[2] * R[0, 1]) > ra + rb) return false;

            // Test axis A1 x B2
            ra = AEx[0] * absR[2, 2] + AEx[2] * absR[0, 2];
            rb = BEx[0] * absR[1, 1] + BEx[1] * absR[1, 0];
            if (Mathf.Abs(t[0] * R[2, 2] - t[2] * R[0, 2]) > ra + rb) return false;

            // Test axis A2 x B0
            ra = AEx[0] * absR[1, 0] + AEx[1] * absR[0, 0];
            rb = BEx[1] * absR[2, 2] + BEx[2] * absR[2, 1];
            if (Mathf.Abs(t[1] * R[0, 0] - t[0] * R[1, 0]) > ra + rb) return false;

            // Test axis A2 x B1
            ra = AEx[0] * absR[1, 1] + AEx[1] * absR[0, 1];
            rb = BEx[0] * absR[2, 2] + BEx[2] * absR[2, 0];
            if (Mathf.Abs(t[1] * R[0, 1] - t[0] * R[1, 1]) > ra + rb) return false;

            // Test axis A2 x B2
            ra = AEx[0] * absR[1, 2] + AEx[1] * absR[0, 2];
            rb = BEx[0] * absR[2, 1] + BEx[1] * absR[2, 0];
            if (Mathf.Abs(t[1] * R[0, 2] - t[0] * R[1, 2]) > ra + rb) return false;

            return true;
        }

        public static Vector3 CalcBoxPtClosestToPt(Vector3 point, Vector3 boxCenter, Vector3 boxSize, Quaternion boxRotation)
        {
            Vector3 fromCenterToPt = point - boxCenter;
            Vector3[] localAxes = new Vector3[] { boxRotation * Vector3.right, boxRotation * Vector3.up, boxRotation * Vector3.forward };
            Vector3 extents = boxSize * 0.5f;

            Vector3 closestPt = boxCenter;
            for (int axisIndex = 0; axisIndex < 3; ++axisIndex)
            {
                float projection = Vector3.Dot(localAxes[axisIndex], fromCenterToPt);
                if (projection > extents[axisIndex]) projection = extents[axisIndex];
                else if (projection < -extents[axisIndex]) projection = -extents[axisIndex];

                closestPt += localAxes[axisIndex] * projection;
            }

            return closestPt;
        }

        public static bool Raycast(Ray ray, Vector3 boxCenter, Vector3 boxSize, Quaternion boxRotation, BoxEpsilon epsilon = new BoxEpsilon())
        {
            float t;
            return Raycast(ray, out t, boxCenter, boxSize, boxRotation, epsilon);
        }

        public static bool Raycast(Ray ray, out float t, Vector3 boxCenter, Vector3 boxSize, Quaternion boxRotation, BoxEpsilon epsilon = new BoxEpsilon())
        {
            t = 0.0f;
            boxSize += epsilon.SizeEps;

            const float minBoxSize = 1e-6f;
            int invalidSizeCounter = 0;
            if (boxSize.x < minBoxSize) ++invalidSizeCounter;
            if (boxSize.y < minBoxSize) ++invalidSizeCounter;
            if (boxSize.z < minBoxSize) ++invalidSizeCounter;
            if (invalidSizeCounter > 1) return false;

            if (invalidSizeCounter == 1)
            {
                if (boxSize.x < minBoxSize)
                {
                    Vector3 quadRight = boxRotation * Vector3.forward;
                    Vector3 quadUp = boxRotation * Vector3.up;
                    return QuadMath.Raycast(ray, out t, boxCenter, boxSize.z, boxSize.y, quadRight, quadUp);
                }
                else
                if (boxSize.y < minBoxSize)
                {
                    Vector3 quadRight = boxRotation * Vector3.right;
                    Vector3 quadUp = boxRotation * Vector3.forward;
                    return QuadMath.Raycast(ray, out t, boxCenter, boxSize.x, boxSize.z, quadRight, quadUp);
                }
                else
                {
                    Vector3 quadRight = boxRotation * Vector3.right;
                    Vector3 quadUp = boxRotation * Vector3.up;
                    return QuadMath.Raycast(ray, out t, boxCenter, boxSize.x, boxSize.y, quadRight, quadUp);
                }
            }

            Matrix4x4 boxMatrix = Matrix4x4.TRS(boxCenter, boxRotation, boxSize);
            Ray modelRay = ray.InverseTransform(boxMatrix);
            if (modelRay.direction.sqrMagnitude == 0.0f) return false;

            Bounds unitCube = new Bounds(Vector3.zero, Vector3.one);
            if (unitCube.IntersectRay(modelRay, out t))
            {
                Vector3 intersectPt = boxMatrix.MultiplyPoint(modelRay.GetPoint(t));
                t = (intersectPt - ray.origin).magnitude;
                return true;
            }

            return false;
        }

        public static bool ContainsPoint(Vector3 point, Vector3 boxCenter, Vector3 boxSize, Quaternion boxRotation, BoxEpsilon epsilon = new BoxEpsilon())
        {
            boxSize += epsilon.SizeEps;
            Vector3 extents = boxSize * 0.5f;

            Vector3 right = boxRotation * Vector3.right;
            Vector3 up = boxRotation * Vector3.up;
            Vector3 look = boxRotation * Vector3.forward;

            point -= boxCenter;
            float dot = Vector3.Dot(right, point);
            if (dot < -extents.x || dot > extents.x) return false;

            dot = Vector3.Dot(up, point);
            if (dot < -extents.y || dot > extents.y) return false;

            dot = Vector3.Dot(look, point);
            return (dot >= -extents.z && dot <= extents.z);
        }
    }
}
#endif