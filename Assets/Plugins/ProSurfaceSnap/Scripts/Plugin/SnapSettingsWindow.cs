#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace PSS
{
    public class SnapSettingsWindow : EditorWindow
    {
        private Vector2 _scrollPos;

        [MenuItem("Tools/ProSurfaceSnap/Snap settings...")]
        public static void Init()
        {
            SnapSettingsWindow window = (SnapSettingsWindow)EditorWindow.GetWindow(typeof(SnapSettingsWindow));
            window.Show();
        }

        private void OnGUI()
        {
            ProSurfaceSnap surfaceSnap = ProSurfaceSnap.Get;
            if (surfaceSnap == null) return;

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            surfaceSnap.GrabSettings.RenderEditorGUI(surfaceSnap);
            surfaceSnap.GrabLookAndFeel.RenderEditorGUI(surfaceSnap);
            EditorGUILayout.EndScrollView();
        }

        private void OnEnable()
        {
            ProSurfaceSnap surfaceSnap = ProSurfaceSnap.Get;
            if (surfaceSnap == null) return;
           
            surfaceSnap.GrabSettings.ToggleVisibility = true;
            surfaceSnap.GrabSettings.VisibilityToggleLabel = "Settings";

            surfaceSnap.GrabLookAndFeel.ToggleVisibility = true;
            surfaceSnap.GrabLookAndFeel.VisibilityToggleLabel = "Look and feel";

            EditorApplication.modifierKeysChanged -= Repaint;
            EditorApplication.modifierKeysChanged += Repaint;
        }

        private void OnDestroy()
        {
            EditorApplication.modifierKeysChanged -= Repaint;
        }
    }
}
#endif