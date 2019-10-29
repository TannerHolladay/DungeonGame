#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor;

namespace PSS
{
    [Serializable]
    public class ObjectGrabLookAndFeel : Settings
    {
        [SerializeField]
        private bool _showAnchorLines = true;
        [SerializeField]
        private Color _anchorLineColor = Color.green;
        [SerializeField]
        private bool _showTargetBoxes = true;
        [SerializeField]
        private Color _targetBoxWireColor = ColorEx.KeepAllButAlpha(Color.white, 0.5f);
        [SerializeField]
        private bool _showAnchorLineTicks = true;
        [SerializeField]
        private Color _anchorLineTickColor = Color.white;
        [SerializeField]
        private float _anchorLineTickSize = 0.05f;

        public bool ShowAnchorLines { get { return _showAnchorLines; } set { _showAnchorLines = value; } }
        public Color AnchorLineColor { get { return _anchorLineColor; } set { _anchorLineColor = value; } }
        public bool ShowTargetBoxes { get { return _showTargetBoxes; } set { _showTargetBoxes = value; } }
        public Color TargetWireBoxColor { get { return _targetBoxWireColor; } set { _targetBoxWireColor = value; } }
        public bool ShowAnchorLineTicks { get { return _showAnchorLineTicks; } set { _showAnchorLineTicks = value; } }
        public Color AnchorLineTickColor { get { return _anchorLineTickColor; } set { _anchorLineTickColor = value; } }
        public float AnchorLineTickSize { get { return _anchorLineTickSize; } set { _anchorLineTickSize = Mathf.Max(1e-5f, value); } }

        #if UNITY_EDITOR
        protected override void RenderContent(UnityEngine.Object undoRecordObject)
        {
            bool newBool; Color newColor; float newFloat;

            var content = new GUIContent();
            content.text = "Show anchor lines";
            content.tooltip = "If this is checked, a line will be drawn between each object's position and the intersection point between the mouse cursor and the snap surface.";
            newBool = EditorGUILayout.ToggleLeft(content, ShowAnchorLines);
            if(newBool != ShowAnchorLines)
            {
                EditorUndoEx.Record(undoRecordObject);
                ShowAnchorLines = newBool;
            }

            content.text = "Show target boxes";
            content.tooltip = "If this is checked, a wire box will be rendered for all hierarchies that are involved in a surface snap session.";
            newBool = EditorGUILayout.ToggleLeft(content, ShowTargetBoxes);
            if (newBool != ShowTargetBoxes)
            {
                EditorUndoEx.Record(undoRecordObject);
                ShowTargetBoxes = newBool;
            }

            content.text = "Show anchor line ticks";
            content.tooltip = "If this is checked, ticks will be drawn at the beginning and end points of anchor lines.";
            newBool = EditorGUILayout.ToggleLeft(content, ShowAnchorLineTicks);
            if (newBool != ShowAnchorLineTicks)
            {
                EditorUndoEx.Record(undoRecordObject);
                ShowAnchorLineTicks = newBool;
            }

            content.text = "Anchor line color";
            content.tooltip = "Allows you to change the color of the anchor lines. These are the lines that are drawn from the " + 
                              "object positions to the intersection point between the mouse cursor and the snap surface.";
            newColor = EditorGUILayout.ColorField(content, AnchorLineColor);
            if(newColor != AnchorLineColor)
            {
                EditorUndoEx.Record(undoRecordObject);
                AnchorLineColor = newColor;
            }

            content.text = "Target wire box color";
            content.tooltip = "Allows you to change the wire color of target boxes. These are the boxes that are drawn for each hierarchy " + 
                              "that is controlled by a snap session.";
            newColor = EditorGUILayout.ColorField(content, TargetWireBoxColor);
            if (newColor != TargetWireBoxColor)
            {
                EditorUndoEx.Record(undoRecordObject);
                TargetWireBoxColor = newColor;
            }

            content.text = "Anchor line tick color";
            content.tooltip = "Allows you to change the color of the anchor line ticks.";
            newColor = EditorGUILayout.ColorField(content, AnchorLineTickColor);
            if (newColor != AnchorLineTickColor)
            {
                EditorUndoEx.Record(undoRecordObject);
                AnchorLineTickColor = newColor;
            }

            content.text = "Anchor line tick size";
            content.tooltip = "Allows you to change the size of the anchor line ticks.";
            newFloat = EditorGUILayout.FloatField(content, AnchorLineTickSize);
            if (newFloat != AnchorLineTickSize)
            {
                EditorUndoEx.Record(undoRecordObject);
                AnchorLineTickSize = newFloat;
            }
        }
        #endif
    }
}
#endif