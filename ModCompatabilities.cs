using BepInEx.Configuration;
using EmotesAPI;
using RiskOfOptions;
using RiskOfOptions.Options;
using RoR2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Goobo13
{
    public static class ModCompatabilities
    {
        public static class EmoteCompatability
        {
            public const string GUID = "com.weliveinasociety.CustomEmotesAPI";
            public static void Init()
            {
                CustomEmotesAPI.ImportArmature(Assets.Goobo13Body, Assets.Goobo13Emotes);
                CustomEmotesAPI.ImportArmature(Assets.Goobo13CloneBody, Assets.Goobo13CloneEmotes);
                CustomEmotesAPI.animChanged += CustomEmotesAPI_animChanged;
            }
            private static void CustomEmotesAPI_animChanged(string newAnimation, BoneMapper mapper)
            {
                if (mapper.name == "Goobo13Emotes")
                {
                    CharacterModel characterModel = mapper.transform.parent.GetComponent<CharacterModel>();
                    if (characterModel == null) return;
                    CharacterBody characterBody = characterModel.body;
                    if (characterBody == null) return;
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
                        BoneMapper boneMapper = BoneMapper.characterBodiesToBoneMappers[minionBody];
                        if (boneMapper == null) continue;
                        CustomEmotesAPI.PlayAnimation(newAnimation, boneMapper);
                    }
                }
            }
        }
        public static class RiskOfOptionsCompatability
        {
            public const string GUID = "com.rune580.riskofoptions";
            public static void AddConfig<T>(ConfigEntry<T> config, T value)
            {
                if (value is float) ModSettingsManager.AddOption(new FloatFieldOption(config as ConfigEntry<float>));
                if (value is bool)ModSettingsManager.AddOption(new CheckBoxOption(config as ConfigEntry<bool>));
                if (value is int) ModSettingsManager.AddOption(new IntFieldOption(config as ConfigEntry<int>));
                if (value is string) ModSettingsManager.AddOption(new StringInputFieldOption(config as ConfigEntry<string>));
                if (value is Enum)
                {
                    if (value.GetType().GetCustomAttributes<FlagsAttribute>().Any()) return;
                    ModSettingsManager.AddOption(new ChoiceOption(config));
                }
            }
        }
    }
}
