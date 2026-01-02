using R2API;
using R2API.Networking.Interfaces;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static UnityEngine.UI.Image;

namespace Goobo13
{
    public class GooboOrb : Orb
    {
        public float damage;
        public float procCoefficient;
        public float radius;
        public float effectScale = 1f;
        public float force;
        public bool crit;
        public DamageTypeCombo damageTypeCombo;
        public BlastAttack.FalloffModel falloffModel;
        private GameObject _attacker;
        public TeamIndex teamIndex;
        public int gooboAmount;
        public GameObject visualPrefab;
        public GameObject attacker
        {
            get => _attacker;
            set
            {
                if (value == null)
                {
                    _attacker = value;
                    attackerBody = null;
                    attackerMaster = null;
                    return;
                }
                _attacker = value;
                attackerBody = value.GetComponent<CharacterBody>();
                attackerMaster = attackerBody.master;
            }
        }
        public CharacterBody attackerBody { get; private set; }
        public CharacterMaster attackerMaster { get; private set; }
        public override void Begin()
        {
            base.Begin();
            EffectData effectData = new EffectData
            {
                scale = effectScale,
                origin = origin,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(target);
            EffectManager.SpawnEffect(visualPrefab, effectData, true);
        }
        public override void OnArrival()
        {
            base.OnArrival();
            if (!target || !target.collider) return;
            BlastAttack blastAttack = new BlastAttack
            {
                attacker = attacker,
                baseDamage = damage,
                baseForce = force,
                crit = crit,
                damageType = damageTypeCombo,
                falloffModel = falloffModel,
                damageColorIndex = DamageColorIndex.Default,
                inflictor = attacker,
                losType = BlastAttack.LoSType.None,
                procCoefficient = procCoefficient,
                radius = radius,
                teamIndex = teamIndex,
                position = target.collider.bounds.center,
            };
            blastAttack.Fire();
            EffectData effectData = new EffectData
            {
                origin = blastAttack.position,
                scale = blastAttack.radius,
            };
            EffectManager.SpawnEffect(Assets.GooboExplosion.prefab, effectData, true);
            if (gooboAmount <= 0) return;
            if (!attackerMaster) return;
            for (int i = 0; i < gooboAmount; i++)
            {
                Vector3 position = target.collider.bounds.center;
                //CharacterBody targetBody = target?.healthComponent?.body;
                //if (targetBody && attackerBody)
                //{
                //    position += (attackerBody.corePosition - position).normalized * (targetBody.bestFitRadius + 2f);
                //}
                Utils.SpawnGooboClone(attackerMaster, position, Quaternion.identity);
            }
        }
    }
    public class GooboConsumeOrb : Orb
    {
        public GameObject visualPrefab;
        public float heal;
        public float effectScale = 1f;
        public bool addSlam;
        public override void OnArrival()
        {
            base.OnArrival();
            HealthComponent healthComponent = target?.healthComponent;
            if (!healthComponent) return;
            CharacterBody characterBody = healthComponent.body;
            if (!characterBody) return;
            SkillLocator skillLocator = characterBody.skillLocator;
            if (!skillLocator) return;
            GenericSkill genericSkill = skillLocator.primary;
            if (!genericSkill) return;
            if (addSlam)
            {
                if (!characterBody.HasBuff(Assets.GooboConsumptionCharge)) new AddSlamSkillOverride(characterBody.netId).Send(R2API.Networking.NetworkDestination.Clients);
                characterBody.AddBuff(Assets.GooboConsumptionCharge);
            }
            healthComponent.itemCounts.barrierOnOverHeal++;
            healthComponent.HealFraction(heal, default);
            healthComponent.itemCounts.barrierOnOverHeal--;
        }
        public override void Begin()
        {
            base.Begin();
            EffectData effectData = new EffectData
            {
                scale = effectScale,
                origin = origin,
                genericFloat = duration
            };
            effectData.SetHurtBoxReference(target);
            EffectManager.SpawnEffect(visualPrefab, effectData, true);
        }
    }
}
