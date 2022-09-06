using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Xft;
using CustomSkins;
using Settings;
using UI;
using ApplicationManagers;
using Weather;
using GameProgress;
using GameManagers;
using Controllers;
using Utility;
using Effects;
using SimpleJSONFixed;
using System.IO;

namespace Characters
{
    class Human: BaseCharacter
    {
        // setup
        public HumanComponentCache HumanCache;
        public BaseUseable Skill;
        public BaseUseable Weapon;
        public HookUseable HookLeft;
        public HookUseable HookRight;
        public HumanMountState MountState = HumanMountState.None;
        public Horse Horse;
        public HumanSetup Setup;
        public bool FinishSetup;
        private HumanCustomSkinLoader _customSkinLoader;
        public override List<string> EmoteActions => new List<string>() { "Salute", "Dance", "Flip", "Wave1", "Wave2", "Eat" };
        public static LayerMask AimMask = PhysicsLayer.GetMask(PhysicsLayer.TitanPushbox, PhysicsLayer.MapObjectMapObjects,
           PhysicsLayer.MapObjectEntities, PhysicsLayer.MapObjectAll);

        // state
        public HumanState State = HumanState.Idle;
        public float CurrentGas;
        public float MaxGas;
        private float GasUsage;

        // physics
        public float ReelInAxis = 0f;
        public float ReelOutAxis = 0f;
        public float ReelOutScrollTimeLeft = 0f;
        public float TargetMagnitude = 0f;
        private const float MaxVelocityChange = 10f;
        public float RunSpeed;
        private float _originalDashSpeed;
        public Quaternion _targetRotation;
        private float _wallRunTime = 0f;
        private bool _wallJump = false;
        private bool _launchLeft;
        private bool _launchRight;
        private float _launchLeftTime;
        private float _launchRightTime;
        private bool _needLean;
        private bool _almostSingleHook;
        private bool _leanLeft;
        protected override LayerMask GroundMask => PhysicsLayer.GetMask(PhysicsLayer.TitanPushbox, PhysicsLayer.MapObjectEntities,
                PhysicsLayer.MapObjectCharacters, PhysicsLayer.MapObjectAll);

        // actions
        public string StandAnimation;
        public string AttackAnimation;
        public string RunAnimation;
        public bool _attackRelease;
        public bool _attackButtonRelease;
        private float _stateTimeLeft = 0f;
        private float _dashTimeLeft = 0f;
        private bool _cancelGasDisable;
        private bool _leftArmAim;
        private bool _rightArmAim;
        private bool _animationStopped;
        private Vector3 _gunTarget;

        // test
        public static JSONNode WeaponData;

        public Vector3 GetAimPoint()
        {
            RaycastHit hit;
            Ray ray = SceneLoader.CurrentCamera.Camera.ScreenPointToRay(Input.mousePosition);
            Vector3 target = ray.origin + ray.direction * 1000f;
            if (Physics.Raycast(ray, out hit, 1000f, AimMask.value))
                target = hit.point;
            return target;
        }

        public bool CanJump()
        {
            return (Grounded && (State == HumanState.Idle || State == HumanState.Slide) && 
                !Cache.Animation.IsPlaying(HumanAnimations.Jump) && !Cache.Animation.IsPlaying(HumanAnimations.HorseMount));
        }

        public void Jump()
        {
            Idle();
            CrossFade(HumanAnimations.Jump, 0.1f);
            HumanCache.Sparks.enableEmission = false;
        }

        public void MountHorse()
        {
            if (Horse != null && MountState == HumanMountState.None && Vector3.Distance(Horse.Cache.Transform.position, Cache.Transform.position) < 15f)
            {
                PlayAnimation(HumanAnimations.HorseMount);
                TargetAngle = Horse.transform.rotation.eulerAngles.y;
            }
        }

        public void Unmount()
        {

        }

        public void Dodge(float targetAngle)
        {
            State = HumanState.GroundDodge;
            TargetAngle = targetAngle;
            _targetRotation = GetTargetRotation();
            CrossFade(HumanAnimations.Dodge, 0.1f);
            HumanCache.Sparks.enableEmission = false;
        }

        public void DodgeWall()
        {
            State = HumanState.GroundDodge;
            PlayAnimation(HumanAnimations.Dodge, 0.2f);
            HumanCache.Sparks.enableEmission = false;
        }

        public void Dash(float targetAngle)
        {
            if (_dashTimeLeft <= 0f && CurrentGas >= MaxGas * 0.04f && MountState == HumanMountState.None)
            {
                UseGas(MaxGas * 0.04f);
                TargetAngle = targetAngle;
                Vector3 direction = GetTargetDirection();
                _originalDashSpeed = Cache.Rigidbody.velocity.magnitude;
                _targetRotation = GetTargetRotation();
                Cache.Rigidbody.rotation = _targetRotation;
                EffectSpawner.Spawn(EffectPrefabs.GasBurst, Cache.Transform.position, Cache.Transform.rotation);
                _dashTimeLeft = 0.5f;
                CrossFade(HumanAnimations.Dash, 0.1f, 0.1f);
                State = HumanState.AirDodge;
                FalseAttack();
                Cache.Rigidbody.AddForce(direction * 40f, ForceMode.VelocityChange);
            }
        }

        public void Idle()
        {
            if (State == HumanState.Attack)
                FalseAttack();
            State = HumanState.Idle;
            string animation = HumanAnimations.StandFemale;
            if (Setup.Weapon == HumanWeapon.Gun)
                animation = HumanAnimations.StandGun;
            else if (Setup.CustomSet.Sex.Value == (int)HumanSex.Male)
                animation = HumanAnimations.StandMale;
            CrossFade(animation, 0.1f);
        }

        public void Reload()
        {
        }

       
        public override void Emote(string emote)
        {
            if (State != HumanState.Grab && State != HumanState.AirDodge)
            {
                string animation = HumanAnimations.Salute;
                if (emote == "Salute")
                    animation = HumanAnimations.Salute;
                else if (emote == "Dance")
                    animation = HumanAnimations.SpecialArmin;
                else if (emote == "Flip")
                    animation = HumanAnimations.Dodge;
                else if (emote == "Wave1")
                    animation = HumanAnimations.SpecialMarco0;
                else if (emote == "Wave2")
                    animation = HumanAnimations.SpecialMarco1;
                else if (emote == "Eat")
                    animation = HumanAnimations.SpecialSasha;
                State = HumanState.EmoteAction;
                CrossFade(animation, 0.1f);
                _stateTimeLeft = Cache.Animation[animation].length;
            }
        }

        public override Transform GetCameraAnchor()
        {
            return HumanCache.Head;
        }

        protected override void CreateCache(BaseComponentCache cache)
        {
            HumanCache = new HumanComponentCache(gameObject);
            base.CreateCache(HumanCache);
        }

        protected override IEnumerator WaitAndDie()
        {
            PlaySound(HumanSounds.Die);
            EffectSpawner.Spawn(EffectPrefabs.Blood1, Cache.Transform.position, Cache.Transform.rotation);
            yield return new WaitForSeconds(2f);
            PhotonNetwork.Destroy(gameObject);
        }

        public void Init(bool ai, string team, InGameCharacterSettings settings)
        {
            base.Init(ai, team);
            Setup.Copy(settings);
            if (!ai)
                gameObject.AddComponent<HumanPlayerController>();
        }

        private void OnPhotonPlayerJoined(PhotonPlayer player)
        {
            Cache.PhotonView.RPC("SetupRPC", player, new object[] { Setup.CustomSet.SerializeToJsonString(), (int)Setup.Weapon });
        }

