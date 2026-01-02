using RoR2;
using System;
using System.Collections.Generic;
using System.Text;

namespace Goobo13
{
    public static class Language
    {
        public static void Init()
        {
            if (Assets.Goobo13CharacterBody)
            {
                AddLanguageToken(Assets.Goobo13CharacterBody.baseNameToken, "Goobo13");
                AddLanguageToken(Assets.Goobo13CharacterBody.subtitleNameToken, "Sloppy Tuffer lololololol");
            }
            if (Assets.Goobo13CloneCharacterBody)
            {
                AddLanguageToken(Assets.Goobo13CloneCharacterBody.baseNameToken, "Goobo13 Clone");
                AddLanguageToken(Assets.Goobo13CloneCharacterBody.subtitleNameToken, "Sloppy Tuffer clone lololololol");
            }
            if (Assets.Goobo13)
            {
                AddLanguageToken(Assets.Goobo13.displayNameToken, "Goobo13");
                AddLanguageToken(Assets.Goobo13.mainEndingEscapeFailureFlavorToken, "... And so he vanished, Agent and Gummy no longer.");
                AddLanguageToken(Assets.Goobo13.outroFlavorToken, "... And so he left, two minds searching for one.");
            }
            AddLanguageToken("KEYWORD_CORROSIVE", $"{keywordPrefix}Corrosive{endPrefix}{subPrefix}Deal {damagePrefix}100%{endPrefix} base damage over 6 seconds. <i>Corrosion can stack.</i>{endPrefix}");
            AddLanguageToken("KEYWORD_JUXTAPOSE", $"{keywordPrefix}Juxtapose{endPrefix}{subPrefix}On hit spawn Goobo clone with {Hooks.GooboChanceSpawn}% chance. <i>Goobo clones have {Hooks.copyStatsPercentage}% stats of their owner.</i>{endPrefix}");
            AddLanguageToken(Assets.GooboPunch.skillNameToken, "Goobo Punch");
            AddLanguageToken(Assets.GooboPunch.skillDescriptionToken, $"{utilityPrefix}Juxtapose{endPrefix}. Swing at nearby enemies for {damagePrefix}{Punch.baseDamageCoefficient * 100f}% damage{endPrefix}. Third Every 3rd hit strikes in a greater area and {utilityPrefix}Juxtaposes{endPrefix}.");
            //AddLanguageToken(Assets.GooboGrenade.skillNameToken, "Goobo Grenade");
            //AddLanguageToken(Assets.GooboGrenade.skillDescriptionToken, $"Throw a random grenade for {damagePrefix}{ThrowGrenade.damageCoefficient * 100f} damage{endPrefix}.");
            AddLanguageToken(Assets.GooboMissile.skillNameToken, "Goobo Missile");
            AddLanguageToken(Assets.GooboMissile.skillDescriptionToken, $"{damagePrefix}Corrosive{endPrefix}. Fire a seeking missile for {damagePrefix}{GooboMissile.damageCoefficient * 100f} damage{endPrefix} and {damagePrefix}Corrode{endPrefix} enemy. {utilityPrefix}Juxtaposes {GooboMissile.baseGooboAmount} " + (GooboMissile.baseGooboAmount > 1 ? "clones" : "clone") + $"{endPrefix}.");
            AddLanguageToken(Assets.CloneWalk.skillNameToken, "Clone Walk");
            AddLanguageToken(Assets.CloneWalk.skillDescriptionToken, $"Become {utilityPrefix}invisible for {Decoy.cloakDuration} seconds{endPrefix} and {utilityPrefix}juxtapose 1 clone{endPrefix}");
            //AddLanguageToken(Assets.UnstableCloneWalk.skillNameToken, "Unstable Clone Walk");
            //AddLanguageToken(Assets.UnstableCloneWalk.skillDescriptionToken, $"Become {utilityPrefix}invulnerable for {UnstableDecoy.baseDuration} seconds{endPrefix} and {utilityPrefix}juxtapose {UnstableDecoy.gooboAmount} " + (UnstableDecoy.gooboAmount > 1 ? "clones" : "clone") + $"{endPrefix}.");
            AddLanguageToken(Assets.CorrosiveDogpile.skillNameToken, "Corrosive Dogpile");
            AddLanguageToken(Assets.CorrosiveDogpile.skillDescriptionToken, $"Fire all your clones to targeted enemy for {damagePrefix}{FireMinions.damageCoefficient * 100f}% damage{endPrefix}");
            AddLanguageToken(Assets.GooboConsumption.skillNameToken, "Corrosive Consumption");
            AddLanguageToken(Assets.GooboConsumption.skillDescriptionToken, $"{damagePrefix}Corrosive{endPrefix}. Consume all your clones. Your next primary attack will slam in a greater area for {Slam.baseDamageCoefficient * 100f}% damage and {damagePrefix}Corrode{endPrefix} hit enemies for the amount of clones consumed");
        }
        public static void AddLanguageToken(string token, string text) => AddLanguageToken(token, text, "en");
        public static void AddLanguageToken(string token, string text, string lang)
        {
            RoR2.Language language = RoR2.Language.languagesByName[lang];
            if (language == null) return;
            if (language.stringsByToken.ContainsKey(token))
            {
                language.stringsByToken[token] = text;
            }
            else
            {
                language.stringsByToken.Add(token, text);
            }
        }
        public const string damagePrefix = "<style=cIsDamage>";
        public const string keywordPrefix = "<style=cKeywordName>";
        public const string subPrefix = "<style=cSub>";
        public const string stackPrefix = "<style=cStack>";
        public const string utilityPrefix = "<style=cIsUtility>";
        public const string healingPrefix = "<style=cIsHealing>";
        public const string endPrefix = "</style>";
    }
}
