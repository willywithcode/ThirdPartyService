namespace ThirdPartyService.Core.AdsService.MREC
{
    public interface IMRECAdsService
    {
        void ShowMREC(AdViewPosition             adViewPosition);
        void HideMREC(AdViewPosition             adViewPosition);
        void StopMRECAutoRefresh(AdViewPosition  adViewPosition);
        void StartMRECAutoRefresh(AdViewPosition adViewPosition);
        void LoadMREC(AdViewPosition             adViewPosition);
        bool IsMRECReady(AdViewPosition          adViewPosition);
        void HideAllMREC();
    }
}