        protected override void Awake()
        {
            base.Awake();
            HumanCache = (HumanComponentCache)Cache;
            Cache.Rigidbody.freezeRotation = true;
            Cache.Rigidbody.useGravity = false;
            Setup = gameObject.AddComponent<HumanSetup>();
            _customSkinLoader = gameObject.AddComponent<HumanCustomSkinLoader>();
            Destroy(gameObject.GetComponent<SmoothSyncMovement>());
            if (_inGameManager != null)
                _inGameManager.Humans.Add(this);
        }

        protected override void Start()
        {
            base.Start();
            if (IsMine())
            {
                // spawn horses
                SetInterpolation(true);
                Cache.Transform.localScale = Vector3.one;
                Cache.PhotonView.RPC("SetupRPC", PhotonTargets.All, new object[] { Setup.CustomSet.SerializeToJsonString(), (int)Setup.Weapon });
                LoadSkin();
            }
            else
            {
            }
        }

        public override void OnHit(BaseHitbox hitbox, BaseCharacter victim, Collider collider)
        {
            EffectSpawner.Spawn(EffectPrefabs.Blood1, hitbox.transform.position, hitbox.transform.rotation);
        }

        protected void Update()
        {
            if (IsMine())
            {
                _stateTimeLeft -= Time.deltaTime;
                _dashTimeLeft -= Time.deltaTime;
                UpdateBladeTrails();
                if (State == HumanState.Attack)
                {
                    if (Setup.Weapon == HumanWeapon.Blade)
                    {
                        var bladeWeapon = (BladeWeapon)Weapon;
                        if (!bladeWeapon.IsActive)
                            _attackButtonRelease = true;
                        if (!_attackRelease)
                        {
                            if (_attackButtonRelease)
                            {
                                ContinueAnimation();
                                _attackRelease = true;
                            }
                            else if (Cache.Animation[AttackAnimation].normalizedTime >= 0.32f)
                                PauseAnimation();
                        }
                        float startTime;
                        float endTime;
                        if (bladeWeapon.CurrentDurability <= 0f)
                            startTime = endTime = -1f;
                        else if (AttackAnimation == "attack5")
                        {
                            startTime = 0.35f;
                            endTime = 0.5f;
                        }
                        else if (AttackAnimation == "attack4")
                        {
                            startTime = 0.6f;
                            endTime = 0.9f;
                        }
                        else
                        {
                            startTime = 0.5f;
                            endTime = 0.85f;
                        }
                        if (Cache.Animation[AttackAnimation].normalizedTime > startTime && Cache.Animation[AttackAnimation].normalizedTime < endTime)
                        {
                            if (!HumanCache.BladeHitLeft.IsActive())
                            {
                                HumanCache.BladeHitLeft.Activate();
                                PlaySound(HumanSounds.BladeSwing);
                                ToggleBladeTrails(true);
                            }
                            if (!HumanCache.BladeHitRight.IsActive())
                                HumanCache.BladeHitRight.Activate();
                        }
                        else if (HumanCache.BladeHitLeft.IsActive())
                        {
                            HumanCache.BladeHitLeft.Deactivate();
                            HumanCache.BladeHitRight.Deactivate();
                            ToggleBladeTrails(false, 0.1f);
                        }
                        if (Cache.Animation[AttackAnimation].normalizedTime >= 1f)
                            Idle();
                    }
                    else if (Setup.Weapon == HumanWeapon.Gun || Setup.Weapon == HumanWeapon.ThunderSpear)
                    {
                        if (Cache.Animation[AttackAnimation].normalizedTime >= 1f)
                            Idle();
                    }
                }
                else if (State == HumanState.EmoteAction)
                {
                    if (_stateTimeLeft <= 0f)
                        Idle();
                }
                else if (State == HumanState.GroundDodge)
                {
                    if (Cache.Animation.IsPlaying("dodge"))
                    {
                        if (!(Grounded || (Cache.Animation["dodge"].normalizedTime <= 0.6f)))
                            Idle();
                        if (Cache.Animation["dodge"].normalizedTime >= 1f)
                            Idle();
                    }
                }
                else if (State == HumanState.Land)
                {
                    if (Cache.Animation.IsPlaying("dash_land") && (Cache.Animation["dash_land"].normalizedTime >= 1f))
                        Idle();
                }
                else if (State == HumanState.FillGas)
                {
                    if (Cache.Animation.IsPlaying("supply") && (Cache.Animation["supply"].normalizedTime >= 1f))
                    {
                        // supply
                        Idle();
                    }
                }
                else if (State == HumanState.Slide)
                {
                    if (!Grounded)
                        Idle();
                }
                else if (State == HumanState.AirDodge)
                {
                    if (_dashTimeLeft > 0f)
                    {
                        if (Cache.Rigidbody.velocity.magnitude > _originalDashSpeed)
                            Cache.Rigidbody.AddForce(-Cache.Rigidbody.velocity * Time.deltaTime * 1.7f, ForceMode.VelocityChange);
                    }
                    else
                        Idle();
                }
            }
        }

