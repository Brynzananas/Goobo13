using R2API;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Goobo13
{
    public static class Hooks
    {
        private static bool hooksSet;
        public static void SetHooks()
        {
            if (hooksSet) return;
            On.RoR2.CharacterBody.SetBuffCount += CharacterBody_SetBuffCount;
            //On.RoR2.CharacterBody.OnEnable += CharacterBody_OnEnable;
            //On.RoR2.CharacterBody.OnDisable += CharacterBody_OnDisable;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.CharacterAI.BaseAI.OnBodyStart += BaseAI_OnBodyStart;
            On.RoR2.CharacterAI.BaseAI.OnBodyLost += BaseAI_OnBodyLost;
            GlobalEventManager.onServerDamageDealt += GlobalEventManager_onServerDamageDealt;
            On.RoR2.BodyCatalog.SetBodyPrefabs += BodyCatalog_SetBodyPrefabs;
            hooksSet = true;
        }

        private static void BodyCatalog_SetBodyPrefabs(On.RoR2.BodyCatalog.orig_SetBodyPrefabs orig, GameObject[] newBodyPrefabs)
        {
            orig(newBodyPrefabs);
            Assets.Goobo13BodyIndex = BodyCatalog.FindBodyIndex(Assets.Goobo13Body.name);
            Assets.Goobo13CloneBodyIndex = BodyCatalog.FindBodyIndex(Assets.Goobo13CloneBody.name);
        }

        public static float GooboCorrosionDamageCoefficient = 1f;
        public static float GooboCorrosionDuration = 6f;
        public static float GooboChanceSpawn = 20f;
        private static void GlobalEventManager_onServerDamageDealt(DamageReport obj)
        {
            CharacterBody attackerBody = obj.attackerBody;
            CharacterBody victimBody = obj.victimBody;
            DamageInfo damageInfo = obj.damageInfo;
            Inventory attackerInventory = attackerBody?.inventory;
            if (attackerBody)
            {
                int chargeCount = attackerBody.GetBuffCount(Assets.GooboCorrosionCharge);
                if (chargeCount > 0 || damageInfo.HasModdedDamageType(Assets.GooboCorrosionDamageType))
                {
                    if (chargeCount < 1) chargeCount = 1;
                    int buffCount = victimBody.GetBuffCount(Assets.GooboCorrosion);
                    if (buffCount < gooboBuffMaxStacks)
                    {
                        InflictDotInfo dotInfo = new InflictDotInfo()
                        {
                            attackerObject = attackerBody.gameObject,
                            victimObject = obj.victimBody.gameObject,
                            totalDamage = attackerBody.damage * GooboCorrosionDuration,
                            damageMultiplier = GooboCorrosionDamageCoefficient,
                            duration = GooboCorrosionDuration,
                            dotIndex = Assets.GooboCorrosionDotIndex,
                        };
                        for (int i = 0; i < chargeCount; i++)
                        DotController.InflictDot(ref dotInfo);
                    }
                }
                if (attackerInventory && attackerInventory.GetItemCount(Assets.GooboJuxtaposePassive) > 0 && damageInfo.HasModdedDamageType(Assets.ChanceToSpawnGooboDamageType))
                {
                    float luck = obj.attackerMaster ? Mathf.Max(0f, obj.attackerMaster.luck) : 0f;
                    if (Util.CheckRoll(GooboChanceSpawn * damageInfo.procCoefficient, luck))
                    {
                        Vector3 vector3 = Utils.GetClosestNodePosition(damageInfo.position, HullClassification.Human, float.PositiveInfinity, out Vector3 nodePosition) ? nodePosition : damageInfo.position;
                        CharacterMaster decoyMaster = Utils.SpawnGooboClone(obj.attackerMaster, vector3, Quaternion.identity);
                    }
                }
            }
        }

        public static void UnsetHooks()
        {
            if (!hooksSet) return;
            hooksSet = false;
        }
        private static void BaseAI_OnBodyLost(On.RoR2.CharacterAI.BaseAI.orig_OnBodyLost orig, BaseAI self, CharacterBody characterBody)
        {
            orig(self, characterBody);
            baseAIs.Remove(self);
        }

        public static List<BaseAI> baseAIs = [];
        private static void BaseAI_OnBodyStart(On.RoR2.CharacterAI.BaseAI.orig_OnBodyStart orig, BaseAI self, CharacterBody newBody)
        {
            orig(self, newBody);
            baseAIs.Add(self);
        }

        public static float CorrosionArmorDecrease = 1f;
        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            int buffCount = sender.GetBuffCount(Assets.GooboCorrosion);
            args.armorAdd -= CorrosionArmorDecrease * buffCount;
        }

        private static void CharacterBody_OnDisable(On.RoR2.CharacterBody.orig_OnDisable orig, CharacterBody self)
        {
            orig(self);
            if (self.bodyIndex == Assets.Goobo13BodyIndex) goobs.Remove(self);
        }

        private static void CharacterBody_OnEnable(On.RoR2.CharacterBody.orig_OnEnable orig, CharacterBody self)
        {
            orig(self);
            if (self.bodyIndex == Assets.Goobo13BodyIndex) goobs.Add(self);
        }

        public static List<CharacterBody> goobs = [];
        public static int gooboBuffMaxStacks = 30;
        private static void CharacterBody_SetBuffCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            if (buffType == Assets.GooboCorrosion.buffIndex) if (newCount > gooboBuffMaxStacks) newCount = gooboBuffMaxStacks;
            /*if (self.bodyIndex == Assets.DemolisherBodyIndex && buffType == Assets.GooboCorrosion.buffIndex)
            {
                int buffs = 0;
                int selfBuffs = 0;
                foreach (CharacterBody characterBody in goobs)
                {
                    if (characterBody == null) continue;
                    int buffCount = characterBody.GetBuffCount(buffType);
                    if (characterBody == self) selfBuffs = buffCount;
                    buffs += buffCount;
                }
                if (buffs - selfBuffs + newCount >= gooboBuffMaxStacks)
                {
                    newCount = gooboBuffMaxStacks - (buffs - selfBuffs);
                }
            }*/
            orig(self, buffType, newCount);
        }
    }
}
