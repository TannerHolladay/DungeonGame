using KinematicCharacterController;
using ProBuilder2.Common;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Characters.Scripts
{
    [DisallowMultipleComponent, RequireComponent(typeof(KinematicCharacterMotor))]
    public class Enemy : Humanoid
    {
        private Player _player;
        private NavMeshAgent _navmesh;
        private float _search;
        private float _strafeAngle;
        private Vector3 _direction;
        private Vector3 _destination;
        private Vector3 _startPos;
        private Vector3 _position;
        private Vector3 _playerPos;
        private Vector3 _playerDir;
        private int _randomAttack;

        [Header("Visibility Zone")]
        public float fov = 135;
        public float radius = 10;
        public bool angry = true;
        public float cooldown = 5;
        public float cooltime;

        public Attack[] attacks;

        private void OnDrawGizmosSelected()
        {
            Handles.color = Color.red;
            var position = transform.position;
            var forward = transform.forward;
            Handles.DrawWireArc(position, Vector3.up, Quaternion.AngleAxis(-fov / 2, Vector3.up) * forward, fov, radius);
            Handles.DrawLine(position, position + Quaternion.AngleAxis(-fov / 2, Vector3.up) * forward * radius);
            Handles.DrawLine(position, position + Quaternion.AngleAxis(fov / 2, Vector3.up) * forward * radius);
        }

        protected override void Start()
        {
            base.Start();
            Player.Lockables.Add(transform.gameObject);
            _startPos = transform.position;
            _destination = _startPos;
            _navmesh = GetComponent<NavMeshAgent>();
            _player = Player.Instance;
        }

        protected override void SetInputs()
        {
            if (!angry || !_player) return;
            _position = transform.position;
            _playerPos = _player.transform.position;
            _playerDir = _position - _playerPos;
            CharInput.Attack = false;
            if (transform.GetAngleTo(_playerPos) < fov && _playerDir.magnitude < radius || _search > Time.time)
            {
                Physics.Linecast(_position + Vector3.up, _playerPos + Vector3.up, out var hit, LayerMask.GetMask("Default"));
                if (!hit.transform)
                {
                    _search = Time.time + 5;
                }

                var path = _navmesh.path.corners;
                if (_playerDir.magnitude < 4 && path.Length <= 2)
                {
                    CharInput.Aiming = true;
                    CurrentSpeed = WalkSpeed;
                    CharInput.LookVector = -_playerDir;

                    var attack = attacks[_randomAttack];
                    if (Time.time > attack.Time + attack.cooldown && Time.time > cooltime + cooldown)
                    {
                        CurrentSpeed = RunSpeed;
                        if (_playerDir.magnitude < attack.distance)
                        {
                            attack.Time = Time.time;
                            CurrentSword.GetComponent<Sword>().damage = attack.damage;
                            Anim.Play(attack.name, Anim.GetLayerIndex("Attack"));
                        }
                        else
                        {
                            _destination = _playerPos;
                        }
                    }
                    else if(!Anim.GetBool(Animhashes.IsAttacking))
                    {
                        CurrentSpeed = WalkSpeed;
                        _randomAttack = Random.Range(0, attacks.Length);
                        if (_playerDir.magnitude > 3)
                        {
                            _destination = _playerPos + _playerDir.normalized * 3;
                        } else if (_playerDir.magnitude <= 1.4)
                        {
                            _destination = _playerPos + _playerDir.normalized * 2;
                        }
                    }
                    else
                    {
                        cooltime = Time.time;
                    }
                    
                }
                else
                {
                    CharInput.Aiming = false;
                    CurrentSpeed = RunSpeed;
                    _destination = _playerPos;
                }
            }
            else
            {
                CharInput.Aiming = false;
                _destination = _startPos;
            }
            _navmesh.SetDestination(_destination);
            _direction = _navmesh.remainingDistance >= _navmesh.stoppingDistance ? _navmesh.desiredVelocity : Vector3.zero; 
            CharInput.MoveVector = Vector3.Lerp(CharInput.MoveVector, Vector3.ClampMagnitude(_direction, 1), .1f);

        }

        public Vector3 RandomNavSphere(Vector3 origin, float distance, int layermask)
        {
            var randomDirection = Quaternion.AngleAxis(Random.Range(-90, 90), Vector3.up) * transform.forward * distance;

            randomDirection += origin;

            NavMesh.SamplePosition(randomDirection, out var navHit, distance, layermask);

            return navHit.position;
        }
    }

    [System.Serializable]
    public class Attack
    {
        public string name;
        [Range(1.4f, 20)]
        public float distance = 1.5f;
        public float cooldown;
        public float damage = 30f;
        internal float Time;
    }
}