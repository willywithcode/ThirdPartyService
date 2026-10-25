namespace ThirdPartyService.Core.Analytics
{
    public readonly struct AdImpression
    {
        public string AdPlatform { get; }
        public string AdNetwork { get; }
        public string AdFormat { get; }
        public string AdUnitName { get; }
        public double? Revenue { get; }
        public string Currency { get; }

        public AdImpression(string adPlatform, string adNetwork, string adFormat, string adUnitName, double? revenue, string currency)
        {
            this.AdPlatform = adPlatform;
            this.AdNetwork  = adNetwork;
            this.AdFormat   = adFormat;
            this.AdUnitName = adUnitName;
            this.Revenue    = revenue;
            this.Currency   = currency;
        }
    }
}
