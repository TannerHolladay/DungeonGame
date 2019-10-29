using System;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

namespace Characters.Scripts
{
    [DisallowMultipleComponent, RequireComponent(typeof(KinematicCharacterMotor))]
    public class Humanoid : Health, ICharacterController
    {

        // Public Variables
        [Header("Humanoid Settings")]
        public float WalkSpeed = 4.0f;
        public float RunSpeed = 5.2f;
        public float StunTime = 4;
        public bool IsDead;
        public Stamina Stamina = new Stamina();
        [Header("Attachments")]
        public GameObject CurrentSword;
        public GameObject CurrentShield;
        public Transform SwordHandle;
        public Transform ShieldHandle;
        public float IFrames = 2f;
        public float IFramesCount;
        public Sound[] Sounds;

        // Private Variables
        private float _stunned;
        private KinematicCharacterMotor _motor;
        private const float Gravity = -30f;
        private readonly Dictionary<string, float> _cooldowns = new Dictionary<string, float>();

        // Shared Variables
        protected float CurrentSpeed;
        protected Animator Anim;
        protected CharacterInput CharInput;

        protected override void Start()
        {
            base.Start();
            _motor = GetComponent<KinematicCharacterMotor>();
            _motor.CharacterController = this;
            Anim = GetComponent<Animator>();
            CharInput = new CharacterInput();
            if (SwordHandle)
            {
                SwordHandle = Instantiate(SwordHandle, Anim.GetBoneTransform(HumanBodyBones.RightHand));
            }
            if (ShieldHandle)
            {
                ShieldHandle = Instantiate(ShieldHandle, Anim.GetBoneTransform(HumanBodyBones.LeftHand));
            }

            Equip();

            CurrentSpeed = WalkSpeed;
        }

        private void Equip()
        {
            if (CurrentSword)
            {
                CurrentSword = Instantiate(CurrentSword, SwordHandle);
                if (CurrentShield)
                {
                    CurrentShield = Instantiate(CurrentShield, ShieldHandle);
                }
                else
                {
                    //One Handed
                }
            }
            else
            {
                //Unequiped
            }
        }

        private void Update()
        {
            SetInputs();
            CharInput.MoveVector = CharInput.MoveVector.Flatten();
            CharInput.LookVector = CharInput.LookVector.Flatten();
            if (_stunned > 0)
            {
                CharInput.Blocking = false;
                CharInput.Attack = false;
            }
            if (CharInput.Attack)
            {
                Anim.SetBool(Animhashes.Attack, true);
            }
            else if(CharInput.Roll && Time.time + IFrames > IFramesCount)
            {
                IFramesCount = Time.time;
                Anim.Play("Roll");
            }

            Anim.SetBool(Animhashes.Blocking, CharInput.Blocking);
        }

        private void FixedUpdate()
        {
            Regen();
        }

        private void Regen()
        {
            // Everything is multiplied by .02 because FixedUpdate is 50 actions per second
            if (Stamina.Current < Stamina.Max && !Anim.GetBool(Animhashes.IsAttacking))
            {
                if (CharInput.Blocking)
                {
                    AddStamina(Stamina.Recovery * Stamina.BlockingMultiplier * .02f);
                }
                else
                {
                    AddStamina(Stamina.Recovery * .02f);
                }
            }

            if (_stunned > 0)
            {
                _stunned -= .02f;
            }
            else
            {
                _stunned = 0;
            }
        }

        private void AddStamina(float ammount)
        {
            Stamina.Current = Mathf.Clamp(Stamina.Current += ammount, 0, Stamina.Max);
            if (Stamina.Current <= 0)
            {
                _stunned = StunTime;
            }
        }

        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            var rotation = currentRotation;
            if (CharInput.Aiming && !CharInput.Sprinting && CharInput.LookVector.magnitude > 0)
            {
                rotation = Quaternion.LookRotation(CharInput.LookVector, Vector3.up);
            }
            else if (CharInput.MoveVector.magnitude > .2f)
            {
                rotation = Quaternion.LookRotation(CharInput.MoveVector, Vector3.up);
            }
            rotation.x = 0;
            rotation.z = 0;
            currentRotation = Quaternion.Lerp(currentRotation, rotation, deltaTime * 8) * Anim.deltaRotation;
        }

        public void SetSword(Sword obj)
        {
            foreach (GameObject child in SwordHandle.transform)
            {
                Destroy(child);
            }
        }

        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            var grounded = _motor.GroundingStatus.FoundAnyGround;
            Anim.SetBool(Animhashes.Falling, !grounded);

