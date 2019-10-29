#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace PSS
{
    public class SnapHotkeysWindow : EditorWindow
    {
        private Vector2 _scrollPos;

        [MenuItem("Tools/ProSurfaceSnap/Snap hotkeys...")]
        public static void Init()
        {
            SnapHotkeysWindow window = (SnapHotkeysWindow)EditorWindow.GetWindow(typeof(SnapHotkeysWindow));
            window.Show();
        }

        private void OnEnable()
        {
            EditorApplication.modifierKeysChanged -= Repaint;
            EditorApplication.modifierKeysChanged += Repaint;
        }

        private void OnDestroy()
        {
            EditorApplication.modifierKeysChanged -= Repaint;
        }

        private void OnGUI()
        {
            ProSurfaceSnap surfaceSnap = ProSurfaceSnap.Get;
            if (surfaceSnap == null) return;

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            surfaceSnap.GrabHotkeys.RenderEditorGUI(surfaceSnap);
            EditorGUILayout.EndScrollView();
        }
    }
}
#endif