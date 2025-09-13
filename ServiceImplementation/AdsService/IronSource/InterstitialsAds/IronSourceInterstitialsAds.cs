#if IronSource
namespace ThirdPartyService.ServiceImplementation.AdsService.IronSource.InterstitialsAds
{
    using com.unity3d.mediation;
    using GameFoundation.Scripts.Addressable;
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.IronSource.Blueprints;
    using UnityEngine.Events;

    public class IronSourceInterstitialsAds : IInterstitialAdsService
    {
        private readonly string adUnitId;

        public IronSourceInterstitialsAds(IAssetsManager assetsManager)
        {
            this.adUnitId = assetsManager.LoadAsset<IronSourceSetting>("IronSourceSetting").interstitialAdKey;
        }

        private LevelPlayInterstitialAd interstitialAd;
        private UnityAction             onAdClosed;
        private UnityAction             onAdFailedToShow;

        public void Initialize()
        {
            // Create interstitial ad object
            this.interstitialAd                   =  new(this.adUnitId);
            this.interstitialAd.OnAdLoaded        += this.InterstitialOnAdLoadedEvent;
            this.interstitialAd.OnAdLoadFailed    += this.InterstitialOnAdLoadFailedEvent;
            this.interstitialAd.OnAdDisplayed     += this.InterstitialOnAdDisplayedEvent;
            this.interstitialAd.OnAdDisplayFailed += this.InterstitialOnAdDisplayFailedEvent;
            this.interstitialAd.OnAdClicked       += this.InterstitialOnAdClickedEvent;
            this.interstitialAd.OnAdClosed        += this.InterstitialOnAdClosedEvent;
            this.interstitialAd.OnAdInfoChanged   += this.InterstitialOnAdInfoChangedEvent;
            this.LoadAd();
        }

        public bool IsInitialized() => this.interstitialAd != null;

        public void ShowInterstitial(string where, UnityAction onAdClosed = null, UnityAction onAdFailedToShow = null)
        {
            this.interstitialAd.ShowAd(where);
            this.onAdClosed       = onAdClosed;
            this.onAdFailedToShow = onAdFailedToShow;
        }

        public bool IsInterstitialReady() => this.interstitialAd != null && this.interstitialAd.IsAdReady();

        private void LoadAd()
        {
            this.interstitialAd.LoadAd();
        }

        // Implement the events
        private void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo) { }

        private void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
        {
            this.LoadAd();
        }

        private void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
        {
            // Ad displayed callback
        }

        private void InterstitialOnAdDisplayFailedEvent(LevelPlayAdDisplayInfoError infoError)
        {
            this.onAdFailedToShow?.Invoke();
            this.onAdFailedToShow = null;
            this.LoadAd();
        }

        private void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
        {
            // Ad clicked callback
        }

        private void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
        {
            this.onAdClosed?.Invoke();
            this.onAdClosed = null;
            this.LoadAd();
        }

        private void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
        {
            // Ad info changed callback
        }
    }
}
#endif