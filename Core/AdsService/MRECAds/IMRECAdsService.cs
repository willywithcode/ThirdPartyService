namespace ThirdPartyService.Core.AdsService.MRECAds {
    public interface IMRECAdsService {
        public void Initialize();
        public void ShowMREC(MRECAdsPosition position);
        public void HideMREC();
        public bool IsShown();
        public bool IsInitialized();
    }

    public enum MRECAdsPosition {
        TopLeft,
        TopCenter,
        TopRight,
        Centered,
        CenterLeft,
        CenterRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }
}