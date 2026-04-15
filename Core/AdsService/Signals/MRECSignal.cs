namespace ThirdPartyService.Core.AdsService.Signals
{
    public readonly struct OnMRECAdLoadedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnMRECAdLoadedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnMRECAdLoadFailedEventSignal : IAdsSignal
    {
        public string AdsPlatform  { get; }
        public string ErrorMessage { get; }

        public OnMRECAdLoadFailedEventSignal(string adsPlatform, string errorMessage)
        {
            this.AdsPlatform  = adsPlatform;
            this.ErrorMessage = errorMessage;
        }
    }

    public readonly struct OnMRECAdClickedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnMRECAdClickedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnMRECAdRevenuePaidEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }
        public double Revenue     { get; }
        public string Currency    { get; }

        public OnMRECAdRevenuePaidEventSignal(string adsPlatform, string placementId, double revenue, string currency)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
            this.Revenue     = revenue;
            this.Currency    = currency;
        }
    }

    public readonly struct OnMRECAdExpandedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnMRECAdExpandedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnMRECAdCollapsedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnMRECAdCollapsedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnShowMRECSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnShowMRECSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnHideMRECSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnHideMRECSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }
}
