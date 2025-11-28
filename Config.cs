using BepInEx.Configuration;
using RoR2;
using RoR2.Projectile;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements.UIR;
using static Goobo13.Utils;

namespace Goobo13
{
    public static class SummonGoobosConfig
    {
        public const string name = SummonGoobosName;
        public static void Init()
        {
            lifetime = CreateConfig(name, "Clone Lifetime", 10, "");
            maxAmount = CreateConfig(name, "Clone max Amount", 30, "");
            statSharing = CreateConfig(name, "Share Stats Percentage", 50f, "");
            maxAmount.SettingChanged += MaxAmount_SettingChanged;
            MaxAmount_SettingChanged(maxAmount, null);
        }
        private static void MaxAmount_SettingChanged(object sender, EventArgs e) => Assets.CorrosiveDogpile.baseMaxStock = maxAmount.Value;
        public static ConfigEntry<int> lifetime;
        public static ConfigEntry<int> maxAmount;
        public static ConfigEntry<float> statSharing;
    }
    public static class TrackerConfig
    {
        public const string name = "Tracker";
        public static void Init()
        {
            maxDistance = CreateConfig(name, DistanceName, 48f, "");
            maxAngle = CreateConfig(name, "Angle", 90f, "");
            sortMode = CreateConfig(name, "Sort Mode", BullseyeSearch.SortMode.Angle, "");
            sortByPriority = CreateConfig(name, "Sort by Priority", true, "Sort by: Boss -> Elite -> Enemy");
        }
        public static ConfigEntry<float> maxDistance;
        public static ConfigEntry<float> maxAngle;
        public static ConfigEntry<BullseyeSearch.SortMode> sortMode;
        public static ConfigEntry<bool> sortByPriority;
    }
    public static class PunchConfig
    {
        public const string name = PunchName;
        public static void Init()
        {
            damageCoefficient = CreateConfig(name, DamageCoefficientName, 3f, "");
            procCoefficient = CreateConfig(name, ProcCoefficientName, 1f, "");
            duration = CreateConfig(name, DurationName, 0.5f, "");
            timeToAttack = CreateConfig(name, TimeToAttackName, 0.25f, "");
            selfPush = CreateConfig(name, SelfPushName, 3f, "");
            damageType = CreateConfig(name, DamageTypeName, DamageType.Generic, "");
            damageTypeExtended = CreateConfig(name, DamageTypeExtendedName, DamageTypeExtended.Generic, "");
        }
        public static ConfigEntry<float> damageCoefficient;
        public static ConfigEntry<float> procCoefficient;
        public static ConfigEntry<float> duration;
        public static ConfigEntry<float> timeToAttack;
        public static ConfigEntry<float> selfPush;
        public static ConfigEntry<DamageType> damageType;
        public static ConfigEntry<DamageTypeExtended> damageTypeExtended;
    }
    public static class SuperPunchConfig
    {
        public const string name = SuperPunchName;
        public static void Init()
        {
            damageCoefficient = CreateConfig(name, DamageCoefficientName, 3f, "");
            procCoefficient = CreateConfig(name, ProcCoefficientName, 1f, "");
            radius = CreateConfig(name, RadiusName, 6f, "");
            force = CreateConfig(name, ForceName, 300f, "");
            duration = CreateConfig(name, DurationName, 1f, "");
            timeToAttack = CreateConfig(name, TimeToAttackName, 0.5f, "");
            falloffModel = CreateConfig(name, FalloffName, BlastAttack.FalloffModel.None, "");
            damageType = CreateConfig(name, DamageTypeName, DamageType.Generic, "");
            damageTypeExtended = CreateConfig(name, DamageTypeExtendedName, DamageTypeExtended.Generic, "");
        }
        public static ConfigEntry<float> damageCoefficient;
        public static ConfigEntry<float> procCoefficient;
        public static ConfigEntry<float> radius;
        public static ConfigEntry<float> force;
        public static ConfigEntry<float> duration;
        public static ConfigEntry<float> timeToAttack;
        public static ConfigEntry<BlastAttack.FalloffModel> falloffModel;
        public static ConfigEntry<DamageType> damageType;
        public static ConfigEntry<DamageTypeExtended> damageTypeExtended;
    }
    public static class ThrowGrenadeConfig
    {
        public const string name = ThrowGrenadeName;
        public static void Init()
        {
            damageCoefficient = CreateConfig(name, DamageCoefficientName, 3f, "");
            force = CreateConfig(name, ForceName, 300f, "");
            duration = CreateConfig(name, DurationName, 0.5f, "");
            timeToAttack = CreateConfig(name, TimeToAttackName, 0.25f, "");
            damageType = CreateConfig(name, DamageTypeName, DamageType.Generic, "");
            damageTypeExtended = CreateConfig(name, DamageTypeExtendedName, DamageTypeExtended.Generic, "");
        }
        public static ConfigEntry<float> damageCoefficient;
        public static ConfigEntry<float> force;
        public static ConfigEntry<float> duration;
        public static ConfigEntry<float> timeToAttack;
        public static ConfigEntry<DamageType> damageType;
        public static ConfigEntry<DamageTypeExtended> damageTypeExtended;
    }
    public static class DecoyConfig
    {
        public const string name = DecoyName;
        public static void Init()
        {
            duration = CreateConfig(name, "Cloak " + DurationName, 3f, "");
            lifetime = CreateConfig(name, LifetimeName, 30f, "");
        }
        public static ConfigEntry<float> duration;
        public static ConfigEntry<float> lifetime;
    }
    public static class FireMinionsConfig
    {
        public const string name = FireMinionsName;
        public static void Init()
        {
            damageCoefficient = CreateConfig(name, DamageCoefficientName, 3f, "");
            procCoefficient = CreateConfig(name, ProcCoefficientName, 1f, "");
            force = CreateConfig(name, ForceName, 100f, "");
            radius = CreateConfig(name, RadiusName, 3f, "");
            timeToTarget = CreateConfig(name, TimeToTargetName, 0.5f, "");
            falloffModel = CreateConfig(name, FalloffName, BlastAttack.FalloffModel.None, "");
            damageType = CreateConfig(name, DamageTypeName, DamageType.Generic, "");
            damageTypeExtended = CreateConfig(name, DamageTypeExtendedName, DamageTypeExtended.Generic, "");
        }

