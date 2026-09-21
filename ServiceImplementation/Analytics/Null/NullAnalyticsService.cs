namespace ThirdPartyService.ServiceImplementation.Analytics.Null
{
    using System.Collections.Generic;
    using ThirdPartyService.Core.Analytics;

    /// <summary>
    /// Stands in when no analytics provider is compiled into the build, so callers can depend on
    /// IAnalyticsService without every one of them repeating the provider's #if. A build with the
    /// defines off then drops its events here rather than failing to resolve the dependency.
    /// </summary>
    public class NullAnalyticsService : IAnalyticsService
    {
        public void SendEvent(string eventName, Dictionary<string, string> eventParams)
        {
        }
    }
}
