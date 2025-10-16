using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Projectile;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Goobo13
{
    public static class Assets
    {
        public static AssetBundle assetBundle;
        public static SurvivorDef Goobo13;
        public static GameObject Goobo13Body;
        public static GameObject Goobo13CloneBody;
        public static GameObject Goobo13Master;
        public static GameObject Goobo13CloneMaster;
        public static GameObject Goobo13Emotes;
        public static GameObject GooboGrenadeType1Projectile;
        public static GameObject GooboGrenadeType2Projectile;
        public static GameObject GooboGrenadeType3Projectile;
        public static GameObject GooboGrenadeType4Projectile;
        public static GameObject GooboGrenadeType5Projectile;
        public static EffectDef GooboExplosion;
        public static PassiveItemSkillDef GooboJuxtapose;
        public static SteppedSkillDef GooboPunch;
        public static GooboRandomGrenadeSkillDef GooboGrenade;
        public static SkillDef CloneWalk;
        public static GooboThrowGooboMinionsSkillDef CorrosiveDogpile;
        public static SkillFamily Passive;
        public static SkillFamily Primary;
        public static SkillFamily Secondary;
        public static SkillFamily Utility;
        public static SkillFamily Special;
        public static BuffDef GooboCorrosion;
        public static ItemDef GooboJuxtaposePassive;
        public static DotController.DotDef GooboCorrosionDot;
        public static DotController.DotIndex GooboCorrosionDotIndex;
        public static DamageAPI.ModdedDamageType ChanceToSpawnGooboDamageType;
        public static DamageAPI.ModdedDamageType GooboCorrosionDamageType;
        public static DeployableSlot GooboDeployableSlot;
        public static BodyIndex Goobo13BodyIndex;
        public static BodyIndex Goobo13CloneBodyIndex;
        public static void Init()
        {
            assetBundle = AssetBundle.LoadFromFileAsync(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(Main.PInfo.Location), "assetbundles", "goobo13assets")).assetBundle;
            Material[] materials = assetBundle.LoadAllAssets<Material>();
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (!material.shader.name.StartsWith("StubbedRoR2"))
                {
                    continue;
                }
                if (material.name == "matCommando")
                {
                    material = Addressables.LoadAssetAsync<Material>("RoR2/Base/Commando/matCommandoDualies.mat").WaitForCompletion();
                    continue;
                }
                string shaderName = material.shader.name.Replace("StubbedRoR2", "RoR2") + ".shader";
                Shader replacementShader = Addressables.LoadAssetAsync<Shader>(shaderName).WaitForCompletion();
                if (replacementShader)
                {
                    material.shader = replacementShader;
                }
            }
            Goobo13 = assetBundle.LoadAsset<SurvivorDef>("Assets/Goobo13/Character/Goobo13.asset").RegisterSurvivorDef();
            Goobo13Body = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Character/Goobo13Body.prefab").RegisterCharacterBody();
            Goobo13CloneBody = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Character/Goobo13CloneBody.prefab").RegisterCharacterBody();
            Goobo13Master = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Character/Goobo13MonsterMaster.prefab").RegisterCharacterMaster();
            Goobo13CloneMaster = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Character/Goobo13CloneMonsterMaster.prefab").RegisterCharacterMaster();
            Goobo13Emotes = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Character/Goobo13Emotes.prefab");
            if (Main.emotesEnabled) ModCompatabilities.EmoteCompatability.Init();
            CharacterBody characterBody = Goobo13Body.GetComponent<CharacterBody>();
            CameraTargetParams cameraTargetParams = Goobo13Body.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = Addressables.LoadAssetAsync<CharacterCameraParams>("RoR2/Base/Common/ccpStandardMelee.asset").WaitForCompletion();
            characterBody.preferredPodPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/SurvivorPod/SurvivorPod.prefab").WaitForCompletion();
            characterBody._defaultCrosshairPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Croco/CrocoCrosshair.prefab").WaitForCompletion();
            GameObject gameObject = Goobo13Body.GetComponent<ModelLocator>().modelTransform.gameObject;
            gameObject.GetComponent<FootstepHandler>().footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();
            GooboGrenadeType1Projectile = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Projectiles/GooboGrenadeType1Projectile.prefab").RegisterProjectile();
            ProjectileDamage projectileDamage = GooboGrenadeType1Projectile.GetComponent<ProjectileDamage>();
            GooboCorrosionDamageType = DamageAPI.ReserveDamageType();
            ChanceToSpawnGooboDamageType = DamageAPI.ReserveDamageType();
            projectileDamage.damageType.AddModdedDamageType(GooboCorrosionDamageType);
            GooboGrenadeType3Projectile = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Projectiles/GooboGrenadeType3Projectile.prefab").RegisterProjectile();
            GooboGrenadeType4Projectile = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Projectiles/GooboGrenadeType4Projectile.prefab").RegisterProjectile();
            GooboGrenadeType5Projectile = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Projectiles/GooboGrenadeType5Projectile.prefab").RegisterProjectile();
            GooboExplosion = assetBundle.LoadAsset<GameObject>("Assets/Goobo13/Effects/GooboExplosion.prefab").RegisterEffect();
            GooboJuxtapose = assetBundle.LoadAsset<PassiveItemSkillDef>("Assets/Goobo13/Skills/GooboJuxtapose.asset").RegisterSkillDef();
            GooboPunch = assetBundle.LoadAsset<SteppedSkillDef>("Assets/Goobo13/Skills/GooboPunch.asset").RegisterSkillDef();
            GooboGrenade = assetBundle.LoadAsset<GooboRandomGrenadeSkillDef>("Assets/Goobo13/Skills/GooboGrenade.asset").RegisterSkillDef();
            CloneWalk = assetBundle.LoadAsset<SkillDef>("Assets/Goobo13/Skills/CloneWalk.asset").RegisterSkillDef();
            CorrosiveDogpile = assetBundle.LoadAsset<GooboThrowGooboMinionsSkillDef>("Assets/Goobo13/Skills/CorrosiveDogpile.asset").RegisterSkillDef();
            GooboJuxtaposePassive = assetBundle.LoadAsset<ItemDef>("Assets/Goobo13/Items/GooboJuxtaposePassive.asset").RegisterItemDef();
            GooboCorrosion = assetBundle.LoadAsset<BuffDef>("Assets/Goobo13/Buffs/bdGooboCorrosion.asset").RegisterBuffDef();
            Passive = assetBundle.LoadAsset<SkillFamily>("Assets/Goobo13/SkillFamilies/Goobo13Passive.asset").RegisterSkillFamily();
            Primary = assetBundle.LoadAsset<SkillFamily>("Assets/Goobo13/SkillFamilies/Goobo13Primary.asset").RegisterSkillFamily();
            Secondary = assetBundle.LoadAsset<SkillFamily>("Assets/Goobo13/SkillFamilies/Goobo13Secondary.asset").RegisterSkillFamily();
            Utility = assetBundle.LoadAsset<SkillFamily>("Assets/Goobo13/SkillFamilies/Goobo13Utility.asset").RegisterSkillFamily();
            Special = assetBundle.LoadAsset<SkillFamily>("Assets/Goobo13/SkillFamilies/Goobo13Special.asset").RegisterSkillFamily();
            GooboCorrosionDot = Utils.CreateDOT(GooboCorrosion, out GooboCorrosionDotIndex, true, 1f, 1f, DamageColorIndex.DeathMark, null);
            GooboDeployableSlot = DeployableAPI.RegisterDeployableSlot(GetGobooDeployableSlot);
            ContentManager.collectContentPackProviders += (addContentPackProvider) => addContentPackProvider(new Content());
        }
        public static int GetGobooDeployableSlot(CharacterMaster self, int deployableSlotMultiplier) => 30;
    }
}
