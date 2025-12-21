namespace ThirdPartyService.ServiceImplementation.AdsService.DummyAds.RewardedAds
{
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using UnityEngine;
    using UnityEngine.Events;

    public class DummyRewardedAds : IRewardedAdsService
    {
        public int GetPriority() => 1;
        public void Initialize()
        {
            // Do nothing
        }

        public void ShowAd(UnityAction<bool> onAdComplete, string where)
        {
            onAdComplete?.Invoke(true);
            Debug.LogWarning("DummyRewardedAds: ShowAd called, but this is a dummy implementation.");
        }

        public bool IsAdReady() => true;
    }
}