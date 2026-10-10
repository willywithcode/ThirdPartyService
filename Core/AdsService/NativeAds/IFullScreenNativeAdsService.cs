namespace ThirdPartyService.Core.AdsService.NativeAds
{
    using UnityEngine.Events;

    /// <summary>
    /// A native ad shown full screen over the game, closable after a countdown (FSN). Ads are
    /// addressed by placement name; the provider maps a placement to its ad units. It does not check
    /// Remove Ads: callers that honour the purchase skip <see cref="Show"/> themselves.
    /// </summary>
    public interface IFullScreenNativeAdsService
    {
        /// <summary>Starts loading the placement's ad units that have no ad and are not loading.</summary>
        void Load(string placement);

        bool IsReady(string placement);

        /// <summary>
        /// Shows the placement's first ready ad unit. <paramref name="onClosed"/> runs on the main
        /// thread once: true after a shown ad is closed, false when nothing could be shown.
        /// </summary>
        void Show(string placement, UnityAction<bool> onClosed);
    }
}
