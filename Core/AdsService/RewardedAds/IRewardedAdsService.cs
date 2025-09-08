namespace ThirdPartyService.ServiceImplementation.DI.RewardedAds {
    using UnityEngine.Events;

    public interface IRewardedAdsService {
        public void LoadAd();
        public void ShowAd(UnityAction<bool> onAdComplete, string where);
        public bool IsAdReady();

    }
}