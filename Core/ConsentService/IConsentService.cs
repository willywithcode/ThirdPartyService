namespace ThirdPartyService.Core.ConsentService
{
    using Cysharp.Threading.Tasks;

    public interface IConsentService
    {
        UniTask GatherConsentAsync();
        bool IsGathered { get; }
        bool IsPrivacyOptionsRequired { get; }
        UniTask ShowPrivacyOptionsAsync();
    }
}
