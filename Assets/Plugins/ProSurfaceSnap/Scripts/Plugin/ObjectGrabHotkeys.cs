#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace PSS
{
    [Serializable]
    public class ObjectGrabHotkeys : Settings
    {
        [SerializeField]
        private Hotkeys _toggleGrab = new Hotkeys("Toggle on/off" )
        {
            UseStrictModifierCheck = true,
            Key0 = KeyCode.C
        };
        [SerializeField]
        private Hotkeys _switchToXAlignmentAxis = new Hotkeys("Switch to X alignment axis")
        {
            UseStrictModifierCheck = true,
            Key0 = KeyCode.X
        };
        [SerializeField]
        private Hotkeys _switchToYAlignmentAxis = new Hotkeys("Switch to Y alignment axis")
        {
            UseStrictModifierCheck = true,
            Key0 = KeyCode.Y
        };
        [SerializeField]
        private Hotkeys _switchToZAlignmentAxis = new Hotkeys("Switch to Z alignment axis")
        {
            UseStrictModifierCheck = true,
            Key0 = KeyCode.Z
        };
        [SerializeField]
        private Hotkeys _enableRotation = new Hotkeys("Enable rotation")
        {
            UseStrictModifierCheck = true,
            LShift = true
        };
        [SerializeField]
        private Hotkeys _enableRotationAroundAnchor = new Hotkeys("Enable rotation around anchor")
        {
            UseStrictModifierCheck = true,
            LShift = true,
            LCtrl = true
        };
        [SerializeField]
        private Hotkeys _enableScaling = new Hotkeys("Enable scaling")
        {
            UseStrictModifierCheck = true,
            LCtrl = true
        };
        [SerializeField]
        private Hotkeys _enableOffsetFromSurface = new Hotkeys("Enable offset from surface")
        {
            UseStrictModifierCheck = true,
            Key0 = KeyCode.Q
        };
        [SerializeField]
        private Hotkeys _enableAnchorAdjust = new Hotkeys("Enable anchor adjust")
        {
            UseStrictModifierCheck = true,
            LAlt = true
        };
        [SerializeField]
        private Hotkeys _enableOffsetFromAnchor = new Hotkeys("Enable offset from anchor")
        {
            UseStrictModifierCheck = true,
            Key0 = KeyCode.Space
        };

        public Hotkeys ToggleGrab { get { return _toggleGrab; } }
        public Hotkeys SwitchToXAlignmentAxis { get { return _switchToXAlignmentAxis; } }
        public Hotkeys SwitchToYAlignmentAxis { get { return _switchToYAlignmentAxis; } }
        public Hotkeys SwitchToZAlignmentAxis { get { return _switchToZAlignmentAxis; } }
        public Hotkeys EnableRotation { get { return _enableRotation; } }
        public Hotkeys EnableRotationAroundAnchor { get { return _enableRotationAroundAnchor; } }
        public Hotkeys EnableScaling { get { return _enableScaling; } }
        public Hotkeys EnableOffsetFromSurface { get { return _enableOffsetFromSurface; } }
        public Hotkeys EnableAnchorAdjust { get { return _enableAnchorAdjust; } }
        public Hotkeys EnableOffsetFromAnchor { get { return _enableOffsetFromAnchor; } }

        public ObjectGrabHotkeys()
        {
            EstablishPotentialOverlaps();
        }

        #if UNITY_EDITOR
        protected override void RenderContent(UnityEngine.Object undoRecordObject)
        {
            ToggleGrab.RenderEditorGUI(undoRecordObject);
            EnableRotation.RenderEditorGUI(undoRecordObject);
            EnableRotationAroundAnchor.RenderEditorGUI(undoRecordObject);
            EnableScaling.RenderEditorGUI(undoRecordObject);
            EnableOffsetFromSurface.RenderEditorGUI(undoRecordObject);
            EnableAnchorAdjust.RenderEditorGUI(undoRecordObject);
            EnableOffsetFromAnchor.RenderEditorGUI(undoRecordObject);
            SwitchToXAlignmentAxis.RenderEditorGUI(undoRecordObject);
            SwitchToYAlignmentAxis.RenderEditorGUI(undoRecordObject);
            SwitchToZAlignmentAxis.RenderEditorGUI(undoRecordObject);
        }
        #endif

        private void EstablishPotentialOverlaps()
        {
            var list = new List<Hotkeys>()
            {
                EnableRotation, EnableRotationAroundAnchor, EnableScaling,
                EnableOffsetFromSurface, EnableAnchorAdjust, EnableOffsetFromAnchor
            };
            Hotkeys.EstablishPotentialOverlaps(list);
        }
    }
}
#endif