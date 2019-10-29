#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace PSS
{
    public static class ObjectVertexCollect
    {
        public static List<Vector3> CollectModelSpriteVerts(Sprite sprite, AABB collectAABB)
        {
            var spriteModelVerts = sprite.vertices;
            var collectedVerts = new List<Vector3>(7);

            foreach(var vertPos in spriteModelVerts)
            {
                if(BoxMath.ContainsPoint(vertPos, collectAABB.Center, collectAABB.Size, Quaternion.identity))
                    collectedVerts.Add(vertPos);
            }

            return collectedVerts;
        }

        public static List<Vector3> CollectWorldSpriteVerts(Sprite sprite, Transform spriteTransform, OOBB collectOOBB)
        {
            var spriteWorldVerts = sprite.GetWorldVerts(spriteTransform);
            var collectedVerts = new List<Vector3>(7);

            foreach (var vertPos in spriteWorldVerts)
            {
                if (BoxMath.ContainsPoint(vertPos, collectOOBB.Center, collectOOBB.Size, collectOOBB.Rotation))
                    collectedVerts.Add(vertPos);
            }

            return collectedVerts;
        }

        public static List<Vector3> CollectHierarchyVerts(GameObject root, BoxFace collectFace, float collectBoxScale, float collectEps)
        {
            var meshObjects = root.GetMeshObjectsInHierarchy();
            var spriteObjects = root.GetSpriteObjectsInHierarchy();
            if (meshObjects.Count == 0 && spriteObjects.Count == 0) return new List<Vector3>();

            var boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.ObjectTypes = GameObjectType.Mesh | GameObjectType.Sprite;

            OOBB hierarchyOOBB = ObjectBounds.CalcHierarchyWorldOOBB(root, boundsQConfig);
            if (!hierarchyOOBB.IsValid) return new List<Vector3>();

            int faceAxisIndex = BoxMath.GetFaceAxisIndex(collectFace);
            Vector3 faceCenter = BoxMath.CalcBoxFaceCenter(hierarchyOOBB.Center, hierarchyOOBB.Size, hierarchyOOBB.Rotation, collectFace);
            Vector3 faceNormal = BoxMath.CalcBoxFaceNormal(hierarchyOOBB.Center, hierarchyOOBB.Size, hierarchyOOBB.Rotation, collectFace);

            float sizeEps = collectEps * 2.0f;
            Vector3 collectAABBSize = hierarchyOOBB.Size;
            collectAABBSize[faceAxisIndex] = (hierarchyOOBB.Size[faceAxisIndex] * collectBoxScale) + sizeEps;
            collectAABBSize[(faceAxisIndex + 1) % 3] += sizeEps;
            collectAABBSize[(faceAxisIndex + 2) % 3] += sizeEps;

            OOBB collectOOBB = new OOBB(faceCenter + faceNormal * (-collectAABBSize[faceAxisIndex] * 0.5f + collectEps), collectAABBSize);
            collectOOBB.Rotation = hierarchyOOBB.Rotation;
           
            var collectedVerts = new List<Vector3>(80);
            foreach(var meshObject in meshObjects)
            {
                Mesh mesh = meshObject.GetMesh();
                EditorMesh editorMesh = EditorMeshDb.Get.GetEditorMesh(mesh);
                if (editorMesh == null) continue;

                var verts = editorMesh.OverlapVerts(collectOOBB, meshObject.transform);
                if (verts.Count != 0) collectedVerts.AddRange(verts);
            }

            foreach (var spriteObject in spriteObjects)
            {
                var verts = CollectWorldSpriteVerts(spriteObject.GetSprite(), spriteObject.transform, collectOOBB);
                if (verts.Count != 0) collectedVerts.AddRange(verts);
            }

            return collectedVerts;
        }
    }
}
#endif