        protected void FixedUpdate()
        {
            if (IsMine())
            {
                FixedUpdateUseables();
                FixedUpdateLookTitan();
                Vector3 currentVelocity = Cache.Rigidbody.velocity;
                float currentSpeed = Cache.Rigidbody.velocity.magnitude;
                GameProgressManager.RegisterSpeed(gameObject, currentSpeed);
                if (!Cache.Animation.IsPlaying("attack3_2") && !Cache.Animation.IsPlaying("attack5") && !Cache.Animation.IsPlaying("special_petra"))
                    Cache.Transform.rotation = Quaternion.Lerp(Cache.Transform.rotation, _targetRotation, Time.deltaTime * 6f);
                if (State == HumanState.Grab)
                {
                    Cache.Rigidbody.velocity = Vector3.zero;
                    return;
                }
                CheckGround();
                bool pivotLeft = FixedUpdateLaunch(true);
                bool pivotRight = FixedUpdateLaunch(false);
                bool pivot = pivotLeft || pivotRight;
                if (Grounded)
                {
                    Vector3 newVelocity = Vector3.zero;
                    if (State == HumanState.Attack)
                    {
                        if (AttackAnimation == "attack5")
                        {
                            if (Cache.Animation[AttackAnimation].normalizedTime > 0.4f && Cache.Animation[AttackAnimation].normalizedTime < 0.61f)
                                Cache.Rigidbody.AddForce(Cache.Transform.forward * 200f);
                        }
                        else if (Cache.Animation.IsPlaying("attack1") || Cache.Animation.IsPlaying("attack2"))
                        {
                            Cache.Rigidbody.AddForce(Cache.Transform.forward * 200f);
                        }
                    }
                    if (JustGrounded)
                    {
                        if (State != HumanState.Attack || (AttackAnimation != "attack3_1" && AttackAnimation != "attack5" && AttackAnimation != "special_petra"))
                        {
                            if (State != HumanState.Attack && !HasDirection && !HasHook())
                            {
                                State = HumanState.Land;
                                CrossFade(HumanAnimations.Land, 0.01f);
                            }
                            else
                            {
                                _attackButtonRelease = true;
                                Vector3 v = Cache.Rigidbody.velocity;
                                if (State != HumanState.Attack && (v.x * v.x + v.z * v.z > RunSpeed * RunSpeed * 1.5f) && State != HumanState.FillGas)
                                {
                                    State = HumanState.Slide;
                                    CrossFade(HumanAnimations.Slide, 0.05f);
                                    TargetAngle = Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;
                                    _targetRotation = GetTargetRotation();
                                    HasDirection = true;
                                    HumanCache.Sparks.enableEmission = true;
                                }
                            }
                        }
                        newVelocity = Cache.Rigidbody.velocity;
                    }
                    if (State == HumanState.Attack && AttackAnimation == "attack3_1" && Cache.Animation[AttackAnimation].normalizedTime >= 1f)
                    {
                        PlayAnimation("attack3_2");
                        ResetAnimationSpeed();
                        newVelocity = Vector3.zero;
                        Cache.Rigidbody.velocity = newVelocity;
                    }
                    else if (State == HumanState.GroundDodge)
                    {
                        if (Cache.Animation[HumanAnimations.Dodge].normalizedTime >= 0.2f && Cache.Animation[HumanAnimations.Dodge].normalizedTime < 0.8f)
                            newVelocity = -Cache.Transform.forward * 2.4f * RunSpeed;
                        else if (Cache.Animation[HumanAnimations.Dodge].normalizedTime > 0.8f)
                            newVelocity = Cache.Rigidbody.velocity * 0.9f;
                    }
                    else if (State == HumanState.Idle)
                    {
                        newVelocity = Vector3.zero;
                        if (HasDirection)
                        {
                            newVelocity = GetTargetDirection() * TargetMagnitude * RunSpeed;
                            if (!Cache.Animation.IsPlaying(HumanAnimations.Run) && !Cache.Animation.IsPlaying(HumanAnimations.Jump) &&
                                !Cache.Animation.IsPlaying(HumanAnimations.RunBuffed) && (!Cache.Animation.IsPlaying(HumanAnimations.HorseMount) ||
                                Cache.Animation[HumanAnimations.HorseMount].normalizedTime >= 0.5f))
                            {
                                CrossFade(RunAnimation, 0.1f);
                            }
                            _targetRotation = GetTargetRotation();
                        }
                        else if (!(Cache.Animation.IsPlaying(StandAnimation) || State == HumanState.Land || Cache.Animation.IsPlaying(HumanAnimations.Jump) || Cache.Animation.IsPlaying(HumanAnimations.HorseMount) || Cache.Animation.IsPlaying(HumanAnimations.Grabbed)))
                        {
                            CrossFade(StandAnimation, 0.1f);
                        }
                    }
                    else if (State == HumanState.Land)
                    {
                        newVelocity = Cache.Rigidbody.velocity * 0.96f;
                    }
                    else if (State == HumanState.Slide)
                    {
                        newVelocity = Cache.Rigidbody.velocity * 0.99f;
                        if (currentSpeed < RunSpeed * 1.2f)
                        {
                            Idle();
                            HumanCache.Sparks.enableEmission = false;
                        }
                    }
                    Vector3 force = newVelocity - currentVelocity;
                    force.x = Mathf.Clamp(force.x, -MaxVelocityChange, MaxVelocityChange);
                    force.z = Mathf.Clamp(force.z, -MaxVelocityChange, MaxVelocityChange);
                    force.y = 0f;
                    if (Cache.Animation.IsPlaying(HumanAnimations.Jump) && Cache.Animation[HumanAnimations.Jump].normalizedTime > 0.18f)
                        force.y += 8f;
                    if (Cache.Animation.IsPlaying(HumanAnimations.HorseMount) && Cache.Animation[HumanAnimations.HorseMount].normalizedTime > 0.18f && Cache.Animation[HumanAnimations.HorseMount].normalizedTime < 1f)
                    {
                        force = -currentVelocity;
                        force.y = 6f;
                        float distance = Vector3.Distance(Horse.Cache.Transform.position, Cache.Transform.position);
                        force += (Horse.Cache.Transform.position - Cache.Transform.position).normalized * 0.6f * Gravity.magnitude * distance / 12f;
                    }
                    if (State != HumanState.Attack || Setup.Weapon != HumanWeapon.Gun)
                    {
                        Cache.Rigidbody.AddForce(force, ForceMode.VelocityChange);
                        Cache.Rigidbody.rotation = Quaternion.Lerp(Cache.Transform.rotation, Quaternion.Euler(0f, TargetAngle, 0f), Time.deltaTime * 10f);
                    }
                }
                else
                {
                    if (HumanCache.Sparks.enableEmission)
                        HumanCache.Sparks.enableEmission = false;
                    if (Horse != null && (Cache.Animation.IsPlaying(HumanAnimations.HorseMount) || Cache.Animation.IsPlaying("air_fall")) && Cache.Rigidbody.velocity.y < 0f && Vector3.Distance(Horse.Cache.Transform.position + Vector3.up * 1.65f, Cache.Transform.position) < 0.5f)
                    {
                        Cache.Transform.position = Horse.Cache.Transform.position + Vector3.up * 1.65f;
                        Cache.Transform.rotation = Horse.Cache.Transform.rotation;
                        MountState = HumanMountState.Horse;
                        if (!Cache.Animation.IsPlaying("horse_idle"))
                            CrossFade("horse_idle", 0.1f);
                    }
                    if (Cache.Animation[HumanAnimations.Dash].normalizedTime >= 0.99f || (State == HumanState.Idle && !Cache.Animation.IsPlaying(HumanAnimations.Dash) && !Cache.Animation.IsPlaying("wallrun") && !Cache.Animation.IsPlaying("toRoof")
                        && !Cache.Animation.IsPlaying(HumanAnimations.HorseMount) && !Cache.Animation.IsPlaying(HumanAnimations.HorseDismount) && !Cache.Animation.IsPlaying("air_release")
                        && MountState == HumanMountState.None && (!Cache.Animation.IsPlaying("air_hook_l_just") || Cache.Animation["air_hook_l_just"].normalizedTime >= 1f) && (!Cache.Animation.IsPlaying("air_hook_r_just") || Cache.Animation["air_hook_r_just"].normalizedTime >= 1f)))
                    {
                        if (!IsHookedAny() && (Cache.Animation.IsPlaying("air_hook_l") || Cache.Animation.IsPlaying("air_hook_r") || Cache.Animation.IsPlaying("air_hook")) && Cache.Rigidbody.velocity.y > 20f)
                        {
                            CrossFade("air_release");
                        }
                        else
                        {
                            if ((Mathf.Abs(currentVelocity.x) + Mathf.Abs(currentVelocity.z)) <= 25f)
                            {
                                if (currentVelocity.y < 0f)
                                {
                                    if (!Cache.Animation.IsPlaying("air_fall"))
                                        CrossFade("air_fall", 0.2f);
                                }
                                else if (!Cache.Animation.IsPlaying("air_rise"))
                                    CrossFade("air_rise", 0.2f);
                            }
                            else if (!IsHookedAny())
                            {
                                float angle = -Mathf.DeltaAngle(-Mathf.Atan2(currentVelocity.z, currentVelocity.x) * Mathf.Rad2Deg, Cache.Transform.rotation.eulerAngles.y - 90f);
                                if (Mathf.Abs(angle) < 45f)
                                {
                                    if (!Cache.Animation.IsPlaying("air2"))
                                        CrossFade("air2", 0.2f);
                                }
                                else if ((angle < 135f) && (angle > 0f))
                                {
                                    if (!Cache.Animation.IsPlaying("air2_right"))
                                        CrossFade("air2_right", 0.2f);
                                }
                                else if ((angle > -135f) && (angle < 0f))
                                {
                                    if (!Cache.Animation.IsPlaying("air2_left"))
                                        CrossFade("air2_left", 0.2f);
                                }
                                else if (!Cache.Animation.IsPlaying("air2_backward"))
                                    CrossFade("air2_backward", 0.2f);
                            }
                            else if (Setup.Weapon == HumanWeapon.Gun)
                            {
                                if (IsHookedRight())
                                {
                                    if (!Cache.Animation.IsPlaying("AHSS_hook_forward_l"))
                                        CrossFade("AHSS_hook_forward_l", 0.1f);
                                }
                                else if (IsHookedLeft())
                                {
                                    if (!Cache.Animation.IsPlaying("AHSS_hook_forward_r"))
                                        CrossFade("AHSS_hook_forward_r", 0.1f);
                                }
                                else if (!Cache.Animation.IsPlaying("AHSS_hook_forward_both"))
                                    CrossFade("AHSS_hook_forward_both", 0.1f);
                            }
                            else if (IsHookedRight())
                            {
                                if (!Cache.Animation.IsPlaying("air_hook_l"))
                                    CrossFade("air_hook_l", 0.1f);
                            }
                            else if (IsHookedLeft())
                            {
                                if (!Cache.Animation.IsPlaying("air_hook_r"))
                                    CrossFade("air_hook_r", 0.1f);
                            }
                            else if (!Cache.Animation.IsPlaying("air_hook"))
                                CrossFade("air_hook", 0.1f);
                        }
                    }
                    if (!Cache.Animation.IsPlaying("air_rise"))
                    {
                        if (State == HumanState.Idle && Cache.Animation.IsPlaying("air_release") && Cache.Animation["air_release"].normalizedTime >= 1f)
                            CrossFade("air_rise", 0.2f);
                        else if (Cache.Animation.IsPlaying(HumanAnimations.HorseDismount) && Cache.Animation[HumanAnimations.HorseDismount].normalizedTime >= 1f)
                            CrossFade("air_rise", 0.2f);
                    }
                    if (Cache.Animation.IsPlaying("toRoof"))
                    {
                        if (Cache.Animation["toRoof"].normalizedTime < 0.22f)
                        {
                            Cache.Rigidbody.velocity = Vector3.zero;
                            Cache.Rigidbody.AddForce(new Vector3(0f, Gravity.magnitude * Cache.Rigidbody.mass, 0f));
                        }
                        else
                        {
                            if (!_wallJump)
                            {
                                _wallJump = true;
                                Cache.Rigidbody.AddForce(Vector3.up * 8f, ForceMode.Impulse);
                            }
                            Cache.Rigidbody.AddForce(Cache.Transform.forward * 0.05f, ForceMode.Impulse);
                        }
                        if (Cache.Animation["toRoof"].normalizedTime >= 1f)
                        {
                            PlayAnimation("air_rise");
                        }
                    }
                    else if (!(((((State != HumanState.Idle) || !IsPressDirectionTowardsHero()) || (SettingsManager.InputSettings.Human.Jump.GetKey() || SettingsManager.InputSettings.Human.HookLeft.GetKey())) || ((SettingsManager.InputSettings.Human.HookRight.GetKey() || SettingsManager.InputSettings.Human.HookBoth.GetKey()) || (!IsFrontGrounded() || Cache.Animation.IsPlaying("wallrun")))) || Cache.Animation.IsPlaying("dodge")))
                    {
                        CrossFade("wallrun", 0.1f);
                        _wallRunTime = 0f;
                    }
                    else if (Cache.Animation.IsPlaying("wallrun"))
                    {
                        Cache.Rigidbody.AddForce(Vector3.up * RunSpeed - Cache.Rigidbody.velocity, ForceMode.VelocityChange);
                        _wallRunTime += Time.deltaTime;
                        if (_wallRunTime > 1f || !HasDirection)
                        {
                            Cache.Rigidbody.AddForce(-Cache.Transform.forward * RunSpeed * 0.75f, ForceMode.Impulse);
                            DodgeWall();
                        }
                        else if (!IsUpFrontGrounded())
                        {
                            _wallJump = false;
                            CrossFade("toRoof", 0.1f);
                        }
                        else if (!IsFrontGrounded())
                            CrossFade("air_fall", 0.1f);
                    }
                    else if (!Cache.Animation.IsPlaying("attack5") && !Cache.Animation.IsPlaying("special_petra") && !Cache.Animation.IsPlaying("dash") && !Cache.Animation.IsPlaying("jump") && !IsFiringThunderSpear())
                    {
                        Vector3 targetDirection = GetTargetDirection() * TargetMagnitude * Setup.CustomSet.Gas.Value / 5f;
                        if (!HasDirection)
                        {
                            if (State == HumanState.Attack)
                                targetDirection = Vector3.zero;
                        }
                        else
                            _targetRotation = GetTargetRotation();
                        if (((!pivotLeft && !pivotRight) && (MountState == HumanMountState.None && SettingsManager.InputSettings.Human.Jump.GetKey())) && (CurrentGas > 0f))
                        {
                            if (HasDirection)
                            {
                                Cache.Rigidbody.AddForce(targetDirection, ForceMode.Acceleration);
                            }
                            else
                            {
                                Cache.Rigidbody.AddForce((Cache.Transform.forward * targetDirection.magnitude), ForceMode.Acceleration);
                            }
                            pivot = true;
                        }
                    }
                    if ((Cache.Animation.IsPlaying("air_fall") && (currentSpeed < 0.2f)) && this.IsFrontGrounded())
                    {
                        CrossFade("onWall", 0.3f);
                    }
                }
                if (pivotLeft && pivotRight)
                    FixedUpdatePivot((HookRight.GetHookPosition() + HookLeft.GetHookPosition()) * 0.5f);
                else if (pivotLeft)
                    FixedUpdatePivot(HookLeft.GetHookPosition());
                else if (pivotRight)
                    FixedUpdatePivot(HookRight.GetHookPosition());
                bool lowerGravity = false;
                if (IsHookedLeft() && HookLeft.GetHookPosition().y > Cache.Transform.position.y && _launchLeft)
                    lowerGravity = true;
                else if (IsHookedRight() && HookRight.GetHookPosition().y > Cache.Transform.position.y && _launchRight)
                    lowerGravity = true;
                Vector3 gravity;
                if (lowerGravity)
                    gravity = Gravity * 0.5f * Cache.Rigidbody.mass;
                else
                    gravity = Gravity * Cache.Rigidbody.mass;
                Cache.Rigidbody.AddForce(gravity);
                if (!_cancelGasDisable)
                {
                    if (pivot)
                    {
                        UseGas(GasUsage * Time.deltaTime);
                        if (!HumanCache.Smoke.enableEmission)
                            Cache.PhotonView.RPC("SetSmokeRPC", PhotonTargets.All, new object[] { true });
                    }
                    else
                    {
                        if (HumanCache.Smoke.enableEmission)
                            Cache.PhotonView.RPC("SetSmokeRPC", PhotonTargets.All, new object[] { false });
                    }
                }
                else
                    _cancelGasDisable = false;
                if (WindWeatherEffect.WindEnabled)
                {
                    if (!HumanCache.Wind.enableEmission)
                        HumanCache.Wind.enableEmission = true;
                    HumanCache.Wind.startSpeed = 100f;
                    HumanCache.Wind.transform.LookAt(Cache.Transform.position + WindWeatherEffect.WindDirection);
                }
                else if (currentSpeed > 80f && SettingsManager.GraphicsSettings.WindEffectEnabled.Value)
                {
                    if (!HumanCache.Wind.enableEmission)
                        HumanCache.Wind.enableEmission = true;
                    HumanCache.Wind.startSpeed = currentSpeed;
                    HumanCache.Wind.transform.LookAt(Cache.Transform.position - currentVelocity);
                }
                else if (HumanCache.Wind.enableEmission)
                    HumanCache.Wind.enableEmission = false;
            }
            FixedUpdateSetHookedDirection();
            FixedUpdateBodyLean();
            ReelInAxis = 0f;
        }

