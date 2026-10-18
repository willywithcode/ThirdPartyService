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

        /// <summary>
        /// <see cref="Show(string, UnityAction{bool})"/> with the close button appearing after
        /// <paramref name="closeAfterSeconds"/> for this show; a negative value keeps the configured
        /// delay. Providers without a countdown ignore it.
        /// </summary>
        void Show(string placement, int closeAfterSeconds, UnityAction<bool> onClosed) => this.Show(placement, onClosed);
    }
}
