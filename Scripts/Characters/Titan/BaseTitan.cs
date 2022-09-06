using System;
using UnityEngine;
using ApplicationManagers;
using GameManagers;
using UnityEngine.UI;
using Utility;
using System.Collections;
using SimpleJSONFixed;

namespace Characters
{
    abstract class BaseTitan: BaseCharacter
    {
        public TitanState State;
        public BaseTitanComponentCache BaseTitanCache;
        public TitanColliderToggler TitanColliderToggler;
        public bool IsWalk;
        protected BaseTitanAnimations BaseTitanAnimations;
        protected override float DefaultMaxHealth => 1f;
        protected virtual float DefaultRunSpeed => 20f;
        protected virtual float DefaultWalkSpeed => 10f;
        protected float RunSpeed;
        protected float WalkSpeed;
        protected float JumpForce => 200f;
        protected virtual float RotateSpeed => 5f;
        protected override Vector3 Gravity => Vector3.down * 100f;
        protected Vector3 LastTargetDirection;
        protected Vector3 _baseScale = Vector3.one;
        protected override float GroundDistance => 1f;
        protected float _stateTimeLeft;
        protected string _currentAttack;

        public virtual void Init(bool ai, string team, JSONNode data)
        {
            base.Init(ai, team);
            if (data != null)
            {
                if (data.HasKey("RunSpeed"))
                    RunSpeed = data["RunSpeed"].AsFloat;
                if (data.HasKey("WalkSpeed"))
                    WalkSpeed = data["WalkSpeed"].AsFloat;
                if (data.HasKey("Health"))
                    SetMaxHealth(data["Health"].AsFloat);
            }
        }

        public virtual bool CanAction()
        {
            return (State == TitanState.Idle && _stateTimeLeft <= 0f) || State == TitanState.Run;
        }

        public virtual bool CanStun()
        {
            return State != TitanState.Stun;
        }

        public virtual void Jump()
        {
            StateAction(TitanState.Jump, BaseTitanAnimations.Jump, 0.2f);
            Cache.Rigidbody.AddForce(Vector3.up * JumpForce, ForceMode.VelocityChange);
        }
      
        public virtual void Attack(string attack)
        {
        }

        public virtual void Kick()
        {
            StateAction(TitanState.Kick, BaseTitanAnimations.Kick);
        }

        public virtual void Block()
        {
            StateAction(TitanState.Block, BaseTitanAnimations.Block, 1f);
        }

        public virtual void Stun()
        {
            StateAction(TitanState.Stun, BaseTitanAnimations.Stun, 0.5f);
        }

        public virtual void Special()
        {
            StateAction(TitanState.Special, BaseTitanAnimations.Special);
        }

        public virtual void Run()
        {
            StateAction(TitanState.Run, BaseTitanAnimations.Run, 0f);
        }

        public virtual void Walk()
        {
            StateAction(TitanState.Walk, BaseTitanAnimations.Walk, 0f);
        }

        public virtual void Idle()
        {
            StateAction(TitanState.Idle, BaseTitanAnimations.Idle, 0f);
        }

        public virtual void Land()
        {
            StateAction(TitanState.Land, BaseTitanAnimations.Land, 0.2f);
        }

        public virtual void Fall()
        {
            StateAction(TitanState.Fall, BaseTitanAnimations.Fall, 0f);
        }

        public override void Emote(string emote)
        {
        }

        protected override IEnumerator WaitAndDie()
        {
            StateAction(TitanState.Dead, BaseTitanAnimations.Die, 0f);
            yield return new WaitForSeconds(2f);
            PhotonNetwork.Destroy(gameObject);
        }

        [RPC]
        public override void GetHitRPC(int viewId, int damage, string type)
        {
            if (viewId >= 0)
            {
                var character = PhotonView.Find(viewId).GetComponent<BaseCharacter>();
                if (character is BaseTitan && CanStun())
                    Stun();
            }
            TakeDamage(damage);
        }

        protected void StateAction(TitanState state, string animation)
        {
            StateAction(state, animation, Cache.Animation[animation].length);
        }

