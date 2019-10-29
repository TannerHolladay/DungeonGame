#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace PSS
{
    [ExecuteInEditMode]
    public class ProSurfaceSnap : MonoSingleton<ProSurfaceSnap>
    {
        private ObjectGrabSession _grabSession = new ObjectGrabSession();
        [SerializeField][HideInInspector]
        private ObjectGrabSettings _grabSettings = new ObjectGrabSettings();
        [SerializeField][HideInInspector]
        private ObjectGrabHotkeys _grabHotkeys = new ObjectGrabHotkeys();
        [SerializeField][HideInInspector]
        private ObjectGrabLookAndFeel _grabLookAndFeel = new ObjectGrabLookAndFeel();

        public ObjectGrabSettings GrabSettings { get { return _grabSettings; } }
        public ObjectGrabHotkeys GrabHotkeys { get { return _grabHotkeys; } }
        public ObjectGrabLookAndFeel GrabLookAndFeel { get { return _grabLookAndFeel; } }

        #region Menu items
        [MenuItem("Tools/ProSurfaceSnap/Initialize")]
        private static void Initialize()
        {
            var proSurfaceSnapInstances = GameObject.FindObjectsOfType<ProSurfaceSnap>();
            foreach (var instance in proSurfaceSnapInstances) DestroyImmediate(instance.gameObject);

            GameObject newObject = new GameObject("ProSurfaceSnap");
            newObject.AddComponent<ProSurfaceSnap>();

            SnapSettingsWindow.Init();
        }
        #endregion

        private void OnEnable()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;

            Selection.selectionChanged -= OnSelectionChanged;
            Selection.selectionChanged += OnSelectionChanged;

            _grabSession.SharedHotkeys = _grabHotkeys;
            _grabSession.SharedSettings = _grabSettings;
            _grabSession.SharedLookAndFeel = _grabLookAndFeel;
        }

        private void OnDisable()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void OnDestroy()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            Selection.selectionChanged -= OnSelectionChanged;
        }

        private void Update()
        {
            EditorScene.Get.Update();
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            Event e = Event.current;
            switch (e.type)
            {
                case EventType.MouseDown:

                    InputDeviceManager.Get.OnMouseButtonDown((MouseButton)e.button);
                    if (_grabSession.IsActive && e.button == (int)MouseButton.Left)
                    {
                        _grabSession.End();
                        e.Disable();
                    }
                    break;

                case EventType.MouseUp:
                   
                    InputDeviceManager.Get.OnMouseButtonUp((MouseButton)e.button);
                    break;

                case EventType.KeyDown:

                    if (e.keyCode != KeyCode.None)
                    {
                        if(e.keyCode == KeyCode.Delete)
                        {
                            _grabSession.End();
                        }
                        else
                        {
                            InputDeviceManager.Get.OnKeyDown(e.keyCode);
                            if (_grabHotkeys.ToggleGrab.IsActive())
                            {
                                if (_grabSession.IsActive) _grabSession.End();
                                else _grabSession.Begin(SelectionEx.GetSelectedObjects());
                            }
                        }
                    }
                    break;

                case EventType.KeyUp:

                    InputDeviceManager.Get.OnKeyUp(e.keyCode);
                    break;
            }

            InputDeviceManager.Get.OnSceneGUIUpdate();
            EditorScene.Get.OnSceneGUIUpdate();
            _grabSession.OnSceneGUIUpdate();
            _grabSession.OnSceneGUIRender();
        }

        private void OnSelectionChanged()
        {
            InputDeviceManager.Get.OnMouseButtonUp((MouseButton)0);
        }
    }
}
#endif