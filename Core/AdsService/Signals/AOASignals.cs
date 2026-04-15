namespace ThirdPartyService.Core.AdsService.Signals
{
    public readonly struct OnAOAAdLoadedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnAOAAdLoadedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnAOAAdLoadFailedEventSignal : IAdsSignal
    {
        public string AdsPlatform  { get; }
        public string ErrorMessage { get; }

        public OnAOAAdLoadFailedEventSignal(string adsPlatform, string errorMessage)
        {
            this.AdsPlatform  = adsPlatform;
            this.ErrorMessage = errorMessage;
        }
    }

    public readonly struct OnAOAAdDisplayedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnAOAAdDisplayedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnAOAAdDisplayFailedEventSignal : IAdsSignal
    {
        public string AdsPlatform  { get; }
        public string PlacementId  { get; }
        public string ErrorMessage { get; }

        public OnAOAAdDisplayFailedEventSignal(string adsPlatform, string placementId, string errorMessage)
        {
            this.AdsPlatform  = adsPlatform;
            this.PlacementId  = placementId;
            this.ErrorMessage = errorMessage;
        }
    }

    public readonly struct OnAOAAdClickedEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnAOAAdClickedEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnAOAAdHiddenEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnAOAAdHiddenEventSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }

    public readonly struct OnAOAAdRevenuePaidEventSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }
        public double Revenue     { get; }
        public string Currency    { get; }

        public OnAOAAdRevenuePaidEventSignal(string adsPlatform, string placementId, double revenue, string currency)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
            this.Revenue     = revenue;
            this.Currency    = currency;
        }
    }

    public readonly struct OnAOAShowSignal : IAdsSignal
    {
        public string AdsPlatform { get; }
        public string PlacementId { get; }

        public OnAOAShowSignal(string adsPlatform, string placementId)
        {
            this.AdsPlatform = adsPlatform;
            this.PlacementId = placementId;
        }
    }
}
