#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace PSS
{
    public static class SelectionEx
    {
        public static List<GameObject> GetSelectedObjects()
        {
            var selectedTransforms = Selection.transforms;
            var seletedObjects = new List<GameObject>(50);

            foreach (var transform in selectedTransforms)
                seletedObjects.Add(transform.gameObject);

            return seletedObjects;
        }
    }
}
#endif