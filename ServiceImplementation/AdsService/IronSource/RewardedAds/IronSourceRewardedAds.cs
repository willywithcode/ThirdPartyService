#if IronSource
namespace ThirdPartyService.ServiceImplementation.AdsService.IronSource.RewardedAds
{
    using GameFoundation.Scripts.Addressable;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using ThirdPartyService.ServiceImplementation.AdsService.IronSource.Blueprints;
    using Unity.Services.LevelPlay;
    using UnityEngine.Events;

    public class IronSourceRewardedAds : IRewardedAdsService
    {
        private readonly string adUnitId;

        public IronSourceRewardedAds(IAssetsManager assetsManager)
        {
            this.adUnitId = assetsManager.LoadAsset<IronSourceSetting>("IronSourceRewardedAds").rewardedAdKey;
        }

        private LevelPlayRewardedAd rewardedAd;
        private UnityAction<bool>   onAdComplete;

        public void Initialize()
        {
            //Create RewardedAd instance
            this.rewardedAd = new(this.adUnitId);

            //Subscribe RewardedAd events
            this.rewardedAd.OnAdLoaded        += this.RewardedOnAdLoadedEvent;
            this.rewardedAd.OnAdLoadFailed    += this.RewardedOnAdLoadFailedEvent;
            this.rewardedAd.OnAdDisplayed     += this.RewardedOnAdDisplayedEvent;
            this.rewardedAd.OnAdDisplayFailed += this.RewardedOnAdDisplayFailedEvent;
            this.rewardedAd.OnAdClicked       += this.RewardedOnAdClickedEvent;
            this.rewardedAd.OnAdClosed        += this.RewardedOnAdClosedEvent;
            this.rewardedAd.OnAdRewarded      += this.RewardedOnAdRewarded;
            this.rewardedAd.OnAdInfoChanged   += this.RewardedOnAdInfoChangedEvent;
            this.Load();
        }

        public void ShowAd(UnityAction<bool> onAdComplete, string where)
        {
            this.rewardedAd.ShowAd(where);
            this.onAdComplete = onAdComplete;
        }

        public bool IsAdReady() => this.rewardedAd != null && this.rewardedAd.IsAdReady();

        private void Load()
        {
            this.rewardedAd.LoadAd();
        }

        //Implement RewardedAd events
        void RewardedOnAdLoadedEvent(LevelPlayAdInfo adInfo) { }

        void RewardedOnAdLoadFailedEvent(LevelPlayAdError ironSourceError)
        {
            this.Load();
        }

        void RewardedOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            this.onAdComplete?.Invoke(false);
            this.onAdComplete = null;
        }

        void RewardedOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            // Ad displayed callback
        }

        void RewardedOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError adInfoError)
        {
            this.onAdComplete?.Invoke(false);
            this.onAdComplete = null;
            this.Load();
        }

        void RewardedOnAdClosedEvent(LevelPlayAdInfo adInfo)
        {
            this.onAdComplete?.Invoke(false);
            this.onAdComplete = null;
            this.Load();
        }

        void RewardedOnAdRewarded(LevelPlayAdInfo adInfo, LevelPlayReward adReward)
        {
            this.onAdComplete?.Invoke(true);
            this.onAdComplete = null;
            this.Load();
        }

        void RewardedOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
        {
            // Ad info changed callback
        }
    }
}
#endif