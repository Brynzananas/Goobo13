using R2API;
using R2API.Networking;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering.PostProcessing;

namespace Goobo13
{
    public static class Assets
    {
        public static AssetBundle assetBundle;
        public static SurvivorDef Goobo13;
        public static GameObject Goobo13Body;
        public static CharacterBody Goobo13CharacterBody;
        public static GameObject Goobo13CloneBody;
        public static CharacterBody Goobo13CloneCharacterBody;
        public static GameObject Goobo13Master;
        public static GameObject Goobo13CloneMaster;
        public static GameObject Goobo13Emotes;
        public static GameObject Goobo13CloneEmotes;
        public static GameObject GooboBall;
        public static GameObject GooboGrenadeType1Projectile;
        public static GameObject GooboGrenadeType2Projectile;
        public static GameObject GooboGrenadeType3Projectile;
        public static GameObject GooboGrenadeType4Projectile;
        public static GameObject GooboGrenadeType5Projectile;
        public static GameObject RevolutionarySanguineVapor;
        public static GameObject RevolutionaryChainSanguineVapor;
        public static GameObject RevolutionaryLingeringSanguineVapor;
        public static GameObject RevolutionaryAbyssalSpike;
        public static GameObject GooboCorrosiveDogpileTrackingIndicator;
        public static GameObject GooboCloneMissileTrackingIndicator;
        public static GameObject EnterDimensionIndicator;
        public static Material EnterDimensionOverlay;
        public static EffectDef GooboOrb;
        public static EffectDef GooboExplosion;
        public static EffectDef GooboPunchEffect;
        public static EffectDef GooboImpact;
        public static EffectDef GooboSplash;
        public static GameObject BetweenSpacesPP;
        public static PassiveItemSkillDef GooboJuxtapose;
        public static SteppedSkillDef GooboPunch;
        public static SkillDef GooboSlam;
        public static SkillDef Incite;
        public static SkillDef Subdue;
        public static RevolutionarySkillDef Execution;
        public static RevolutionarySkillDef Advance;
        public static RevolutionarySkillDef Exile;
        public static GooboRandomGrenadeSkillDef GooboGrenade;
        public static GooboSkillDef GooboMissile;
        public static SkillDef CloneWalk;
        public static SkillDef UnstableCloneWalk;
        public static SteppedSkillDef Leap;
        public static GooboSkillDef CorrosiveDogpile;
        public static GooboSkillDef GooboConsumption;
        public static SkillFamily Passive;
        public static SkillFamily Primary;
        public static SkillFamily Secondary;
        public static SkillFamily Utility;
        public static SkillFamily Special;
        public static BuffDef GooboCorrosion;
        public static BuffDef GooboCorrosionCharge;
        public static BuffDef GooboConsumptionCharge;
        public static BuffDef SpawnGooboOnEnd;
        public static BuffDef InBetweenSpace;
        public static ItemDef GooboJuxtaposePassive;
        public static ItemDef CopyOwnerStats;
        public static ItemDef ImpStack;
        public static DotController.DotDef GooboCorrosionDot;
        public static DotController.DotIndex GooboCorrosionDotIndex;
        public static DamageAPI.ModdedDamageType ChanceToSpawnGooboDamageType;
        public static DamageAPI.ModdedDamageType GooboCorrosionDamageType;
        public static DamageAPI.ModdedDamageType AbysstouchedDamageType;
        public static DeployableSlot GooboDeployableSlot;
        public static BodyIndex Goobo13BodyIndex;
        public static BodyIndex Goobo13CloneBodyIndex;
        public static void Init()
        {
            assetBundle = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Goobo13Plugin.PInfo.Location), "assetbundles", "goobo13assets")).assetBundle;
            Material[] materials = assetBundle.LoadAllAssets<Material>();
            foreach (Material material in assetBundle.LoadAllAssets<Material>())
            {
                if (material.name == "matEnterDimensionOverlay")
                {
                    EnterDimensionOverlay = material;
                }
                if (!material.shader.name.StartsWith("StubbedRoR2") && !material.shader.name.StartsWith("StubbedDecalicious"))
                {
                    continue;
                }
                bool isRoR2 = material.shader.name.StartsWith("StubbedRoR2");
                string shaderName = (isRoR2 ? material.shader.name.Replace("StubbedRoR2", "RoR2") : material.shader.name.Replace("StubbedDecalicious", "Decalicious")) + ".shader";
                Shader replacementShader = Addressables.LoadAssetAsync<Shader>(shaderName).WaitForCompletion();
                if (replacementShader)
                {
                    material.shader = replacementShader;
                }
            }
            Goobo13 = assetBundle.LoadAsset<SurvivorDef>("Assets/Goobo13/Character/Goobo13.asset").RegisterSurvivorDef();
            Goobo13Body = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Character/Goobo13Body.prefab").RegisterCharacterBody();
            Goobo13CharacterBody = Goobo13Body.GetComponent<CharacterBody>();
            Goobo13CloneBody = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Character/Goobo13CloneBody.prefab").RegisterCharacterBody();
            Goobo13CloneCharacterBody = Goobo13CloneBody.GetComponent<CharacterBody>();
            Goobo13Master = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Character/Goobo13MonsterMaster.prefab").RegisterCharacterMaster();
            Goobo13CloneMaster = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Character/Goobo13CloneMonsterMaster.prefab").RegisterCharacterMaster();
            Goobo13Emotes = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Character/Goobo13Emotes.prefab");
            Goobo13CloneEmotes = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Character/Goobo13CloneEmotes.prefab");
            if (Goobo13Plugin.emotesEnabled) ModCompatabilities.EmoteCompatability.Init();
            CharacterBody characterBody = Goobo13Body.GetComponent<CharacterBody>();
            CameraTargetParams cameraTargetParams = Goobo13Body.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Common/ccpStandardMelee.asset").WaitForCompletion();
            characterBody.preferredPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").WaitForCompletion();
            characterBody._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoCrosshair.prefab").WaitForCompletion();
            GameObject gameObject = Goobo13Body.GetComponent<ModelLocator>().modelTransform.gameObject;
            GameObject genericDust = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
            gameObject.GetComponent<FootstepHandler>().footstepDustPrefab = genericDust;
            gameObject = Goobo13CloneBody.GetComponent<ModelLocator>().modelTransform.gameObject;
            gameObject.GetComponent<FootstepHandler>().footstepDustPrefab = genericDust;
            GooboBall = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Projectiles/GooboBall.prefab").RegisterProjectile();
            GooboCorrosionDamageType = DamageAPI.ReserveDamageType();
            ChanceToSpawnGooboDamageType = DamageAPI.ReserveDamageType();
            AbysstouchedDamageType = DamageAPI.ReserveDamageType();
            GooboCorrosiveDogpileTrackingIndicator = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Huntress/HuntressTrackingIndicator.prefab").WaitForCompletion();
            GooboCloneMissileTrackingIndicator = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC2/Seeker/SeekerTrackingIndicator.prefab").WaitForCompletion();
            GooboGrenadeType1Projectile = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Projectiles/GooboGrenadeType1Projectile.prefab").RegisterProjectile();
            GooboGrenadeType3Projectile = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Projectiles/GooboGrenadeType3Projectile.prefab").RegisterProjectile();
            GooboGrenadeType4Projectile = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Projectiles/GooboGrenadeType4Projectile.prefab").RegisterProjectile();
            GooboGrenadeType5Projectile = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Projectiles/GooboGrenadeType5Projectile.prefab").RegisterProjectile();
            RevolutionarySanguineVapor = assetBundle.LoadAsset<GameObject>("Assets/Revolutionary/Projectiles/RevolutionarySanguineVapor.prefab").RegisterProjectile();
            RevolutionaryChainSanguineVapor = assetBundle.LoadAsset<GameObject>("Assets/Revolutionary/Projectiles/RevolutionaryChainSanguineVapor.prefab").RegisterProjectile();
            RevolutionaryLingeringSanguineVapor = assetBundle.LoadAsset<GameObject>("Assets/Revolutionary/Projectiles/RevolutionaryLingeringSanguineVapor.prefab").RegisterProjectile();
            RevolutionaryAbyssalSpike = assetBundle.LoadAsset<GameObject>("Assets/Revolutionary/Projectiles/RevolutionaryAbyssalSpike.prefab").RegisterProjectile();
            GooboExplosion = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Effects/GooboExplosion.prefab").RegisterEffect();
            GooboPunchEffect = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Effects/GooboPunchEffect.prefab").RegisterEffect();
            GooboImpact = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Effects/GooboImpact.prefab").RegisterEffect();
            GooboSplash = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Effects/GooboCloneExplosion.prefab").RegisterEffect();
            GooboOrb = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Effects/GooboOrb.prefab").RegisterEffect();
            BetweenSpacesPP = assetBundle.LoadAsset<GameObject>("Assets/Revolutionary/Effects/ppBetweenSpace.prefab");
            //PostProcessProfile postProcessProfile = BetweenSpacesPP.GetComponent<PostProcessVolume>().profile;
            //RampFog rampFog = postProcessProfile.AddSettings<RampFog>();
            //rampFog.active = true;
            //rampFog.fogColorStart.overrideState = true;
            //rampFog.fogColorStart.value = new Color(1f, 0.7f, 0.7f, 0.2f);
            //rampFog.fogColorMid.overrideState = true;
            //rampFog.fogColorMid.value = new Color(1f, 0.42f, 0.42f, 0.5f);
            //rampFog.fogColorEnd.overrideState = true;
            //rampFog.fogColorEnd.value = new Color(1f, 0f, 0f, 1f);
            EnterDimensionIndicator = assetBundle.LoadAsset<GameObject>("Assets/Revolutionary/Effects/EnterDimensionIndicator.prefab");
            GooboJuxtapose = assetBundle.LoadAsset<PassiveItemSkillDef>("Assets/Goobo13/Skills/GooboJuxtapose.asset").RegisterSkillDef();
            GooboPunch = assetBundle.LoadAsset<SteppedSkillDef>("Assets/Goobo13/Skills/GooboPunch.asset").RegisterSkillDef();
            GooboSlam = assetBundle.LoadAsset<SkillDef>("Assets/Goobo13/Skills/GooboSlam.asset").RegisterSkillDef();
            Incite = assetBundle.LoadAsset<SkillDef>("Assets/Revolutionary/Skills/Incite.asset").RegisterSkillDef();
            Subdue = assetBundle.LoadAsset<SkillDef>("Assets/Revolutionary/Skills/Subdue.asset").RegisterSkillDef();
            Execution = assetBundle.LoadAsset<RevolutionarySkillDef>("Assets/Revolutionary/Skills/Execution.asset").RegisterSkillDef();
            Advance = assetBundle.LoadAsset<RevolutionarySkillDef>("Assets/Revolutionary/Skills/Advance.asset").RegisterSkillDef();
            Exile = assetBundle.LoadAsset<RevolutionarySkillDef>("Assets/Revolutionary/Skills/Exile.asset").RegisterSkillDef();
            GooboGrenade = assetBundle.LoadAsset<GooboRandomGrenadeSkillDef>("Assets/Goobo13/Skills/GooboGrenade.asset").RegisterSkillDef();
            GooboMissile = assetBundle.LoadAsset<GooboSkillDef>("Assets/Goobo13/Skills/GooboMissile.asset").RegisterSkillDef();
            GooboMissile.indicator = GooboCloneMissileTrackingIndicator;
            CloneWalk = assetBundle.LoadAsset<SkillDef>("Assets/Goobo13/Skills/CloneWalk.asset").RegisterSkillDef();
            UnstableCloneWalk = assetBundle.LoadAsset<SkillDef>("Assets/Goobo13/Skills/UnstableCloneWalk.asset").RegisterSkillDef();
            Leap = assetBundle.LoadAsset<SteppedSkillDef>("Assets/Goobo13/Skills/Leap.asset").RegisterSkillDef();
            CorrosiveDogpile = assetBundle.LoadAsset<GooboSkillDef>("Assets/Goobo13/Skills/CorrosiveDogpile.asset").RegisterSkillDef();
            CorrosiveDogpile.indicator = GooboCorrosiveDogpileTrackingIndicator;
            GooboConsumption = assetBundle.LoadAsset<GooboSkillDef>("Assets/Goobo13/Skills/CorrosiveConsumption.asset").RegisterSkillDef();
            GooboJuxtaposePassive = assetBundle.LoadAsset<ItemDef>("Assets/Goobo13/Items/GooboJuxtaposePassive.asset").RegisterItemDef();
            CopyOwnerStats = assetBundle.LoadAsset<ItemDef>("Assets/Goobo13/Items/CopyOwnerStats.asset").RegisterItemDef();
            ImpStack = assetBundle.LoadAsset<ItemDef>("Assets/Revolutionary/Items/ImpStack.asset").RegisterItemDef();
            GooboCorrosion = assetBundle.LoadAsset<BuffDef>("Assets/Goobo13/Buffs/bdGooboCorrosion.asset").RegisterBuffDef();
            GooboCorrosionCharge = assetBundle.LoadAsset<BuffDef>("Assets/Goobo13/Buffs/bdGooboCorrosionCharge.asset").RegisterBuffDef();
            GooboConsumptionCharge = assetBundle.LoadAsset<BuffDef>("Assets/Goobo13/Buffs/bdGooboConsumptionCharge.asset").RegisterBuffDef();
            SpawnGooboOnEnd = assetBundle.LoadAsset<BuffDef>("Assets/Goobo13/Buffs/bdSpawnGooboOnEnd.asset").RegisterBuffDef();
            InBetweenSpace = assetBundle.LoadAsset<BuffDef>("Assets/Revolutionary/Buffs/InBetweenSpace.asset").RegisterBuffDef();
            Passive = assetBundle.LoadAsset<SkillFamily>("Assets/Goobo13/SkillFamilies/Goobo13Passive.asset").RegisterSkillFamily();
            Primary = assetBundle.LoadAsset<SkillFamily>("Assets/Goobo13/SkillFamilies/Goobo13Primary.asset").RegisterSkillFamily();
            Secondary = assetBundle.LoadAsset<SkillFamily>("Assets/Goobo13/SkillFamilies/Goobo13Secondary.asset").RegisterSkillFamily();
            Utility = assetBundle.LoadAsset<SkillFamily>("Assets/Goobo13/SkillFamilies/Goobo13Utility.asset").RegisterSkillFamily();
            Special = assetBundle.LoadAsset<SkillFamily>("Assets/Goobo13/SkillFamilies/Goobo13Special.asset").RegisterSkillFamily();
            typeof(HandlePunch).RegisterEntityState();
            typeof(Punch).RegisterEntityState();
            typeof(SuperPunch).RegisterEntityState();
            typeof(ThrowGrenade).RegisterEntityState();
            typeof(Decoy).RegisterEntityState();
            typeof(FireMinions).RegisterEntityState();
            typeof(GooboDeath).RegisterEntityState();
            typeof(AimGooboMissile).RegisterEntityState();
            typeof(FireGooboMissile).RegisterEntityState();
            typeof(GooboMissile).RegisterEntityState();
            typeof(ConsumeMinions).RegisterEntityState();
            typeof(Slam).RegisterEntityState();
            typeof(UnstableDecoy).RegisterEntityState();
            typeof(FireSpout).RegisterEntityState();
            typeof(TeleportBehind).RegisterEntityState();
            typeof(TeleportForward).RegisterEntityState();
            typeof(EnterDimension).RegisterEntityState();
            GooboCorrosionDot = Utils.CreateDOT(GooboCorrosion, out GooboCorrosionDotIndex, true, 1f, 1f, DamageColorIndex.DeathMark, null);
            GooboDeployableSlot = DeployableAPI.RegisterDeployableSlot(GetGobooDeployableSlot);
            OrbAPI.AddOrb<GooboOrb>();
            OrbAPI.AddOrb<GooboConsumeOrb>();
            NetworkingAPI.RegisterMessageType<AddSlamSkillOverride>();
            ContentManager.collectContentPackProviders += (addContentPackProvider) => addContentPackProvider(new Content());
        }
        public static int GetGobooDeployableSlot(CharacterMaster self, int deployableSlotMultiplier) => self.inventory && self.inventory.GetItemCount(GooboJuxtaposePassive) > 0 ? SummonGoobosConfig.maxAmount.Value : 0;
    }
}