        public static ConfigEntry<float> damageCoefficient;
        public static ConfigEntry<float> procCoefficient;
        public static ConfigEntry<float> force;
        public static ConfigEntry<float> radius;
        public static ConfigEntry<float> timeToTarget;
        public static ConfigEntry<BlastAttack.FalloffModel> falloffModel;
        public static ConfigEntry<DamageType> damageType;
        public static ConfigEntry<DamageTypeExtended> damageTypeExtended;
    }
    public static class GooboMissileConfig
    {
        public const string name = GooboMissileName;
        public static void Init()
        {
            damageCoefficient = CreateConfig(name, DamageCoefficientName, 3f, "");
            procCoefficient = CreateConfig(name, ProcCoefficientName, 1f, "");
            force = CreateConfig(name, ForceName, 100f, "");
            distance = CreateConfig(name, DistanceName, 24f, "");
            radius = CreateConfig(name, RadiusName, 3f, "");
            duration = CreateConfig(name, DurationName, 0.5f, "");
            timeToAttack = CreateConfig(name, TimeToAttackName, 0.25f, "");
            timeToTarget = CreateConfig(name, TimeToTargetName, 0.5f, "");
            gooboAmount = CreateConfig(name, "Goobo Amount", 1, "");
            falloffModel = CreateConfig(name, FalloffName, BlastAttack.FalloffModel.None, "");
            damageType = CreateConfig(name, DamageTypeName, DamageType.Generic, "");
            damageTypeExtended = CreateConfig(name, DamageTypeExtendedName, DamageTypeExtended.Generic, "");
        }

