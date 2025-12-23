namespace ThirdPartyService.Core.AdsService.Signals
{
    public class OnRewardedAdLoadedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnRewardedAdLoadedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnRewardedAdLoadFailedEventSignal : BaseAdsSignal
    {
        public string ErrorMessage { get; set; }
        public OnRewardedAdLoadFailedEventSignal(string adsPlatform, string errorMessage) : base(adsPlatform)
        {
            this.ErrorMessage = errorMessage;
        }
    }
    public class OnRewardedAdDisplayedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnRewardedAdDisplayedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnRewardedAdDisplayFailedEventSignal : BaseAdsSignal
    {
        public string PlacementId  { get; set; }
        public string ErrorMessage { get; set; }
        public OnRewardedAdDisplayFailedEventSignal(string adsPlatform, string placementId, string errorMessage) : base(adsPlatform)
        {
            this.PlacementId  = placementId;
            this.ErrorMessage = errorMessage;
        }
    }
    public class OnRewardedAdClickedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnRewardedAdClickedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnRewardedAdHiddenEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnRewardedAdHiddenEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnRewardedAdRevenuePaidEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public double Revenue     { get; set; }
        public string Currency    { get; set; }
        public OnRewardedAdRevenuePaidEventSignal(string adsPlatform, string placementId, double revenue, string currency) : base(adsPlatform)
        {
            this.PlacementId = placementId;
            this.Revenue     = revenue;
            this.Currency    = currency;
        }
    }
    public class OnRewardedAdReceivedRewardEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public string RewardType  { get; set; }
        public double RewardAmount { get; set; }
        public OnRewardedAdReceivedRewardEventSignal(string adsPlatform, string placementId, string rewardType, double rewardAmount) : base(adsPlatform)
        {
            this.PlacementId  = placementId;
            this.RewardType   = rewardType;
            this.RewardAmount = rewardAmount;
        }
    }
    public class OnRewardedShowSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnRewardedShowSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
}
