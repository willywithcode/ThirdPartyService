namespace ThirdParty.ServiceImplementation.AdsService.IronSource
{
    using GameFoundation.Scripts.Addressable;
    using ThirdParty.ServiceImplementation.AdsService.IronSource.Banner;
    using ThirdParty.ServiceImplementation.AdsService.IronSource.Blueprints;
    using ThirdParty.ServiceImplementation.AdsService.IronSource.InterstitialsAds;
    using ThirdParty.ServiceImplementation.AdsService.IronSource.MRECAds;
    using ThirdParty.ServiceImplementation.AdsService.IronSource.RewardedAds;
    using Unity.Services.LevelPlay;
    using LevelPlayConfiguration = com.unity3d.mediation.LevelPlayConfiguration;
    using LevelPlayInitError = com.unity3d.mediation.LevelPlayInitError;
    using VContainer.Unity;

    public class Setup : IInitializable
    {
        private readonly IronSourceBannerAds        bannerAds;
        private readonly IronSourceInterstitialsAds interstitialsAds;
        private readonly IronSourceMRECAds          mrecAds;
        private readonly IronSourceRewardedAds      rewardedAds;
        private          string                     appKey;

        public Setup(
            IAssetsManager             assetsManager,
            IronSourceBannerAds        bannerAds,
            IronSourceInterstitialsAds interstitialsAds,
            IronSourceMRECAds          mrecAds,
            IronSourceRewardedAds      rewardedAds
        )
        {
            this.bannerAds        = bannerAds;
            this.interstitialsAds = interstitialsAds;
            this.mrecAds          = mrecAds;
            this.rewardedAds      = rewardedAds;
            this.appKey           = assetsManager.LoadAsset<IronSourceSetting>("IronSourceSetting").appKey;
        }

        public void Initialize()
        {
            LevelPlay.OnInitSuccess += this.SdkInitializationCompletedEvent;
            LevelPlay.OnInitFailed  += this.SdkInitializationFailedEvent;
            // SDK init
            LevelPlay.Init(this.appKey);
        }

        private void SdkInitializationCompletedEvent(LevelPlayConfiguration obj)
        {
            this.bannerAds.Initialize();
            this.interstitialsAds.Initialize();
            this.mrecAds.Initialize();
            this.rewardedAds.Initialize();
        }

        private void SdkInitializationFailedEvent(LevelPlayInitError obj)
        {
        }
    }
}