        protected void LateUpdate()
        {
            if (MountState == HumanMountState.Cannon)
                return;
            LateUpdateTilt();
            LateUpdateGun();
        }

        private void UpdateBladeTrails()
        {
            if (Setup.LeftTrail1 != null && Setup.LeftTrail1.gameObject.activeSelf)
            {
                Setup.LeftTrail1.update();
                Setup.RightTrail1.update();
            }
            if (Setup.LeftTrail2 != null && Setup.LeftTrail2.gameObject.activeSelf)
            {
                Setup.LeftTrail2.update();
                Setup.RightTrail2.update();
            }
            if (Setup.LeftTrail1 != null && Setup.LeftTrail1.gameObject.activeSelf)
            {
                Setup.LeftTrail1.lateUpdate();
                Setup.RightTrail1.lateUpdate();
            }
            if (Setup.LeftTrail2 != null && Setup.LeftTrail2.gameObject.activeSelf)
            {
                Setup.LeftTrail2.lateUpdate();
                Setup.RightTrail2.lateUpdate();
            }
        }

        private bool FixedUpdateLaunch(bool left)
        {
            bool launch;
            HookUseable hook;
            bool pivot = false;
            float launchTime;
            if (left)
            {
                launch = _launchLeft;
                hook = HookLeft;
                _launchLeftTime += Time.deltaTime;
                launchTime = _launchLeftTime;
            }
            else
            {
                launch = _launchRight;
                hook = HookRight;
                _launchRightTime += Time.deltaTime;
                launchTime = _launchRightTime;
            }
            if (launch)
            {
                if (hook.IsHooked())
                {
                    Vector3 v = (hook.GetHookPosition() - Cache.Transform.position).normalized * 10f;
                    if (!(_launchLeft && _launchRight))
                        v *= 2f;
                    if ((Vector3.Angle(Cache.Rigidbody.velocity, v) > 90f) && SettingsManager.InputSettings.Human.Jump.GetKey())
                    {
                        pivot = true;
                    }
                    if (!pivot)
                    {
                        Cache.Rigidbody.AddForce(v);
                        if (Vector3.Angle(Cache.Rigidbody.velocity, v) > 90f)
                            Cache.Rigidbody.AddForce(-Cache.Rigidbody.velocity * 2f, ForceMode.Acceleration);
                    }
                }
                if (hook.IsActive && CurrentGas > 0f)
                    UseGas(GasUsage * Time.deltaTime);
                else if (launchTime > 0.3f)
                {
                    if (left)
                        _launchLeft = false;
                    else
                        _launchRight = false;
                    hook.DisableActiveHook();
                    pivot = false;
                }
            }
            return pivot;
        }

