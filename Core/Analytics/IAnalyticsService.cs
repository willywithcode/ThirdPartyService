namespace ThirdPartyService.Core.Analytics
{
    using System.Collections.Generic;
    public interface IAnalyticsService
    {
        public void SendEvent(string eventName, Dictionary<string, string> eventParams);
    }
}