            var isMoving = CharInput.MoveVector.magnitude > 0 && grounded && Anim.GetBool(Animhashes.IsMoving);

            CurrentSpeed = Mathf.Lerp(CurrentSpeed, CharInput.Sprinting ? RunSpeed : WalkSpeed, Time.deltaTime * 10);
            Anim.SetBool(Animhashes.Sprinting, CharInput.Sprinting && isMoving);

            if (!grounded)
            {
                currentVelocity += new Vector3(0, Gravity, 0) * deltaTime;
            }
            else
            {
                if (isMoving)
                {
                    currentVelocity = CharInput.MoveVector * CurrentSpeed;
                }
                else
                {
                    currentVelocity = Vector3.zero;
                }
                currentVelocity += Anim.deltaPosition / deltaTime;
            }

            var velocity = transform.InverseTransformVector(CharInput.MoveVector);
            Anim.SetFloat(Animhashes.Horizontal, velocity.x, .1f, deltaTime);
            Anim.SetFloat(Animhashes.Vertical, velocity.z, .1f, deltaTime);
        }

        protected virtual void SetInputs()
        {
        }

        public override void TakeDamage(float ammount, Vector3 hitPoint = default)
        {
            if (CharInput.Blocking && CurrentShield && !Anim.GetBool(Animhashes.IsAttacking) && transform.GetAngleTo(hitPoint) < 180 && Time.time + IFrames > IFramesCount)
            {
                //Play Sound, Animation, and Deplete Stamina
                AddStamina(-Stamina.Blocking);
            }
            else
            {
                base.TakeDamage(ammount);
                AudioManager.Play("HitEnemy");
                Anim.Play("Impact", Anim.GetLayerIndex("Actions"));
            }
        }

        protected override void Kill()
        {
            Anim.SetTrigger(Animhashes.Dead);
            CharInput.MoveVector = Vector3.zero;
            foreach (var script in GetComponents<MonoBehaviour>())
            {
                script.enabled = false;
            }
            //Destroy(gameObject);
        }

        public void OpenDamageColliders()
        {
            AddStamina(-Stamina.Attack);
            AudioManager.Play("Attack");
            CurrentSword.GetComponent<Collider>().enabled = true;
        }

        public void CloseDamageColliders()
        {
            CurrentSword.GetComponent<Collider>().enabled = false;
        }

        #region Unused
        public void BeforeCharacterUpdate(float deltaTime){}
        public void PostGroundingUpdate(float deltaTime){}
        public void AfterCharacterUpdate(float deltaTime){}
        public bool IsColliderValidForCollisions(Collider coll){return true;}
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport){}
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport){}
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport){}
        public void OnDiscreteCollisionDetected(Collider hitCollider){}
        #endregion
    }

    internal struct Animhashes
    {
        //Floats
        internal static readonly int Horizontal = Animator.StringToHash("Horizontal");
        internal static readonly int Vertical = Animator.StringToHash("Vertical");
        //Bools
        internal static readonly int Attack = Animator.StringToHash("Attacking");
        internal static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
        internal static readonly int Falling = Animator.StringToHash("Falling");
        internal static readonly int Speed = Animator.StringToHash("Speed");
        internal static readonly int Blocking = Animator.StringToHash("Blocking");
        internal static readonly int Dead = Animator.StringToHash("Dead");
        internal static readonly int Sprinting = Animator.StringToHash("Sprinting");
        internal static readonly int IsMoving = Animator.StringToHash("IsMoving");
        internal static readonly int IsRolling = Animator.StringToHash("Roll");
        //Triggers
        internal static readonly int Roll = Animator.StringToHash("Roll");
        internal static readonly int FrontImpact = Animator.StringToHash("FrontImpact");
        internal static readonly int BackImpact = Animator.StringToHash("BackImpact");
    }

    [Serializable]
    public class Stamina
    {
        public float Max = 100;
        public float Current = 100;
        public float Recovery = 45;
        public float BlockingMultiplier = .4f;
        [Header("Depletion Values")]
        public float Blocking = 10;
        public float Attack = 10;
        public float Rolling = 10;
        public float Parrying = 10;
        public float Jumping = 10;
    }

    public struct CharacterInput
    {
        public Vector3 MoveVector;
        public Vector3 LookVector;
        public bool Attack;
        public bool Aiming;
        public bool Blocking;
        public bool Sprinting;
        public bool Roll;
    }
}