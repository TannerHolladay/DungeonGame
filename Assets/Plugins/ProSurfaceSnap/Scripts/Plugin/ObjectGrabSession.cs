#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PSS
{
    public class ObjectGrabSession
    {
        private enum State
        {
            Inactive = 0,
            ActiveSnapToSurface,
            ActiveRotate,
            ActiveRotateAroundAnchor,
            ActiveScale,
            ActiveOffsetFromSurface,
            ActiveAnchorAdjust,
            ActiveOffsetFromAnchor
        }

        public enum GrabSurfaceType
        {
            Invalid = 0,
            Mesh,
            SphericalMesh,
            UnityTerrain,
            TerrainMesh,
            Grid
        }

        private struct GrabSurfaceInfo
        {
            public GrabSurfaceType SurfaceType;
            public Vector3 AnchorPoint;
            public Vector3 AnchorNormal;
            public Plane AnchorPlane;
            public SceneRaycastHit SceneRaycastHit;
        }

        private class GrabTarget
        {
            private GameObject _gameObject;
            private Transform _transform;

            public GameObject GameObject { get { return _gameObject; } }
            public Transform Transform { get { return _transform; } }
            public Vector3 AnchorVector;
            public Vector3 WorldScaleSnapshot;
            public Vector3 AnchorVectorSnapshot;
            public Plane SittingPlane;
            public Vector3 SittingPoint;
            public float OffsetFromSurface;

            public GrabTarget(GameObject parentObject)
            {
                _gameObject = parentObject;
                _transform = _gameObject.transform;
            }
        }

        private Tool _activeToolRestore;

        private State _state = State.Inactive;

        private ObjectGrabSettings _sharedSettings;
        private ObjectGrabHotkeys _sharedHotkeys;
        private ObjectGrabLookAndFeel _sharedLookAndFeel;

        private List<GameObject> _targetParents = new List<GameObject>();
        private List<GrabTarget> _grabTargets = new List<GrabTarget>();
        private GrabSurfaceInfo _grabSurfaceInfo = new GrabSurfaceInfo();
        private int _deltaCaptureId;

        public ObjectGrabSettings SharedSettings { get { return _sharedSettings; } set { if (value != null) _sharedSettings = value; } }
        public ObjectGrabHotkeys SharedHotkeys { get { return _sharedHotkeys; } set { if (value != null) _sharedHotkeys = value; } }
        public ObjectGrabLookAndFeel SharedLookAndFeel { get { return _sharedLookAndFeel; } set { if (value != null) _sharedLookAndFeel = value; } }
        public bool IsActive { get { return _state != State.Inactive; } }

        public void OnSceneGUIRender()
        {
            if (SharedLookAndFeel == null) return;

            if (IsActive && _grabSurfaceInfo.SurfaceType != GrabSurfaceType.Invalid)
            {
                if (SharedLookAndFeel.ShowAnchorLines)
                {                    
                    Color oldColor = Handles.color;
                    var linePoints = new List<Vector3>(_grabTargets.Count * 2);
                    Handles.color = SharedLookAndFeel.AnchorLineTickColor;
                    foreach (var grabTarget in _grabTargets)
                    {
                        linePoints.Add(grabTarget.Transform.position);
                        linePoints.Add(_grabSurfaceInfo.AnchorPoint);

                        if (SharedLookAndFeel.ShowAnchorLineTicks)
                        {
                            float dotSize = SharedLookAndFeel.AnchorLineTickSize * HandleUtility.GetHandleSize(grabTarget.Transform.position);
                            Handles.DotHandleCap(0, grabTarget.Transform.position, Quaternion.identity, dotSize, EventType.Repaint);
                        }
                    }

                    if (SharedLookAndFeel.ShowAnchorLineTicks)
                    {
                        float dotAnchPtSize = SharedLookAndFeel.AnchorLineTickSize * HandleUtility.GetHandleSize(_grabSurfaceInfo.AnchorPoint);
                        Handles.DotHandleCap(0, _grabSurfaceInfo.AnchorPoint, Quaternion.identity, dotAnchPtSize, EventType.Repaint);
                    }

                    Handles.color = SharedLookAndFeel.AnchorLineColor;
                    Handles.DrawLines(linePoints.ToArray());              
                    Handles.color = oldColor;
                }

                if (SharedLookAndFeel.ShowTargetBoxes)
                {
                    Color oldColor = Handles.color;
                    Matrix4x4 oldMatrix = Handles.matrix;
                    Handles.color = SharedLookAndFeel.TargetWireBoxColor;
                    var boundsQConfig = GetBoundsQConfig();
                    foreach (var grabTarget in _grabTargets)
                    {
                        OOBB targetOOBB = ObjectBounds.CalcHierarchyWorldOOBB(grabTarget.GameObject, boundsQConfig);
                        if (targetOOBB.IsValid)
                        {
                            Handles.matrix = Matrix4x4.TRS(targetOOBB.Center, targetOOBB.Rotation, Vector3.one);
                            Handles.DrawWireCube(Vector3.zero, targetOOBB.Size);
                        }
                    }

                    Handles.color = oldColor;
                    Handles.matrix = oldMatrix;
                }
            }
        }

        public void OnSceneGUIUpdate()
        {
            if (_sharedHotkeys == null || _sharedSettings == null) return;

            if (IsActive)
            {
                var deviceManager = InputDeviceManager.Get;
                State oldState = _state;
                if (SharedHotkeys.EnableOffsetFromAnchor.IsActive())
                {
                    if (_state != State.ActiveOffsetFromAnchor &&
                        deviceManager.CreateMouseDeltaCapture(deviceManager.MousePos, out _deltaCaptureId))
                    {
                        StoreGrabTargetsAnchorVectorSnapshots();
                        _state = State.ActiveOffsetFromAnchor;
                    }
                }
                else if (SharedHotkeys.EnableAnchorAdjust.IsActive()) _state = State.ActiveAnchorAdjust;
                else if (SharedHotkeys.EnableRotation.IsActive()) _state = State.ActiveRotate;
                else if (SharedHotkeys.EnableRotationAroundAnchor.IsActive()) _state = State.ActiveRotateAroundAnchor;
                else if (SharedHotkeys.EnableScaling.IsActive())
                {
                    if (_state != State.ActiveScale &&
                        deviceManager.CreateMouseDeltaCapture(deviceManager.MousePos, out _deltaCaptureId))
                    {
                        StoreGrabTargetsWorldScaleSnapshots();
                        _state = State.ActiveScale;
                    }
                }
                else if (SharedHotkeys.EnableOffsetFromSurface.IsActive()) _state = State.ActiveOffsetFromSurface;
                else _state = State.ActiveSnapToSurface;

                if (_state != State.ActiveScale &&
                    _state != State.ActiveOffsetFromAnchor) deviceManager.RemoveMouseDeltaCapture(_deltaCaptureId);

                if (_state == State.ActiveSnapToSurface)
                {
                    TransformAxis currentAxis = SharedSettings.AlignmentAxis;
                    if (_sharedHotkeys.SwitchToXAlignmentAxis.IsActive())
                    {
                        if (currentAxis == TransformAxis.PositiveX) SharedSettings.AlignmentAxis = TransformAxis.NegativeX;
                        else SharedSettings.AlignmentAxis = TransformAxis.PositiveX;
                    }
                    else if (_sharedHotkeys.SwitchToYAlignmentAxis.IsActive())
                    {
                        if (currentAxis == TransformAxis.PositiveY) SharedSettings.AlignmentAxis = TransformAxis.NegativeY;
                        else SharedSettings.AlignmentAxis = TransformAxis.PositiveY;
                    }
                    else if (_sharedHotkeys.SwitchToZAlignmentAxis.IsActive())
                    {
                        if (currentAxis == TransformAxis.PositiveZ) SharedSettings.AlignmentAxis = TransformAxis.NegativeZ;
                        else SharedSettings.AlignmentAxis = TransformAxis.PositiveZ;
                    }
                }

                if (_state != State.ActiveOffsetFromAnchor && 
                    _state != State.ActiveRotateAroundAnchor)
                {
                    if (!IdentifyGrabSurface()) return;
                }

                if ((oldState == State.ActiveOffsetFromAnchor && _state != State.ActiveOffsetFromAnchor) ||
                    (oldState == State.ActiveRotateAroundAnchor && _state != State.ActiveRotateAroundAnchor))
                {
                    CalculateGrabTargetsAnchorVectors();
                }

                if (InputDeviceManager.Get.MouseDelta.magnitude > 0.0f)
                {
                    if (_state == State.ActiveOffsetFromAnchor) OffsetTargetsFromAnchor();
                    else if (_state == State.ActiveAnchorAdjust) CalculateGrabTargetsAnchorVectors();
                    else if (_state == State.ActiveSnapToSurface) SnapTargetsToSurface();
                    else if (_state == State.ActiveRotate) RotateTargets();
                    else if (_state == State.ActiveRotateAroundAnchor)  RotateTargetsAroundAnchor();
                    else if (_state == State.ActiveScale) ScaleTargets();
                    else if (_state == State.ActiveOffsetFromSurface) OffsetTargetsFromSurface();
                }
            }
        }

        public bool Begin(IEnumerable<GameObject> targetObjects)
        {
            if (_state != State.Inactive || _sharedSettings == null || _sharedHotkeys == null || targetObjects == null) return false;
            if ((int)SharedSettings.SurfaceFlags == 0) return false;

            if (!IdentifyGrabTargets(targetObjects)) return false;
            if (!IdentifyGrabSurface())
            {
                _grabTargets.Clear();
                return false;
            }
            CalculateGrabTargetsAnchorVectors();

            _state = State.ActiveSnapToSurface;

            SnapTargetsToSurface();
            CalculateGrabTargetsAnchorVectors();
            _activeToolRestore = Tools.current;

            return true;
        }

        public void End()
        {
            if (_state == State.Inactive) return;
            
            _grabTargets.Clear();
            _state = State.Inactive;
            _grabSurfaceInfo.SurfaceType = GrabSurfaceType.Invalid;

            _targetParents.Clear();
            Tools.current = _activeToolRestore;
        }

        private void SnapTargetsToSurface()
        {
            if (_grabSurfaceInfo.SurfaceType == GrabSurfaceType.Invalid) return;

            Surface.ObjectSnapConfig snapConfig = new Surface.ObjectSnapConfig();
            snapConfig.SurfaceHitPoint = _grabSurfaceInfo.AnchorPoint;
            snapConfig.SurfaceHitNormal = _grabSurfaceInfo.AnchorNormal;
            snapConfig.SurfaceHitPlane = _grabSurfaceInfo.AnchorPlane;
            snapConfig.SurfaceObject = _grabSurfaceInfo.SceneRaycastHit.WasAnObjectHit ? _grabSurfaceInfo.SceneRaycastHit.ObjectHit.HitObject : null;

            snapConfig.SurfaceType = Surface.Type.UnityTerrain;
            if (_grabSurfaceInfo.SurfaceType == GrabSurfaceType.Mesh) snapConfig.SurfaceType = Surface.Type.Mesh;
            else if (_grabSurfaceInfo.SurfaceType == GrabSurfaceType.Grid) snapConfig.SurfaceType = Surface.Type.SceneGrid;
            else if (_grabSurfaceInfo.SurfaceType == GrabSurfaceType.SphericalMesh) snapConfig.SurfaceType = Surface.Type.SphericalMesh;
            else if (_grabSurfaceInfo.SurfaceType == GrabSurfaceType.TerrainMesh) snapConfig.SurfaceType = Surface.Type.TerrainMesh;

            EditorUndoEx.RecordObjectTransforms(_targetParents);
            foreach (var grabTarget in _grabTargets)
            {
                if (grabTarget.GameObject == null) continue;
                grabTarget.Transform.position = _grabSurfaceInfo.AnchorPoint + grabTarget.AnchorVector;

                var layerGrabSettings = SharedSettings.GetLayerGrabSettings(grabTarget.GameObject.layer);
                if (layerGrabSettings.IsActive)
                {
                    snapConfig.AlignAxis = layerGrabSettings.AlignAxis;
                    snapConfig.AlignmentAxis = layerGrabSettings.AlignmentAxis;
                    snapConfig.OffsetFromSurface = layerGrabSettings.DefaultOffsetFromSurface + grabTarget.OffsetFromSurface;
                }
                else
                {
                    snapConfig.AlignAxis = SharedSettings.AlignAxis;
                    snapConfig.AlignmentAxis = SharedSettings.AlignmentAxis;
                    snapConfig.OffsetFromSurface = SharedSettings.DefaultOffsetFromSurface + grabTarget.OffsetFromSurface;
                }

                var snapResult = Surface.SnapHierarchy(grabTarget.GameObject, snapConfig);
                if (snapResult.WasSnapped)
                {
                    grabTarget.SittingPlane = snapResult.SittingPlane;
                    grabTarget.SittingPoint = snapResult.SittingPoint;
                }
            }
        }

        private void RotateTargets()
        {
            var boundsQConfig = GetBoundsQConfig();
            float rotationAmount = InputDeviceManager.Get.MouseDelta.x * SharedSettings.RotationSensitivity;

            EditorUndoEx.RecordObjectTransforms(_targetParents);
            foreach (var grabTarget in _grabTargets)
            {
                if (grabTarget == null) continue;

                OOBB targetOOBB = ObjectBounds.CalcHierarchyWorldOOBB(grabTarget.GameObject, boundsQConfig);
                if (!targetOOBB.IsValid) continue;

                var layerGrabSettings = SharedSettings.GetLayerGrabSettings(grabTarget.GameObject.layer);
                if (layerGrabSettings.IsActive)
                {
                    Quaternion rotation = Quaternion.AngleAxis(rotationAmount, layerGrabSettings.AlignAxis ? grabTarget.SittingPlane.normal : Vector3.up);
                    grabTarget.Transform.RotateAroundPivot(rotation, targetOOBB.Center);
                }
                else
                {
                    Quaternion rotation = Quaternion.AngleAxis(rotationAmount, SharedSettings.AlignAxis ? grabTarget.SittingPlane.normal : Vector3.up);
                    grabTarget.Transform.RotateAroundPivot(rotation, targetOOBB.Center);
                }
            }

            CalculateGrabTargetsAnchorVectors();
        }

        private void RotateTargetsAroundAnchor()
        {
            EditorUndoEx.RecordObjectTransforms(_targetParents);
            float rotationAmount = InputDeviceManager.Get.MouseDelta.x * SharedSettings.RotationSensitivity;
            foreach(var grabTarget in _grabTargets)
            {
                if (grabTarget == null) continue;

                var layerGrabSettings = SharedSettings.GetLayerGrabSettings(grabTarget.GameObject.layer);
                if (layerGrabSettings.IsActive)
                {
                    if (layerGrabSettings.AlignAxis)
                    {
                        Quaternion rotation = Quaternion.AngleAxis(rotationAmount, _grabSurfaceInfo.AnchorNormal);
                        grabTarget.Transform.RotateAroundPivot(rotation, _grabSurfaceInfo.AnchorPoint);
                        grabTarget.AnchorVector = rotation * grabTarget.AnchorVector;
                    }
                    else
                    {
                        Quaternion rotation = Quaternion.AngleAxis(rotationAmount, Vector3.up);
                        grabTarget.Transform.RotateAroundPivot(rotation, _grabSurfaceInfo.AnchorPoint);
                        grabTarget.AnchorVector = rotation * grabTarget.AnchorVector;
                    }
                }
                else
                {
                    if (SharedSettings.AlignAxis)
                    {
                        Quaternion rotation = Quaternion.AngleAxis(rotationAmount, _grabSurfaceInfo.AnchorNormal);
                        grabTarget.Transform.RotateAroundPivot(rotation, _grabSurfaceInfo.AnchorPoint);
                        grabTarget.AnchorVector = rotation * grabTarget.AnchorVector;
                    }
                    else
                    {
                        Quaternion rotation = Quaternion.AngleAxis(rotationAmount, Vector3.up);
                        grabTarget.Transform.RotateAroundPivot(rotation, _grabSurfaceInfo.AnchorPoint);
                        grabTarget.AnchorVector = rotation * grabTarget.AnchorVector;
                    }
                }
            }

            SnapTargetsToSurface();
        }

        private void ScaleTargets()
        {
            EditorUndoEx.RecordObjectTransforms(_targetParents);
            foreach (var grabTarget in _grabTargets)
            {
                if (grabTarget == null) continue;

                //float maxAbsScaleComp = grabTarget.WorldScaleSnapshot.GetMaxAbsComp();
                //float scaleFactor = 1.0f + inputDevice.GetCaptureDelta(_inputDevScaleDeltaCaptureId).x * (Settings.ScaleDeviceSensitivity / maxAbsScaleComp);
                float scaleFactor = 1.0f + InputDeviceManager.Get.GetMouseCaptureDelta(_deltaCaptureId).x * SharedSettings.ScaleSensitivity;

                Vector3 newScale = grabTarget.WorldScaleSnapshot * scaleFactor;
                grabTarget.GameObject.SetHierarchyWorldScaleByPivot(newScale, grabTarget.SittingPoint + grabTarget.SittingPlane.normal * grabTarget.OffsetFromSurface);
            }

            CalculateGrabTargetsAnchorVectors();
            //SnapTargetsToSurface();
        }

        private void OffsetTargetsFromSurface()
        {
            float offsetAmount = InputDeviceManager.Get.MouseDelta.x * SharedSettings.OffsetFromSurfaceSensitivity;
            foreach (var grabTarget in _grabTargets)
            {
                if (grabTarget == null) continue;

                var layerGrabSettings = SharedSettings.GetLayerGrabSettings(grabTarget.GameObject.layer);
                if (layerGrabSettings.IsActive)
                {
                    if (layerGrabSettings.AlignAxis)
                    {
                        grabTarget.Transform.position += grabTarget.SittingPlane.normal * offsetAmount;
                        grabTarget.OffsetFromSurface += offsetAmount;
                    }
                    else
                    {
                        grabTarget.Transform.position += Vector3.up * offsetAmount;
                        grabTarget.OffsetFromSurface += offsetAmount;
                    }
                }
                else
                {
                    if (SharedSettings.AlignAxis)
                    {
                        grabTarget.Transform.position += grabTarget.SittingPlane.normal * offsetAmount;
                        grabTarget.OffsetFromSurface += offsetAmount;
                    }
                    else
                    {
                        grabTarget.Transform.position += Vector3.up * offsetAmount;
                        grabTarget.OffsetFromSurface += offsetAmount;
                    }
                }
            }

            CalculateGrabTargetsAnchorVectors();
        }

        private void OffsetTargetsFromAnchor()
        {
            EditorUndoEx.RecordObjectTransforms(_targetParents);
            float scaleFactor = 1.0f + InputDeviceManager.Get.GetMouseCaptureDelta(_deltaCaptureId).x * SharedSettings.OffsetFromAnchorSensitivity;
            foreach (var grabTarget in _grabTargets)
            {
                if (grabTarget == null) continue;
                grabTarget.Transform.position = (_grabSurfaceInfo.AnchorPoint + grabTarget.AnchorVectorSnapshot * scaleFactor);
            }

            CalculateGrabTargetsAnchorVectors();
            SnapTargetsToSurface();
        }

        private bool IdentifyGrabTargets(IEnumerable<GameObject> targetObjects)
        {
            _targetParents = GameObjectEx.FilterParentsOnly(targetObjects);
            if (_targetParents == null || _targetParents.Count == 0) return false;

            _grabTargets.Clear();
            foreach (var targetObject in _targetParents)
            {
                if (targetObject.HierarchyHasObjectsOfType(GameObjectType.Terrain)) return false;
                if (!targetObject.HierarchyHasObjectsOfType(GameObjectType.Mesh | GameObjectType.Sprite)) return false;

                _grabTargets.Add(new GrabTarget(targetObject));
            }

            return _grabTargets.Count != 0;
        }

        private void CalculateGrabTargetsAnchorVectors()
        {
            foreach (var grabTarget in _grabTargets)
                grabTarget.AnchorVector = grabTarget.Transform.position - _grabSurfaceInfo.AnchorPoint;
        }

        private void StoreGrabTargetsWorldScaleSnapshots()
        {
            foreach (var grabTarget in _grabTargets)
                grabTarget.WorldScaleSnapshot = grabTarget.Transform.lossyScale;
        }

        private void StoreGrabTargetsAnchorVectorSnapshots()
        {
            foreach (var grabTarget in _grabTargets)
                grabTarget.AnchorVectorSnapshot = grabTarget.AnchorVector;
        }

        private bool IdentifyGrabSurface()
        {
            _grabSurfaceInfo.SurfaceType = GrabSurfaceType.Invalid;

            SceneRaycastFilter raycastFilter = new SceneRaycastFilter();
            raycastFilter.LayerMask = SharedSettings.SurfaceLayers;
            if ((SharedSettings.SurfaceFlags & ObjectGrabSurfaceFlags.Mesh) != 0) raycastFilter.AllowedObjectTypes.Add(GameObjectType.Mesh);
            if ((SharedSettings.SurfaceFlags & ObjectGrabSurfaceFlags.Terrain) != 0) raycastFilter.AllowedObjectTypes.Add(GameObjectType.Terrain);
            foreach (var grabTarget in _grabTargets)
                raycastFilter.IgnoreObjects.AddRange(grabTarget.GameObject.GetAllChildrenAndSelf());

            SceneRaycastHit raycastHit = EditorScene.Get.Raycast(EditorCamera.Pickray, SceneRaycastPrecision.BestFit, raycastFilter);
            if (!raycastHit.WasAnythingHit) return false;

            _grabSurfaceInfo.SceneRaycastHit = raycastHit;
            if (raycastHit.WasAnObjectHit)
            {
                _grabSurfaceInfo.AnchorNormal = raycastHit.ObjectHit.HitNormal;
                _grabSurfaceInfo.AnchorPoint = raycastHit.ObjectHit.HitPoint;
                _grabSurfaceInfo.AnchorPlane = raycastHit.ObjectHit.HitPlane;
         
                GameObjectType hitObjectType = raycastHit.ObjectHit.HitObject.GetGameObjectType();
                if (hitObjectType == GameObjectType.Mesh)
                {
                    _grabSurfaceInfo.SurfaceType = GrabSurfaceType.Mesh;

                    int objectLayer = raycastHit.ObjectHit.HitObject.layer;
                    if (LayerEx.IsLayerBitSet(SharedSettings.SphericalMeshLayers, objectLayer)) _grabSurfaceInfo.SurfaceType = GrabSurfaceType.SphericalMesh;
                    else if (LayerEx.IsLayerBitSet(SharedSettings.TerrainMeshLayers, objectLayer)) _grabSurfaceInfo.SurfaceType = GrabSurfaceType.TerrainMesh;
                }
                else _grabSurfaceInfo.SurfaceType = GrabSurfaceType.UnityTerrain;
            }
            else
            if (raycastHit.WasGridHit && (SharedSettings.SurfaceFlags & ObjectGrabSurfaceFlags.Grid) != 0)
            {
                _grabSurfaceInfo.AnchorNormal = raycastHit.GridHit.HitNormal;
                _grabSurfaceInfo.AnchorPoint = raycastHit.GridHit.HitPoint;
                _grabSurfaceInfo.AnchorPlane = raycastHit.GridHit.HitPlane;
                _grabSurfaceInfo.SurfaceType = GrabSurfaceType.Grid;
            }

            return true;
        }

        private ObjectBounds.QueryConfig GetBoundsQConfig()
        {
            var boundsQConfig = new ObjectBounds.QueryConfig();
            boundsQConfig.ObjectTypes = GameObjectType.Mesh | GameObjectType.Sprite;
            boundsQConfig.NoVolumeSize = Vector3.zero;

            return boundsQConfig;
        }
    }
}
#endif