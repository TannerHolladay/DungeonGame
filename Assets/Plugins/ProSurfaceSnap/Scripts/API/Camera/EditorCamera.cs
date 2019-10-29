#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace PSS
{
    public static class EditorCamera
    {
        public static Camera Get { get { return SceneView.lastActiveSceneView.camera; } }
        public static Ray Pickray { get { return HandleUtility.GUIPointToWorldRay(Event.current.mousePosition); } }
    }
}
#endif