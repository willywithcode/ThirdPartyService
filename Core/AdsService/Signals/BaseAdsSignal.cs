namespace ThirdPartyService.Core.Signals
{
    public class BaseAdsSignal
    {
        public string Placement;

        public BaseAdsSignal(string placement)
        {
            this.Placement = placement;
        }
    }
}