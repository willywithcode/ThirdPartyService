namespace ThirdPartyService.Core.AdsService.Signals
{
    public class OnInterstitialAdLoadedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnInterstitialAdLoadedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnInterstitialAdLoadFailedEventSignal : BaseAdsSignal
    {
        public string ErrorMessage { get; set; }
        public OnInterstitialAdLoadFailedEventSignal(string adsPlatform, string errorMessage) : base(adsPlatform)
        {
            this.ErrorMessage = errorMessage;
        }
    }
    public class OnInterstitialAdDisplayedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnInterstitialAdDisplayedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnInterstitialAdDisplayFailedEventSignal : BaseAdsSignal
    {
        public string PlacementId  { get; set; }
        public string ErrorMessage { get; set; }
        public OnInterstitialAdDisplayFailedEventSignal(string adsPlatform, string placementId, string errorMessage) : base(adsPlatform)
        {
            this.PlacementId  = placementId;
            this.ErrorMessage = errorMessage;
        }
    }
    public class OnInterstitialAdClickedEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnInterstitialAdClickedEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnInterstitialAdHiddenEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnInterstitialAdHiddenEventSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
    public class OnInterstitialAdRevenuePaidEventSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public double Revenue     { get; set; }
        public string Currency    { get; set; }
        public OnInterstitialAdRevenuePaidEventSignal(string adsPlatform, string placementId, double revenue, string currency) : base(adsPlatform)
        {
            this.PlacementId = placementId;
            this.Revenue     = revenue;
            this.Currency    = currency;
        }
    }
    public class OnInterstitialShowSignal : BaseAdsSignal
    {
        public string PlacementId { get; set; }
        public OnInterstitialShowSignal(string adsPlatform, string placementId) : base(adsPlatform)
        {
            this.PlacementId = placementId;
        }
    }
}