        private void FixedUpdatePivot(Vector3 position)
        {
            float newSpeed = Cache.Rigidbody.velocity.magnitude + 0.1f;
            Cache.Rigidbody.AddForce(-Cache.Rigidbody.velocity, ForceMode.VelocityChange);
            Vector3 v = position - Cache.Transform.position;
            float reel = GetReelAxis();
            reel = Mathf.Clamp(reel, -0.8f, 0.8f) + 1f;
            v = Vector3.RotateTowards(v, Cache.Rigidbody.velocity, 1.53938f * reel, 1.53938f * reel).normalized;
            Cache.Rigidbody.velocity = (v * newSpeed);
        }

        private void FixedUpdateSetHookedDirection()
        {
            _almostSingleHook = false;
            float oldTargetAngle = TargetAngle;
            if (IsHookedLeft() && IsHookedRight())
            {
                Vector3 hookDiff = HookLeft.GetHookPosition() - HookRight.GetHookPosition();
                Vector3 direction = (HookLeft.GetHookPosition() + HookRight.GetHookPosition()) * 0.5f - Cache.Transform.position;
                if (hookDiff.sqrMagnitude < 4f)
                {
                    TargetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    if (Setup.Weapon == HumanWeapon.Gun && State != HumanState.Attack)
                    {
                        float current = -Mathf.Atan2(Cache.Rigidbody.velocity.z, Cache.Rigidbody.velocity.x) * Mathf.Rad2Deg;
                        float target = -Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
                        TargetAngle -= Mathf.DeltaAngle(current, target);
                    }
                    _almostSingleHook = true;
                }
                else
                {
                    Vector3 left = Cache.Transform.position - HookLeft.GetHookPosition();
                    Vector3 right = Cache.Transform.position - HookRight.GetHookPosition();
                    if (Vector3.Angle(direction, left) < 30f && Vector3.Angle(direction, right) < 30f)
                    {
                        _almostSingleHook = true;
                        TargetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                    }
                    else
                    {
                        _almostSingleHook = true;
                        Vector3 forward = Cache.Transform.forward;
                        Vector3.OrthoNormalize(ref hookDiff, ref forward);
                        TargetAngle = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
                        float angle = Mathf.Atan2(left.x, left.z) * Mathf.Rad2Deg;
                        if (Mathf.DeltaAngle(angle, TargetAngle) > 0f)
                            TargetAngle += 180f;
                    }
                }
            }
            else
            {
                _almostSingleHook = true;
                Vector3 v;
                if (IsHookedLeft())
                    v = HookLeft.GetHookPosition() - Cache.Transform.position;
                else if (IsHookedRight())
                    v = HookRight.GetHookPosition() - Cache.Transform.position;
                else
                    return;
                TargetAngle = Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;
                if (State != HumanState.Attack)
                {
                    float angle1 = -Mathf.Atan2(Cache.Rigidbody.velocity.z, Cache.Rigidbody.velocity.x) * Mathf.Rad2Deg;
                    float angle2 = -Mathf.Atan2(v.z, v.x) * Mathf.Rad2Deg;
                    float delta = -Mathf.DeltaAngle(angle1, angle2);
                    if (Setup.Weapon == HumanWeapon.Gun)
                        TargetAngle += delta;
                    else
                    {
                        float multiplier = 0.1f;
                        if ((IsHookedLeft() && delta < 0f) || (IsHookedRight() && delta > 0f))
                            multiplier = -0.1f;
                        TargetAngle += delta * multiplier;
                    }
                }
            }
            if (IsFiringThunderSpear())
                TargetAngle = oldTargetAngle;
        }

        private void FixedUpdateBodyLean()
        {
            float z = 0f;
            _needLean = false;
            if (Setup.Weapon != HumanWeapon.Gun && State == HumanState.Attack && AttackAnimation != "attack3_1" && AttackAnimation != "attack3_2" && !IsFiringThunderSpear())
            {
                Vector3 v = Cache.Rigidbody.velocity;
                float diag = Mathf.Sqrt((v.x * v.x) + (v.z * v.z));
                float angle = Mathf.Atan2(v.y, diag) * Mathf.Rad2Deg;
                _targetRotation = Quaternion.Euler(-angle * (1f - (Vector3.Angle(v, Cache.Transform.forward) / 90f)), TargetAngle, 0f);
                if (IsHookedAny())
                    Cache.Transform.rotation = _targetRotation;
            }
            else
            {
                if (IsHookedLeft() && IsHookedRight())
                {
                    if (_almostSingleHook)
                    {
                        _needLean = true;
                        z = GetLeanAngle(HookRight.GetHookPosition(), true);
                    }
                }
                else if (IsHookedLeft())
                {
                    _needLean = true;
                    z = GetLeanAngle(HookLeft.GetHookPosition(), true);
                }
                else if (IsHookedRight())
                {
                    _needLean = true;
                    z = GetLeanAngle(HookRight.GetHookPosition(), false);

                }
                if (_needLean)
                {
                    float a = 0f;
                    if (Setup.Weapon != HumanWeapon.Gun && State != HumanState.Attack)
                    {
                        a = Cache.Rigidbody.velocity.magnitude * 0.1f;
                        a = Mathf.Min(a, 20f);
                    }
                    _targetRotation = Quaternion.Euler(-a, TargetAngle, z);
                }
                else if (State != HumanState.Attack)
                    _targetRotation = Quaternion.Euler(0f, TargetAngle, 0f);
            }
        }

        private void FixedUpdateUseables()
        {
            if (FinishSetup)
            {
                Weapon.OnFixedUpdate();
                HookLeft.OnFixedUpdate();
                HookRight.OnFixedUpdate();
            }
        }

