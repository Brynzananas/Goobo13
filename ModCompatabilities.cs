using BepInEx.Configuration;
using EmotesAPI;
using RiskOfOptions;
using RiskOfOptions.Options;
using System;
using System.Collections.Generic;
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
                //CustomEmotesAPI.ImportArmature(Assets.Goobo13CloneBody, Assets.Goobo13Emotes);
                //CustomEmotesAPI.animChanged += CustomEmotesAPI_animChanged;
            }
            //private static void CustomEmotesAPI_animChanged(string newAnimation, BoneMapper mapper)
            //{
            //    if (mapper.name == "Goobo13Emotes")
            //    {
            //        DemolisherModel demolisherModel = mapper.transform.parent.GetComponent<DemolisherModel>();
            //        if (demolisherModel == null) return;
            //        demolisherModel.emoting = !(newAnimation == "none");
            //    }
            //}
        }
        public static class RiskOfOptionsCompatability
        {
            public const string GUID = "com.rune580.riskofoptions";
            public static void AddConfig<T>(T config) where T : ConfigEntryBase
            {
                if (config is ConfigEntry<float>)
                {
                    ModSettingsManager.AddOption(new FloatFieldOption(config as ConfigEntry<float>));
                }
                if (config is ConfigEntry<bool>)
                {
                    ModSettingsManager.AddOption(new CheckBoxOption(config as ConfigEntry<bool>));
                }
                if (config is ConfigEntry<int>)
                {
                    ModSettingsManager.AddOption(new IntFieldOption(config as ConfigEntry<int>));
                }
                if (config is ConfigEntry<string>)
                {
                    ModSettingsManager.AddOption(new StringInputFieldOption(config as ConfigEntry<string>));
                }
            }
        }
    }
}
