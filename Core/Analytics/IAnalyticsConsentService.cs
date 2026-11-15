namespace ThirdPartyService.Core.Analytics
{
    /// <summary>
    /// The player's consent, in the four signals of Google's consent mode. A provider applies it
    /// to everything it collects from then on; providers that persist it (Firebase does) start the
    /// next launch with the last value.
    /// </summary>
    public readonly struct AnalyticsConsent
    {
        public readonly bool AnalyticsStorage;
        public readonly bool AdStorage;
        public readonly bool AdUserData;
        public readonly bool AdPersonalization;

        public AnalyticsConsent(bool analyticsStorage, bool adStorage, bool adUserData, bool adPersonalization)
        {
            this.AnalyticsStorage  = analyticsStorage;
            this.AdStorage         = adStorage;
            this.AdUserData        = adUserData;
            this.AdPersonalization = adPersonalization;
        }

        /// <summary>Everything granted: a player no consent rule covers.</summary>
        public static AnalyticsConsent Granted => new(true, true, true, true);

        public override string ToString() =>
            $"analytics_storage={this.AnalyticsStorage} ad_storage={this.AdStorage} ad_user_data={this.AdUserData} ad_personalization={this.AdPersonalization}";
    }

    /// <summary>Hands the player's consent to an analytics provider.</summary>
    public interface IAnalyticsConsentService
    {
        void SetConsent(AnalyticsConsent consent);
    }
}
