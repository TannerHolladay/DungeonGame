#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

namespace PSS
{
    public enum SceneRaycastPrecision
    {
        /// <summary>
        /// If the object has a mesh, the raycast will be performed against the object 
        /// mesh surface. If the object doesn't have a mesh, but it has a terrain with 
        /// a terrain collider, it will be performed against the terrain collider. If
        /// none of these are available, the raycast will be performed against the object's 
        /// volume/box.
        /// </summary>
        BestFit = 0,
        /// <summary>
        /// The raycast will always be performed against the object's volume/box.
        /// </summary>
        Box,
    }

    public class SceneRaycastFilter
    {
        private List<GameObjectType> _allowedObjectTypes = new List<GameObjectType>();
        private List<GameObject> _ignoreObjects = new List<GameObject>();
        private int _layerMask = ~0;

        public List<GameObjectType> AllowedObjectTypes { get { return _allowedObjectTypes; } }
        public List<GameObject> IgnoreObjects { get { return _ignoreObjects; } }
        public int LayerMask { get { return _layerMask; } set { _layerMask = value; } }

        public void FilterHits(List<GameObjectRayHit> hits)
        {
            hits.RemoveAll(item => !AllowedObjectTypes.Contains(item.HitObject.GetGameObjectType()) || 
                           IgnoreObjects.Contains(item.HitObject) || !LayerEx.IsLayerBitSet(_layerMask, item.HitObject.layer));
        }
    }

    public class SceneRaycastHit
    {
        private GameObjectRayHit _objectHit;
        private XZGridRayHit _gridHit;

        public bool WasAnythingHit { get { return _objectHit != null || _gridHit != null; } }
        public bool WasAnObjectHit { get { return _objectHit != null; } }
        public bool WasGridHit { get { return _gridHit != null; } }
        public GameObjectRayHit ObjectHit { get { return _objectHit; } }
        public XZGridRayHit GridHit { get { return _gridHit; } }

        public SceneRaycastHit(GameObjectRayHit objectRayHit, XZGridRayHit gridRayHit)
        {
            _objectHit = objectRayHit;
            _gridHit = gridRayHit;
        }
    }

    public class EditorScene : Singleton<EditorScene>
    {
        private SceneTree _sceneTree = new SceneTree();

        public Vector3 NoVolumeObjectSize { get { return Vector3.zero; } }

        public GameObject[] GetSceneObjects()
        {
            return GameObject.FindObjectsOfType<GameObject>();
        }

        public List<GameObject> OverlapBox(OOBB oobb)
        {
            return _sceneTree.OverlapBox(oobb);
        }

        public SceneRaycastHit Raycast(Ray ray, SceneRaycastPrecision rtRaycastPrecision, SceneRaycastFilter raycastFilter)
        {
            List<GameObjectRayHit> allObjectHits = RaycastAllObjectsSorted(ray, rtRaycastPrecision, raycastFilter);
            GameObjectRayHit closestObjectHit = allObjectHits.Count != 0 ? allObjectHits[0] : null;
            XZGridRayHit gridRayHit = RaycastSceneGrid(ray);

            return new SceneRaycastHit(closestObjectHit, gridRayHit);
        }

        public List<GameObjectRayHit> RaycastAllObjects(Ray ray, SceneRaycastPrecision rtRaycastPrecision)
        {
            return _sceneTree.RaycastAll(ray, rtRaycastPrecision);
        }

        public List<GameObjectRayHit> RaycastAllObjectsSorted(Ray ray, SceneRaycastPrecision raycastPresicion)
        {
            List<GameObjectRayHit> allHits = RaycastAllObjects(ray, raycastPresicion);
            GameObjectRayHit.SortByHitDistance(allHits);
  
            return allHits;
        }

        public List<GameObjectRayHit> RaycastAllObjectsSorted(Ray ray, SceneRaycastPrecision rtRaycastPrecision, SceneRaycastFilter raycastFilter)
        {
            if (raycastFilter.AllowedObjectTypes.Count == 0) return new List<GameObjectRayHit>();

            List<GameObjectRayHit> sortedHits = RaycastAllObjectsSorted(ray, rtRaycastPrecision);
            raycastFilter.FilterHits(sortedHits);

            return sortedHits;
        }

        public GameObjectRayHit RaycastMeshObject(Ray ray, GameObject meshObject)
        {
            return _sceneTree.RaycastMeshObject(ray, meshObject);
        }

        public GameObjectRayHit RaycastMeshObjectReverseIfFail(Ray ray, GameObject meshObject)
        {
            GameObjectRayHit hit = RaycastMeshObject(ray, meshObject);
            if (hit == null) hit = RaycastMeshObject(new Ray(ray.origin, -ray.direction), meshObject);

            return hit;
        }

        public GameObjectRayHit RaycastSpriteObject(Ray ray, GameObject spriteObject)
        {
            return _sceneTree.RaycastSpriteObject(ray, spriteObject);
        }

        public GameObjectRayHit RaycastTerrainObject(Ray ray, GameObject terrainObject)
        {
            TerrainCollider terrainCollider = terrainObject.GetComponent<TerrainCollider>();
            if (terrainCollider == null) return null;

            RaycastHit rayHit;
            if (terrainCollider.Raycast(ray, out rayHit, float.MaxValue)) return new GameObjectRayHit(ray, rayHit);

            return null;
        }

        public GameObjectRayHit RaycastTerrainObject(Ray ray, GameObject terrainObject, TerrainCollider terrainCollider)
        {
            RaycastHit rayHit;
            if (terrainCollider.Raycast(ray, out rayHit, float.MaxValue)) return new GameObjectRayHit(ray, rayHit);

            return null;
        }

        public GameObjectRayHit RaycastTerrainObjectReverseIfFail(Ray ray, GameObject terrainObject)
        {
            GameObjectRayHit hit = RaycastTerrainObject(ray, terrainObject);
            if (hit == null) hit = RaycastTerrainObject(new Ray(ray.origin, -ray.direction), terrainObject);

            return hit;
        }

        public XZGridRayHit RaycastSceneGrid(Ray ray)
        {
            float t;
            Plane gridPlane = new Plane(Vector3.up, Vector3.zero);
            if (gridPlane.Raycast(ray, out t)) return new XZGridRayHit(ray, gridPlane, t);

            return null;
        }

        public void OnSceneGUIUpdate()
        {
            _sceneTree.OnSceneGUIUpdate();
        }

        public void Update()
        {
            _sceneTree.Update();
        }
    }
}
#endif