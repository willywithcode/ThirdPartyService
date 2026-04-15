namespace ThirdPartyService.Core.AdsService.Signals
{
    public readonly struct OnRemoveAdsPurchasedSignal
    {
    }

    public interface IAdsSignal
    {
        string AdsPlatform { get; }
    }
}
