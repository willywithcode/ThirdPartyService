namespace ThirdPartyService.Core.AdsService.Signals
{
    public readonly struct OnBannerAdLoadedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnBannerAdLoadedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnBannerAdLoadFailedEventSignal : IAdsSignal
    {
        public string AdsPlatform  { get; }
        public string ErrorMessage { get; }

        public OnBannerAdLoadFailedEventSignal(string adsPlatform, string errorMessage)
        {
            this.AdsPlatform  = adsPlatform;
            this.ErrorMessage = errorMessage;
        }
    }

    public readonly struct OnBannerAdClickedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnBannerAdClickedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnBannerAdRevenuePaidEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }
        public double Revenue     { get; }
        public string Currency    { get; }

        public OnBannerAdRevenuePaidEventSignal(string adsPlatform, string placementId, double revenue, string currency)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
            this.Revenue     = revenue;
            this.Currency    = currency;
        }
    }

    public readonly struct OnBannerAdExpandedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnBannerAdExpandedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnBannerAdCollapsedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnBannerAdCollapsedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnShowBannerSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnShowBannerSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnHideBannerSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnHideBannerSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }
}
