namespace ThirdPartyService.Core.AdsService.Signals
{
    public class OnRemoveAdsPurchasedSignal
    {

    }
    public abstract class BaseAdsSignal
    {
        public string AdsPlatform { get; set; }
        protected BaseAdsSignal(string adsPlatform)
        {
            this.AdsPlatform = adsPlatform;
        }
    }
}