        public void FixedUpdateLookTitan()
        {
            Ray ray = SceneLoader.CurrentCamera.Camera.ScreenPointToRay(Input.mousePosition);
            LayerMask mask = PhysicsLayer.GetMask(PhysicsLayer.EntityDetection);
            RaycastHit[] hitArr = Physics.RaycastAll(ray, 200f, mask.value);
            if (hitArr.Length == 0)
                return;
            List<RaycastHit> hitList = new List<RaycastHit>(hitArr);
            hitList.Sort((x, y) => x.distance.CompareTo(y.distance));
            int maxCount = Math.Min(hitList.Count, 3);
            for (int i = 0; i < maxCount; i++)
            {
                var entity = hitList[i].collider.GetComponent<TitanEntityDetection>();
                entity.Owner.TitanColliderToggler.RegisterLook();
            }
        }

        private void LateUpdateTilt()
        {
            if (IsMainCharacter() && SettingsManager.GeneralSettings.CameraTilt.Value)
            {
                Quaternion rotation;
                Vector3 left = Vector3.zero;
                Vector3 right = Vector3.zero;
                if (_launchLeft && IsHookedLeft())
                    left = HookLeft.GetHookPosition();
                if (_launchRight && IsHookedRight())
                    right = HookRight.GetHookPosition();
                Vector3 target = Vector3.zero;
                if (left.magnitude != 0f && right.magnitude == 0f)
                    target = left;
                else if (right.magnitude != 0f && left.magnitude == 0f)
                    target = right;
                else if (left.magnitude != 0f && right.magnitude != 0f)
                    target = 0.5f * (left + right);
                Transform camera = SceneLoader.CurrentCamera.Cache.Transform;
                Vector3 projectUp = Vector3.Project(target - Cache.Transform.position, camera.up);
                Vector3 projectRight = Vector3.Project(target - Cache.Transform.position, camera.right);
                if (target.magnitude > 0f)
                {
                    Vector3 projectDirection = projectUp + projectRight;
                    float angle = Vector3.Angle(target - Cache.Transform.position, Cache.Rigidbody.velocity) * 0.005f;
                    Vector3 finalRight = camera.right + projectRight.normalized;
                    float finalAngle = Vector3.Angle(projectUp, projectDirection) * angle;
                    rotation = Quaternion.Euler(camera.rotation.eulerAngles.x, camera.rotation.eulerAngles.y, (finalRight.magnitude >= 1f) ? -finalAngle : finalAngle);
                }
                else
                    rotation = Quaternion.Euler(camera.rotation.eulerAngles.x, camera.rotation.eulerAngles.y, 0f);
                camera.rotation = Quaternion.Lerp(camera.rotation, rotation, Time.deltaTime * 2f);
            }
        }

        private void LateUpdateGun()
        {
            if (Setup.Weapon == HumanWeapon.Gun)
            {
                if (_leftArmAim || _rightArmAim)
                {
                    Vector3 direction = _gunTarget - Cache.Transform.position;
                    float angle = -Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
                    float delta = -Mathf.DeltaAngle(angle, Cache.Transform.rotation.eulerAngles.y - 90f);
                    GunHeadMovement();
                    if (!IsHookedLeft() && _leftArmAim && delta < 40f && delta > -90f)
                        LeftArmAim(_gunTarget);
                    if (!IsHookedRight() && _rightArmAim && delta > -40f && delta < 90f)
                        RightArmAim(_gunTarget);
                }
                else if (!Grounded)
                {
                    HumanCache.HandL.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    HumanCache.HandR.localRotation = Quaternion.Euler(-90f, 0f, 0f);
                }
                if (IsHookedLeft())
                    LeftArmAim(HookLeft.GetHookPosition());
                if (IsHookedRight())
                    RightArmAim(HookRight.GetHookPosition());
            }
        }

        private void GunHeadMovement()
        {
            return;
            Vector3 position = Cache.Transform.position;
            float x = Mathf.Sqrt(Mathf.Pow(_gunTarget.x - position.x, 2f) + Mathf.Pow(_gunTarget.z - position.z, 2f));
            var originalRotation = Cache.Transform.rotation;
            var targetRotation = originalRotation;
            Vector3 euler = originalRotation.eulerAngles;
            Vector3 direction = _gunTarget - position;
            float angle = -Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            float deltaY = -Mathf.DeltaAngle(angle, euler.y - 90f);
            deltaY = Mathf.Clamp(deltaY, -40f, 40f);
            float y = HumanCache.Neck.position.y - _gunTarget.y;
            float deltaX = Mathf.Atan2(y, x) * Mathf.Rad2Deg;
            deltaX = Mathf.Clamp(deltaX, -40f, 30f);
            targetRotation = Quaternion.Euler(euler.x + deltaX, euler.y + deltaY, euler.z);
            Cache.Transform.rotation = Quaternion.Lerp(originalRotation, targetRotation, Time.deltaTime * 60f);
        }

        private void LeftArmAim(Vector3 target)
        {

        }

        private void RightArmAim(Vector3 target)
        {

        }

        private void ResetAnimationSpeed()
        {
        }

        protected override void SetColliders()
        {
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                if (c.name == "checkBox")
                    c.gameObject.layer = PhysicsLayer.Hitbox;
                else
                    c.gameObject.layer = PhysicsLayer.NoCollision;
            }
            gameObject.layer = PhysicsLayer.Human;
        }

        [RPC]
        public void SetupRPC(string customSetJson, int humanWeapon, PhotonMessageInfo info)
        {
            if (WeaponData == null)
                WeaponData = JSON.Parse(File.ReadAllText(Application.dataPath + "/TesterData/HumanWeapon.json"));
            if (info.sender != Cache.PhotonView.owner)
                return;
            HumanCustomSet set = new HumanCustomSet();
            set.DeserializeFromJsonString(customSetJson);
            Setup.Load(set, (HumanWeapon)humanWeapon, false);
            HookLeft = new HookUseable(this, true, humanWeapon == (int)HumanWeapon.Gun);
            HookRight = new HookUseable(this, false, humanWeapon == (int)HumanWeapon.Gun);
            CurrentGas = MaxGas = set.Gas.Value;
            Cache.Rigidbody.mass = 0.5f - (set.Acceleration.Value - 100) * 0.001f;
            RunSpeed = set.Speed.Value / 10f;
            StandAnimation = HumanAnimations.StandFemale;
            RunAnimation = HumanAnimations.Run;
            if (humanWeapon == (int)HumanWeapon.Gun)
                StandAnimation = HumanAnimations.StandGun;
            else if (Setup.CustomSet.Sex.Value == (int)HumanSex.Male)
                StandAnimation = HumanAnimations.StandMale;
            if (humanWeapon == (int)HumanWeapon.Blade)
                Weapon = new BladeWeapon(this, 100f, 5);
            else if (humanWeapon == (int)HumanWeapon.Gun)
                Weapon = new GunWeapon(this, 10, 4, 1f);
            else if (humanWeapon == (int)HumanWeapon.ThunderSpear)
                Weapon = new ThunderSpearWeapon(this, 10, -1, 1f, 30f, 600f, 2f);
            FinishSetup = true;
        }

        protected void LoadSkin()
        {
            if (IsMine())
            {
                if (SettingsManager.CustomSkinSettings.Human.SkinsEnabled.Value)
                {
                    HumanCustomSkinSet set = (HumanCustomSkinSet)SettingsManager.CustomSkinSettings.Human.GetSelectedSet();
                    string url = string.Join(",", new string[] { set.Horse.Value, set.Hair.Value, set.Eye.Value, set.Glass.Value, set.Face.Value,
                set.Skin.Value, set.Costume.Value, set.Logo.Value, set.GearL.Value, set.GearR.Value, set.Gas.Value, set.Hoodie.Value,
                    set.WeaponTrail.Value, set.ThunderSpearL.Value, set.ThunderSpearR.Value, set.HookL.Value, set.HookLTiling.Value.ToString(),
                set.HookR.Value, set.HookRTiling.Value.ToString()});
                    int viewID = -1;
                    if (Horse != null)
                    {
                        viewID = Horse.gameObject.GetPhotonView().viewID;
                    }
                    Cache.PhotonView.RPC("LoadSkinRPC", PhotonTargets.AllBuffered, new object[] { viewID, url });
                }
            }
        }

