using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.Navigation;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using static Goobo13.Content;
using static R2API.DotAPI;

namespace Goobo13
{
    public static class Utils
    {
        public const string DamageCoefficientName = "Damage Coefficient";
        public const string ProcCoefficientName = "Proc Coefficient";
        public const string RadiusName = "Radius";
        public const string DistanceName = "Distance";
        public const string ForceName = "Force";
        public const string SelfPushName = "Self Push";
        public const string DurationName = "Duration";
        public const string LifetimeName = "Duration";
        public const string DamageTypeName = "Damage Type";
        public const string DamageTypeExtendedName = "Damage Type Extended";
        public const string GooboAmountName = "Goobo Amount";
        public const string TimeToAttackName = "Time to Attack";
        public const string TimeToTargetName = "Attack Arrival Time";
        public const string FalloffName = "Falloff";
        public const string SummonGoobosName = "Goobo Juxtapose";
        public const string PunchName = "Goobo Punch";
        public const string SuperPunchName = "Goobo Super Punch";
        public const string LeapName = "Leap";
        public const string SlamName = "Slam";
        public const string ThrowGrenadeName = "Goobo Grenade";
        public const string GooboMissileName = "Goobo Missile";
        public const string DecoyName = "Clone Walk";
        public const string UnstableDecoyName = "Unstable Clone Walk";
        public const string FireMinionsName = "Corrosive Dogpile";
        public const string ConsumeMinionsName = "Corrosive Consumption";
        public static bool GetClosestNodePosition(Vector3 position, HullClassification hullClassification, float maxRadius, out Vector3 nodePosition)
        {
            NodeGraph groundNodes = SceneInfo.instance.groundNodes;
            NodeGraph.NodeIndex nodeIndex = groundNodes.FindClosestNode(position, hullClassification, maxRadius);
            if (groundNodes.GetNodePosition(nodeIndex, out nodePosition))
            {
                return true;
            }
            else
            {
                nodePosition = Vector3.zero;
                return false;
            }
        }
        public static CharacterMaster SpawnGooboClone(CharacterMaster characterMaster, Vector3 position, Quaternion rotation)
        {
            CharacterMaster gooboMaster = null;
            CharacterBody characterBody = characterMaster.GetBody();
            if (characterBody == null) return gooboMaster;
            gooboMaster = new MasterSummon
            {
                position = position,
                ignoreTeamMemberLimit = true,
                masterPrefab = Assets.Goobo13CloneMaster,
                summonerBodyObject = characterBody.gameObject,
                teamIndexOverride = characterBody.teamComponent ? characterBody.teamComponent.teamIndex : TeamIndex.None,
                rotation = rotation,
            }.Perform();
            if (gooboMaster)
            {
                Deployable deployable = gooboMaster.gameObject.AddComponent<Deployable>();
                MasterSuicideOnTimer masterSuicideOnTimer = gooboMaster.gameObject.GetOrAddComponent<MasterSuicideOnTimer>();
                masterSuicideOnTimer.lifeTimer = SummonGoobosConfig.lifetime.Value;
                deployable.onUndeploy = new UnityEvent();
                deployable.onUndeploy.AddListener(new UnityAction(gooboMaster.TrueKill));
                characterMaster.AddDeployable(deployable, Assets.GooboDeployableSlot);
                CharacterBody gooboBody = gooboMaster.GetBody();
                if (gooboMaster.inventory)
                {
                    gooboMaster.inventory.GiveItem(Assets.CopyOwnerStats, 1);
                }
                if (gooboBody)
                {
                    gooboBody.doNotReassignToTeamBasedCollisionLayer = true;
                    gooboBody.gameObject.layer = LayerIndex.debris.intVal; // LayerIndex.GetAppropriateFakeLayerForTeam(gooboBody.teamComponent.teamIndex).intVal;
                    if (gooboBody.characterMotor && gooboBody.characterMotor.Motor)
                    gooboBody.characterMotor.Motor.RebuildCollidableLayers();
                }
            }
            return gooboMaster;
        }
        public static bool GetRandomNodePosition(out Vector3 nodePosition)
        {
            NodeGraph groundNodes = SceneInfo.instance.groundNodes;
            List<NodeGraph.NodeIndex> nodeIndices = groundNodes.GetActiveNodesForHullMask(HullMask.Human);
            NodeGraph.NodeIndex nodeIndex = nodeIndices[UnityEngine.Random.Range(0, nodeIndices.Count)];
            if (groundNodes.GetNodePosition(nodeIndex, out nodePosition))
            {
                return true;
            }
            else
            {
                nodePosition = Vector3.zero;
                return false;
            }
        }
        public static CharacterMaster SpawnEvilGooboClone(Vector3 position, Quaternion rotation)
        {
            CharacterMaster gooboMaster = new MasterSummon
            {
                position = position,
                ignoreTeamMemberLimit = true,
                masterPrefab = Assets.Goobo13CloneMaster,
                summonerBodyObject = null,
                teamIndexOverride = TeamIndex.Monster,
                rotation = rotation,
                useAmbientLevel = true,
            }.Perform();
            if (gooboMaster)
            {
                if (gooboMaster.inventory)
                {
                    gooboMaster.inventory.GiveItem(Assets.ImpStack);
                }
            }
            return gooboMaster;
        }
        public static float gooboGummyNoCollisionTime = 2.5f;
        public static CharacterMaster SpawnGoobo(CharacterMaster characterMaster, Vector3 position, Quaternion rotation)
        {
            CharacterMaster gooboMaster = null;
            CharacterBody characterBody = characterMaster.GetBody();
            if (characterBody == null) return gooboMaster;
            gooboMaster = new MasterSummon
            {
                position = position,
                ignoreTeamMemberLimit = true,
                masterPrefab = Assets.Goobo13Master,
                summonerBodyObject = characterBody.gameObject,
                teamIndexOverride = characterBody.teamComponent ? characterBody.teamComponent.teamIndex : TeamIndex.None,
                rotation = rotation
            }.Perform();
            if (gooboMaster)
            {
                MasterSuicideOnTimer masterSuicideOnTimer = gooboMaster.GetOrAddComponent<MasterSuicideOnTimer>();
                masterSuicideOnTimer.lifeTimer = DecoyConfig.lifetime.Value;
                Deployable deployable = gooboMaster.gameObject.AddComponent<Deployable>();
                deployable.onUndeploy = new UnityEvent();
                deployable.onUndeploy.AddListener(new UnityAction(gooboMaster.TrueKill));
                characterMaster.AddDeployable(deployable, DeployableSlot.GummyClone);
                CharacterBody gooboBody = gooboMaster.GetBody();
                if (gooboMaster.inventory)
                {
                }
                if (gooboBody)
                {
                    gooboBody.doNotReassignToTeamBasedCollisionLayer = true;
                    gooboBody.gameObject.layer = LayerIndex.debris.intVal; // LayerIndex.GetAppropriateFakeLayerForTeam(gooboBody.teamComponent.teamIndex).intVal;
                    if (gooboBody.characterMotor && gooboBody.characterMotor.Motor)
                        gooboBody.characterMotor.Motor.RebuildCollidableLayers();
                    ChangeLayerOnTimer changeLayerOnTimer = gooboBody.gameObject.AddComponent<ChangeLayerOnTimer>();
                    changeLayerOnTimer.timer = gooboGummyNoCollisionTime;
                    changeLayerOnTimer.layer = LayerIndex.GetAppropriateLayerForTeam(gooboBody.teamComponent.teamIndex);
                    EntityStateMachine entityStateMachine = gooboBody.GetComponent<EntityStateMachine>();
                    if (entityStateMachine != null)
                    {
                        entityStateMachine.initialStateType = entityStateMachine.mainStateType;
                    }
                }
            }
            return gooboMaster;
        }
        public static DotController.DotDef CreateDOT(BuffDef buffDef, out DotController.DotIndex dotIndex, bool resetTimerOnAdd, float interval, float damageCoefficient, DamageColorIndex damageColorIndex, CustomDotBehaviour customDotBehaviour, CustomDotVisual customDotVisual = null, CustomDotDamageEvaluation customDotDamageEvaluation = null)
        {
            DotController.DotDef dotDef = new DotController.DotDef
            {
                resetTimerOnAdd = resetTimerOnAdd,
                interval = interval,
                damageCoefficient = damageCoefficient,
                damageColorIndex = damageColorIndex,
                associatedBuff = buffDef
            };
            dotIndex = DotAPI.RegisterDotDef(dotDef, customDotBehaviour, customDotVisual, customDotDamageEvaluation);
            return dotDef;

        }
        public static ConfigEntry<T> CreateConfig<T>(string section, string key, T defaultValue, string description)
        {
            return CreateConfig(Goobo13Plugin.configFile, section, key, defaultValue, description);
        }
        public static ConfigEntry<T> CreateConfig<T>(ConfigFile configFile, string section, string key, T defaultValue, string description)
        {
            ConfigEntry<T> entry = configFile.Bind(section, key, defaultValue, description);
            if (Goobo13Plugin.riskOfOptionsEnabled) ModCompatabilities.RiskOfOptionsCompatability.AddConfig(entry, defaultValue);
            return entry;
        }
        public static string GetInScenePath(Transform transform)
        {
            if (transform == null) return "null";
            var current = transform;
            var inScenePath = new List<string> { current.name };
            while (current != transform.root)
            {
                current = current.parent;
                inScenePath.Add(current.name);
            }
            var sb = new StringBuilder();
            foreach (var item in Enumerable.Reverse(inScenePath)) sb.Append($"/{item}");
            return sb.ToString().TrimStart('/');
        }
        public static InputBankTest.ButtonState GetButtonStateFromId(InputBankTest inputBankTest, int id)
        {
            if (id == 1) return inputBankTest.skill1;
            if (id == 2) return inputBankTest.skill2;
            if (id == 3) return inputBankTest.skill3;
            if (id == 4) return inputBankTest.skill4;
            return inputBankTest.skill1;
        }
        public static GameObject RegisterCharacterBody(this GameObject characterBody, Action<GameObject> onCharacterBodyRegistered = null)
        {
            bodies.Add(characterBody);
            onCharacterBodyRegistered?.Invoke(characterBody);
            return characterBody;
        }
        public static GameObject RegisterCharacterMaster(this GameObject characterMaster, Action<GameObject> onCharacterMasterRegistered = null)
        {
            masters.Add(characterMaster);
            onCharacterMasterRegistered?.Invoke(characterMaster);
            return characterMaster;
        }
        public static T RegisterSkillDef<T>(this T skillDef, Action<T> onSkillDefRegistered = null) where T : SkillDef
        {
            skills.Add(skillDef);
            onSkillDefRegistered?.Invoke(skillDef);
            return skillDef;
        }
        public static T RegisterItemDef<T>(this T itemDef, Action<T> onItemDefRegistered = null) where T : ItemDef
        {
            items.Add(itemDef);
            onItemDefRegistered?.Invoke(itemDef);
            return itemDef;
        }
        public static T RegisterSkillFamily<T>(this T skillFamily, Action<T> onSkillFamilyRegistered = null) where T : SkillFamily
        {
            skillFamilies.Add(skillFamily);
            onSkillFamilyRegistered?.Invoke(skillFamily);
            return skillFamily;
        }
        public static T RegisterSurvivorDef<T>(this T survivorDef, Action<T> onSurvivorDefRegistered = null) where T : SurvivorDef
        {
            survivors.Add(survivorDef);
            onSurvivorDefRegistered?.Invoke(survivorDef);
            return survivorDef;
        }
        public static T RegisterSkinDef<T>(this T skinDef, Action<T> onSkinDefRegistered = null) where T : SkinDef
        {
            onSkinDefRegistered?.Invoke(skinDef);
            return skinDef;
        }
        public static T RegisterBuffDef<T>(this T buffDef, Action<T> onBuffDefRegistered = null) where T : BuffDef
        {
            buffs.Add(buffDef);
            onBuffDefRegistered?.Invoke(buffDef);
            return buffDef;
        }
        public static GameObject RegisterProjectile(this GameObject projectile, Action<GameObject> onProjectileRegistered = null)
        {
            projectiles.Add(projectile);
            networkPrefabs.Add(projectile);
            onProjectileRegistered?.Invoke(projectile);
            return projectile;
        }
        public static EffectDef RegisterEffect(this GameObject effect, Action<GameObject> onEffectRegistered = null)
        {
            EffectDef effectDef = new EffectDef
            {
                prefab = effect
            };
            effects.Add(effectDef);
            onEffectRegistered?.Invoke(effect);
            return effectDef;
        }
        public static GameObject RegisterNetworkPrefab(this GameObject networkPrefab, Action<GameObject> onnetworkPrefabRegistered = null)
        {
            networkPrefabs.Add(networkPrefab);
            return networkPrefab;
        }
        public static Type RegisterEntityState(this Type type)
        {
            states.Add(type);
            return type;
        }
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
        }
        public static T GetOrAddComponent<T>(this Transform transform) where T : Component
        {
            return transform.gameObject.GetOrAddComponent<T>();
        }
        public static T GetOrAddComponent<T>(this Component component) where T : Component
        {
            return component.gameObject.GetOrAddComponent<T>();
        }
    }
}
