using System.Collections.Generic;
using UnityEngine;

namespace Characters.Scripts
{
    public class PlayerCamera : MonoBehaviour
    {
        public static List<InterestPoint> InterestPoints = new List<InterestPoint>();
        private float _rad;
        private Transform _player;
        private Vector3 _playerPos;

        public float radius = 10f;
        public float height = 10f;
        [Range(0.1f,10)]
        public float moveSpeed = 4;
        [Range(0.1f, 2)]
        public float turnSpeed = .5f;

        private void Start()
        {
            _player = Player.Instance.transform;
            _rad = GetAngle();
            transform.position = CirclePoint(_rad);
        }

        // Update is called once per frame
        private void Update()
        {
            _playerPos = _player.position;
            _rad = Mathf.Lerp(_rad, GetAngle(), Time.deltaTime * turnSpeed);

            transform.position = Vector3.Lerp(transform.position, CirclePoint(_rad), Time.deltaTime * moveSpeed);
        }

        private float GetAngle()
        {
            float percent = 0;
            float angle = 0;
            foreach (var point in InterestPoints)
            {
                percent = point.GetPercent(_playerPos);
                angle = point.rotation;
            }
            return Mathf.LerpAngle(0, angle, percent);
        }

        private Vector3 CirclePoint(float angle = 0)
        {
            Vector3 pos;
            angle -= 180;
            var center = _playerPos + Vector3.up * height;
            pos.x = center.x + radius * Mathf.Sin((angle) * Mathf.Deg2Rad);
            pos.y = center.y;
            pos.z = center.z + radius * Mathf.Cos((angle) * Mathf.Deg2Rad);
            transform.LookAt(_playerPos + (transform.position - pos));
            return pos;
        }
    }
}
