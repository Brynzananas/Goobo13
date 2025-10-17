using EntityStates;
using MonoMod.RuntimeDetour;
using R2API;
using RoR2;
using RoR2.Audio;
using RoR2.CharacterAI;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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
        public static float baseDamageCoefficient = PunchConfig.damageCoefficient.Value;
        public static float procCoefficient = PunchConfig.procCoefficient.Value;
        public static float baseDuration = PunchConfig.duration.Value;
        public static float baseTimeToAttack = PunchConfig.timeToAttack.Value;
        public static float baseSelfPush = PunchConfig.selfPush.Value;
        public static float baseUpTransitionSpeed = 0.1f;
        public static float baseDownTransitionSpeed = 0.1f;
        public static DamageType damageType = DamageType.Generic;
        public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
        public float upTransitionSpeed;
        public float downTransitionSpeed;
        public float damageCoefficient;
        public float duration;
        public float timeToAttack;
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
            PlayCrossfade("UpperBody, Override", step ? "Punch2Up" : "Punch1Up", "UpperBody.playbackRate", timeToAttack, upTransitionSpeed);
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
                    overlapAttack.AddModdedDamageType(Assets.ChanceToSpawnGooboDamageType);
                }
            }
        }
        public void SetValues()
        {
            damageCoefficient = baseDamageCoefficient;
            duration = baseDuration / characterBody.attackSpeed;
            selfPush = baseSelfPush;
            timeToAttack = baseTimeToAttack / characterBody.attackSpeed;
            upTransitionSpeed = baseUpTransitionSpeed / characterBody.attackSpeed;
            downTransitionSpeed = baseDownTransitionSpeed / characterBody.attackSpeed;
            rootMotionVelocity = selfPush;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= timeToAttack)
            {
                if (isAuthority)
                {
                    overlapAttack.Fire();
                    if (characterMotor)
                    {
                        Vector3 velocityDirection = GetAimRay().direction;
                        velocityDirection.y = 0f;
                        velocityDirection.Normalize();
                        float pushPower = Mathf.SmoothDamp(selfPush, 0f, ref rootMotionVelocity, duration - timeToAttack, float.PositiveInfinity, Time.fixedDeltaTime);
                        characterMotor.rootMotion += velocityDirection * pushPower * Time.fixedDeltaTime;
                    }
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

                    PlayCrossfade("UpperBody, Override", step ? "Punch2Down" : "Punch1Down", "UpperBody.playbackRate", duration - timeToAttack, downTransitionSpeed);
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
        public static float baseUpTransitionSpeed = 0.1f;
        public static float baseDownTransitionSpeed = 0.1f;
        public static DamageType damageType = DamageType.Stun1s;
        public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
        public static BlastAttack.FalloffModel falloffModel = BlastAttack.FalloffModel.SweetSpot;
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
            PlayCrossfade("UpperBody, Override", "Punch3Up", "UpperBody.playbackRate", timeToAttack, upTransitionSpeed);
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
            PlayCrossfade("UpperBody, Override", "Punch3Down", "UpperBody.playbackRate", duration - timeToAttack, downTransitionSpeed);
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
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
    public class Slam : BaseGooboState
    {
    }
    public class ThrowGrenade : BaseGooboState
    {
        public static float damageCoefficient => ThrowGrenadeConfig.damageCoefficient.Value;
        public static float baseTimeToThrow => ThrowGrenadeConfig.timeToAttack.Value;
        public static float baseDuration => ThrowGrenadeConfig.duration.Value;
        public static float force = ThrowGrenadeConfig.force.Value;
        public static DamageType damageType = DamageType.Stun1s;
        public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
        public float timeToThrow;
        public float duration;
        public GameObject currentGrenade;
        private bool fired;
        public override void OnEnter()
        {
            base.OnEnter();
            StartAimMode();
            SetValues();
            PlayAnimation("UpperBody, Override", "ThrowUp", "UpperBody.playbackRate", timeToThrow);
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
            PlayAnimation("UpperBody, Override", "ThrowDown", "UpperBody.playbackRate", duration - timeToThrow);
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
        public void SetValues()
        {
            timeToThrow = baseTimeToThrow / characterBody.attackSpeed;
            duration = baseDuration / characterBody.attackSpeed;
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
    public class Decoy : BaseGooboState
    {
        public static float cloakDuration => DecoyConfig.duration.Value;
        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                Vector3 vector3 = Utils.GetClosestNodePosition(characterBody.footPosition, characterBody.hullClassification, float.PositiveInfinity, out Vector3 nodePosition) ? nodePosition : characterBody.footPosition;
                CharacterMaster decoyMaster = Utils.SpawnGoobo(characterBody.master, vector3, transform.rotation);
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
        public static GameObject projectile = Assets.GooboGrenadeType1Projectile;
        public static float maxDistance = 32f;
        public static float damageCoefficient => FireMinionsConfig.damageCoefficient.Value;
        public static float force = FireMinionsConfig.force.Value;
        public static float timeToTarget = FireMinionsConfig.timeToTarget.Value;
        public static DamageType damageType = DamageType.Generic;
        public static DamageTypeExtended damageTypeExtended = DamageTypeExtended.Generic;
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
            Debug.Log(instanceData);
            if (instanceData == null) return;
            GobooThrowGooboMinionsTracker gobooThrowGooboMinionsTracker = instanceData.gobooThrowGooboMinionsTracker;
            Debug.Log(gobooThrowGooboMinionsTracker);
            if (!gobooThrowGooboMinionsTracker) return;
            HurtBox trackingTarget = gobooThrowGooboMinionsTracker.trackingTarget;
            Debug.Log(trackingTarget);
            if (!trackingTarget) return;
            CharacterMaster characterMaster = characterBody.master;
            Debug.Log(characterMaster);
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
            Debug.Log(minionGroup);
            if (minionGroup == null) return;
            foreach (MinionOwnership minion in minionGroup.members)
            {
                Debug.Log(minion);
                if (minion == null) continue;
                CharacterMaster minionMaster = minion.GetComponent<CharacterMaster>();
                Debug.Log(minionMaster);
                if (minionMaster == null) continue;
                CharacterBody minionBody = minionMaster.GetBody();
                Debug.Log(minionBody);
                if (minionBody == null || minionBody.bodyIndex != Assets.Goobo13BodyIndex) continue;
                Vector3 position = trackingTarget.transform.position;
                Vector3 direction = position - minionBody.corePosition;
                float distance = direction.magnitude;
                //Ray ray = new Ray(position, direction.normalized);
                //if (!Physics.Raycast(ray, maxDistance, LayerIndex.world.mask)) continue;
                //HealthComponent healthComponent = minionBody.healthComponent;
                //Debug.Log(healthComponent);
                //if (healthComponent == null) continue;
                position = minionBody.corePosition;
                minionMaster.TrueKill();
                //healthComponent.Suicide();
                Fire(position, direction, distance);
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
}
