namespace ThirdPartyService.Core.Analytics
{
    public readonly struct AdImpression
    {
        public string AdNetwork { get; }
        public string AdFormat { get; }
        public string AdUnitName { get; }
        public double? Revenue { get; }

        public AdImpression(string adNetwork, string adFormat, string adUnitName, double? revenue)
        {
            this.AdNetwork = adNetwork;
            this.AdFormat = adFormat;
            this.AdUnitName = adUnitName;
            this.Revenue = revenue;
        }
    }
}
