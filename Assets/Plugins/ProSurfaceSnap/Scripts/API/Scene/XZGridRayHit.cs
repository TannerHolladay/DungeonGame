using UnityEngine;

namespace PSS
{
    public class XZGridRayHit
    {
        private Vector3 _hitPoint;
        private float _hitEnter;
        private Vector3 _hitNormal;
        private Plane _hitPlane;

        public Vector3 HitPoint { get { return _hitPoint; } }
        public float HitEnter { get { return _hitEnter; } }
        public Vector3 HitNormal { get { return _hitNormal; } }
        public Plane HitPlane { get { return _hitPlane; } }

        public XZGridRayHit(Ray ray, Plane gridPlane, float hitEnter)
        {
            _hitEnter = hitEnter;
            _hitPoint = ray.GetPoint(hitEnter);
            _hitPlane = gridPlane;
            _hitNormal = _hitPlane.normal;
        }
    }
}
