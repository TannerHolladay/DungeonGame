#if UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace PSS
{
    [Serializable]
    public class Hotkeys
    {
        private static List<KeyCode> _availableKeys;
        private static List<string> _availableKeyNames;

        static Hotkeys()
        {
            _availableKeys = new List<KeyCode>();
            _availableKeys.Add(KeyCode.Space);
            _availableKeys.Add(KeyCode.Backspace);
            _availableKeys.Add(KeyCode.Return);
            _availableKeys.Add(KeyCode.Tab);
            _availableKeys.Add(KeyCode.Delete);

            for (int keyCode = (int)KeyCode.A; keyCode <= (int)KeyCode.Z; ++keyCode)
            {
                _availableKeys.Add((KeyCode)keyCode);
            }
            for (int keyCode = (int)KeyCode.Alpha0; keyCode <= (int)KeyCode.Alpha9; ++keyCode)
            {
                _availableKeys.Add((KeyCode)keyCode);
            }
            _availableKeys.Add(KeyCode.None);

            _availableKeyNames = new List<string>();
            for (int keyIndex = 0; keyIndex < _availableKeys.Count; ++keyIndex)
            {
                _availableKeyNames.Add(_availableKeys[keyIndex].ToString());
            }
        }

        private const int _maxNumberOfKeys = 2;

        [SerializeField]
        private bool _isEnabled = true;

        [SerializeField]
        private KeyCode[] _keys = new KeyCode[_maxNumberOfKeys];

        [SerializeField]
        private bool _lCtrl = false;
        [SerializeField]
        private bool _lCmd = false;
        [SerializeField]
        private bool _lAlt = false;
        [SerializeField]
        private bool _lShift = false;
        [SerializeField]
        private bool _useStrictModifierCheck = true;

        [SerializeField]
        private string _name = "Hotkeys";

        [NonSerialized]
        private List<Hotkeys> _potentialOverlaps = new List<Hotkeys>();

        public static List<KeyCode> AvailableKeys { get { return new List<KeyCode>(_availableKeys); } }
        public static List<string> AvailableKeyNames { get { return new List<string>(_availableKeyNames); } }

        public bool IsEnabled { get { return _isEnabled; } set { _isEnabled = value; } }
        public string Name { get { return _name; } }
        public KeyCode Key0 { get { return _keys[0]; } set { if (_availableKeys.Contains(value)) _keys[0] = value; } }
        public KeyCode Key1 { get { return _keys[1]; } set { if (_availableKeys.Contains(value)) _keys[1] = value; } }
        public bool LCtrl { get { return _lCtrl; } set { _lCtrl = value; } }
        public bool LCmd { get { return _lCmd; } set { _lCmd = value; } }
        public bool LAlt { get { return _lAlt; } set { _lAlt = value; } }
        public bool LShift { get { return _lShift; } set { _lShift = value; } }
        public bool UseStrictModifierCheck { get { return _useStrictModifierCheck; } set { _useStrictModifierCheck = value; } }

        public Hotkeys(string name)
        {
            _name = name;
            for (int keyIndex = 0; keyIndex < _maxNumberOfKeys; ++keyIndex)
            {
                _keys[keyIndex] = KeyCode.None;
            }
        }

        public static void EstablishPotentialOverlaps(List<Hotkeys> hotkeysCollection)
        {
            foreach (var shKeys in hotkeysCollection)
            {
                foreach (var sh in hotkeysCollection)
                {
                    shKeys.AddPotentialOverlap(sh);
                }
            }
        }

        public int GetNumModifiers()
        {
            int counter = 0;
            if (LAlt) ++counter;
            if (LCtrl) ++counter;
            if (LShift) ++counter;

            return counter;
        }

        public int GetNumKeys()
        {
            int counter = 0;
            if (Key0 != KeyCode.None) ++counter;
            if (Key1 != KeyCode.None) ++counter;

            return counter;
        }

        public List<KeyCode> GetAllUsedKeys()
        {
            var keys = new List<KeyCode>(2);
            if (Key0 != KeyCode.None) keys.Add(Key0);
            if (Key1 != KeyCode.None) keys.Add(Key1);

            return keys;
        }

        public bool UsesKeys(List<KeyCode> keys)
        {
            var allkeys = GetAllUsedKeys();
            foreach (var key in keys)
            {
                if (!allkeys.Contains(key)) return false;
            }

            return true;
        }

        public List<KeyCode> GetAllUsedModifiers()
        {
            if (GetNumModifiers() == 0) return new List<KeyCode>();

            var modifiers = new List<KeyCode>(3);
            if (LAlt) modifiers.Add(KeyCode.LeftAlt);
            if (LShift) modifiers.Add(KeyCode.LeftShift);
            if (LCtrl) modifiers.Add(KeyCode.LeftControl);
            
            return modifiers;
        }

        public bool UsesModifiers(List<KeyCode> modifiers)
        {
            var allModifiers = GetAllUsedModifiers();
            foreach (var modifier in modifiers)
            {
                if (!allModifiers.Contains(modifier)) return false;
            }

            return true;
        }

        public void AddPotentialOverlap(Hotkeys hotkeys)
        {
            if (hotkeys == null || ReferenceEquals(hotkeys, this)) return;

            if (!ContainsPotentialOverlap(hotkeys)) _potentialOverlaps.Add(hotkeys);
        }

        public bool ContainsPotentialOverlap(Hotkeys hotkeys)
        {
            return _potentialOverlaps.Contains(hotkeys);
        }

        public bool IsOverlappedBy(Hotkeys hotkeys)
        {
            if (hotkeys == null || ReferenceEquals(hotkeys, this)) return false;

            if (GetNumKeys() <= hotkeys.GetNumKeys() && 
                GetNumModifiers() <= hotkeys.GetNumModifiers())
            {
                if (hotkeys.UsesKeys(GetAllUsedKeys()) && 
                    hotkeys.UsesModifiers(GetAllUsedModifiers())) return true;
            }

            return false;
        }

        public bool IsActive(bool checkForOverlaps = true)
        {
            if (!IsEnabled || IsEmpty()) return false;

            for (int keyIndex = 0; keyIndex < _maxNumberOfKeys; ++keyIndex)
            {
                if (_keys[keyIndex] != KeyCode.None && 
                   !InputDeviceManager.Get.IsKeyDown(_keys[keyIndex])) return false;
            }

            // If strict modifier check is used but at least one modifier key is pressed,
            // it means the key is not active and we return false.
            if (UseStrictModifierCheck && HasNoModifiers() && IsAnyModifierKeyPressed()) return false;
           
            // Check if the corresponding modifier keys are pressed
            Event e = Event.current;
            if (_lCtrl && !e.control) return false;
            if (_lCmd && !e.command) return false;
            if (_lAlt && !e.alt) return false;
            if (_lShift && !e.shift) return false;

            if (checkForOverlaps)
            {
                foreach (var potentialOverlap in _potentialOverlaps)
                {
                    if (potentialOverlap.IsActive(false) &&
                        IsOverlappedBy(potentialOverlap))
                    {
                        return false;
                    }
                }
            }
         
            return true;
        }

        /// <summary>
        /// Checks if the shortcut has any keys assigned to it. This method
        /// returns true if all keys inside the keys array are set to 'None'
        /// or if the number of keys was set to 0.
        /// </summary>
        public bool HasNoKeys()
        {
            // Check each key
            foreach (var key in _keys)
            {
                if (key != KeyCode.None) return false;
            }

            // Doesn't have any keys
            return true;
        }

        public bool HasNoModifiers()
        {
            return !_lAlt && !_lCmd && !_lCtrl && !_lShift;
        }

        public bool IsEmpty()
        {
            return HasNoKeys() && HasNoModifiers();
        }

        #if UNITY_EDITOR
        public void RenderEditorGUI(UnityEngine.Object undoRecordObject)
        {
            bool newBool;
            const float toggleWidth = 65.0f;

            // Shortcut name label
            EditorGUILayout.BeginVertical("Box");
            EditorGUILayoutEx.SectionHeader(Name);

            // Enabled/disabled
            var content = new GUIContent();
            content.text = "Is enabled";
            content.tooltip = "Allows you to enable/disable a shortcut key.";
            newBool = EditorGUILayout.ToggleLeft(content, IsEnabled);
            if(newBool != IsEnabled)
            {
                EditorUndoEx.Record(undoRecordObject);
                IsEnabled = newBool;
            }

            // For each possible key, let the user specify its key code
            for (int keyIndex = 0; keyIndex < _maxNumberOfKeys; ++keyIndex)
            {
                int selectedIndex = _availableKeyNames.IndexOf(_keys[keyIndex].ToString());
                int newIndex = EditorGUILayout.Popup("Key" + keyIndex.ToString(), selectedIndex, _availableKeyNames.ToArray());
                if (newIndex != selectedIndex)
                {
                    EditorUndoEx.Record(undoRecordObject);
                    _keys[keyIndex] = _availableKeys[newIndex];
                }
            }

            // Modifiers
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical("Box");
            newBool = EditorGUILayout.ToggleLeft("LCtrl", _lCtrl, GUILayout.Width(toggleWidth));
            if (newBool != _lCtrl)
            {
                EditorUndoEx.Record(undoRecordObject);
                _lCtrl = newBool;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            newBool = EditorGUILayout.ToggleLeft("LCmd", _lCmd, GUILayout.Width(toggleWidth));
            if (newBool != _lCmd)
            {
                EditorUndoEx.Record(undoRecordObject);
                _lCmd = newBool;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            newBool = EditorGUILayout.ToggleLeft("LAlt", _lAlt, GUILayout.Width(toggleWidth));
            if (newBool != _lAlt)
            {
                EditorUndoEx.Record(undoRecordObject);
                _lAlt = newBool;
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            newBool = EditorGUILayout.ToggleLeft("LShift", _lShift, GUILayout.Width(toggleWidth));
            if (newBool != _lShift)
            {
                EditorUndoEx.Record(undoRecordObject);
                _lShift = newBool;
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }
        #endif

        /// <summary>
        /// Checks if at least one modifier key is pressed.
        /// </summary>
        private bool IsAnyModifierKeyPressed()
        {
            Event e = Event.current;
            if (e.control) return true;
            if (e.command) return true;
            if (e.alt) return true;
            if (e.shift) return true;

            return false;
        }
    }
}
#endif