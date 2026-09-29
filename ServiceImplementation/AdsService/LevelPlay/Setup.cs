namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay
{
    #if LevelPlay
    using System;
    using Cysharp.Threading.Tasks;
    using ThirdPartyService.Core.Analytics;
    using ThirdPartyService.Core.ConsentService;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.MRECAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.RewardedAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;
    using ThirdPartyService.ServiceImplementation.ConsentService;
    using VContainer.Unity;

    // Asks every LevelPlay wrapper to start loading, then starts the SDK. Runs at Start, after the
    // blueprint services' Initialize has loaded LevelPlaySettings.
    //
    // The Editor and development builds always start the SDK; a release build starts it only when
    // LevelPlaySettings.startSdkInReleaseBuilds is on. When the SDK is not started the wrappers stay
    // registered but never become ready, and every show fails cleanly through the interface's own
    // failure callback.
    public class Setup : IStartable, IDisposable
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
        private readonly IConsentService            consent;
        private readonly ICcpaConsentReader         ccpa;
        private readonly IAdsSdk                    sdk;
        private readonly IAdRevenueService          analytics;

        public Setup(
            LevelPlaySdkSession        session,
            ILevelPlaySettingsProvider settings,
            LevelPlayBannerAds         bannerAds,
            LevelPlayMRECAds           mrecAds,
            LevelPlayInterstitialAds   interstitialAds,
            LevelPlayRewardedAds       rewardedAds,
            IConsentService            consent,
            ICcpaConsentReader         ccpa,
            IAdsSdk                    sdk,
            IAdRevenueService          analytics
        )
        {
            this.session         = session;
            this.settings        = settings;
            this.bannerAds       = bannerAds;
            this.mrecAds         = mrecAds;
            this.interstitialAds = interstitialAds;
            this.rewardedAds     = rewardedAds;
            this.consent         = consent;
            this.ccpa            = ccpa;
            this.sdk             = sdk;
            this.analytics       = analytics;
        }

        public void Start()
        {
            this.sdk.ImpressionDataReady += this.OnImpression;
            if (this.consent is IConsentUpdates updates) updates.PrivacyOptionsCompleted += this.OnPrivacyOptionsCompleted;
            this.bannerAds.Initialize();
            this.mrecAds.Initialize();
            this.interstitialAds.Initialize();
            this.rewardedAds.Initialize();
            if (ShouldStartSdk(IsEditorOrDevelopmentBuild, this.settings.StartSdkInReleaseBuilds))
                this.StartAfterConsentAsync().Forget();
        }

        private async UniTaskVoid StartAfterConsentAsync()
        {
            await this.consent.GatherConsentAsync();
            this.sdk.SetCOPPA(false);
            var optedOut = this.ccpa.ReadOptOut();
            if (optedOut.HasValue) this.sdk.SetCCPA(optedOut.Value);
            this.session.Start();
        }

        private void OnImpression(AdImpression impression) => this.analytics.SendAdImpression(impression);

        private void OnPrivacyOptionsCompleted()
        {
            var optedOut = this.ccpa.ReadOptOut();
            if (optedOut.HasValue) this.sdk.SetCCPA(optedOut.Value);
        }

        public void Dispose()
        {
            this.sdk.ImpressionDataReady -= this.OnImpression;
            if (this.consent is IConsentUpdates updates) updates.PrivacyOptionsCompleted -= this.OnPrivacyOptionsCompleted;
        }
    }
    #endif
}
