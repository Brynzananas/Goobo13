using EntityStates;
using HG;
using MonoMod.RuntimeDetour;
using R2API;
using RoR2;
using RoR2.Audio;
using RoR2.CharacterAI;
using RoR2.Orbs;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UIElements;
using UnityEngine.UIElements.StyleSheets;

namespace Goobo13
{
    public abstract class BaseGooboState : BaseSkillState
    {
        public DamageSource GetDamageSource()
        {
            if (skillLocator == null || activatorSkillSlot == null) return DamageSource.NoneSpecified;
            if (skillLocator.primary == activatorSkillSlot) return DamageSource.Primary;
            if (skillLocator.secondary == activatorSkillSlot) return DamageSource.Secondary;
            if (skillLocator.utility == activatorSkillSlot) return DamageSource.Utility;
            if (skillLocator.special == activatorSkillSlot) return DamageSource.Special;
            return DamageSource.NoneSpecified;
        }
    }
    public class HandlePunch : BaseGooboState, SteppedSkillDef.IStepSetter
    {
        public int step;
        public override void OnEnter()
        {
            base.OnEnter();
            if (!isAuthority) return;
            if (step >= 2)
            {
                outer.SetState(new SuperPunch() { activatorSkillSlot = activatorSkillSlot });
            }
            else
            {
                outer.SetState(new Punch() { step = step >= 1, activatorSkillSlot = activatorSkillSlot });
            }
        }
        public void SetStep(int i) => step = i;
    }
    public class Punch : BaseGooboState
    {
        public static float baseDamageCoefficient => PunchConfig.damageCoefficient.Value;
        public static float procCoefficient => PunchConfig.procCoefficient.Value;
        public static float baseDuration => PunchConfig.duration.Value;
        public static float baseTimeToAttack => PunchConfig.timeToAttack.Value;
        public static float baseSelfPush => PunchConfig.selfPush.Value;
        public static DamageType damageType => PunchConfig.damageType.Value;
        public static DamageTypeExtended damageTypeExtended => PunchConfig.damageTypeExtended.Value;
        public static float baseUpTransitionSpeed = 0.05f;
        public static float baseDownTransitionSpeed = 0.05f;
        public static float effectScale = 3f;
        public static float effectRotation = -35f;
        public float upTransitionSpeed;
        public float downTransitionSpeed;
        public float damageCoefficient;
        public float duration;
        public float timeToAttack;
        public float remainingTime;
        public float selfPush;
        public HitBoxGroup hitBoxGroup;
        public OverlapAttack overlapAttack;
        public Vector3 forceVector;
        public float pushAwayForce;
        public bool step;
        private bool fired;
        private float rootMotionVelocity;
        public override void OnEnter()
        {
            base.OnEnter();
            SetValues();
            PlayCrossfade("UpperBody, Override", step ? "Punch2Up" : "Punch1Up", "UpperBody.playbackRate", Mathf.Max(0.01f, timeToAttack), upTransitionSpeed);
            if (isAuthority)
            {
                StartAimMode();
                hitBoxGroup = FindHitBoxGroup("Punch");
                if (hitBoxGroup)
                {
                    overlapAttack = new OverlapAttack();
                    overlapAttack.attacker = gameObject;
                    overlapAttack.damage = damageCoefficient * damageStat;
                    overlapAttack.damageColorIndex = DamageColorIndex.Default;
                    overlapAttack.damageType = new DamageTypeCombo(DamageType.Generic, DamageTypeExtended.Generic, GetDamageSource());
                    overlapAttack.forceVector = forceVector;
                    overlapAttack.hitBoxGroup = hitBoxGroup;
                    overlapAttack.hitEffectPrefab = null;
                    NetworkSoundEventDef networkSoundEventDef = null;
                    overlapAttack.impactSound = ((networkSoundEventDef != null) ? networkSoundEventDef.index : NetworkSoundEventIndex.Invalid);
                    overlapAttack.inflictor = base.gameObject;
                    overlapAttack.isCrit = RollCrit();
                    overlapAttack.procChainMask = default(ProcChainMask);
                    overlapAttack.pushAwayForce = pushAwayForce;
                    overlapAttack.procCoefficient = procCoefficient;
                    overlapAttack.teamIndex = GetTeam();
                }
            }
        }
        public void SetValues()
        {
            damageCoefficient = baseDamageCoefficient;
            duration = baseDuration / characterBody.attackSpeed;
            selfPush = baseSelfPush * characterBody.attackSpeed;
            timeToAttack = baseTimeToAttack / characterBody.attackSpeed;
            upTransitionSpeed = baseUpTransitionSpeed / characterBody.attackSpeed;
            downTransitionSpeed = baseDownTransitionSpeed / characterBody.attackSpeed;
            remainingTime = duration - timeToAttack;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= timeToAttack)
            {
                if (isAuthority)
                {
                    overlapAttack.Fire();
                    //if (characterMotor)
                    //{
                    //    Vector3 velocityDirection = GetAimRay().direction;
                    //    velocityDirection.y = 0f;
                    //    velocityDirection.Normalize();
                    //    float pushPower = selfPush / remainingTime;
                    //    characterMotor.rootMotion += velocityDirection * pushPower * Time.fixedDeltaTime;
                    //}
                }
                if (!fired)
                {
                    //if (isAuthority)
                    //{
                        //Vector3 velocity = (characterDirection ? GetAimRay().direction : characterDirection.forward) * selfPush;
                        //if (characterMotor)
                        //{
                        //    characterMotor.velocity += velocity;
                        //}
                        //else if (rigidbody)
                        //{
                        //    rigidbody.velocity += velocity;
                        //}
                    //}

                    PlayCrossfade("UpperBody, Override", step ? "Punch2Down" : "Punch1Down", "UpperBody.playbackRate", Mathf.Max(0.01f, remainingTime), downTransitionSpeed);
                    Transform transform = characterBody.aimOriginTransform;
                    Transform mdlTransform = characterBody.modelLocator && characterBody.modelLocator.modelTransform ? characterBody.modelLocator.modelTransform : this.transform;
                    Vector3 rotation = new Vector3(mdlTransform.eulerAngles.x, mdlTransform.eulerAngles.y, mdlTransform.eulerAngles.z + (step ? effectRotation : 180f - effectRotation));
                    EffectData effectData = new EffectData()
                    {
                        origin = transform.position,
                        rootObject = transform.gameObject,
                        genericFloat = duration - timeToAttack,
                        rotation = Quaternion.Euler(rotation),
                        scale = effectScale
                    };
                    EffectManager.SpawnEffect(Assets.GooboPunchEffect.prefab, effectData, false);
                    fired = true;
                }
            }
            if (!isAuthority) return;
            overlapAttack.damage = damageCoefficient * characterBody.damage;
            StartAimMode();
            if (fixedAge >= duration) outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
    public class SuperPunch : BaseGooboState
    {
        public static float baseDamageCoefficient => SuperPunchConfig.damageCoefficient.Value;
        public static float procCoefficient => SuperPunchConfig.procCoefficient.Value;
        public static float baseDuration => SuperPunchConfig.duration.Value;
        public static float baseTimeToAttack => SuperPunchConfig.timeToAttack.Value;
        public static float baseRadius => SuperPunchConfig.radius.Value;
        public static float force => SuperPunchConfig.force.Value;
        public static DamageType damageType => SuperPunchConfig.damageType.Value;
        public static DamageTypeExtended damageTypeExtended => SuperPunchConfig.damageTypeExtended.Value;
        public static BlastAttack.FalloffModel falloffModel => SuperPunchConfig.falloffModel.Value;
        public static float baseUpTransitionSpeed = 0.05f;
        public static float baseDownTransitionSpeed = 0.05f;
        public float upTransitionSpeed;
        public float downTransitionSpeed;
        public float damageCoefficient;
        public float duration;
        public float radius;
        public float timeToAttack;
        public Vector3 forceVector;
        public float pushAwayForce;
        private bool fired;
        public override void OnEnter()
        {
            base.OnEnter();
            SetValues();
            PlayCrossfade("UpperBody, Override", "Punch3Up", "UpperBody.playbackRate", Mathf.Max(0.01f, timeToAttack), upTransitionSpeed);
            if (isAuthority)
            {
                StartAimMode();
            }
        }
        public void SetValues()
        {
            damageCoefficient = baseDamageCoefficient;
            duration = baseDuration / characterBody.attackSpeed;
            timeToAttack = baseTimeToAttack / characterBody.attackSpeed;
            radius = baseRadius;
            upTransitionSpeed = baseUpTransitionSpeed / characterBody.attackSpeed;
            downTransitionSpeed = baseDownTransitionSpeed / characterBody.attackSpeed;
        }
        public void Fire(Vector3 position)
        {
            PlayCrossfade("UpperBody, Override", "Punch3Down", "UpperBody.playbackRate", Mathf.Max(0.01f, duration - timeToAttack), downTransitionSpeed);
            if (isAuthority)
            {
                BlastAttack blastAttack = new BlastAttack()
                {
                    attacker = gameObject,
                    baseDamage = characterBody.damage * damageCoefficient,
                    baseForce = force,
                    crit = RollCrit(),
                    damageType = new DamageTypeCombo(damageType, damageTypeExtended, GetDamageSource()),
                    falloffModel = falloffModel,
                    damageColorIndex = DamageColorIndex.Default,
                    inflictor = gameObject,
                    losType = BlastAttack.LoSType.None,
                    procCoefficient = procCoefficient,
                    radius = radius,
                    teamIndex = GetTeam(),
                    position = position
                };
                blastAttack.AddModdedDamageType(Assets.ChanceToSpawnGooboDamageType);
                blastAttack.Fire();
                //EffectData effectData = new()
                //{
                //    origin = blastAttack.position,
                //    scale = blastAttack.radius,
                //};
                //EffectManager.SpawnEffect(Assets.GooboExplosion.prefab, effectData, true);
            }
            fired = true;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!fired && fixedAge >= timeToAttack) Fire(characterBody.footPosition);
            if (!isAuthority) return;
            if (fixedAge >= duration) outer.SetNextStateToMain();
        }
        public override void OnExit()
        {
            base.OnExit();
            PlayCrossfade("UpperBody, Override", "Punch3Transition", "UpperBody.playbackRate", Mathf.Max(0.01f, duration - timeToAttack), downTransitionSpeed);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
    public class ThrowGrenade : BaseGooboState
    {
        public static float damageCoefficient => ThrowGrenadeConfig.damageCoefficient.Value;
        public static float baseTimeToThrow => ThrowGrenadeConfig.timeToAttack.Value;
        public static float baseDuration => ThrowGrenadeConfig.duration.Value;
        public static float force = ThrowGrenadeConfig.force.Value;
        public static DamageType damageType => ThrowGrenadeConfig.damageType.Value;
        public static DamageTypeExtended damageTypeExtended => ThrowGrenadeConfig.damageTypeExtended.Value;
        public static float baseUpTransitionSpeed = 0.05f;
        public static float baseDownTransitionSpeed = 0.05f;
        public float upTransitionSpeed;
        public float downTransitionSpeed;
        public float timeToThrow;
        public float duration;
        public GameObject currentGrenade;
        private bool fired;
        public override void OnEnter()
        {
            base.OnEnter();
            StartAimMode();
            SetValues();
            PlayCrossfade("UpperBody, Override", "ThrowUp", "UpperBody.playbackRate", timeToThrow, upTransitionSpeed);
            if (isAuthority)
            {
                GooboRandomGrenadeSkillDef.InstanceData instanceData = activatorSkillSlot && activatorSkillSlot.skillInstanceData != null ? activatorSkillSlot.skillInstanceData as GooboRandomGrenadeSkillDef.InstanceData : null;
                if (instanceData != null)
                {
                    currentGrenade = instanceData.currentGrenade;
                }
            }
        }
        public void Fire(Ray ray)
        {
            PlayAnimation("UpperBody, Override", "ThrowDown", "UpperBody.playbackRate", duration - timeToThrow, downTransitionSpeed);
            if (isAuthority)
            {
                DamageTypeCombo damageTypeCombo = new DamageTypeCombo(damageType, damageTypeExtended, GetDamageSource());
                if (currentGrenade == Assets.GooboGrenadeType1Projectile) damageTypeCombo.AddModdedDamageType(Assets.GooboCorrosionDamageType); // This is stupid
                characterBody.SetBuffCount(Assets.GooboCorrosionCharge.buffIndex, 1);
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = currentGrenade,
                    position = ray.origin,
                    rotation = Util.QuaternionSafeLookRotation(ray.direction),
                    owner = gameObject,
                    damage = characterBody.damage * damageCoefficient,
                    force = force,
                    crit = RollCrit(),
                    damageTypeOverride = new DamageTypeCombo?(damageTypeCombo),
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
            fired = true;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!fired && fixedAge >= timeToThrow) Fire(GetAimRay());
            if (!isAuthority) return;
            if (fixedAge >= duration) outer.SetNextStateToMain();
        }
        public void SetValues()
        {
            timeToThrow = baseTimeToThrow / characterBody.attackSpeed;
            duration = baseDuration / characterBody.attackSpeed;
            upTransitionSpeed = baseUpTransitionSpeed / characterBody.attackSpeed;
            downTransitionSpeed = baseDownTransitionSpeed / characterBody.attackSpeed;
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
    public class Decoy : BaseState
    {
        public static float cloakDuration => DecoyConfig.duration.Value;
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                //Vector3 vector3 = Utils.GetClosestNodePosition(characterBody.footPosition, characterBody.hullClassification, float.PositiveInfinity, out Vector3 nodePosition) ? nodePosition : characterBody.footPosition;
                Vector3 directionVector = characterDirection ? characterDirection.forward : transform.forward;
                CharacterMaster decoyMaster = Utils.SpawnGooboClone(characterBody.master, transform.position, Quaternion.LookRotation(directionVector));
                CharacterBody decoyBody = decoyMaster ? decoyMaster.GetBody() : null;
                if (decoyBody)
                    foreach (BaseAI baseAI in Hooks.baseAIs)
                    {
                        if (baseAI == null || baseAI.currentEnemy == null || baseAI.currentEnemy.gameObject == null) continue;
                        if (baseAI.currentEnemy.gameObject == gameObject)
                        {
                            baseAI.currentEnemy.gameObject = decoyBody.gameObject;
                        }
                    }
                characterBody.AddTimedBuff(RoR2Content.Buffs.Cloak, cloakDuration);
                characterBody.AddTimedBuff(RoR2Content.Buffs.CloakSpeed, cloakDuration);
            }
            if (!isAuthority) return;
            outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
    public class FireMinions : BaseGooboState
    {
        public static GameObject projectile = Assets.GooboBall;
        public static float maxDistance = 32f;
        public static float damageCoefficient => FireMinionsConfig.damageCoefficient.Value;
        public static float procCoefficient => FireMinionsConfig.procCoefficient.Value;
        public static float force => FireMinionsConfig.force.Value;
        public static float timeToTarget => FireMinionsConfig.timeToTarget.Value;
        public static BlastAttack.FalloffModel falloffModel => FireMinionsConfig.falloffModel.Value;
        public static DamageType damageType => FireMinionsConfig.damageType.Value;
        public static DamageTypeExtended damageTypeExtended => FireMinionsConfig.damageTypeExtended.Value;
        public bool crit;
        public override void OnEnter()
        {
            base.OnEnter();
            crit = RollCrit();
            if (NetworkServer.active) ConvertMinionsToProjectiles();
            if (!isAuthority) return;
            outer.SetNextStateToMain();
        }
        public void ConvertMinionsToProjectiles()
        {
            GooboThrowGooboMinionsSkillDef.InstanceData instanceData = activatorSkillSlot == null || activatorSkillSlot.skillInstanceData == null ? null : activatorSkillSlot.skillInstanceData as GooboThrowGooboMinionsSkillDef.InstanceData;
            if (instanceData == null) return;
            GobooThrowGooboMinionsTracker gobooThrowGooboMinionsTracker = instanceData.gobooThrowGooboMinionsTracker;
            if (!gobooThrowGooboMinionsTracker) return;
            HurtBox trackingTarget = gobooThrowGooboMinionsTracker.trackingTarget;
            if (!trackingTarget) return;
            CharacterMaster characterMaster = characterBody.master;
            if (!characterMaster) return;
            MinionOwnership.MinionGroup minionGroup = null;
            for (int i = 0; i < MinionOwnership.MinionGroup.instancesList.Count; i++)
            {
                MinionOwnership.MinionGroup minionGroup2 = MinionOwnership.MinionGroup.instancesList[i];
                if (MinionOwnership.MinionGroup.instancesList[i].ownerId == characterMaster.netId)
                {
                    minionGroup = minionGroup2;
                    break;
                }
            }
            if (minionGroup == null) return;
            foreach (MinionOwnership minion in minionGroup.members)
            {
                if (minion == null) continue;
                CharacterMaster minionMaster = minion.GetComponent<CharacterMaster>();
                if (minionMaster == null) continue;
                CharacterBody minionBody = minionMaster.GetBody();
                if (minionBody == null || minionBody.bodyIndex != Assets.Goobo13CloneBodyIndex) continue;
                Vector3 position = trackingTarget.transform.position;
                Vector3 direction = position - minionBody.corePosition;
                float distance = direction.magnitude;
                //Ray ray = new Ray(position, direction.normalized);
                //if (!Physics.Raycast(ray, maxDistance, LayerIndex.world.mask)) continue;
                position = minionBody.corePosition;
                minionMaster.TrueKill();
                DamageTypeCombo damageTypeCombo = new DamageTypeCombo(damageType, damageTypeExtended, GetDamageSource());
                damageTypeCombo.AddModdedDamageType(Assets.GooboCorrosionDamageType);
                GooboOrb gooboOrb = new GooboOrb
                {
                    attacker = gameObject,
                    duration = timeToTarget,
                    gooboAmount = 0,
                    damage = characterBody.damage * damageCoefficient,
                    procCoefficient = procCoefficient,
                    crit = RollCrit(),
                    falloffModel = falloffModel,
                    force = force,
                    damageTypeCombo = damageTypeCombo,
                    origin = position,
                    radius = 3f,
                    teamIndex = GetTeam(),
                    target = trackingTarget,
                    visualPrefab = Assets.GooboOrb.prefab
                };
                OrbManager.instance.AddOrb(gooboOrb);
                //Fire(position, direction, distance);
            }
        }
        public void Fire(Vector3 position, Vector3 direction, float distance)
        {
            Vector2 vector2 = new Vector2(direction.x, direction.z) / distance;
            float verticalSpeed = Trajectory.CalculateInitialYSpeed(timeToTarget, direction.y);
            float horizontalSpeed = distance / timeToTarget;
            Vector3 vector3 = new Vector3(vector2.x * horizontalSpeed, verticalSpeed, vector2.y * horizontalSpeed);
            DamageTypeCombo damageTypeCombo = new DamageTypeCombo(damageType, damageTypeExtended, GetDamageSource());
            damageTypeCombo.AddModdedDamageType(Assets.GooboCorrosionDamageType);
            FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
            {
                projectilePrefab = projectile,
                position = position,
                rotation = Util.QuaternionSafeLookRotation(vector3.normalized),
                owner = gameObject,
                damage = characterBody.damage * damageCoefficient,
                force = force,
                crit = crit,
                speedOverride = vector3.magnitude,
                damageTypeOverride = new DamageTypeCombo?(damageTypeCombo),
            };
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
    public class GooboDeath : BaseState
    {
        public override void OnEnter()
        {
            base.OnEnter();
            if (base.modelLocator)
            {
                if (base.modelLocator.modelBaseTransform)
                {
                    EntityState.Destroy(base.modelLocator.modelBaseTransform.gameObject);
                }
                if (base.modelLocator.modelTransform)
                {
                    EntityState.Destroy(base.modelLocator.modelTransform.gameObject);
                }
            }
            EffectManager.SpawnEffect(Assets.GooboSplash.prefab, new EffectData
            {
                origin = base.transform.position,
                rotation = base.transform.rotation
            }, false);

            if (NetworkServer.active)
            {
                EntityState.Destroy(base.gameObject);
            }
        }
    }
    public class AimGooboMissile : BaseGooboState
    {
        public static float baseDuration => GooboMissileConfig.timeToAttack.Value;
        public static float maxDistance => GooboMissileConfig.distance.Value;
        public static float maxAngle = 25f;
        public static GameObject trackingPrefab = Assets.GooboCloneMissileTrackingIndicator;
        public float duration;
        public BullseyeSearch bullseyeSearch = new BullseyeSearch();
        public HurtBox targetHurtbox;
        public HealthComponent targetHealthComponent;
        public Indicator indicator;
        public void SearchTarget()
        {
            Ray aimRay = base.GetAimRay();
            bullseyeSearch.filterByDistinctEntity = true;
            bullseyeSearch.filterByLoS = true;
            bullseyeSearch.minDistanceFilter = 0f;
            bullseyeSearch.maxDistanceFilter = maxDistance;
            bullseyeSearch.minAngleFilter = 0f;
            bullseyeSearch.maxAngleFilter = maxAngle;
            bullseyeSearch.viewer = characterBody;
            bullseyeSearch.searchOrigin = aimRay.origin;
            bullseyeSearch.searchDirection = aimRay.direction;
            bullseyeSearch.sortMode = BullseyeSearch.SortMode.Angle;
            bullseyeSearch.teamMaskFilter = TeamMask.GetUnprotectedTeams(base.GetTeam());
            bullseyeSearch.RefreshCandidates();
            bullseyeSearch.FilterOutGameObject(base.gameObject);
            foreach (HurtBox hurtBox in bullseyeSearch.GetResults())
            {
                if (hurtBox.healthComponent && hurtBox.healthComponent.alive)
                {
                    targetHurtbox = hurtBox;
                    return;
                }
            }
            targetHurtbox = null;
        }
        public void SetValues()
        {
            duration = baseDuration / characterBody.attackSpeed;
        }
        public override void OnEnter()
        {
            base.OnEnter();
            SetValues();
            if (isAuthority)
            {
                indicator = new Indicator(gameObject, trackingPrefab);
                indicator.active = true;
            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (indicator != null)
            {
                indicator.active = false;
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            StartAimMode();
            if (isAuthority)
            {
                SearchTarget();
                if (fixedAge >= duration && !IsKeyDownAuthority())
                {
                    if (targetHurtbox && targetHurtbox.healthComponent)
                    {
                        NetworkInstanceId networkInstanceId = targetHurtbox.healthComponent.netId;
                        outer.SetNextState(new FireGooboMissile { networkInstanceId = networkInstanceId, hurtboxIndex = targetHurtbox.indexInGroup, damageSource = GetDamageSource(), targetHurtbox = targetHurtbox });
                        if (activatorSkillSlot) activatorSkillSlot.stock--;
                    }
                    else
                    {
                        outer.SetNextStateToMain();
                    }
                }
            }
        }
        public override void Update()
        {
            base.Update();
            if (indicator != null)
            {
                if (targetHurtbox)
                {
                    indicator.active = true;
                    indicator.targetTransform = targetHurtbox.transform;
                }
                else
                {
                    indicator.active = false;
                    indicator.targetTransform = null;
                }
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
    public class FireGooboMissile : BaseState
    {
        public static float damageCoefficient => GooboMissileConfig.damageCoefficient.Value;
        public static float procCoefficient => GooboMissileConfig.procCoefficient.Value;
        public static float force => GooboMissileConfig.force.Value;
        public static int baseGooboAmount => GooboMissileConfig.gooboAmount.Value;
        public static float baseArrivalTime => GooboMissileConfig.timeToTarget.Value;
        public static float baseDuration => GooboMissileConfig.duration.Value;
        public static float radius => GooboMissileConfig.radius.Value;
        public static BlastAttack.FalloffModel falloffModel => GooboMissileConfig.falloffModel.Value;
        public static DamageType damageType => GooboMissileConfig.damageType.Value;
        public static DamageTypeExtended damageTypeExtended => GooboMissileConfig.damageTypeExtended.Value;
        public DamageSource damageSource;
        public HurtBox targetHurtbox;
        public int hurtboxIndex;
        public NetworkInstanceId networkInstanceId;
        public float duration;
        public float arrivalTime;
        public override void OnEnter()
        {
            base.OnEnter();
            SetValues();
            StartAimMode();
            if (NetworkServer.active && targetHurtbox)
            {
                DamageTypeCombo damageTypeCombo = new DamageTypeCombo(damageType, damageTypeExtended, damageSource);
                GooboOrb gooboOrb = new GooboOrb
                {
                    attacker = gameObject,
                    duration = arrivalTime,
                    gooboAmount = baseGooboAmount,
                    damage = characterBody.damage * damageCoefficient,
                    procCoefficient = procCoefficient,
                    crit = RollCrit(),
                    falloffModel = falloffModel,
                    force = force,
                    damageTypeCombo = damageSource,
                    origin = characterBody.aimOrigin,
                    radius = radius,
                    teamIndex = GetTeam(),
                    target = targetHurtbox,
                    visualPrefab = Assets.GooboOrb.prefab
                };
                OrbManager.instance.AddOrb(gooboOrb);
            }
        }
        public void SetValues()
        {
            duration = baseDuration / characterBody.attackSpeed;
            arrivalTime = baseArrivalTime / characterBody.attackSpeed;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(networkInstanceId);
            writer.Write(hurtboxIndex);
            writer.Write((int)damageSource);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            networkInstanceId = reader.ReadNetworkId();
            hurtboxIndex = reader.ReadInt32();
            damageSource = (DamageSource)reader.ReadInt32();
            GameObject gameObject = Util.FindNetworkObject(networkInstanceId);
            if (!gameObject) return;
            CharacterBody characterBody = gameObject.GetComponent<CharacterBody>();
            if (!characterBody) return;
            HurtBoxGroup hurtBoxGroup = characterBody.hurtBoxGroup;
            if (!hurtBoxGroup) return;
            targetHurtbox = hurtBoxGroup.hurtBoxes[hurtboxIndex];
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
    public class ConsumeMinions : BaseState
    {
        public static float arrivalTime => ConsumeMinionsConfig.timeToTarget.Value;
        public float timeToTarget;
        public override void OnEnter()
        {
            base.OnEnter();
            timeToTarget = arrivalTime / characterBody.attackSpeed;
            if (NetworkServer.active) ConvertMinionsToBuffs();
            if (!isAuthority) return;
            outer.SetNextStateToMain();
        }
        public void ConvertMinionsToBuffs()
        {
            CharacterMaster characterMaster = characterBody.master;
            if (!characterMaster) return;
            MinionOwnership.MinionGroup minionGroup = null;
            for (int i = 0; i < MinionOwnership.MinionGroup.instancesList.Count; i++)
            {
                MinionOwnership.MinionGroup minionGroup2 = MinionOwnership.MinionGroup.instancesList[i];
                if (MinionOwnership.MinionGroup.instancesList[i].ownerId == characterMaster.netId)
                {
                    minionGroup = minionGroup2;
                    break;
                }
            }
            if (minionGroup == null) return;
            foreach (MinionOwnership minion in minionGroup.members)
            {
                if (minion == null) continue;
                CharacterMaster minionMaster = minion.GetComponent<CharacterMaster>();
                if (minionMaster == null) continue;
                CharacterBody minionBody = minionMaster.GetBody();
                if (minionBody == null || minionBody.bodyIndex != Assets.Goobo13CloneBodyIndex) continue;
                GooboConsumeOrb gooboConsumeOrb = new GooboConsumeOrb
                {
                    duration = timeToTarget,
                    effectScale = 1f,
                    origin = minionBody.corePosition,
                    target = characterBody.mainHurtBox,
                    visualPrefab = Assets.GooboOrb.prefab
                };
                OrbManager.instance.AddOrb(gooboConsumeOrb);
                minionMaster.TrueKill();
            }
        }
    }
    public class Slam : BaseGooboState
    {
        public static float baseDamageCoefficient => ConsumeMinionsConfig.damageCoefficient.Value;
        public static float procCoefficient => ConsumeMinionsConfig.procCoefficient.Value;
        public static float baseDuration => ConsumeMinionsConfig.duration.Value;
        public static float baseRadius => ConsumeMinionsConfig.radius.Value;
        public static float force => ConsumeMinionsConfig.force.Value;
        public static DamageType damageType => ConsumeMinionsConfig.damageType.Value;
        public static DamageTypeExtended damageTypeExtended => ConsumeMinionsConfig.damageTypeExtended.Value;
        public static BlastAttack.FalloffModel falloffModel => ConsumeMinionsConfig.falloffModel.Value;
        public static float baseUpTransitionSpeed = 0.05f;
        public static float baseDownTransitionSpeed = 0.05f;
        public float upTransitionSpeed;
        public float downTransitionSpeed;
        public float damageCoefficient;
        public float duration;
        public float radius;
        public float timeToAttack;
        public Vector3 forceVector;
        public float pushAwayForce;
        private bool fired;
        private bool fired2;
        public override void OnEnter()
        {
            base.OnEnter();
            SetValues();
            PlayCrossfade("UpperBody, Override", "SlamUp", "UpperBody.playbackRate", Mathf.Max(0.01f, timeToAttack), upTransitionSpeed);
            if (isAuthority)
            {
                StartAimMode();
            }
        }
        public void SetValues()
        {
            damageCoefficient = baseDamageCoefficient;
            duration = baseDuration / characterBody.attackSpeed;
            timeToAttack = duration / 2f;
            radius = baseRadius;
            upTransitionSpeed = baseUpTransitionSpeed / characterBody.attackSpeed;
            downTransitionSpeed = baseDownTransitionSpeed / characterBody.attackSpeed;
        }
        public void Fire()
        {
            PlayCrossfade("UpperBody, Override", "SlamDown", "UpperBody.playbackRate", Mathf.Max(0.01f, duration - timeToAttack), downTransitionSpeed);
            fired = true;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!fired && fixedAge >= timeToAttack) Fire();
            if (fixedAge >= duration)
            {
                if (!fired2)
                {
                    if (NetworkServer.active)
                    {
                        int buffCount = characterBody.GetBuffCount(Assets.GooboConsumptionCharge);
                        characterBody.SetBuffCount(Assets.GooboConsumptionCharge.buffIndex, 0);
                        characterBody.SetBuffCount(Assets.GooboCorrosionCharge.buffIndex, buffCount);
                    }
                    if (isAuthority)
                    {
                        BlastAttack blastAttack = new BlastAttack()
                        {
                            attacker = gameObject,
                            baseDamage = characterBody.damage * damageCoefficient,
                            baseForce = force,
                            crit = RollCrit(),
                            damageType = new DamageTypeCombo(damageType, damageTypeExtended, GetDamageSource()),
                            falloffModel = falloffModel,
                            damageColorIndex = DamageColorIndex.Default,
                            inflictor = gameObject,
                            losType = BlastAttack.LoSType.None,
                            procCoefficient = procCoefficient,
                            radius = radius,
                            teamIndex = GetTeam(),
                            position = characterBody.footPosition
                        };
                        blastAttack.Fire();
                        EffectData effectData = new()
                        {
                            origin = blastAttack.position,
                            scale = blastAttack.radius,
                        };
                        EffectManager.SpawnEffect(Assets.GooboExplosion.prefab, effectData, true);
                        outer.SetNextStateToMain();
                    }
                    fired2 = true;

                }

            }
        }
        public override void OnExit()
        {
            base.OnExit();
            if (fired2) PlayCrossfade("UpperBody, Override", "SlamTransition", "UpperBody.playbackRate", Mathf.Max(0.01f, duration - timeToAttack), downTransitionSpeed);
            if (activatorSkillSlot) activatorSkillSlot.UnsetSkillOverride(gameObject, Assets.GooboSlam, GenericSkill.SkillOverridePriority.Contextual);
            if (NetworkServer.active) characterBody.SetBuffCount(Assets.GooboCorrosionCharge.buffIndex, 0);
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
    public class UnstableDecoy : BaseState
    {
        public static float healthPercentage => UnstableDecoyConfig.healthPercentage.Value;
        public static int gooboAmount => UnstableDecoyConfig.gooboAmount.Value;
        public static float cloakDuration => UnstableDecoyConfig.duration.Value;
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                //Vector3 vector3 = Utils.GetClosestNodePosition(characterBody.footPosition, characterBody.hullClassification, float.PositiveInfinity, out Vector3 nodePosition) ? nodePosition : characterBody.footPosition;
                Vector3 directionVector = characterDirection ? characterDirection.forward : transform.forward;
                for (int i = 0; i < gooboAmount; i++) Utils.SpawnGooboClone(characterBody.master, transform.position, Quaternion.LookRotation(directionVector));
                characterBody.AddTimedBuff(RoR2Content.Buffs.Cloak, cloakDuration);
                characterBody.AddTimedBuff(RoR2Content.Buffs.CloakSpeed, cloakDuration);
                if (healthComponent && healthPercentage > 0f)
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        damage = healthComponent.combinedHealth * healthPercentage,
                        position = characterBody.corePosition,
                        force = Vector3.zero,
                        damageColorIndex = DamageColorIndex.Default,
                        crit = false,
                        attacker = null,
                        inflictor = null,
                        damageType = DamageType.NonLethal,
                        procCoefficient = 0f,
                        procChainMask = default
                    };
                    healthComponent.TakeDamage(damageInfo);
                }
            }
            if (!isAuthority) return;
            outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