        protected void StateAction(TitanState state, string animation, float stateTime)
        {
            State = state;
            CrossFade(animation, 0.1f);
            _stateTimeLeft = stateTime;
        }

        protected override void Awake()
        {
            base.Awake();
            CreateAnimations(null);
            Cache.Rigidbody.freezeRotation = true;
            Cache.Rigidbody.useGravity = false;
            TitanColliderToggler = TitanColliderToggler.Create(this);
            _baseScale = Cache.Transform.localScale;
            RunSpeed = DefaultRunSpeed;
            WalkSpeed = DefaultWalkSpeed;
        }

        protected override void CreateCache(BaseComponentCache cache)
        {
            BaseTitanCache = (BaseTitanComponentCache)cache;
            base.CreateCache(cache);
        }

        protected virtual void CreateAnimations(BaseTitanAnimations animations)
        {
            if (animations == null)
                animations = new BaseTitanAnimations();
            BaseTitanAnimations = animations;
        }

        public override Transform GetCameraAnchor()
        {
            return BaseTitanCache.Head;
        }

        protected virtual void Update()
        {
            if (IsMine())
            {
                if (State == TitanState.Fall || State == TitanState.Dead)
                    return;
                _stateTimeLeft -= Time.deltaTime;
                if (State == TitanState.Idle)
                {
                    if (HasDirection && _stateTimeLeft <= 0f)
                    {
                        if (IsWalk)
                            Walk();
                        else
                            Run();
                    }
                }
                else if (State == TitanState.Run)
                {
                    if (!HasDirection)
                        Idle();
                    else if (IsWalk)
                        Walk();
                }
                else if (State == TitanState.Walk)
                {
                    if (!HasDirection)
                        Idle();
                    else if (!IsWalk)
                        Run();
                }
                else if (_stateTimeLeft <= 0f)
                    Idle();
            }
        }

        protected virtual void FixedUpdate()
        {
            if (IsMine())
            {
                CheckGround();
                if (State == TitanState.Attack || State == TitanState.Kick)
                {
                }
                else if (State == TitanState.Dead)
                    Cache.Rigidbody.velocity = Vector3.zero;
                else if (Grounded && State != TitanState.Jump)
                {
                    Cache.Rigidbody.velocity = Vector3.zero;
                    LastTargetDirection = Vector3.zero;
                    if (JustGrounded)
                        Land();
                    else if (HasDirection && (State == TitanState.Run || State == TitanState.Walk))
                    {
                        LastTargetDirection = GetTargetDirection();
                        if (State == TitanState.Run)
                            Cache.Rigidbody.velocity = LastTargetDirection * RunSpeed;
                        else if (State == TitanState.Walk)
                            Cache.Rigidbody.velocity = LastTargetDirection * WalkSpeed;
                    }
                }
                else if (State != TitanState.Fall && State != TitanState.Jump)
                    Fall();
                Cache.Rigidbody.AddForce(Gravity, ForceMode.Acceleration);
            }
        }

        protected virtual void LateUpdate()
        {
            if (IsMine())
            {
                if (State == TitanState.Run && HasDirection)
                {
                    Cache.Transform.rotation = Quaternion.Lerp(Cache.Transform.rotation, GetTargetRotation(), Time.deltaTime * RotateSpeed);
                }
            }
        }

        protected override void CheckGround()
        {
            JustGrounded = false;
            if (Physics.Raycast(Cache.Transform.position + Vector3.up, -Vector3.up, out RaycastHit hit, GroundDistance + 1f, GroundMask.value))
            {
                if (!Grounded)
                    Grounded = JustGrounded = true;
                Cache.Transform.position = hit.point + Vector3.up * 0.1f;
            }
            else
                Grounded = false;
        }
    }

    public enum TitanState
    {
        Idle,
        Run,
        Walk,
        Jump,
        Fall,
        Emote,
        Land,
        Attack,
        Special,
        Kick,
        Stun,
        Block,
        Dead,
        Blind,
        Cripple,
        Sit
    }
}
