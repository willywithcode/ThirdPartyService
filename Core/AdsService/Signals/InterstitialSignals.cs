namespace ThirdPartyService.Core.AdsService.Signals
{
    public readonly struct OnInterstitialAdLoadedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnInterstitialAdLoadedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnInterstitialAdLoadFailedEventSignal : IAdsSignal
    {
        public string AdsPlatform  { get; }
        public string ErrorMessage { get; }

        public OnInterstitialAdLoadFailedEventSignal(string adsPlatform, string errorMessage)
        {
            this.AdsPlatform  = adsPlatform;
            this.ErrorMessage = errorMessage;
        }
    }

    public readonly struct OnInterstitialAdDisplayedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }
        public string AdId        { get; }

        public OnInterstitialAdDisplayedEventSignal(string adsPlatform, string placementId, string adId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
            this.AdId        = adId;
        }
    }

    public readonly struct OnInterstitialAdDisplayFailedEventSignal : IAdsSignal
    {
        public string AdsPlatform  { get; }
        public string PlacementId  { get; }
        public string ErrorMessage { get; }

        public OnInterstitialAdDisplayFailedEventSignal(string adsPlatform, string placementId, string errorMessage)
        {
            this.AdsPlatform  = adsPlatform;
            this.PlacementId  = placementId;
            this.ErrorMessage = errorMessage;
        }
    }

    public readonly struct OnInterstitialAdClickedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnInterstitialAdClickedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnInterstitialAdHiddenEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnInterstitialAdHiddenEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnInterstitialAdRevenuePaidEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }
        public double Revenue     { get; }
        public string Currency    { get; }
        public string AdId        { get; }

        public OnInterstitialAdRevenuePaidEventSignal(string adsPlatform, string placementId, double revenue, string currency, string adId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
            this.Revenue     = revenue;
            this.Currency    = currency;
            this.AdId        = adId;
        }
    }

    public readonly struct OnInterstitialShowSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnInterstitialShowSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }
}
