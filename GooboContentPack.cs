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
    public class GooboContentPack : IContentPackProvider
    {
        internal ContentPack contentPack = new ContentPack();
        public string identifier => Goobo13Plugin.ModGuid + ".ContentProvider";
        public static List<GameObject> bodies = [];
        public static List<BuffDef> buffs = [];
        public static List<SkillDef> skills = [];
        public static List<SkillFamily> skillFamilies = [];
        public static List<GameObject> projectiles = [];
        public static List<GameObject> networkPrefabs = [];
        public static List<SurvivorDef> survivors = [];
        public static List<Type> states = [];
        public static List<NetworkSoundEventDef> sounds = [];
        public static List<UnlockableDef> unlockableDefs = [];
        public static List<GameObject> masters = [];
        public static List<EffectDef> effects = [];
        public static List<ItemDef> items = [];
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
