using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Goobo13
{
    public class Content : IContentPackProvider
    {
        internal ContentPack contentPack = new ContentPack();
        public string identifier => Main.ModGuid + ".ContentProvider";
        public static List<GameObject> bodies = new List<GameObject>();
        public static List<BuffDef> buffs = new List<BuffDef>();
        public static List<SkillDef> skills = new List<SkillDef>();
        public static List<SkillFamily> skillFamilies = new List<SkillFamily>();
        public static List<GameObject> projectiles = new List<GameObject>();
        public static List<GameObject> networkPrefabs = new List<GameObject>();
        public static List<SurvivorDef> survivors = new List<SurvivorDef>();
        public static List<Type> states = new List<Type>();
        public static List<NetworkSoundEventDef> sounds = new List<NetworkSoundEventDef>();
        public static List<UnlockableDef> unlockableDefs = new List<UnlockableDef>();
        public static List<GameObject> masters = new List<GameObject>();
        public static List<EffectDef> effects = new List<EffectDef>();
        public static List<ItemDef> items = new List<ItemDef>();
        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(this.contentPack, args.output);
            args.ReportProgress(1f);
            yield break;
        }

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            this.contentPack.identifier = this.identifier;
            contentPack.skillDefs.Add([.. skills]);
            contentPack.skillFamilies.Add([.. skillFamilies]);
            contentPack.bodyPrefabs.Add([.. bodies]);
            contentPack.buffDefs.Add([.. buffs]);
            contentPack.projectilePrefabs.Add([.. projectiles]);
            contentPack.survivorDefs.Add([.. survivors]);
            contentPack.entityStateTypes.Add([.. states]);
            contentPack.networkSoundEventDefs.Add([.. sounds]);
            contentPack.networkedObjectPrefabs.Add([.. networkPrefabs]);
            contentPack.unlockableDefs.Add([.. unlockableDefs]);
            contentPack.masterPrefabs.Add([.. masters]);
            contentPack.effectDefs.Add([.. effects]);
            contentPack.itemDefs.Add([.. items]);
            yield break;
        }
    }
}
