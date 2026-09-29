namespace ThirdPartyService.ServiceImplementation.Analytics.Firebase
{
    using System.Collections.Generic;
    using ThirdPartyService.Core.Analytics;

    public static class AdImpressionFirebaseParameters
    {
        public static Dictionary<string, object> Map(AdImpression impression)
        {
            var parameters = new Dictionary<string, object>
            {
                ["ad_platform"] = "ironSource",
                ["ad_source"] = impression.AdNetwork ?? "",
                ["ad_format"] = impression.AdFormat ?? "",
                ["ad_unit_name"] = impression.AdUnitName ?? "",
                ["currency"] = "USD",
            };
            if (impression.Revenue.HasValue) parameters["value"] = impression.Revenue.Value;
            return parameters;
        }
    }
}
