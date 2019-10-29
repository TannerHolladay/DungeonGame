#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PSS
{
    public static class EditorUndoEx
    {
        private static string UndoActionName = "PSS";

        public static void Record(UnityEngine.Object objectToRecord)
        {
            if (!Application.isPlaying) Undo.RecordObject(objectToRecord, UndoActionName);
        }

        public static void RecordObjectTransforms(IEnumerable<GameObject> gameObjects)
        {
            var transforms = GameObjectEx.GetObjectTransforms(gameObjects);
            RecordTransforms(transforms);
        }

        public static void RecordTransforms(List<Transform> transforms)
        {
            foreach(var transform in transforms)
                Undo.RecordObject(transform, UndoActionName);
        }
    }
}
#endif