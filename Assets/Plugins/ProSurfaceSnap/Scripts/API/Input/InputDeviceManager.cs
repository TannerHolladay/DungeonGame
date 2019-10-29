#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;

namespace PSS
{
    public class InputDeviceManager : Singleton<InputDeviceManager>
    {
        private Dictionary<KeyCode, bool> _keyToState = new Dictionary<KeyCode, bool>();
        private bool[] _mouseBtnStates = new bool[3];

        private int _maxNumMouseDeltaCaptures = 1;
        private MouseDeltaCapture[] _mouseDeltaCaptures;
        private Vector2 _prevMousePos;
        private Vector2 _mouseDelta;

        public Vector2 MousePos { get { return Event.current.mousePosition; } }
        public Vector2 MouseDelta { get { return _mouseDelta; } }

        public InputDeviceManager()
        {
            SetMaxNumMouseDeltaCaptures(1);
        }

        public void SetMaxNumMouseDeltaCaptures(int maxNumDeltaCaptures)
        {
            _maxNumMouseDeltaCaptures = Mathf.Max(1, maxNumDeltaCaptures);
            _mouseDeltaCaptures = new MouseDeltaCapture[_maxNumMouseDeltaCaptures];
        }

        public bool CreateMouseDeltaCapture(Vector3 deltaOrigin, out int deltaCaptureId)
        {
            deltaCaptureId = 0;
            while (deltaCaptureId < _maxNumMouseDeltaCaptures && _mouseDeltaCaptures[deltaCaptureId] != null) ++deltaCaptureId;
            if (deltaCaptureId == _maxNumMouseDeltaCaptures)
            {
                deltaCaptureId = -1;
                return false;
            }

            var deltaCapture = new MouseDeltaCapture(deltaCaptureId, deltaOrigin);
            _mouseDeltaCaptures[deltaCaptureId] = deltaCapture;

            return true;
        }

        public void RemoveMouseDeltaCapture(int deltaCaptureId)
        {
            if (deltaCaptureId >= 0 && deltaCaptureId < _maxNumMouseDeltaCaptures) _mouseDeltaCaptures[deltaCaptureId] = null;
        }

        public Vector3 GetMouseCaptureDelta(int deltaCaptureId)
        {
            if (deltaCaptureId >= 0 &&
                deltaCaptureId < _maxNumMouseDeltaCaptures && _mouseDeltaCaptures[deltaCaptureId] != null) return _mouseDeltaCaptures[deltaCaptureId].Delta;
            return Vector3.zero;
        }

        public bool IsKeyDown(KeyCode keyCode)
        {
            if (ContainsKeyEntry(keyCode)) return _keyToState[keyCode];
            return false;
        }

        public bool IsMouseButtonDown(MouseButton mouseButton)
        {
            return _mouseBtnStates[(int)mouseButton];
        }

        public void OnKeyDown(KeyCode keyCode)
        {
            if (ContainsKeyEntry(keyCode)) _keyToState[keyCode] = true;
            else _keyToState.Add(keyCode, true);
        }

        public void OnKeyUp(KeyCode keyCode)
        {
            if (ContainsKeyEntry(keyCode)) _keyToState[keyCode] = false;
            else _keyToState.Add(keyCode, false);
        }

        public void OnMouseButtonDown(MouseButton mouseButton)
        {
            _mouseBtnStates[(int)mouseButton] = true;
        }

        public void OnMouseButtonUp(MouseButton mouseButton)
        {
            _mouseBtnStates[(int)mouseButton] = false;
        }

        public void OnSceneGUIUpdate()
        {
            UpdateMouseDelta();
            UpdateDeltaCaptures();
        }

        private bool ContainsKeyEntry(KeyCode keyCode)
        {
            return _keyToState.ContainsKey(keyCode);
        }

        private void UpdateDeltaCaptures()
        {
            int deltaCaptureId = 0;
            Vector3 devicePosition = MousePos;
            while (deltaCaptureId < _maxNumMouseDeltaCaptures && _mouseDeltaCaptures[deltaCaptureId] != null)
            {
                _mouseDeltaCaptures[deltaCaptureId++].Update(devicePosition);
            }
        }

        private void UpdateMouseDelta()
        {
            _mouseDelta = MousePos - _prevMousePos;
            _prevMousePos = MousePos;
        }
    }
}
#endif