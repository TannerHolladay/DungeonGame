#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace PSS
{
    public class EditorMeshDb : Singleton<EditorMeshDb>
    {
        private Dictionary<Mesh, EditorMesh> _meshes = new Dictionary<Mesh, EditorMesh>();

        public bool Contains(EditorMesh editorMesh)
        {
            if (editorMesh == null) return false;
            return _meshes.ContainsKey(editorMesh.UnityMesh);
        }

        public bool Contains(Mesh unityMesh)
        {
            if (unityMesh == null) return false;
            return _meshes.ContainsKey(unityMesh);
        }

        public EditorMesh GetEditorMesh(Mesh unityMesh)
        {
            if (unityMesh == null) return null;
            
            if (!_meshes.ContainsKey(unityMesh)) return CreateEditorMesh(unityMesh);
            else return _meshes[unityMesh];
        }

        public void RemoveNullMeshEntries()
        {
            var newMeshDictionary = new Dictionary<Mesh, EditorMesh>();
            foreach (var pair in _meshes)
            {
                if (pair.Key != null) newMeshDictionary.Add(pair.Key, pair.Value);
            }

            _meshes.Clear();
            _meshes = newMeshDictionary;
        }

        private EditorMesh CreateEditorMesh(Mesh unityMesh)
        {
            EditorMesh editorMesh = EditorMesh.Create(unityMesh);
            if (editorMesh != null)
            {
                _meshes.Add(unityMesh, editorMesh);
                return editorMesh;
            }
            else return null;
        }
    }
}
#endif