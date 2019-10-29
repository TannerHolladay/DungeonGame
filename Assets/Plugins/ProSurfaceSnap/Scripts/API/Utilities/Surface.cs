#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace PSS
{
    public class Surface
    {
        public enum Type
        {
            UnityTerrain = 0,
            Mesh,
            TerrainMesh,
            SphericalMesh,
            SceneGrid,
        }

        public struct ObjectSnapConfig
        {
            public bool AlignAxis;
            public TransformAxis AlignmentAxis;
            public Type SurfaceType;
            public float OffsetFromSurface;
            public Vector3 SurfaceHitPoint;
            public Vector3 SurfaceHitNormal;
            public Plane SurfaceHitPlane;
            public GameObject SurfaceObject;

            public bool IsSurfaceMesh()
            {
                return SurfaceType == Type.Mesh || SurfaceType == Type.SphericalMesh || SurfaceType == Type.TerrainMesh;
            }
        }

        public struct ObjectSnapResult
        {
            public bool WasSnapped;
            public Plane SittingPlane;
            public Vector3 SittingPoint;

            public ObjectSnapResult(Plane sittingPlane, Vector3 sittingPoint)
            {
                WasSnapped = true;
                SittingPlane = sittingPlane;
                SittingPoint = sittingPoint;
            }
        }

        public static ObjectSnapResult SnapHierarchy(GameObject root, ObjectSnapConfig snapConfig)
        {
            const float collectEps = 1e-2f;
            const float collectBoxScale = 1e-3f;

            bool hierarchyHasMeshes = root.HierarchyHasMesh();
            bool hierarchyHasSprites = root.HierarchyHasSprite();
            if (!hierarchyHasMeshes && !hierarchyHasSprites) return new ObjectSnapResult();

            var boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.ObjectTypes = GameObjectType.Sprite | GameObjectType.Mesh;

            bool isSurfaceSpherical = snapConfig.SurfaceType == Type.SphericalMesh;
            bool isSurfaceTerrain = snapConfig.SurfaceType == Type.UnityTerrain || snapConfig.SurfaceType == Type.TerrainMesh;
            bool isSurfaceUnityTerrain = snapConfig.SurfaceType == Type.UnityTerrain;

            var raycaster = CreateSurfaceRaycaster(snapConfig.SurfaceType, snapConfig.SurfaceObject, true);
            if (snapConfig.SurfaceType != Type.SceneGrid)
            {
                Transform rootTransform = root.transform;
                if (snapConfig.AlignAxis)
                {
                    if (isSurfaceTerrain)
                    {
                        rootTransform.Align(Vector3.up, snapConfig.AlignmentAxis);

                        OOBB hierarchyOOBB = ObjectBounds.CalcHierarchyWorldOOBB(root, boundsQConfig);
                        if (!hierarchyOOBB.IsValid) return new ObjectSnapResult();

                        BoxFace pivotFace = BoxMath.GetMostAlignedFace(hierarchyOOBB.Center, hierarchyOOBB.Size, hierarchyOOBB.Rotation, -Vector3.up);
                        var collectedVerts = ObjectVertexCollect.CollectHierarchyVerts(root, pivotFace, collectBoxScale, collectEps);
  
                        if (collectedVerts.Count != 0)
                        {
                            Vector3 vertsCenter = Vector3Ex.GetPointCloudCenter(collectedVerts);
                            Ray ray = new Ray(vertsCenter + Vector3.up * 1e-3f, -Vector3.up);
                            GameObjectRayHit surfaceHit = raycaster.Raycast(ray);

                            if (surfaceHit != null)
                            {
                                Vector3 alignmentAxis = surfaceHit.HitNormal;
                                if (isSurfaceUnityTerrain)
                                {
                                    Terrain terrain = snapConfig.SurfaceObject.GetComponent<Terrain>();
                                    alignmentAxis = terrain.GetInterpolatedNormal(surfaceHit.HitPoint);
                                }
                                Quaternion appliedRotation = rootTransform.Align(alignmentAxis, snapConfig.AlignmentAxis);

                                hierarchyOOBB = ObjectBounds.CalcHierarchyWorldOOBB(root, boundsQConfig);
                                appliedRotation.RotatePoints(collectedVerts, rootTransform.position);

                                Vector3 sitOnPlaneOffset = Surface.CalculateSitOnSurfaceOffset(hierarchyOOBB, new Plane(Vector3.up, surfaceHit.HitPoint), 0.1f);
                                rootTransform.position += sitOnPlaneOffset;
                                hierarchyOOBB.Center += sitOnPlaneOffset;
                                Vector3Ex.OffsetPoints(collectedVerts, sitOnPlaneOffset);

                                Vector3 embedVector = Surface.CalculateEmbedVector(collectedVerts, snapConfig.SurfaceObject, -Vector3.up, snapConfig.SurfaceType);
                                rootTransform.position += (embedVector + alignmentAxis * snapConfig.OffsetFromSurface);
                                return new ObjectSnapResult(new Plane(alignmentAxis, surfaceHit.HitPoint), surfaceHit.HitPoint);
                            }
                        }
                    }
                    else
                    {
                        if (!isSurfaceSpherical)
                        {
                            rootTransform.Align(snapConfig.SurfaceHitNormal, snapConfig.AlignmentAxis);
                            OOBB hierarchyOOBB = ObjectBounds.CalcHierarchyWorldOOBB(root, boundsQConfig);
                            if (!hierarchyOOBB.IsValid) return new ObjectSnapResult();

                            BoxFace pivotFace = BoxMath.GetMostAlignedFace(hierarchyOOBB.Center, hierarchyOOBB.Size, hierarchyOOBB.Rotation, -snapConfig.SurfaceHitNormal);
                            var collectedVerts = ObjectVertexCollect.CollectHierarchyVerts(root, pivotFace, collectBoxScale, collectEps);

                            if (collectedVerts.Count != 0)
                            {
                                Vector3 vertsCenter = Vector3Ex.GetPointCloudCenter(collectedVerts);

                                // Note: Cast the ray from far away enough so that we don't cast from the interior of the mesh.
                                //       This can happen when the object is embedded inside the mesh surface.
                                AABB surfaceAABB = ObjectBounds.CalcMeshWorldAABB(snapConfig.SurfaceObject);
                                float sphereRadius = surfaceAABB.Extents.magnitude;
                                Vector3 rayOrigin = vertsCenter + snapConfig.SurfaceHitNormal * sphereRadius;

                                Ray ray = new Ray(rayOrigin, -snapConfig.SurfaceHitNormal);   
                                GameObjectRayHit surfaceHit = raycaster.Raycast(ray);
                                
                                if (surfaceHit != null)
                                {
                                    Vector3 alignmentAxis = surfaceHit.HitNormal;
                                    Quaternion appliedRotation = rootTransform.Align(alignmentAxis, snapConfig.AlignmentAxis);

                                    hierarchyOOBB = ObjectBounds.CalcHierarchyWorldOOBB(root, boundsQConfig);
                                    appliedRotation.RotatePoints(collectedVerts, rootTransform.position);

                                    Vector3 sitOnPlaneOffset = Surface.CalculateSitOnSurfaceOffset(hierarchyOOBB, surfaceHit.HitPlane, 0.0f);
                                    rootTransform.position += sitOnPlaneOffset;
                                    hierarchyOOBB.Center += sitOnPlaneOffset;
                                    Vector3Ex.OffsetPoints(collectedVerts, sitOnPlaneOffset);

                                    return new ObjectSnapResult(new Plane(alignmentAxis, surfaceHit.HitPoint), surfaceHit.HitPoint);
                                }
                            }
                        }
                        else
                        {
                            Transform surfaceObjectTransform = snapConfig.SurfaceObject.transform;
                            Vector3 sphereCenter = surfaceObjectTransform.position;
                            Vector3 radiusDir = (rootTransform.position - sphereCenter).normalized;
                            float sphereRadius = surfaceObjectTransform.lossyScale.GetMaxAbsComp() * 0.5f;

                            rootTransform.Align(radiusDir, snapConfig.AlignmentAxis);
                            OOBB hierarchyOOBB = ObjectBounds.CalcHierarchyWorldOOBB(root, boundsQConfig);
                            if (!hierarchyOOBB.IsValid) return new ObjectSnapResult();

                            BoxFace pivotFace = BoxMath.GetMostAlignedFace(hierarchyOOBB.Center, hierarchyOOBB.Size, hierarchyOOBB.Rotation, -radiusDir);
                            var collectedVerts = ObjectVertexCollect.CollectHierarchyVerts(root, pivotFace, collectBoxScale, collectEps);

                            Vector3 sitPoint = sphereCenter + radiusDir * sphereRadius;
                            Plane sitPlane = new Plane(radiusDir, sitPoint);
                            Vector3 sitOnPlaneOffset = Surface.CalculateSitOnSurfaceOffset(hierarchyOOBB, sitPlane, 0.0f);

                            rootTransform.position += sitOnPlaneOffset;
                            hierarchyOOBB.Center += sitOnPlaneOffset;
                            Vector3Ex.OffsetPoints(collectedVerts, sitOnPlaneOffset);

                            return new ObjectSnapResult(sitPlane, sitPoint);
                        }
                    }
                }
                else
                {
                    OOBB hierarchyOOBB = ObjectBounds.CalcHierarchyWorldOOBB(root, boundsQConfig);
                    if (!hierarchyOOBB.IsValid) return new ObjectSnapResult();

                    if (isSurfaceTerrain || (!isSurfaceSpherical && snapConfig.SurfaceType == Type.Mesh))
                    {
                        Ray ray = new Ray(hierarchyOOBB.Center, isSurfaceTerrain ? -Vector3.up : -snapConfig.SurfaceHitNormal);
                        GameObjectRayHit surfaceHit = raycaster.Raycast(ray);
                        if (surfaceHit != null)
                        {
                            Vector3 sitOnPlaneOffset = Surface.CalculateSitOnSurfaceOffset(hierarchyOOBB, surfaceHit.HitPlane, 0.0f);
                            rootTransform.position += sitOnPlaneOffset;
                            hierarchyOOBB.Center += sitOnPlaneOffset;

                            if (isSurfaceTerrain)
                            {
                                BoxFace pivotFace = BoxMath.GetMostAlignedFace(hierarchyOOBB.Center, hierarchyOOBB.Size, hierarchyOOBB.Rotation, -surfaceHit.HitNormal);
                                var collectedVerts = ObjectVertexCollect.CollectHierarchyVerts(root, pivotFace, collectBoxScale, collectEps);

                                Vector3 embedVector = Surface.CalculateEmbedVector(collectedVerts, snapConfig.SurfaceObject, -surfaceHit.HitNormal, snapConfig.SurfaceType);
                                rootTransform.position += (embedVector + surfaceHit.HitNormal * snapConfig.OffsetFromSurface);
                            }
                            return new ObjectSnapResult(surfaceHit.HitPlane, surfaceHit.HitPoint);
                        }
                    }
                    else
                    if (isSurfaceSpherical)
                    {
                        Transform surfaceObjectTransform = snapConfig.SurfaceObject.transform;
                        Vector3 sphereCenter = surfaceObjectTransform.position;
                        Vector3 radiusDir = (rootTransform.position - sphereCenter).normalized;
                        float sphereRadius = surfaceObjectTransform.lossyScale.GetMaxAbsComp() * 0.5f;

                        BoxFace pivotFace = BoxMath.GetMostAlignedFace(hierarchyOOBB.Center, hierarchyOOBB.Size, hierarchyOOBB.Rotation, -radiusDir);
                        var collectedVerts = ObjectVertexCollect.CollectHierarchyVerts(root, pivotFace, collectBoxScale, collectEps);

                        Vector3 sitPoint = sphereCenter + radiusDir * sphereRadius;
                        Plane sitPlane = new Plane(radiusDir, sitPoint);
                        Vector3 sitOnPlaneOffset = Surface.CalculateSitOnSurfaceOffset(hierarchyOOBB, sitPlane, 0.0f);
              
                        rootTransform.position += sitOnPlaneOffset;
                        hierarchyOOBB.Center += sitOnPlaneOffset;
                        Vector3Ex.OffsetPoints(collectedVerts, sitOnPlaneOffset);

                        return new ObjectSnapResult(sitPlane, sitPoint);
                    }
                }
            }          
            if (snapConfig.SurfaceType == Type.SceneGrid)
            {
                OOBB hierarchyOOBB = ObjectBounds.CalcHierarchyWorldOOBB(root, boundsQConfig);
                if (!hierarchyOOBB.IsValid) return new ObjectSnapResult();

                Transform rootTransform = root.transform;
                if (snapConfig.AlignAxis)
                {
                    rootTransform.Align(snapConfig.SurfaceHitNormal, snapConfig.AlignmentAxis);
                    hierarchyOOBB = ObjectBounds.CalcHierarchyWorldOOBB(root, boundsQConfig);
                }

                rootTransform.position += Surface.CalculateSitOnSurfaceOffset(hierarchyOOBB, snapConfig.SurfaceHitPlane, snapConfig.OffsetFromSurface);
                return new ObjectSnapResult(snapConfig.SurfaceHitPlane, snapConfig.SurfaceHitPlane.ProjectPoint(hierarchyOOBB.Center));
            }

            return new ObjectSnapResult();
        }

        public static Vector3 CalculateSitOnSurfaceOffset(OOBB oobb, Plane surfacePlane, float offsetFromSurface)
        {
            List<Vector3> oobbCorners = oobb.GetCornerPoints();
            int pivotPointIndex = surfacePlane.GetFurthestPtBehind(oobbCorners);
            if (pivotPointIndex < 0) pivotPointIndex = surfacePlane.GetClosestPtInFrontOrOnPlane(oobbCorners);

            if (pivotPointIndex >= 0)
            {
                Vector3 pivotPt = oobbCorners[pivotPointIndex];
                Vector3 prjPt = surfacePlane.ProjectPoint(pivotPt);
                return (prjPt - pivotPt) + surfacePlane.normal * offsetFromSurface;
            }

            return Vector3.zero;
        }

        public static Vector3 CalculateSitOnSurfaceOffset(AABB aabb, Plane surfacePlane, float offsetFromSurface)
        {
            List<Vector3> aabbCorners = aabb.GetCornerPoints();
            int pivotPointIndex = surfacePlane.GetFurthestPtBehind(aabbCorners);
            if (pivotPointIndex < 0) pivotPointIndex = surfacePlane.GetClosestPtInFrontOrOnPlane(aabbCorners);

            if (pivotPointIndex >= 0)
            {
                Vector3 pivotPt = aabbCorners[pivotPointIndex];
                Vector3 prjPt = surfacePlane.ProjectPoint(pivotPt);
                return (prjPt - pivotPt) + surfacePlane.normal * offsetFromSurface;
            }

            return Vector3.zero;
        }

        public static Vector3 CalculateEmbedVector(List<Vector3> embedPoints, GameObject embedSurface, Vector3 embedDirection, Type surfaceType)
        {
            var raycaster = CreateSurfaceRaycaster(surfaceType, embedSurface, false);

            bool needToMove = false;
            float maxDistSq = float.MinValue;
            GameObjectRayHit objectHit;
            foreach (var point in embedPoints)
            {
                Ray ray = new Ray(point, -embedDirection);
                objectHit = raycaster.Raycast(ray);
                if (objectHit != null) continue;

                ray = new Ray(point, embedDirection);
                objectHit = raycaster.Raycast(ray);
                if (objectHit != null)
                {
                    float distSq = (point - objectHit.HitPoint).sqrMagnitude;
                    if (distSq > maxDistSq)
                    {
                        maxDistSq = distSq;
                        needToMove = true;
                    }
                }
            }

            if (needToMove) return embedDirection * Mathf.Sqrt(maxDistSq);
            return Vector3.zero;
        }

        #region Surface raycasters
        private static SurfaceRaycaster CreateSurfaceRaycaster(Type surfaceType, GameObject surfaceObject, bool raycastReverse)
        {
            if (surfaceType == Type.Mesh || surfaceType == Type.TerrainMesh || surfaceType == Type.SphericalMesh) return new MeshSurfaceRaycaster(surfaceObject, raycastReverse);
            else if (surfaceType == Type.UnityTerrain) return new TerrainSurfaceRaycaster(surfaceObject, raycastReverse);
            return null;
        }

        private abstract class SurfaceRaycaster
        {
            protected GameObject _surfaceObject;
            protected bool _raycastReverse;

            public SurfaceRaycaster(GameObject surfaceObject, bool raycastReverse)
            {
                _surfaceObject = surfaceObject;
                _raycastReverse = raycastReverse;
            }

            public abstract GameObjectRayHit Raycast(Ray ray);
        }

        private class MeshSurfaceRaycaster : SurfaceRaycaster
        {
            public MeshSurfaceRaycaster(GameObject surfaceObject, bool raycastReverse)
                : base(surfaceObject, raycastReverse) { }

            public override GameObjectRayHit Raycast(Ray ray)
            {
                if (_raycastReverse) return EditorScene.Get.RaycastMeshObject(ray, _surfaceObject);
                return EditorScene.Get.RaycastMeshObject(ray, _surfaceObject);
            }
        }

        private class TerrainSurfaceRaycaster : SurfaceRaycaster
        {
            private TerrainCollider _terrainCollider;

            public TerrainSurfaceRaycaster(GameObject surfaceObject, bool raycastReverse)
                : base(surfaceObject, raycastReverse)
            {
                _terrainCollider = surfaceObject.GetComponent<TerrainCollider>();
            }

            public override GameObjectRayHit Raycast(Ray ray)
            {
                if (_raycastReverse) return EditorScene.Get.RaycastTerrainObjectReverseIfFail(ray, _surfaceObject);
                return EditorScene.Get.RaycastTerrainObject(ray, _surfaceObject, _terrainCollider);
            }
        }
        #endregion
    }
}
#endif