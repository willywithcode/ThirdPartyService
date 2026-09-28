namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints
{
    #if LevelPlay
    using System;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Blueprints.ScriptableObject.Attributes;
    using GameFoundation.Scripts.Blueprints.ScriptableObject.Services;
    using UnityEngine;

    public interface ILevelPlaySettingsProvider
    {
        // The keys for the platform this build targets; null when the settings asset is missing.
        LevelPlayPlatformSettings Current { get; }

        // Whether a release build starts the SDK. The Editor and development builds always start it.
        bool StartSdkInReleaseBuilds { get; }
    }

    [SOBlueprintAttributes(nameof(LevelPlaySettings))]
    public class LevelPlaySettingsBlueprintService : BaseSOBlueprintService<LevelPlaySettings>, ILevelPlaySettingsProvider
    {
        public LevelPlaySettingsBlueprintService(IAssetsManager assetsManager) : base(assetsManager) { }

        public LevelPlayPlatformSettings Current => this.GetBlueprint()?.ForCurrentPlatform();

        // Without the asset there is no app key, so the SDK cannot start either way.
        public bool StartSdkInReleaseBuilds => this.GetBlueprint()?.startSdkInReleaseBuilds ?? LevelPlaySettings.DefaultStartSdkInReleaseBuilds;
    }

    [Serializable]
    public class LevelPlayPlatformSettings
    {
        public string appKey;
        public string bannerAdUnitId;
        public string interstitialAdUnitId;
        public string rewardedAdUnitId;

        [Tooltip("Leave empty to request the MREC size on the banner ad unit.")]
        public string mrecAdUnitId;

        public string MrecAdUnitId => string.IsNullOrEmpty(this.mrecAdUnitId) ? this.bannerAdUnitId : this.mrecAdUnitId;
    }

    // The game's LevelPlay dashboard values per platform, loaded from the Addressable asset at the
    // address "LevelPlaySettings". LevelPlay's official demo keys are in the
    // com.unity.services.levelplay 9.5.1 sample (Samples~/UnityLevelPlaySample/Scripts/AdConfig.cs).
    [CreateAssetMenu(fileName = "LevelPlaySettings", menuName = "ThirdParty/ServiceImplementation/AdsService/LevelPlay/LevelPlaySettings")]
    public class LevelPlaySettings : ScriptableObject
    {
        // On, so a game that adopts LevelPlay shows ads in release without further setup.
        public const bool DefaultStartSdkInReleaseBuilds = true;

        public LevelPlayPlatformSettings android = new();
        public LevelPlayPlatformSettings ios     = new();

        [Tooltip("Release builds start the LevelPlay SDK only when this is on. The Editor and development builds always start it.")]
        public bool startSdkInReleaseBuilds = DefaultStartSdkInReleaseBuilds;

        public LevelPlayPlatformSettings ForCurrentPlatform()
        {
            #if UNITY_IOS
            return this.ios;
            #else
            return this.android;
            #endif
        }
    }
    #endif
}