        [RPC]
        public void LoadSkinRPC(int horse, string url, PhotonMessageInfo info)
        {
            if (info.sender != photonView.owner)
                return;
            HumanCustomSkinSettings settings = SettingsManager.CustomSkinSettings.Human;
            if (settings.SkinsEnabled.Value && (!settings.SkinsLocal.Value || photonView.isMine))
            {
                StartCoroutine(_customSkinLoader.LoadSkinsFromRPC(new object[] { horse, url }));
            }
        }

        [RPC]
        public void SetHookStateRPC(bool left, int hookId, int state, PhotonMessageInfo info)
        {
            if (left)
                HookLeft.Hooks[hookId].OnSetHookState(state, info);
            else
                HookRight.Hooks[hookId].OnSetHookState(state, info);
        }

        [RPC]
        public void SetHookingRPC(bool left, int hookId, Vector3 baseVelocity, Vector3 relativeVelocity, PhotonMessageInfo info)
        {
            if (left)
                HookLeft.Hooks[hookId].OnSetHooking(baseVelocity, relativeVelocity, info);
            else
                HookRight.Hooks[hookId].OnSetHooking(baseVelocity, relativeVelocity, info);
        }

        [RPC]
        public void SetHookedRPC(bool left, int hookId, Vector3 position, int viewId, int objectId, PhotonMessageInfo info)
        {
            if (left)
                HookLeft.Hooks[hookId].OnSetHooked(position, viewId, objectId, info);
            else
                HookRight.Hooks[hookId].OnSetHooked(position, viewId, objectId, info);
        }

        [RPC]
        public void SetSmokeRPC(bool active, PhotonMessageInfo info)
        {
            if (info.sender != Cache.PhotonView.owner)
                return;
            HumanCache.Smoke.enableEmission = active;
        }

        public void SetThunderSpears(bool hasLeft, bool hasRight)
        {
            photonView.RPC("SetThunderSpearsRPC", PhotonTargets.All, new object[] { hasLeft, hasRight });
        }

        [RPC]
        public void SetThunderSpearsRPC(bool hasLeft, bool hasRight, PhotonMessageInfo info)
        {
            if (info.sender != photonView.owner)
                return;
            if (Setup.ThunderSpearLModel != null)
                Setup.ThunderSpearLModel.SetActive(hasLeft);
            if (Setup.ThunderSpearRModel != null)
                Setup.ThunderSpearRModel.SetActive(hasRight);
        }

        public void OnHooked(bool left, Vector3 position)
        {
            if (State == HumanState.Grab)
                return;
            if (left)
            {
                _launchLeft = true;
                _launchLeftTime = 0f;
            }
            else
            {
                _launchRight = true;
                _launchRightTime = 0f;
            }
            if (MountState == HumanMountState.Horse)
                Unmount();
            if (State != HumanState.Attack)
                Idle();
            Vector3 v = (position - Cache.Transform.position).normalized * 20f;
            if (IsHookedLeft() && IsHookedRight())
                v *= 0.8f;
            FalseAttack();
            Idle();
            if (Setup.Weapon == HumanWeapon.Gun)
                CrossFade("AHSS_hook_forward_both", 0.1f);
            else if (left && !IsHookedRight())
                CrossFade("air_hook_l_just", 0.1f);
            else if (!left && !IsHookedLeft())
                CrossFade("air_hook_r_just", 0.1f);
            else
            {
                CrossFade(HumanAnimations.Dash, 0.1f);
            }
            Vector3 force = v;
            if (v.y < 30f)
                force += Vector3.up * (30f - v.y);
            if (position.y >= Cache.Transform.position.y)
                force += Vector3.up * (position.y - Cache.Transform.position.y) * 10f;
            Cache.Rigidbody.AddForce(force);
            TargetAngle = Mathf.Atan2(force.x, force.z) * Mathf.Rad2Deg;
            _targetRotation = GetTargetRotation();
            Cache.Transform.rotation = _targetRotation;
            Cache.Rigidbody.rotation = _targetRotation;
            HumanCache.Sparks.enableEmission = false;
            _cancelGasDisable = true;
        }

        private void SetInterpolation(bool interpolate)
        {
            if (IsMine())
            {
                if (interpolate && SettingsManager.GraphicsSettings.InterpolationEnabled.Value)
                    Cache.Rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
                else
                    Cache.Rigidbody.interpolation = RigidbodyInterpolation.None;
            }
        }

        private float GetReelAxis()
        {
            if (ReelInAxis != 0f)
                return ReelInAxis;
            return ReelOutAxis;
        }

        private float GetLeanAngle(Vector3 hookPosition, bool left)
        {
            if (Setup.Weapon != HumanWeapon.Gun && State == HumanState.Attack)
                return 0f;
            float height = hookPosition.y - Cache.Transform.position.y;
            float dist = Vector3.Distance(hookPosition, Cache.Transform.position);
            float angle = Mathf.Acos(height / dist) * Mathf.Rad2Deg * 0.1f * (1f + Mathf.Pow(Cache.Rigidbody.velocity.magnitude, 0.2f));
            Vector3 v = hookPosition - Cache.Transform.position;
            float current = Mathf.Atan2(v.x, v.z) * Mathf.Rad2Deg;
            float target = Mathf.Atan2(Cache.Rigidbody.velocity.x, Cache.Rigidbody.velocity.z) * Mathf.Rad2Deg;
            float delta = Mathf.DeltaAngle(current, target);
            angle += Mathf.Abs(delta * 0.5f);
            if (State != HumanState.Attack)
                angle = Mathf.Min(angle, 80f);
            _leanLeft = delta > 0f;
            if (Setup.Weapon == HumanWeapon.Gun)
                return angle * (delta >= 0f ? 1f : -1f);
            float multiplier = 0.5f;
            if ((left && delta < 0f) || (!left && delta > 0f))
                multiplier = 0.1f;
            return angle * (delta >= 0f ? multiplier : -multiplier);
        }

        public void StartBladeSwing()
        {
            if (_needLean)
            {
                if (SettingsManager.InputSettings.General.Left.GetKey())
                    AttackAnimation = (UnityEngine.Random.Range(0, 100) >= 50) ? "attack1_hook_l1" : "attack1_hook_l2";
                else if (SettingsManager.InputSettings.General.Right.GetKey())
                    AttackAnimation = (UnityEngine.Random.Range(0, 100) >= 50) ? "attack1_hook_r1" : "attack1_hook_r2";
                else if (_leanLeft)
                    AttackAnimation = (UnityEngine.Random.Range(0, 100) >= 50) ? "attack1_hook_l1" : "attack1_hook_l2";
                else
                    AttackAnimation = (UnityEngine.Random.Range(0, 100) >= 50) ? "attack1_hook_r1" : "attack1_hook_r2";
            }
            else if (SettingsManager.InputSettings.General.Left.GetKey())
                AttackAnimation = "attack2";
            else if (SettingsManager.InputSettings.General.Right.GetKey())
                AttackAnimation = "attack1";
            if (HookLeft.IsHooked() && HookLeft.GetHookParent() != null)
            {
                BaseCharacter character = HookLeft.GetHookCharacter();
                if (character != null && character is BaseTitan)
                    AttackAnimation = GetBladeAnimationTarget(((BaseTitan)character).BaseTitanCache.Neck);
                else
                    AttackAnimation = GetBladeAnimationMouse();
            }
            else if (HookRight.IsHooked() && HookRight.GetHookParent() != null)
            {
                BaseCharacter character = HookRight.GetHookCharacter();
                if (character != null && character is BaseTitan)
                    AttackAnimation = GetBladeAnimationTarget(((BaseTitan)character).BaseTitanCache.Neck);
                else
                    AttackAnimation = GetBladeAnimationMouse();
            }
            else
            {
                BaseTitan titan = FindNearestTitan();
                if (titan != null)
                    AttackAnimation = GetBladeAnimationTarget(titan.BaseTitanCache.Neck);
                else
                    AttackAnimation = GetBladeAnimationMouse();
            }
            if (Grounded)
            {
                Cache.Rigidbody.AddForce(Cache.Transform.forward * 200f);
            }
            PlayAnimation(AttackAnimation);
            _attackButtonRelease = false;
            State = HumanState.Attack;
            if (Grounded || AttackAnimation == "attack3_1" || AttackAnimation == "attack5" || AttackAnimation == "special_petra")
            {
                _attackRelease = true;
                _attackButtonRelease = true;
            }
            else
                _attackRelease = false;
            HumanCache.Sparks.enableEmission = false;
        }