        public static ConfigEntry<float> damageCoefficient;
        public static ConfigEntry<float> procCoefficient;
        public static ConfigEntry<float> force;
        public static ConfigEntry<float> distance;
        public static ConfigEntry<float> radius;
        public static ConfigEntry<float> timeToAttack;
        public static ConfigEntry<float> timeToTarget;
        public static ConfigEntry<float> duration;
        public static ConfigEntry<int> gooboAmount;
        public static ConfigEntry<BlastAttack.FalloffModel> falloffModel;
        public static ConfigEntry<DamageType> damageType;
        public static ConfigEntry<DamageTypeExtended> damageTypeExtended;
    }
    public static class ConsumeMinionsConfig
    {
        public const string name = ConsumeMinionsName;
        public static void Init()
        {
            healPercentage = CreateConfig(name, "Health Percentage to Heal", 5f, "");
            damageCoefficient = CreateConfig(name, DamageCoefficientName, 5f, "");
            procCoefficient = CreateConfig(name, ProcCoefficientName, 1f, "");
            radius = CreateConfig(name, RadiusName, 9f, "");
            force = CreateConfig(name, ForceName, 300f, "");
            duration = CreateConfig(name, DurationName, 1f, "");
            timeToTarget = CreateConfig(name, TimeToTargetName, 0.5f, "");
            falloffModel = CreateConfig(name, FalloffName, BlastAttack.FalloffModel.None, "");
            damageType = CreateConfig(name, DamageTypeName, DamageType.Generic, "");
            damageTypeExtended = CreateConfig(name, DamageTypeExtendedName, DamageTypeExtended.Generic, "");
        }
        public static ConfigEntry<float> healPercentage;
        public static ConfigEntry<float> damageCoefficient;
        public static ConfigEntry<float> procCoefficient;
        public static ConfigEntry<float> radius;
        public static ConfigEntry<float> force;
        public static ConfigEntry<float> duration;
        public static ConfigEntry<float> timeToTarget;
        public static ConfigEntry<BlastAttack.FalloffModel> falloffModel;
        public static ConfigEntry<DamageType> damageType;
        public static ConfigEntry<DamageTypeExtended> damageTypeExtended;
    }
    public static class UnstableDecoyConfig
    {
        public const string name = UnstableDecoyName;
        public static void Init()
        {
            healthPercentage = CreateConfig(name, "Health Percentage to Take", 15f, "");
            healthPercentageRandomSpread = CreateConfig(name, "Health Percentage to Take Random Spread", 10f, "");
            gooboAmount = CreateConfig(name, GooboAmountName, 2, "");
            duration = CreateConfig(name, "Immunity " + DurationName, 2f, "");
        }
        public static ConfigEntry<float> healthPercentage;
        public static ConfigEntry<float> healthPercentageRandomSpread;
        public static ConfigEntry<int> gooboAmount;
        public static ConfigEntry<float> duration;
    }
    public static class LeapConfig
    {
        public const string name = LeapName;
        public static void Init()
        {
            damageCoefficient = CreateConfig(name, DamageCoefficientName, 3f, "");
            procCoefficient = CreateConfig(name, ProcCoefficientName, 1f, "");
            radius = CreateConfig(name, RadiusName, 6f, "");
            force = CreateConfig(name, ForceName, 300f, "");
            minimumYVelocity = CreateConfig(name, "Minimum Y Velocity", 24f, "");
            velocityPerLeap = CreateConfig(name, "Speed Multiplier", 3f, "");
            duration = CreateConfig(name, "Cloak " + DurationName, 3f, "");
            airControl = CreateConfig(name, "Air Control", 5f, "");
            falloffModel = CreateConfig(name, FalloffName, BlastAttack.FalloffModel.None, "");
            damageType = CreateConfig(name, DamageTypeName, DamageType.Generic, "");
            damageTypeExtended = CreateConfig(name, DamageTypeExtendedName, DamageTypeExtended.Generic, "");
        }
        public static ConfigEntry<float> damageCoefficient;
        public static ConfigEntry<float> procCoefficient;
        public static ConfigEntry<float> radius;
        public static ConfigEntry<float> force;
        public static ConfigEntry<float> minimumYVelocity;
        public static ConfigEntry<float> velocityPerLeap;
        public static ConfigEntry<float> duration;
        public static ConfigEntry<float> airControl;
        public static ConfigEntry<BlastAttack.FalloffModel> falloffModel;
        public static ConfigEntry<DamageType> damageType;
        public static ConfigEntry<DamageTypeExtended> damageTypeExtended;
    }
}
