namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk
{
    #if LevelPlay
    using System;
    using ThirdPartyService.Core.Analytics;
    using ThirdPartyService.Core.AdsService.BannerAds;

    // The seam between the LevelPlay ad wrappers and the mediation SDK. Everything the wrappers need
    // from LevelPlay passes through here in ThirdPartyService's own types, so the wrappers can be
    // driven by a fake in EditMode tests and LevelPlay's own types stay inside LevelPlaySdkAdapter.
    public interface IAdsSdk
    {
        event Action<AdImpression> ImpressionDataReady;
        void SetCOPPA(bool value);
        void SetCCPA(bool value);
        // Exactly one of the callbacks fires, once, for each call.
        void Init(string appKey, Action onSuccess, Action<string> onFailed);

        IFullscreenAdUnit CreateInterstitial(string adUnitId);
        IRewardedAdUnit   CreateRewarded(string adUnitId);

        // The unit never shows on load; it waits for Show().
        IBannerAdUnit CreateBanner(string adUnitId, BannerAdSize size, BannerPosition position);
    }

    public enum BannerAdSize
    {
        Banner,
        MediumRectangle,
    }

    // Disposing a unit destroys the SDK ad object and drops every subscription to it.
    public interface IAdUnit : IDisposable
    {
        event Action         Loaded;
        event Action<string> LoadFailed;
        event Action         Displayed;
        event Action<string> DisplayFailed;
        event Action         Clicked;

        void Load();
    }

    public interface IFullscreenAdUnit : IAdUnit
    {
        event Action Closed;

        bool IsReady { get; }

        void Show(string placement);
    }

    public interface IRewardedAdUnit : IFullscreenAdUnit
    {
        // Carries the SDK's reward description, for logs only.
        event Action<string> Rewarded;
    }

    public interface IBannerAdUnit : IAdUnit
    {
        event Action Expanded;
        event Action Collapsed;
        event Action LeftApplication;

        BannerPosition Position     { get; }
        float          HeightPixels { get; }

        void Show();
        void Hide();
    }
    #endif
}