        private string GetBladeAnimationMouse()
        {
            if (Input.mousePosition.x < (Screen.width * 0.5))
                return "attack2";
            else
                return "attack1";
        }

        private string GetBladeAnimationTarget(Transform target)
        {
            Vector3 v = target.position - Cache.Transform.position;
            float current = -Mathf.Atan2(v.z, v.x) * Mathf.Rad2Deg;
            float delta = -Mathf.DeltaAngle(current, Cache.Transform.rotation.eulerAngles.y - 90f);
            if (((Mathf.Abs(delta) < 90f) && (v.magnitude < 6f)) && ((target.position.y <= (Cache.Transform.position.y + 2f)) && (target.position.y >= (Cache.Transform.position.y - 5f))))
                return "attack4";
            else if (delta > 0f)
                return "attack1";
            else
                return "attack2";
        }

        public void StartGunShoot(bool left)
        {
            
            // this.baseTransform.rotation = Quaternion.Lerp(this.baseTransform.rotation, this.gunDummy.transform.rotation, Time.deltaTime * 30f);
        }

        private BaseTitan FindNearestTitan()
        {
            float nearestDistance = float.PositiveInfinity;
            BaseTitan nearestTitan = null;
            foreach (BaseTitan titan in _inGameManager.Titans)
            {
                float distance = Vector3.Distance(Cache.Transform.position, titan.Cache.Transform.position);
                if (distance < nearestDistance)
                {
                    nearestTitan = titan;
                    nearestDistance = distance;
                }
            }
            foreach (BaseTitan titan in _inGameManager.Shifters)
            {
                float distance = Vector3.Distance(Cache.Transform.position, titan.Cache.Transform.position);
                if (distance < nearestDistance)
                {
                    nearestTitan = titan;
                    nearestDistance = distance;
                }
            }
            return nearestTitan;
        }


        private void FalseAttack()
        {
            if (Setup.Weapon == HumanWeapon.Gun || Setup.Weapon == HumanWeapon.ThunderSpear)
            {
               
            }
            else
            {
                ToggleBladeTrails(false, 0.2f);
                if (!_attackRelease)
                {
                    ContinueAnimation();
                    _attackRelease = true;
                }
            }
        }

        private void ContinueAnimation()
        {
            if (!_animationStopped)
                return;
            _animationStopped = false;
            Cache.PhotonView.RPC("ContinueAnimationRPC", PhotonTargets.All, new object[0]);
        }

        [RPC]
        private void ContinueAnimationRPC(PhotonMessageInfo info)
        {
            if (info.sender != Cache.PhotonView.owner)
                return;
            foreach (AnimationState animation in Cache.Animation)
            {
                animation.speed = 1f;
            }
            CustomAnimationSpeed();
            // PlayAnimation(GetCurrentPlayingClip());
        }

        private void PauseAnimation()
        {
            if (_animationStopped)
                return;
            _animationStopped = true;
            Cache.PhotonView.RPC("PauseAnimationRPC", PhotonTargets.All, new object[0]);
        }

        [RPC]
        public void PauseAnimationRPC(PhotonMessageInfo info)
        {
            if (info.sender != Cache.PhotonView.owner)
                return;
            foreach (AnimationState animation in Cache.Animation)
                animation.speed = 0f;
        }

        private void CustomAnimationSpeed()
        {
            Cache.Animation["attack5"].speed = 1.85f;
            Cache.Animation["changeBlade"].speed = 1.2f;
            Cache.Animation["air_release"].speed = 0.6f;
            Cache.Animation["changeBlade_air"].speed = 0.8f;
            Cache.Animation["AHSS_gun_reload_both"].speed = 0.38f;
            Cache.Animation["AHSS_gun_reload_both_air"].speed = 0.5f;
            Cache.Animation["AHSS_gun_reload_l"].speed = 0.4f;
            Cache.Animation["AHSS_gun_reload_l_air"].speed = 0.5f;
            Cache.Animation["AHSS_gun_reload_r"].speed = 0.4f;
            Cache.Animation["AHSS_gun_reload_r_air"].speed = 0.5f;
        }

        private bool HasHook()
        {
            return HookLeft.HasHook() || HookRight.HasHook();
        }

        private bool IsHookedAny()
        {
            return IsHookedLeft() || IsHookedRight();
        }

        private bool IsHookedLeft()
        {
            return HookLeft.IsHooked();
        }

        private bool IsHookedRight()
        {
            return HookRight.IsHooked();
        }

        private bool IsFrontGrounded()
        {
            return Physics.Raycast(Cache.Transform.position + Cache.Transform.up * 1f, Cache.Transform.forward, 1f, GroundMask.value);
        }

        private bool IsPressDirectionTowardsHero()
        {
            if (!HasDirection)
                return false;
            return (Mathf.Abs(Mathf.DeltaAngle(TargetAngle, Cache.Transform.rotation.eulerAngles.y)) < 45f);
        }

        private bool IsUpFrontGrounded()
        {
            return Physics.Raycast(Cache.Transform.position + Cache.Transform.up * 3f, Cache.Transform.forward, 1.2f, GroundMask.value);
        }

        public bool IsFiringThunderSpear()
        {
            return Setup.Weapon == HumanWeapon.ThunderSpear && (Cache.Animation.IsPlaying("AHSS_shoot_r") || Cache.Animation.IsPlaying("AHSS_shoot_l"));
        }

        private void UseGas(float amount)
        {
            CurrentGas -= amount;
            CurrentGas = Mathf.Max(CurrentGas, 0f);
        }

        private void ToggleBladeTrails(bool toggle, float fadeTime = 0f)
        {
            if (toggle)
            {
                if (SettingsManager.GraphicsSettings.WeaponTrailEnabled.Value)
                {
                    Setup.LeftTrail1.Activate();
                    Setup.LeftTrail2.Activate();
                    Setup.RightTrail1.Activate();
                    Setup.RightTrail2.Activate();
                }
            }
            else
            {
                if (fadeTime == 0f)
                {
                    Setup.LeftTrail1.Deactivate();
                    Setup.LeftTrail2.Deactivate();
                    Setup.RightTrail1.Deactivate();
                    Setup.RightTrail2.Deactivate();
                }
                else
                {
                    Setup.LeftTrail1.StopSmoothly(fadeTime);
                    Setup.LeftTrail2.StopSmoothly(fadeTime);
                    Setup.RightTrail1.StopSmoothly(fadeTime);
                    Setup.RightTrail2.StopSmoothly(fadeTime);
                }
            }
        }
    }

    public enum HumanState
    {
        Idle,
        Attack,
        GroundDodge,
        AirDodge,
        Reload,
        FillGas,
        Die,
        Grab,
        EmoteAction,
        Slide,
        Run,
        Land
    }

    public enum HumanMountState
    {
        None,
        Horse,
        Cannon
    }

    public enum HumanWeapon
    {
        Blade,
        Gun,
        ThunderSpear
    }

    public enum HumanDashDirection
    {
        None,
        Forward,
        Back,
        Left,
        Right
    }
}
