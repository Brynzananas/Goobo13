using EntityStates;
using HG;
using MonoMod.RuntimeDetour;
using R2API;
using Rewired.UI.ControlMapper;
using RoR2;
using RoR2.Audio;
using RoR2.CameraModes;
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
using static Rewired.ComponentControls.Effects.RotateAroundAxis;
using static RoR2.CameraRigController;
using static UnityEngine.ParticleSystem.PlaybackState;

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
                    overlapAttack.hitEffectPrefab = Assets.GooboImpact.prefab;
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
        public static float transitionAnimationSpeed = 0.5f;
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
                    impactEffect = Assets.GooboImpact.index,
                    position = position
                };
                blastAttack.AddModdedDamageType(Assets.ChanceToSpawnGooboDamageType);
                blastAttack.Fire();
                EffectData effectData = new()
                {
                    origin = blastAttack.position,
                    scale = blastAttack.radius,
                };
                EffectManager.SpawnEffect(Assets.GooboExplosion.prefab, effectData, true);
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
            PlayCrossfade("UpperBody, Override", "Punch3Transition", "UpperBody.playbackRate", Mathf.Max(0.01f, transitionAnimationSpeed), downTransitionSpeed);
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
            PlayCrossfade("UpperBody, Override", "ThrowUp", "UpperBody.playbackRate", Mathf.Max(0.01f, timeToThrow), upTransitionSpeed);
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
            PlayAnimation("UpperBody, Override", "ThrowDown", "UpperBody.playbackRate", Mathf.Max(0.01f, duration - timeToThrow), downTransitionSpeed);
            if (isAuthority)
            {
                DamageTypeCombo damageTypeCombo = new DamageTypeCombo(damageType, damageTypeExtended, GetDamageSource());
                if (currentGrenade == Assets.GooboGrenadeType1Projectile) damageTypeCombo.AddModdedDamageType(Assets.GooboCorrosionDamageType); // This is stupid
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
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
        public void SetValues()
        {
            timeToThrow = baseTimeToThrow / characterBody.attackSpeed;
            duration = baseDuration / characterBody.attackSpeed;
            upTransitionSpeed = baseUpTransitionSpeed / characterBody.attackSpeed;
            downTransitionSpeed = baseDownTransitionSpeed / characterBody.attackSpeed;
        }
    }
    public class ThrowClone : BaseGooboState
    {
        public static float baseTimeToThrow => ThrowCloneConfig.timeToAttack.Value;
        public static float baseDuration => ThrowCloneConfig.duration.Value;
        public static int amount => ThrowCloneConfig.amount.Value;
        public static float force => ThrowCloneConfig.velocity.Value;
        public static float forceStacking => ThrowCloneConfig.velocityStacking.Value;
        public static float baseUpTransitionSpeed = 0.05f;
        public static float baseDownTransitionSpeed = 0.05f;
        public float upTransitionSpeed;
        public float downTransitionSpeed;
        public float timeToThrow;
        public float duration;
        private bool fired;

        public override void OnEnter()
        {
            base.OnEnter();
            StartAimMode();
            SetValues();
            PlayCrossfade("UpperBody, Override", "ThrowUp", "UpperBody.playbackRate", Mathf.Max(0.01f, timeToThrow), upTransitionSpeed);
        }
        public void SetValues()
        {
            timeToThrow = baseTimeToThrow / characterBody.attackSpeed;
            duration = baseDuration / characterBody.attackSpeed;
            upTransitionSpeed = baseUpTransitionSpeed / characterBody.attackSpeed;
            downTransitionSpeed = baseDownTransitionSpeed / characterBody.attackSpeed;
        }
        public void Fire(Ray ray)
        {
            fired = true;
            PlayAnimation("UpperBody, Override", "ThrowDown", "UpperBody.playbackRate", Mathf.Max(0.01f, duration - timeToThrow), downTransitionSpeed);
            if (NetworkServer.active)
            {
                bool crit = RollCrit();
                GobooThrowGooboMinionsTracker gobooThrowGooboMinionsTracker = gameObject.GetComponent<GobooThrowGooboMinionsTracker>();
                float velocity = force;
                Quaternion quaternion = Util.QuaternionSafeLookRotation(ray.direction);
                for (int i = 0; i < amount; i++)
                {
                    CharacterMaster characterMaster = Utils.SpawnGooboClone(this.characterBody.master, ray.origin, quaternion);
                    if (!characterMaster) continue;
                    CharacterBody characterBody = characterMaster.GetBody();
                    if (!characterBody) continue;
                    GameObject masterObject = characterBody.masterObject; // Without this inventory in body is null
                    Vector3 vector3 = ray.direction * velocity + Physics.gravity * -0.2f;
                    if (characterBody.characterMotor)
                    {
                        characterBody.characterMotor.velocity = vector3;
                    }
                    else if (characterBody.rigidbody)
                    {
                        characterBody.rigidbody.velocity = vector3;
                    }
                    EntityStateMachine entityStateMachine = characterBody.gameObject.GetComponent<EntityStateMachine>();
                    if (entityStateMachine)
                    {
                        entityStateMachine.initialStateType.stateType = null;
                        entityStateMachine.SetNextState(new CloneFlying {attacker = gameObject, attackerBody = this.characterBody, crit = crit, damageSource = GetDamageSource() });
                    }
                    //if (characterBody.skillLocator && characterBody.skillLocator.primary) characterBody.skillLocator.primary.ExecuteIfReady();
                    if (gobooThrowGooboMinionsTracker && gobooThrowGooboMinionsTracker.trackingTarget && gobooThrowGooboMinionsTracker.trackingTarget.healthComponent && characterMaster.AiComponents != null)
                    {
                        foreach (BaseAI baseAI in characterMaster.AiComponents)
                        {
                            if (!baseAI || baseAI.currentEnemy == null) continue;
                            baseAI.currentEnemy.gameObject = gobooThrowGooboMinionsTracker.trackingTarget.healthComponent.gameObject;
                        }
                    }
                    velocity += forceStacking;
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!fired && fixedAge >= timeToThrow) Fire(GetAimRay());
            if (!isAuthority) return;
            if (fixedAge >= duration) outer.SetNextStateToMain();
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
                characterBody.AddBuff(RoR2Content.Buffs.Cloak);
                //characterBody.AddTimedBuff(RoR2Content.Buffs.CloakSpeed, cloakDuration);
            }
            if (!isAuthority) return;
            characterBody.onSkillActivatedAuthority += CharacterBody_onSkillActivatedAuthority;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= cloakDuration && isAuthority) outer.SetNextStateToMain();
        }
        private void CharacterBody_onSkillActivatedAuthority(GenericSkill obj)
        {
            if (!obj.skillDef.isCombatSkill) return;
            outer.SetNextStateToMain();
        }

        public override void OnExit()
        {
            base.OnExit();
            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(RoR2Content.Buffs.Cloak);
            }
            if (!isAuthority) return;
            characterBody.onSkillActivatedAuthority -= CharacterBody_onSkillActivatedAuthority;
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Any;
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
            GooboSkillDef.InstanceData instanceData = activatorSkillSlot == null || activatorSkillSlot.skillInstanceData == null ? null : activatorSkillSlot.skillInstanceData as GooboSkillDef.InstanceData;
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
                GooboConsumeOrb gooboConsumeOrb = new GooboConsumeOrb
                {
                    duration = timeToTarget,
                    effectScale = 1f,
                    origin = minionBody.corePosition,
                    target = characterBody.mainHurtBox,
                    visualPrefab = Assets.GooboOrb.prefab,
                    addSlam = false,
                    heal = FireMinionsConfig.healPercentage.Value / 100f,
                };
                OrbManager.instance.AddOrb(gooboConsumeOrb);
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
    public class GooboMissile : BaseGooboState
    {
        public static float damageCoefficient => GooboMissileConfig.damageCoefficient.Value;
        public static float procCoefficient => GooboMissileConfig.procCoefficient.Value;
        public static float force => GooboMissileConfig.force.Value;
        public static int baseGooboAmount => GooboMissileConfig.gooboAmount.Value;
        public static float baseArrivalTime => GooboMissileConfig.timeToTarget.Value;
        public static float baseDuration => GooboMissileConfig.duration.Value;
        public static float baseTimeToAttack => GooboMissileConfig.timeToAttack.Value;
        public static float radius => GooboMissileConfig.radius.Value;
        public static BlastAttack.FalloffModel falloffModel => GooboMissileConfig.falloffModel.Value;
        public static DamageType damageType => GooboMissileConfig.damageType.Value;
        public static DamageTypeExtended damageTypeExtended => GooboMissileConfig.damageTypeExtended.Value;
        public static float baseUpTransitionSpeed = 0.05f;
        public static float baseDownTransitionSpeed = 0.05f;
        public float arrivalTime;
        public float upTransitionSpeed;
        public float downTransitionSpeed;
        public float timeToThrow;
        public float duration;
        private bool fired;
        public HurtBox hurtboxTarget;
        public override void OnEnter()
        {
            base.OnEnter();
            StartAimMode();
            SetValues();
            PlayCrossfade("UpperBody, Override", "GooboMissileUp", "UpperBody.playbackRate", Mathf.Max(0.01f, timeToThrow), upTransitionSpeed);
            if (NetworkServer.active) FindTarget();
        }
        public void Fire(Ray ray)
        {
            PlayAnimation("UpperBody, Override", "GooboMissileDown", "UpperBody.playbackRate", Mathf.Max(0.01f, duration - timeToThrow), downTransitionSpeed);
            if (NetworkServer.active)
            {
                DamageTypeCombo damageTypeCombo = new DamageTypeCombo(damageType, damageTypeExtended, GetDamageSource());
                damageTypeCombo.AddModdedDamageType(Assets.GooboCorrosionDamageType);
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
                    damageTypeCombo = damageTypeCombo,
                    origin = characterBody.aimOrigin,
                    radius = radius,
                    teamIndex = GetTeam(),
                    target = hurtboxTarget,
                    visualPrefab = Assets.GooboOrb.prefab
                };
                OrbManager.instance.AddOrb(gooboOrb);
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
            timeToThrow = baseTimeToAttack / characterBody.attackSpeed;
            duration = baseDuration / characterBody.attackSpeed;
            upTransitionSpeed = baseUpTransitionSpeed / characterBody.attackSpeed;
            downTransitionSpeed = baseDownTransitionSpeed / characterBody.attackSpeed;
            arrivalTime = baseArrivalTime / characterBody.attackSpeed;
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
        public void FindTarget()
        {
            GooboSkillDef.InstanceData instanceData = activatorSkillSlot == null || activatorSkillSlot.skillInstanceData == null ? null : activatorSkillSlot.skillInstanceData as GooboSkillDef.InstanceData;
            if (instanceData == null) return;
            GobooThrowGooboMinionsTracker gobooThrowGooboMinionsTracker = instanceData.gobooThrowGooboMinionsTracker;
            if (!gobooThrowGooboMinionsTracker) return;
            hurtboxTarget = gobooThrowGooboMinionsTracker.trackingTarget;
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
                    visualPrefab = Assets.GooboOrb.prefab,
                    addSlam = true,
                    heal = ConsumeMinionsConfig.healPercentage.Value / 100f,
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
        private int buffCount;
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
                        buffCount = characterBody.GetBuffCount(Assets.GooboConsumptionCharge);
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
            if (fired2)
            {
                PlayCrossfade("UpperBody, Override", "SlamTransition", "UpperBody.playbackRate", Mathf.Max(0.01f, duration - timeToAttack), downTransitionSpeed);
                if (activatorSkillSlot) activatorSkillSlot.UnsetSkillOverride(gameObject, Assets.GooboSlam, GenericSkill.SkillOverridePriority.Contextual);
                if (NetworkServer.active) characterBody.SetBuffCount(Assets.GooboCorrosionCharge.buffIndex, 0);
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
    public class UnstableDecoy : BaseState
    {
        public static float healthPercentage => UnstableDecoyConfig.healthPercentage.Value;
        public static float healthPercentageRandomSpread => UnstableDecoyConfig.healthPercentageRandomSpread.Value;
        public static int gooboAmount => UnstableDecoyConfig.gooboAmount.Value;
        public static float baseDuration => UnstableDecoyConfig.duration.Value;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / attackSpeedStat;
            if (NetworkServer.active)
            {
                if (healthComponent && healthPercentage > 0f)
                {
                    float percentage = healthPercentage;
                    float spread = UnityEngine.Random.Range(0f, healthPercentageRandomSpread);
                    percentage += spread * (Util.CheckRoll(50f) ? 1f : -1f);
                    percentage /= 100f;
                    DamageInfo damageInfo = new DamageInfo
                    {
                        damage = healthComponent.combinedHealth * percentage,
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
                characterBody.AddTimedBuff(RoR2Content.Buffs.HiddenInvincibility, duration);
                characterBody.AddTimedBuff(RoR2Content.Buffs.Slow50, duration);
                for (int i = 0; i < gooboAmount; i++) characterBody.AddTimedBuff(Assets.SpawnGooboOnEnd, duration);
            }
            if (!isAuthority) return;
            outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
    public class Leap : BaseGooboState, SteppedSkillDef.IStepSetter
    {
        public static float baseDamageCoefficient => LeapConfig.damageCoefficient.Value;
        public static float procCoefficient => LeapConfig.procCoefficient.Value;
        public static float baseRadius => LeapConfig.radius.Value;
        public static float force => LeapConfig.force.Value;
        public static DamageType damageType => LeapConfig.damageType.Value;
        public static DamageTypeExtended damageTypeExtended => LeapConfig.damageTypeExtended.Value;
        public static BlastAttack.FalloffModel falloffModel => LeapConfig.falloffModel.Value;
        public static float baseMinimumYVelocity => LeapConfig.minimumYVelocity.Value;
        public static float baseVelocityPerLeap => LeapConfig.velocityPerLeap.Value;
        public static float cloakDuration => LeapConfig.duration.Value;
        public static float airControl => LeapConfig.airControl.Value;
        public Vector3 vector3;
        public int leapCount;
        public bool fired;
        public override void OnEnter()
        {
            base.OnEnter();
            StartAimMode();
            if (NetworkServer.active)
            {
                characterBody.AddBuff(JunkContent.Buffs.IgnoreFallDamage);
            }
            if (!isAuthority) return;
            if (!characterMotor)
            {
                outer.SetNextStateToMain();
                return;
            }
            Ray ray = GetAimRay();
            Vector3 vector3 = ray.direction * characterBody.moveSpeed * baseVelocityPerLeap * (leapCount);
            vector3.y = Mathf.Max(baseMinimumYVelocity, vector3.y);
            characterMotor.velocity = vector3;
            characterMotor.Motor?.ForceUnground();
            characterMotor.onMovementHit += CharacterMotor_onMovementHit;
            characterMotor.disableAirControlUntilCollision = true;
            PlayCrossfade("UpperBody, Override", "SlamUp", "UpperBody.playbackRate", 1f, 0.05f);
        }

        private void CharacterMotor_onMovementHit(ref CharacterMotor.MovementHitInfo movementHitInfo)
        {
            if (fired) return;
            BlastAttack blastAttack = new BlastAttack()
            {
                attacker = gameObject,
                baseDamage = characterBody.damage * baseDamageCoefficient,
                baseForce = force,
                crit = RollCrit(),
                damageType = new DamageTypeCombo(damageType, damageTypeExtended, GetDamageSource()),
                falloffModel = falloffModel,
                damageColorIndex = DamageColorIndex.Default,
                inflictor = gameObject,
                losType = BlastAttack.LoSType.None,
                procCoefficient = procCoefficient,
                radius = baseRadius,
                teamIndex = GetTeam(),
                impactEffect = Assets.GooboImpact.index,
                position = characterBody.footPosition
            };
            blastAttack.AddModdedDamageType(Assets.ChanceToSpawnGooboDamageType);
            blastAttack.Fire();
            EffectData effectData = new()
            {
                origin = blastAttack.position,
                scale = blastAttack.radius,
            };
            EffectManager.SpawnEffect(Assets.GooboExplosion.prefab, effectData, true);
            outer.SetNextStateToMain();
            if (leapCount == 2) characterBody.AddTimedBuffAuthority(Assets.SpawnGooboOnEnd.buffIndex, 0f);
            if (leapCount >= 3) characterBody.AddTimedBuffAuthority(RoR2Content.Buffs.Cloak.buffIndex, cloakDuration);
            fired = true;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority) return;
            if (characterMotor)
            {
                if (inputBank)  vector3 = Vector3.MoveTowards(vector3, inputBank.moveVector, characterMotor.airControl * Time.fixedDeltaTime);
                characterMotor.rootMotion += vector3 * characterBody.moveSpeed * airControl * Time.fixedDeltaTime;
            }
            if (inputBank && inputBank.sprint.justPressed) outer.SetNextStateToMain();
        }
        public override void OnExit()
        {
            base.OnExit();
            if (fired) PlayCrossfade("UpperBody, Override", "SlamTransition", "UpperBody.playbackRate", 1f, 0.05f);
            if (NetworkServer.active)
            {
                characterBody.RemoveBuff(JunkContent.Buffs.IgnoreFallDamage);
                characterBody.AddTimedBuff(JunkContent.Buffs.IgnoreFallDamage, 0.25f);
            }
            if (!isAuthority) return;
            if (characterMotor) characterMotor.onMovementHit -= CharacterMotor_onMovementHit;
        }
        public void SetStep(int i) => leapCount = i + 1;
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
    public class Ball : BaseGooboState
    {
        public static float baseJumpMultiplier => BallConfig.jumpCoefficient.Value;
        public static float baseSpeedMultiplier => BallConfig.speedCoefficient.Value;
        public static float baseAirControlMultiplier => BallConfig.airControlCoefficient.Value;
        public static float effectScale = 3f;
        public float jump;
        public float speed;
        public float airControl;
        public bool stateTaken;
        public static float damageCoefficient => BallConfig.damageCoefficient.Value;
        public static float force => BallConfig.force.Value;
        public static DamageTypeCombo damageType => BallConfig.damageType.Value;
        public static DamageTypeExtended damageTypeExtended => BallConfig.damageTypeExtended.Value;
        public static BlastAttack.FalloffModel falloffModel => BallConfig.falloffModel.Value;
        public static float procCoefficient => BallConfig.procCoefficient.Value;
        public static float radius => BallConfig.radius.Value;

        public virtual void SetValues()
        {
            speed = this.characterBody.moveSpeed * baseSpeedMultiplier;
            airControl = characterBody.acceleration * baseAirControlMultiplier;
            jump = characterBody.jumpPower * baseJumpMultiplier;
        }
        public override void OnEnter()
        {
            base.OnEnter();
            /*EffectData effectData = new()
            {
                origin = characterBody.corePosition,
                scale = effectScale,
            };
            EffectManager.SpawnEffect(Assets.GooboExplosion.index, effectData, false);*/
            characterBody.isSprinting = true;
            SetValues();
            Explode(characterBody.corePosition);
            if (NetworkServer.active)
            {
                Ray ray = GetAimRay();
                Quaternion quaternion = Util.QuaternionSafeLookRotation(ray.direction);
                Utils.SpawnGooboClone(characterBody.master, transform.position, quaternion);
                GameObject gameObject = GameObject.Instantiate(Assets.GooboBallVehicle, ray.origin, quaternion);
                if (gameObject)
                {
                    Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
                    if (rigidbody) rigidbody.velocity = Physics.gravity.normalized * -jump + inputBank.moveVector * speed + Physics.gravity * -0.2f;
                    GooboBallVehicleController gooboBallVehicleController = gameObject.GetComponent<GooboBallVehicleController>();
                    if (gooboBallVehicleController)
                    {
                        VehicleSeat vehicleSeat = gooboBallVehicleController.vehicleSeat;
                        if (vehicleSeat)
                        {
                            vehicleSeat.AssignPassenger(this.gameObject);
                        }
                    }
                    NetworkUser networkUser;
                    if (characterBody == null)
                    {
                        networkUser = null;
                    }
                    else
                    {
                        CharacterMaster master = characterBody.master;
                        if (master == null)
                        {
                            networkUser = null;
                        }
                        else
                        {
                            PlayerCharacterMasterController playerCharacterMasterController = master.playerCharacterMasterController;
                            networkUser = ((playerCharacterMasterController != null) ? playerCharacterMasterController.networkUser : null);
                        }
                    }
                    if (networkUser)
                    {
                        NetworkServer.SpawnWithClientAuthority(gameObject, networkUser.gameObject);
                    }
                    else
                    {
                        NetworkServer.Spawn(gameObject);
                    }
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            SetValues();
        }
        public void Explode(Vector3 position)
        {
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
                    impactEffect = Assets.GooboImpact.index,
                    position = position
                };
                blastAttack.AddModdedDamageType(Assets.GooboCorrosionDamageType);
                blastAttack.Fire();
                EffectData effectData = new()
                {
                    origin = blastAttack.position,
                    scale = blastAttack.radius,
                };
                EffectManager.SpawnEffect(Assets.GooboExplosion.prefab, effectData, true);
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
    public class CloneFlying : BaseState
    {
        public static float damageCoefficient = 5f;
        public static float procCoefficient = 1f;
        public static DamageType damageType = DamageType.Generic;
        public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
        public HitBoxGroup hitBoxGroup;
        public OverlapAttack overlapAttack;
        public DamageSource damageSource;
        public GameObject attacker;
        public CharacterBody attackerBody;
        public bool crit;
        public bool hasEvent;
        public override void OnEnter()
        {
            base.OnEnter();
            if (!characterMotor)
            {
                if (isAuthority) outer.SetNextStateToMain();
                return;
            }
            if (isAuthority)
            {
                characterMotor.onHitGroundAuthority += CharacterMotor_onHitGroundAuthority;
                hasEvent = true;
            }
            if (!NetworkServer.active) return;
            hitBoxGroup = FindHitBoxGroup("Flying");
            if (!hitBoxGroup) return;
            overlapAttack = new OverlapAttack
            {
                attacker = attacker ?? gameObject,
                damage = (attackerBody ? attackerBody.damage : characterBody.damage) * damageCoefficient,
                damageType = new DamageTypeCombo(damageType, damageTypeExtended, damageSource),
                hitBoxGroup = hitBoxGroup,
                hitEffectPrefab = Assets.GooboPunchEffect.prefab,
                inflictor = gameObject,
                isCrit = crit,
                procCoefficient = procCoefficient,
                teamIndex = attacker ? TeamComponent.GetObjectTeam(attacker) : GetTeam()
            };
        }
        private void CharacterMotor_onHitGroundAuthority(ref CharacterMotor.HitGroundInfo hitGroundInfo)
        {
            outer.SetNextStateToMain();
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (overlapAttack == null) return;
            overlapAttack.Fire();
            overlapAttack.damage = (attackerBody ? attackerBody.damage : characterBody.damage) * damageCoefficient;
        }
        public override void OnExit()
        {
            base.OnExit();
            if (hasEvent)  characterMotor.onHitGroundAuthority -= CharacterMotor_onHitGroundAuthority;
        }
    }
    public class FireSpout : BaseState
    {
        public static float damageCoefficient = 2.75f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.5f;
        public static float force = 0f;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / characterBody.attackSpeed;
            Fire(GetAimRay());
        }
        public void Fire(Ray ray)
        {
            if (isAuthority)
            {
                DamageTypeCombo damageTypeCombo = new DamageTypeCombo(DamageType.Generic, DamageTypeExtended.Generic, DamageSource.Primary);
                damageTypeCombo.AddModdedDamageType(Assets.AbysstouchedDamageType);
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = Assets.RevolutionarySanguineVapor,
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
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority) return;
            if (fixedAge >= duration) outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
    public class TeleportBehind : BaseState, ICameraStateProvider
    {
        public Quaternion newRotation;
        public void GetCameraState(CameraRigController cameraRigController, ref CameraState cameraState)
        {
            cameraState.rotation = newRotation;
        }

        public bool IsHudAllowed(CameraRigController cameraRigController) => true;

        public bool IsUserControlAllowed(CameraRigController cameraRigController) => true;

        public bool IsUserLookAllowed(CameraRigController cameraRigController) => true;

        public override void OnEnter()
        {
            base.OnEnter();
            if (isAuthority) outer.SetNextStateToMain();
            if (!NetworkServer.active) return;
            RevolutionaryController revolutionaryController = GetComponent<RevolutionaryController>();
            if (revolutionaryController.currentTarget)
            {
                CharacterDirection targetDirection = revolutionaryController.currentTarget.characterDirection;
                Vector3 look = targetDirection ? targetDirection.forward : revolutionaryController.currentTarget.transform.forward;
                look *= -1f;
                look.y = 0f;
                look.Normalize();
                EffectData effectData = new EffectData
                {
                    origin = characterBody.corePosition,
                    scale = characterBody.radius
                };
                EffectManager.SpawnEffect(Assets.GooboExplosion.prefab, effectData, true);
                Vector3 newPosition = revolutionaryController.currentTarget.corePosition + revolutionaryController.currentTarget.radius * look + characterBody.radius * look;
                Vector3 vector3 = (revolutionaryController.currentTarget.corePosition - newPosition).normalized;
                float height = 1f;
                if (characterMotor)
                {
                    height = characterMotor.capsuleHeight;
                }
                if (Physics.Raycast(newPosition + transform.up * height, Vector3.down, out RaycastHit hitInfo, height * 2f, LayerIndex.world.mask))
                {
                    Debug.Log(hitInfo.collider);
                    Debug.Log(hitInfo.point);
                    newPosition = hitInfo.point;
                }
                newRotation = Quaternion.LookRotation(vector3);
                foreach (CameraRigController cameraRigController in CameraRigController.readOnlyInstancesList)
                {
                    //cameraRigController.transform.rotation = newRotation;
                    cameraRigController.SetOverrideCam(this, 0f);
                    cameraRigController.LateUpdate();
                    cameraRigController.SetOverrideCam(null, 0f);
                }
                if (characterDirection)
                {
                    characterDirection.forward = vector3;
                    characterDirection.moveVector = vector3;
                }
                TeleportHelper.TeleportBody(characterBody, newPosition, true);
                Inventory inventory = characterBody.inventory;
                EffectData effectData2 = new EffectData
                {
                    origin = newPosition,
                    scale = characterBody.radius
                };
                EffectManager.SpawnEffect(Assets.GooboExplosion.prefab, effectData2, true);
                if (inventory && inventory.GetItemCount(Assets.ImpStack) > 0)
                {
                    Utils.SpawnGooboClone(characterBody.master, newPosition, Quaternion.LookRotation(vector3));
                    inventory.RemoveItem(Assets.ImpStack);
                }
            }
        }
    }
    public class TeleportForward : BaseState
    {
        public static float damageCoefficient = 2.7f;
        public static float force = 100f;
        public static float distance = 24f;
        public static int spikesCount = 3;
        public static float minSpread = 0f;
        public static float maxSpread = 35f;
        public static float pitchScale = 1f;
        public static float yawScale = 1f;
        private bool crit;
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                EffectData effectData = new EffectData
                {
                    origin = characterBody.corePosition,
                    scale = characterBody.radius
                };
                EffectManager.SpawnEffect(Assets.GooboExplosion.prefab, effectData, true);
                Inventory inventory = characterBody.inventory;
                if (inventory && inventory.GetItemCount(Assets.ImpStack) > 0)
                {
                    Vector3 directionVector = characterDirection ? characterDirection.forward : transform.forward;
                    Utils.SpawnGooboClone(characterBody.master, transform.position, Quaternion.LookRotation(directionVector));
                    inventory.RemoveItem(Assets.ImpStack);
                }
            }
            if (!isAuthority) return;
            Ray ray = GetAimRay();
            Vector3 oldPosition = characterBody.corePosition;
            Vector3 finalPosition;
            if (Physics.Raycast(ray, out RaycastHit hitInfo, distance, LayerIndex.world.mask))
            {
                finalPosition = hitInfo.point;
            }
            else
            {
                finalPosition = ray.origin + ray.direction * distance;
            }
            if (characterMotor)
            {
                characterMotor.Motor.SetPosition(finalPosition);
            }
            else if (rigidbody)
            {
                rigidbody.position = finalPosition;
            }
            EffectData effectData2 = new EffectData
            {
                origin = finalPosition,
                scale = characterBody.radius
            };
            crit = RollCrit();
            EffectManager.SpawnEffect(Assets.GooboExplosion.prefab, effectData2, true);
            for (int i = 0; i < spikesCount; i++)
            {
                Vector3 direction = Quaternion.AngleAxis(-90f, Vector3.right) * Util.ApplySpread(transform.forward, minSpread, maxSpread, yawScale, pitchScale);
                Ray ray1 = new Ray(finalPosition, direction);
                Fire(ray1);
            }
            if (characterDirection)
            {
                Vector3 direction = (finalPosition - oldPosition);
                direction.y = 0f;
                direction.Normalize();
                characterDirection.forward = direction;
                characterDirection.moveVector = direction;
            }
            outer.SetNextStateToMain();
        }
        public void Fire(Ray ray)
        {
            if (isAuthority)
            {
                DamageTypeCombo damageTypeCombo = new DamageTypeCombo(DamageType.Generic, DamageTypeExtended.Generic, DamageSource.Utility);
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = Assets.RevolutionaryAbyssalSpike,
                    position = ray.origin,
                    rotation = Util.QuaternionSafeLookRotation(ray.direction),
                    owner = gameObject,
                    damage = characterBody.damage * damageCoefficient,
                    force = force,
                    crit = crit,
                    damageTypeOverride = new DamageTypeCombo?(damageTypeCombo),
                };
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }
    }
    public class EnterDimension : BaseSkillState
    {
        public static float distance = 24f;
        public static float baseDuration = 8f;
        public List<CharacterBody> characterBodies = [];
        public override void OnEnter()
        {
            base.OnEnter();
            if (activatorSkillSlot) activatorSkillSlot.stock--;
            EffectData effectData = new EffectData
            {
                origin = characterBody.corePosition,
                scale = distance
            };
            EffectManager.SpawnEffect(Assets.GooboExplosion.prefab, effectData, false);
            if (NetworkServer.active)
            {
                float sqrDistance = distance * distance;
                foreach (CharacterBody characterBody in CharacterBody.readOnlyInstancesList)
                {
                    Vector3 vector3 = characterBody.corePosition - this.characterBody.corePosition;
                    if (vector3.sqrMagnitude > sqrDistance) continue;
                    characterBodies.Add(characterBody);
                    characterBody.AddBuff(Assets.InBetweenSpace);
                }
                RevolutionaryController revolutionaryController = gameObject.GetComponent<RevolutionaryController>();
                CharacterBody currentTarget = revolutionaryController?.currentTarget;
                if (currentTarget && !characterBodies.Contains(currentTarget))
                {
                    characterBodies.Add(currentTarget);
                    currentTarget.AddBuff(Assets.InBetweenSpace);
                }
                CharacterMaster characterMaster = characterBody.master;
                if (characterMaster)
                {
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
                        if (minionBody == null || minionBody.bodyIndex != Assets.Goobo13CloneBodyIndex || characterBodies.Contains(minionBody)) continue;
                        Vector3 poistion = Vector3.zero;
                        if (!Utils.GetClosestNodePosition(characterBody.corePosition + (UnityEngine.Random.insideUnitSphere * distance), HullClassification.Human, distance, out poistion)) continue;
                        TeleportHelper.TeleportBody(minionBody, poistion, true);
                        characterBodies.Add(minionBody);
                        minionBody.AddBuff(Assets.InBetweenSpace);
                    }
                    int itemCount = characterBody.inventory ? characterBody.inventory.GetItemCount(Assets.ImpStack) : 0;
                    if (itemCount > 0)
                    {
                        for (int i = 0; i < itemCount; i++)
                        {
                            CharacterMaster impMaster = Utils.SpawnGooboClone(characterMaster, characterBody.corePosition + (UnityEngine.Random.insideUnitSphere * distance), UnityEngine.Random.rotation);
                            CharacterBody impBody = impMaster?.GetBody();
                            if (impBody)
                            {
                                characterBodies.Add(impBody);
                                impBody.AddBuff(Assets.InBetweenSpace);
                            }
                        }
                        characterBody.inventory.RemoveItem(Assets.ImpStack, itemCount);
                    }
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority) return;
            if (fixedAge >= baseDuration) outer.SetNextStateToMain();
        }
        public override void OnExit()
        {
            base.OnExit();
            EffectData effectData = new EffectData
            {
                origin = characterBody.corePosition,
                scale = distance
            };
            EffectManager.SpawnEffect(Assets.GooboExplosion.prefab, effectData, false);
            if (NetworkServer.active)
            foreach (CharacterBody characterBody in characterBodies)
            {
                if (!characterBody) continue;
                characterBody.RemoveBuff(Assets.InBetweenSpace);
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
    public class PrepareEnterDimension : BaseSkillState
    {
        public static float indicatorSmoothTime = 0.1f;
        public float indicatorVelocity;
        public GameObject indicator;
        public override void OnEnter()
        {
            base.OnEnter();
            indicator = GameObject.Instantiate(Assets.EnterDimensionIndicator);
            indicator.transform.position = characterBody.corePosition;
            indicator.transform.localScale = Vector3.zero;
        }
        public override void Update()
        {
            base.Update();
            if (indicator)
            {
                float scale = Mathf.SmoothDamp(indicator.transform.localScale.x, EnterDimension.distance, ref indicatorVelocity, indicatorSmoothTime);
                indicator.transform.position = characterBody.corePosition;
                indicator.transform.localScale = new Vector3(scale, scale, scale);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority || IsKeyDownAuthority()) return;
            outer.SetNextState(new EnterDimension{activatorSkillSlot = activatorSkillSlot});
        }
        public override void OnExit()
        {
            base.OnExit();
            if (indicator) Destroy(indicator);
        }
        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.PrioritySkill;
    }
    public class FireSpikes : BaseState
    {
        public static float damageCoefficient = 2.5f;
        public static float procCoefficient = 1f;
        public static float baseDuration = 0.5f;
        public static float force = 0f;
        public float duration;
        public override void OnEnter()
        {
            base.OnEnter();
            duration = baseDuration / characterBody.attackSpeed;
            Fire(GetAimRay());
        }
        public void Fire(Ray ray)
        {
            if (isAuthority)
            {
                DamageTypeCombo damageTypeCombo = new DamageTypeCombo(DamageType.Generic, DamageTypeExtended.Generic, DamageSource.Primary);
                damageTypeCombo.AddModdedDamageType(Assets.AbysstouchedDamageType);
                FireProjectileInfo fireProjectileInfo = new FireProjectileInfo
                {
                    projectilePrefab = Assets.RevolutionaryChainSanguineVapor,
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
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (!isAuthority) return;
            if (fixedAge >= duration) outer.SetNextStateToMain();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
