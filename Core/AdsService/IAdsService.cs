namespace ThirdPartyService.Core.AdsService
{
    using System;

    public interface IAdsService
    {

        #region Banner

        void ShowBannerAd(BannerAdsPosition bannerAdsPosition = BannerAdsPosition.Bottom, int width = 320, int height = 50);
        void HideBannedAd();
        void DestroyBannerAd();

        #endregion

        #region Interstitial

        bool IsInterstitialAdReady(string place);
        void ShowInterstitialAd(string    place);

        #endregion

        #region Reward

        bool IsRewardedAdReady(string place);

        // void ShowRewardedAd(string    place);
        void ShowRewardedAd(string place, Action onCompleted, Action onFailed = null);

        #endregion

        #region RewardedInterstitialAd

        // bool IsRewardedInterstitialAdReady();
        // void ShowRewardedInterstitialAd(string place);
        // void ShowRewardedInterstitialAd(string place, Action onCompleted);

        #endregion

        #region RemoveAds

        void RemoveAds(bool revokeConsent = false);

        #endregion

        bool IsAdsInitialized();
        bool IsRemoveAds();
    }
}