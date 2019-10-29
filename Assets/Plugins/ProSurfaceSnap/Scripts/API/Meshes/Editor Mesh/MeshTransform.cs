#if UNITY_EDITOR
using UnityEngine;

namespace PSS
{
    public class MeshTransform
    {
        private Vector3 _position;
        private Quaternion _rotation;
        private Vector3 _scale;

        public Vector3 Position { get { return _position; } }
        public Quaternion Rotation { get { return _rotation; } }
        public Vector3 Scale { get { return _scale; } }

        public MeshTransform(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            _position = position;
            _rotation = rotation;
            _scale = scale;
        }

        public MeshTransform(Transform transform)
        {
            _position = transform.position;
            _rotation = transform.rotation;
            _scale = transform.lossyScale;
        }

        public OOBB InverseTransformOOBB(OOBB oobb)
        {
            OOBB meshSpaceOOBB = new OOBB(InverseTransformPoint(oobb.Center), Vector3.Scale(_scale.GetInverse(), oobb.Size));
            meshSpaceOOBB.Rotation = Quaternion.Inverse(_rotation) * oobb.Rotation;

            return meshSpaceOOBB;
        }

        public Vector3 TransformPoint(Vector3 point)
        {
            return (_rotation * Vector3.Scale(point, _scale)) + _position;
        }

        public Vector3 InverseTransformPoint(Vector3 point)
        {
            return Vector3.Scale(_scale.GetInverse(), Quaternion.Inverse(_rotation) * (point - _position));
        }
    }
}
#endif