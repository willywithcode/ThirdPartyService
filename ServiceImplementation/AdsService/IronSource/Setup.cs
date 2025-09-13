#if IronSource
namespace ThirdParty.ServiceImplementation.AdsService.IronSource
{
    using GameFoundation.Scripts.Addressable;
    using ThirdPartyService.ServiceImplementation.AdsService.IronSource.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.IronSource.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.IronSource.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.IronSource.MRECAds;
    using ThirdPartyService.ServiceImplementation.AdsService.IronSource.RewardedAds;
    using Unity.Services.LevelPlay;
    using VContainer.Unity;
    using LevelPlayConfiguration = com.unity3d.mediation.LevelPlayConfiguration;
    using LevelPlayInitError = com.unity3d.mediation.LevelPlayInitError;

    public class Setup : IInitializable
    {
        private readonly IronSourceBannerAds        bannerAds;
        private readonly IronSourceInterstitialsAds interstitialsAds;
        private readonly IronSourceMRECAds          mrecAds;
        private readonly IronSourceRewardedAds      rewardedAds;
        private readonly string                     appKey;

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
#endif