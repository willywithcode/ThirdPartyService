namespace ThirdPartyService.Core.AdsService.Signals
{
    public readonly struct OnRewardedAdLoadedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnRewardedAdLoadedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnRewardedAdLoadFailedEventSignal : IAdsSignal
    {
        public string AdsPlatform  { get; }
        public string ErrorMessage { get; }

        public OnRewardedAdLoadFailedEventSignal(string adsPlatform, string errorMessage)
        {
            this.AdsPlatform  = adsPlatform;
            this.ErrorMessage = errorMessage;
        }
    }

    public readonly struct OnRewardedAdDisplayedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnRewardedAdDisplayedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnRewardedAdDisplayFailedEventSignal : IAdsSignal
    {
        public string AdsPlatform  { get; }
        public string PlacementId  { get; }
        public string ErrorMessage { get; }

        public OnRewardedAdDisplayFailedEventSignal(string adsPlatform, string placementId, string errorMessage)
        {
            this.AdsPlatform  = adsPlatform;
            this.PlacementId  = placementId;
            this.ErrorMessage = errorMessage;
        }
    }

    public readonly struct OnRewardedAdClickedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnRewardedAdClickedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnRewardedAdHiddenEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnRewardedAdHiddenEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnRewardedAdRevenuePaidEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }
        public double Revenue     { get; }
        public string Currency    { get; }

        public OnRewardedAdRevenuePaidEventSignal(string adsPlatform, string placementId, double revenue, string currency)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
            this.Revenue     = revenue;
            this.Currency    = currency;
        }
    }

    public readonly struct OnRewardedAdReceivedRewardEventSignal : IAdsSignal
    {
        public string AdsPlatform  { get; }
        public string PlacementId  { get; }
        public string RewardType   { get; }
        public double RewardAmount { get; }

        public OnRewardedAdReceivedRewardEventSignal(string adsPlatform, string placementId, string rewardType, double rewardAmount)
        {
            this.AdsPlatform  = adsPlatform;
            this.PlacementId  = placementId;
            this.RewardType   = rewardType;
            this.RewardAmount = rewardAmount;
        }
    }

    public readonly struct OnRewardedShowSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnRewardedShowSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }
}
