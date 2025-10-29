using Mono.Cecil.Cil;
using MonoMod.Cil;
using R2API;
using R2API.Utils;
using RoR2;
using RoR2.CharacterAI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

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
            GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            On.RoR2.BodyCatalog.SetBodyPrefabs += BodyCatalog_SetBodyPrefabs;
            On.RoR2.CharacterBody.GetVisibilityLevel_CharacterBody += CharacterBody_GetVisibilityLevel_CharacterBody;
            On.RoR2.CharacterBody.OnClientBuffsChanged += CharacterBody_OnClientBuffsChanged;
            IL.RoR2.CharacterModel.UpdateForCamera += CharacterModel_UpdateForCamera;
            On.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            //On.RoR2.CharacterAI.BaseAI.ManagedFixedUpdate += BaseAI_ManagedFixedUpdate;
            RoR2Application.onLoadFinished += OnRoR2Loaded;
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            On.RoR2.GenericSkill.CanExecute += GenericSkill_CanExecute;
            RoR2Application.onLoadFinished += Language.Init;
            hooksSet = true;
        }

        private static bool GenericSkill_CanExecute(On.RoR2.GenericSkill.orig_CanExecute orig, GenericSkill self)
        {
            bool ret = orig(self);
            if (self.characterBody && self.characterBody.HasBuff(Assets.SpawnGooboOnEnd)) ret = false;
            return ret;
        }

        public static float copyStatsPercentage => SummonGoobosConfig.statSharing.Value;
        private static void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            int itemCount = self.inventory ? self.inventory.GetItemCount(Assets.CopyOwnerStats) : 0;
            if (itemCount <= 0) return;
            CharacterBody ownerBody = self?.master?.minionOwnership?.ownerMaster?.GetBody();
            if (ownerBody == null) return;
            float value = copyStatsPercentage / 100f;
            self.damage = ownerBody.damage * value;
            self.attackSpeed = ownerBody.attackSpeed * value;
            self.moveSpeed = ownerBody.moveSpeed * value;
            self.maxHealth = ownerBody.maxHealth * value;
            self.crit = ownerBody.crit * value;
            self.critMultiplier = ownerBody.critMultiplier * value;
            self.regen = ownerBody.regen * value;
            self.armor = ownerBody.armor * value;
        }

        private static void OnRoR2Loaded()
        {
            pp = GameObject.Instantiate(Assets.BetweenSpacesPP);
            GameObject.DontDestroyOnLoad(pp);
            pp.SetActive(false);
        }

        private static void BaseAI_ManagedFixedUpdate(On.RoR2.CharacterAI.BaseAI.orig_ManagedFixedUpdate orig, BaseAI self, float deltaTime)
        {
            orig(self, deltaTime);
            if (!self.body || self.currentEnemy == null) return;
            CharacterBody enemyBody = self.currentEnemy.characterBody;
            if (!enemyBody) return;
            if (enemyBody.GetVisibilityLevel(self.body) >= VisibilityLevel.Revealed) return;
            Debug.Log("isInvisible");
            self.currentEnemy = null;
        }

        private static void HealthComponent_TakeDamageProcess(On.RoR2.HealthComponent.orig_TakeDamageProcess orig, HealthComponent self, DamageInfo damageInfo)
        {
            if (self.body && damageInfo.attacker)
            {
                CharacterBody attackerBody = damageInfo.attacker.GetComponent<CharacterBody>();
                if (attackerBody)
                {
                    bool attackerHasBuff = attackerBody.HasBuff(Assets.InBetweenSpace);
                    bool selfHasBuff = self.body.HasBuff(Assets.InBetweenSpace);
                    if (attackerHasBuff ^ selfHasBuff) return;
                }
            }
            orig(self, damageInfo);
        }

        private static void CharacterModel_UpdateForCamera(MonoMod.Cil.ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdarg(0),
                    x => x.MatchLdfld<CharacterModel>(nameof(CharacterModel.body)),
                    x => x.MatchLdarg(1),
                    x => x.MatchCallvirt(typeof(CameraRigController).GetPropertyGetter("targetTeamIndex")),
                    x => x.MatchCallvirt<CharacterBody>(nameof(CharacterBody.GetVisibilityLevel))
                ))
            {
                Instruction instruction = c.Next;
                Instruction instruction2 = c.Next.Next.Next.Next.Next.Next;
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate(NullCheck);
                bool NullCheck(CameraRigController cameraRigController) => cameraRigController.targetBody;
                c.Emit(OpCodes.Brfalse_S, instruction);
                c.Emit(OpCodes.Ldarg_0);
                c.Emit(OpCodes.Ldarg_1);
                c.EmitDelegate(GetIt);
                VisibilityLevel GetIt(CharacterModel characterModel, CameraRigController cameraRigController) => characterModel.body.GetVisibilityLevel(cameraRigController.targetBody);
                c.Emit(OpCodes.Br_S, instruction2);
            }
            else
            {
                Debug.LogError(il.Method.Name + " IL Hook failed!");
            }
        }

        private static void CharacterBody_OnClientBuffsChanged(On.RoR2.CharacterBody.orig_OnClientBuffsChanged orig, CharacterBody self)
        {
            orig(self);
            if (self.HasBuff(Assets.InBetweenSpace))
            {
                if (!pp.activeSelf) pp.SetActive(true);
            }
            else
            {
                if (pp.activeSelf) pp.SetActive(false);
            }

        }

        private static VisibilityLevel CharacterBody_GetVisibilityLevel_CharacterBody(On.RoR2.CharacterBody.orig_GetVisibilityLevel_CharacterBody orig, CharacterBody self, CharacterBody observer)
        {
            bool observerHasBuff = observer.HasBuff(Assets.InBetweenSpace);
            bool selfHasBuff = self.HasBuff(Assets.InBetweenSpace);
            if (observerHasBuff ^ selfHasBuff) return VisibilityLevel.Invisible;
            return orig(self, observer);
        }

        public static int maxImpStacks = 8;
        private static void GlobalEventManager_onCharacterDeathGlobal(DamageReport obj)
        {
            CharacterBody attackerBody = obj.attackerBody;
            CharacterMaster victimMaster = obj.victimMaster;
            Inventory victimInventory = victimMaster?.inventory;
            Inventory attackerInventory = attackerBody?.inventory;
            if (attackerBody)
            {
                if (victimInventory && attackerInventory)
                {
                    int impStacks = victimInventory.GetItemCount(Assets.ImpStack);
                    if (impStacks > 0)
                    {
                        victimInventory.RemoveItem(Assets.ImpStack, impStacks);
                        int attackerImpStacks = attackerInventory.GetItemCount(Assets.ImpStack);
                        if (attackerImpStacks + impStacks > maxImpStacks)
                        {
                            int diff = maxImpStacks - attackerImpStacks;
                            if (diff < 0) return;
                            impStacks -= maxImpStacks - attackerImpStacks;
                            if (impStacks <= 0) return;
                        }
                        attackerInventory.GiveItem(Assets.ImpStack, impStacks);
                    }
                }
            }
           
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
                if (damageInfo.HasModdedDamageType(Assets.AbysstouchedDamageType))
                {
                    RevolutionaryController revolutionaryController = attackerBody.GetComponent<RevolutionaryController>();
                    if (revolutionaryController != null) revolutionaryController.currentTarget = victimBody;
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
        public static int gooboConsumptionBuffMaxStacks = 10;
        public static GameObject pp;
        private static void CharacterBody_SetBuffCount(On.RoR2.CharacterBody.orig_SetBuffCount orig, CharacterBody self, BuffIndex buffType, int newCount)
        {
            if (NetworkServer.active)
            {
                if (self.master && buffType == Assets.SpawnGooboOnEnd.buffIndex)
                {
                    int buffCount = self.GetBuffCount(buffType);
                    int removalCount = buffCount - newCount;
                    if (removalCount > 0)
                    {
                        Vector3 direction = self.characterDirection ? self.characterDirection.forward : self.transform.forward;
                        for (int i = 0; i < removalCount; i++)
                        {
                            Utils.SpawnGooboClone(self.master, self.transform.position, Quaternion.LookRotation(direction));
                        }
                    }
                }
            }
            if (buffType == Assets.GooboCorrosion.buffIndex) if (newCount > gooboBuffMaxStacks) newCount = gooboBuffMaxStacks;
            if (buffType == Assets.GooboConsumptionCharge.buffIndex) if (newCount > gooboConsumptionBuffMaxStacks) newCount = gooboConsumptionBuffMaxStacks;
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
