namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay
{
    #if LevelPlay
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.MRECAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.RewardedAds;
    using VContainer.Unity;

    // Asks every LevelPlay wrapper to start loading, then starts the SDK. Runs at Start, after the
    // blueprint services' Initialize has loaded LevelPlaySettings.
    //
    // The Editor and development builds always start the SDK; a release build starts it only when
    // LevelPlaySettings.startSdkInReleaseBuilds is on. When the SDK is not started the wrappers stay
    // registered but never become ready, and every show fails cleanly through the interface's own
    // failure callback.
    public class Setup : IStartable
    {
        public static bool IsEditorOrDevelopmentBuild =>
            #if DEVELOPMENT_BUILD || UNITY_EDITOR
            true;
            #else
            false;
            #endif

        public static bool ShouldStartSdk(bool editorOrDevelopmentBuild, bool startSdkInReleaseBuilds) =>
            editorOrDevelopmentBuild || startSdkInReleaseBuilds;

        private readonly LevelPlaySdkSession        session;
        private readonly ILevelPlaySettingsProvider settings;
        private readonly LevelPlayBannerAds         bannerAds;
        private readonly LevelPlayMRECAds           mrecAds;
        private readonly LevelPlayInterstitialAds   interstitialAds;
        private readonly LevelPlayRewardedAds       rewardedAds;

        public Setup(
            LevelPlaySdkSession        session,
            ILevelPlaySettingsProvider settings,
            LevelPlayBannerAds         bannerAds,
            LevelPlayMRECAds           mrecAds,
            LevelPlayInterstitialAds   interstitialAds,
            LevelPlayRewardedAds       rewardedAds
        )
        {
            this.session         = session;
            this.settings        = settings;
            this.bannerAds       = bannerAds;
            this.mrecAds         = mrecAds;
            this.interstitialAds = interstitialAds;
            this.rewardedAds     = rewardedAds;
        }

        public void Start()
        {
            this.bannerAds.Initialize();
            this.mrecAds.Initialize();
            this.interstitialAds.Initialize();
            this.rewardedAds.Initialize();
            if (ShouldStartSdk(IsEditorOrDevelopmentBuild, this.settings.StartSdkInReleaseBuilds)) this.session.Start();
        }
    }
    #endif
}
