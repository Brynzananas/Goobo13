using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements.UIR;
using static Goobo13.Utils;

namespace Goobo13
{
    public static class Config
    {
        public static void Init()
        {
            SummonGoobosConfig.Init();
            PunchConfig.Init();
            SuperPunchConfig.Init();
            ThrowGrenadeConfig.Init();
            DecoyConfig.Init();
            FireMinionsConfig.Init();
        }
    }
    public static class SummonGoobosConfig
    {
        public static void Init()
        {
            lifetime = CreateConfig(SummonGoobosName, "Clone Lifetime", 10, "");
            maxAmount = CreateConfig(SummonGoobosName, "Clone max Amount", 30, "");

        }
        public static ConfigEntry<int> lifetime;
        public static ConfigEntry<int> maxAmount;
    }
    public static class PunchConfig
    {
        public static void Init()
        {
            damageCoefficient = CreateConfig(PunchName, DamageCoefficientName, 3f, "");
            procCoefficient = CreateConfig(PunchName, ProcCoefficientName, 1f, "");
            duration = CreateConfig(PunchName, DurationName, 0.5f, "");
            timeToAttack = CreateConfig(PunchName, TimeToAttackName, 0.25f, "");
            selfPush = CreateConfig(PunchName, SelfPushName, 24f, "");
        }
        public static ConfigEntry<float> damageCoefficient;
        public static ConfigEntry<float> procCoefficient;
        public static ConfigEntry<float> duration;
        public static ConfigEntry<float> timeToAttack;
        public static ConfigEntry<float> selfPush;
    }
    public static class SuperPunchConfig
    {
        public static void Init()
        {
            damageCoefficient = CreateConfig(SuperPunchName, DamageCoefficientName, 3f, "");
            procCoefficient = CreateConfig(SuperPunchName, ProcCoefficientName, 1f, "");
            radius = CreateConfig(SuperPunchName, RadiusName, 6f, "");
            force = CreateConfig(SuperPunchName, ForceName, 300f, "");
            duration = CreateConfig(SuperPunchName, DurationName, 0.5f, "");
            timeToAttack = CreateConfig(SuperPunchName, TimeToAttackName, 0.25f, "");
        }
        public static ConfigEntry<float> damageCoefficient;
        public static ConfigEntry<float> procCoefficient;
        public static ConfigEntry<float> radius;
        public static ConfigEntry<float> force;
        public static ConfigEntry<float> duration;
        public static ConfigEntry<float> timeToAttack;
    }
    public static class ThrowGrenadeConfig
    {
        public static void Init()
        {
            damageCoefficient = CreateConfig(ThrowGrenadeName, DamageCoefficientName, 3f, "");
            force = CreateConfig(ThrowGrenadeName, ForceName, 300f, "");
            duration = CreateConfig(ThrowGrenadeName, DurationName, 0.5f, "");
            timeToAttack = CreateConfig(ThrowGrenadeName, TimeToAttackName, 0.25f, "");
        }
        public static ConfigEntry<float> damageCoefficient;
        public static ConfigEntry<float> force;
        public static ConfigEntry<float> duration;
        public static ConfigEntry<float> timeToAttack;
    }
    public static class DecoyConfig
    {
        public static void Init()
        {
            duration = CreateConfig(DecoyName, "Cloak " + DurationName, 3f, "");
        }
        public static ConfigEntry<float> duration;
    }
    public static class FireMinionsConfig
    {
        public static void Init()
        {
            damageCoefficient = CreateConfig(FireMinionsName, DamageCoefficientName, 3f, "");
            force = CreateConfig(FireMinionsName, ForceName, 100f, "");
            timeToTarget = CreateConfig(FireMinionsName, "Time to Target", 0.25f, "");
        }
        public static ConfigEntry<float> damageCoefficient;
        public static ConfigEntry<float> force;
        public static ConfigEntry<float> timeToTarget;
    }
}
