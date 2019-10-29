#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace PSS
{
    [Serializable]
    public abstract class Settings
    {
        [SerializeField]
        private bool _canBeDisplayed = true;
        [SerializeField]
        protected bool _isVisibleInGUI = true;
        private string _visibilityToggleLabel = "Settings";

        public bool CanBeDisplayed { get { return _canBeDisplayed; } set { _canBeDisplayed = value; } }
        public bool ToggleVisibility { get; set; }
        public string VisibilityToggleLabel { get { return _visibilityToggleLabel; } set { if (value != null) _visibilityToggleLabel = value; } }

        #if UNITY_EDITOR
        public void RenderEditorGUI(UnityEngine.Object undoRecordObject)
        {
            if (!CanBeDisplayed) return;

            if(ToggleVisibility)
            {
                _isVisibleInGUI = EditorGUILayout.Foldout(_isVisibleInGUI, VisibilityToggleLabel);
                if (_isVisibleInGUI) RenderContent(undoRecordObject);
            }
            else RenderContent(undoRecordObject);
        }

        protected abstract void RenderContent(UnityEngine.Object undoRecordObject);
        #endif
    }
}
#endif