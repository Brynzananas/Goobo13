using BepInEx.Configuration;
using EmotesAPI;
using RiskOfOptions;
using RiskOfOptions.Options;
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
            public static void AddConfig<T1, T2>(T1 config, T2 value) where T1 : ConfigEntryBase
            {
                if (value is float)
                {
                    ModSettingsManager.AddOption(new FloatFieldOption(config as ConfigEntry<float>));
                }
                if (value is bool)
                {
                    ModSettingsManager.AddOption(new CheckBoxOption(config as ConfigEntry<bool>));
                }
                if (value is int)
                {
                    ModSettingsManager.AddOption(new IntFieldOption(config as ConfigEntry<int>));
                }
                if (value is string)
                {
                    ModSettingsManager.AddOption(new StringInputFieldOption(config as ConfigEntry<string>));
                }
                if (value is Enum)
                {
                    Enum @enum = value as Enum;
                    if (@enum.GetType().GetCustomAttributes<FlagsAttribute>().Any()) return;
                    ModSettingsManager.AddOption(new ChoiceOption(config as ConfigEntry<T2>));
                }
            }
        }
    }
}
