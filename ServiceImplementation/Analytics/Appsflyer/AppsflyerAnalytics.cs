namespace ThirdPartyService.ServiceImplementation.Analytics.Appsflyer
{
    using System.Collections.Generic;
    using AppsFlyerSDK;
    using ThirdPartyService.Core.Analytics;
    using VContainer.Unity;
    public class AppsflyerAnalytics : IAnalyticsService , IInitializable
    {

        public void SendEvent(string eventName, Dictionary<string, string> eventParams)
        {
            AppsFlyer.sendEvent(eventName, eventParams);
        }
        public void SendAdRevenue(string country, string adUnit, string type, string placement, MediationNetwork mediationNetwork, string currency, double revenue)
        {
            var additionalParams = new Dictionary<string, string>
            {
                { AdRevenueScheme.COUNTRY, country },
                { AdRevenueScheme.AD_UNIT, adUnit },
                { AdRevenueScheme.AD_TYPE, type },
                { AdRevenueScheme.PLACEMENT, placement },
            };
            var logRevenue = new AFAdRevenueData("monetizationNetworkEx", mediationNetwork, currency, revenue);
            AppsFlyer.logAdRevenue(logRevenue, additionalParams);
        }
        public void Initialize()
        {
            AppsFlyer.startSDK();
        }
    }
}