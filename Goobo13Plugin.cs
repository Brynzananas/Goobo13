using BepInEx;
using BepInEx.Configuration;
using R2API.Utils;
using System.Security;
using System.Security.Permissions;

[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[assembly: HG.Reflection.SearchableAttribute.OptIn]
[assembly: HG.Reflection.SearchableAttribute.OptInAttribute]
[module: UnverifiableCode]
#pragma warning disable CS0618
#pragma warning restore CS0618
namespace Goobo13
{
    [BepInPlugin(ModGuid, ModName, ModVer)]
    [BepInDependency(R2API.R2API.PluginGUID, R2API.R2API.PluginVersion)]
    [BepInDependency(R2API.DamageAPI.PluginGUID, R2API.DamageAPI.PluginVersion)]
    [BepInDependency(R2API.DeployableAPI.PluginGUID, R2API.DeployableAPI.PluginVersion)]
    [BepInDependency(R2API.DotAPI.PluginGUID, R2API.DotAPI.PluginVersion)]
    [BepInDependency(R2API.OrbAPI.PluginGUID, R2API.OrbAPI.PluginVersion)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID, R2API.Networking.NetworkingAPI.PluginVersion)]
    [BepInDependency(ModCompatabilities.RiskOfOptionsCompatability.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [BepInDependency(ModCompatabilities.EmoteCompatability.GUID, BepInDependency.DependencyFlags.SoftDependency)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    [System.Serializable]
    public class Goobo13Plugin : BaseUnityPlugin
    {
        public const string ModGuid = "com.brynzananas.goobo13";
        public const string ModName = "Goobo13";
        public const string ModVer = "1.0.0";
        public static bool emotesEnabled { get; private set; }
        public static bool riskOfOptionsEnabled { get; private set; }
        public static BepInEx.PluginInfo PInfo { get; private set; }
        public static ConfigFile configFile { get; private set; }
        public void Awake()
        {
            PInfo = Info;
            configFile = Config;
            emotesEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatabilities.EmoteCompatability.GUID);
            riskOfOptionsEnabled = BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey(ModCompatabilities.RiskOfOptionsCompatability.GUID);
            Assets.Init();
            SummonGoobosConfig.Init();
            TrackerConfig.Init();
            PunchConfig.Init();
            SuperPunchConfig.Init();
            ThrowGrenadeConfig.Init();
            DecoyConfig.Init();
            FireMinionsConfig.Init();
            GooboMissileConfig.Init();
            ConsumeMinionsConfig.Init();
            UnstableDecoyConfig.Init();
            LeapConfig.Init();
            Hooks.SetHooks();
        }
        public void OnDestroy()
        {
            Hooks.UnsetHooks();
        }
    }
}