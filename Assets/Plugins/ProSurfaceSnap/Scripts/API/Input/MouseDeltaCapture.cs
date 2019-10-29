#if UNITY_EDITOR
using UnityEngine;

namespace PSS
{
    public class MouseDeltaCapture
    {
        private int _id;
        private Vector3 _origin;
        private Vector3 _delta;

        public int Id { get { return _id; } }
        public Vector3 Origin { get { return _origin; } }
        public Vector3 Delta { get { return _delta; } }

        public MouseDeltaCapture(int id, Vector3 origin)
        {
            _id = id;
            _origin = origin;
        }

        public void Update(Vector3 devicePosition)
        {
            _delta = devicePosition - _origin;
        }
    }
}
#endif