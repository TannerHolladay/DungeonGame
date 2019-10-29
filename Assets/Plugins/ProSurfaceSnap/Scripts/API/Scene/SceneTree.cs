#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PSS
{
    public class SceneTree
    {
        private class ObjectTransformData
        {
            public Vector3 WorldPosition;
            public Quaternion WorldRotation;
            public Vector3 WorldScale;

            public void Sync(Transform objectTransform)
            {
                WorldPosition = objectTransform.position;
                WorldRotation = objectTransform.rotation;
                WorldScale = objectTransform.lossyScale;
            }

            public bool NeedsSync(Transform objectTransform)
            {
                return objectTransform.position != WorldPosition ||
                       objectTransform.rotation != WorldRotation ||
                       objectTransform.lossyScale != WorldScale;
            }
        }

        private static readonly float _nullCleanupTargetTime = 1.0f;

        private float _elapsedNullCleanupTime = 0.0f;
        private SphereTree<GameObject> _objectTree = new SphereTree<GameObject>(2);
        private Dictionary<GameObject, SphereTreeNode<GameObject>> _objectToNode = new Dictionary<GameObject, SphereTreeNode<GameObject>>();
        //private Dictionary<GameObject, ObjectTransformData> _objectToTransformData = new Dictionary<GameObject, ObjectTransformData>();

        public SceneTree()
        {
            EditorApplication.hierarchyWindowChanged -= HierarchyWindowChanged;
            EditorApplication.hierarchyWindowChanged += HierarchyWindowChanged;
        }

        public void OnSceneGUIUpdate()
        {
            _elapsedNullCleanupTime += Time.deltaTime;
            if (_elapsedNullCleanupTime >= _nullCleanupTargetTime)
            {
                RemoveNodesWithNullObjects();
                EditorMeshDb.Get.RemoveNullMeshEntries();
                _elapsedNullCleanupTime = 0.0f;
            }

            if (_objectToNode.Count == 0) RegisterUnregisteredObjects();;
        }

        public void Update()
        {
            HandleTransformChanges();
        }

        public GameObjectRayHit RaycastMeshObject(Ray ray, GameObject gameObject)
        {
            EditorMesh editorMesh = EditorMeshDb.Get.GetEditorMesh(gameObject.GetMesh());
            if (editorMesh != null)
            {
                MeshRayHit meshRayHit = editorMesh.Raycast(ray, gameObject.transform.localToWorldMatrix);
                if (meshRayHit != null) return new GameObjectRayHit(ray, gameObject, meshRayHit);
            }
            else
            {
                // If no EditorMesh instance is available, we will cast a ray against
                // the object's MeshCollider as a last resort. This is actually useful
                // when dealing with static mesh objects. These objects' meshes have
                // their 'isReadable' flag set to false and can not be used to create
                // an EditorMesh instance. Thus a mesh collider is the next best choice.
                MeshCollider meshCollider = gameObject.GetComponent<MeshCollider>();
                if (meshCollider != null)
                {
                    RaycastHit rayHit;
                    if (meshCollider.Raycast(ray, out rayHit, float.MaxValue)) return new GameObjectRayHit(ray, rayHit);
                }
            }

            return null;
        }

        public GameObjectRayHit RaycastSpriteObject(Ray ray, GameObject gameObject)
        {
            float t;
            OOBB worldOOBB = ObjectBounds.CalcSpriteWorldOOBB(gameObject);
            if (!worldOOBB.IsValid) return null;

            if (BoxMath.Raycast(ray, out t, worldOOBB.Center, worldOOBB.Size, worldOOBB.Rotation)) 
                return new GameObjectRayHit(ray, gameObject, worldOOBB.GetPointFaceNormal(ray.GetPoint(t)), t);

            return null;
        }

        public List<GameObjectRayHit> RaycastAll(Ray ray, SceneRaycastPrecision raycastPresicion)
        {
            var nodeHits = _objectTree.RaycastAll(ray);
            if (nodeHits.Count == 0) return new List<GameObjectRayHit>();

            var boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.ObjectTypes = GameObjectTypeHelper.AllCombined;
            boundsQConfig.NoVolumeSize = EditorScene.Get.NoVolumeObjectSize;

            if (raycastPresicion == SceneRaycastPrecision.BestFit)
            {
                var hitList = new List<GameObjectRayHit>(10);
                foreach (var nodeHit in nodeHits)
                {
                    GameObject sceneObject = nodeHit.HitNode.Data;
                    if (sceneObject == null || !sceneObject.activeInHierarchy) continue;

                    Renderer renderer = sceneObject.GetComponent<Renderer>();
                    if (renderer != null && !renderer.isVisible) continue;

                    GameObjectType objectType = sceneObject.GetGameObjectType();
                    if (objectType == GameObjectType.Mesh)
                    {
                        GameObjectRayHit objectHit = RaycastMeshObject(ray, sceneObject);
                        if (objectHit != null) hitList.Add(objectHit);
                    }
                    else
                    if (objectType == GameObjectType.Terrain)
                    {
                        TerrainCollider terrainCollider = sceneObject.GetComponent<TerrainCollider>();
                        if(terrainCollider != null)
                        {
                            RaycastHit hitInfo;
                            if (terrainCollider.Raycast(ray, out hitInfo, float.MaxValue)) hitList.Add(new GameObjectRayHit(ray, hitInfo));
                        }
                    }
                    else
                    if(objectType == GameObjectType.Sprite)
                    {
                        GameObjectRayHit objectHit = RaycastSpriteObject(ray, sceneObject);
                        if (objectHit != null) hitList.Add(objectHit);
                    }
                    else
                    {
                        OOBB worldOOBB = ObjectBounds.CalcWorldOOBB(sceneObject, boundsQConfig);
                        if (worldOOBB.IsValid)
                        {
                            float t;
                            if (BoxMath.Raycast(ray, out t, worldOOBB.Center, worldOOBB.Size, worldOOBB.Rotation))
                            {
                                var faceDesc = BoxMath.GetFaceClosestToPoint(ray.GetPoint(t), worldOOBB.Center, worldOOBB.Size, worldOOBB.Rotation);
                                var hit = new GameObjectRayHit(ray, sceneObject, faceDesc.Plane.normal, t);
                                hitList.Add(hit);
                            }
                        }
                    }
                }

                return hitList;
            }
            else
            if (raycastPresicion == SceneRaycastPrecision.Box)
            {
                var hitList = new List<GameObjectRayHit>(10);
                foreach (var nodeHit in nodeHits)
                {
                    GameObject sceneObject = nodeHit.HitNode.Data;
                    if (sceneObject == null || !sceneObject.activeInHierarchy) continue;

                    Renderer renderer = sceneObject.GetComponent<Renderer>();
                    if (renderer != null && !renderer.isVisible) continue;

                    OOBB worldOOBB = ObjectBounds.CalcWorldOOBB(sceneObject, boundsQConfig);
                    if (worldOOBB.IsValid)
                    {
                        float t;
                        if (BoxMath.Raycast(ray, out t, worldOOBB.Center, worldOOBB.Size, worldOOBB.Rotation))
                        {
                            var faceDesc = BoxMath.GetFaceClosestToPoint(ray.GetPoint(t), worldOOBB.Center, worldOOBB.Size, worldOOBB.Rotation);
                            var hit = new GameObjectRayHit(ray, sceneObject, faceDesc.Plane.normal, t);
                            hitList.Add(hit);
                        }
                    }
                }

                return hitList;
            }
            
            return new List<GameObjectRayHit>();
        }

        public List<GameObject> OverlapBox(OOBB oobb)
        {
            List<SphereTreeNode<GameObject>> overlappedNodes = _objectTree.OverlapBox(oobb);
            if (overlappedNodes.Count == 0) return new List<GameObject>();

            var boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.ObjectTypes = GameObjectTypeHelper.AllCombined;
            boundsQConfig.NoVolumeSize = EditorScene.Get.NoVolumeObjectSize;

            var overlappedObjects = new List<GameObject>();
            foreach (SphereTreeNode<GameObject> node in overlappedNodes)
            {
                GameObject sceneObject = (GameObject)node.Data;
                if (sceneObject == null || !sceneObject.activeInHierarchy) continue;

                OOBB worldOOBB = ObjectBounds.CalcWorldOOBB(sceneObject, boundsQConfig);
                if (oobb.IntersectsOOBB(worldOOBB)) overlappedObjects.Add(sceneObject);
            }

            return overlappedObjects;
        }

        private bool IsObjectRegistered(GameObject gameObject)
        {
            return _objectToNode.ContainsKey(gameObject);
        }

        private bool CanRegisterObject(GameObject gameObject)
        {
            if (gameObject == null || IsObjectRegistered(gameObject)) return false;
            if (gameObject.GetComponent<RectTransform>() != null) return false;

            return true;
        }

        private void RegisterObject(GameObject gameObject)
        {
            var boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.ObjectTypes = GameObjectTypeHelper.AllCombined;
            boundsQConfig.NoVolumeSize = EditorScene.Get.NoVolumeObjectSize;

            AABB worldAABB = ObjectBounds.CalcWorldAABB(gameObject, boundsQConfig);
            Sphere worldSphere = new Sphere(worldAABB);

            SphereTreeNode<GameObject> objectNode = _objectTree.AddNode(gameObject, worldSphere);
            _objectToNode.Add(gameObject, objectNode);

            var objectTransformData = new ObjectTransformData();
            objectTransformData.Sync(gameObject.transform);
            //_objectToTransformData.Add(gameObject, objectTransformData);
        }

        private void OnObjectTransformChanged(Transform objectTransform)
        {
            var boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.ObjectTypes = GameObjectTypeHelper.AllCombined;
            boundsQConfig.NoVolumeSize = EditorScene.Get.NoVolumeObjectSize;

            AABB worldAABB = ObjectBounds.CalcWorldAABB(objectTransform.gameObject, boundsQConfig);
            Sphere worldSphere = new Sphere(worldAABB);

            SphereTreeNode<GameObject> objectNode = _objectToNode[objectTransform.gameObject];
            objectNode.Sphere = worldSphere;

            _objectTree.OnNodeSphereUpdated(objectNode);
        }

        private void RemoveNodesWithNullObjects()
        {
            var newObjectToNodeDictionary = new Dictionary<GameObject, SphereTreeNode<GameObject>>();
            foreach (var pair in _objectToNode)
            {
                if (pair.Key == null)  _objectTree.RemoveNode(pair.Value);
                else newObjectToNodeDictionary.Add(pair.Key, pair.Value);
            }

            _objectToNode.Clear();
            _objectToNode = newObjectToNodeDictionary;
        }

        private void RegisterUnregisteredObjects()
        {
            GameObject[] sceneObjects = EditorScene.Get.GetSceneObjects();
            foreach (var sceneObject in sceneObjects)
            {
                if (!IsObjectRegistered(sceneObject) && 
                    CanRegisterObject(sceneObject)) RegisterObject(sceneObject);
            }
        }

        private void HandleTransformChanges()
        {
            foreach (var pair in _objectToNode)
            {
                GameObject sceneObject = pair.Key;
                if (sceneObject == null) continue;

                Transform objectTransform = sceneObject.transform;
                if (objectTransform.hasChanged)
                {
                    OnObjectTransformChanged(objectTransform);
                    objectTransform.hasChanged = false;
                }
            }
        }

        private void HierarchyWindowChanged()
        {
            RegisterUnregisteredObjects();
        }
    }
}
#endif