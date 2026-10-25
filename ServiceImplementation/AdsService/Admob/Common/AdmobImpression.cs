namespace ThirdPartyService.ServiceImplementation.AdsService.Admob.Common
{
    #if Admob
    using GoogleMobileAds.Api;
    using ThirdPartyService.Core.Analytics;

    // Turns AdMob's OnAdPaid into the AdImpression the analytics services log as ad_impression, the
    // same event LevelPlay's impressions go to. AdMob reports the value in micros of its own currency,
    // so 1 USD arrives as 1,000,000 and is divided back before it is logged.
    public static class AdmobImpression
    {
        public const string Platform = "AdMob";

        public const string Banner       = "banner";
        public const string Interstitial = "interstitial";
        public const string Rewarded     = "rewarded";
        public const string AppOpen      = "app_open";
        public const string Native       = "native";

        public static AdImpression From(string adFormat, string adUnitId, string adSourceName, long valueMicros, string currencyCode) =>
            new(Platform, adSourceName ?? "", adFormat, adUnitId ?? "", valueMicros / 1_000_000d, currencyCode ?? "");

        // The network inside AdMob that filled the ad ("AdMob Network", or a mediated one); empty when
        // AdMob did not say.
        public static string AdSource(ResponseInfo responseInfo) =>
            responseInfo?.GetLoadedAdapterResponseInfo()?.AdSourceName ?? "";
    }
    #endif
}
