namespace ThirdPartyService.ServiceImplementation.AdsService.Fsn
{
    using System;

    /// <summary>One FSN ad unit (one Java <c>FsnAd</c>). Events and callbacks arrive on the main thread.</summary>
    public interface IFsnAdUnit
    {
        string AdUnitId  { get; }
        bool   IsReady   { get; }
        bool   IsLoading { get; }

        /// <summary>A load finished: null on success, the error otherwise.</summary>
        event Action<string> LoadCompleted;

        /// <summary>The shown ad earned revenue: ad source, value in currency units, currency code.</summary>
        event Action<string, double, string> Paid;

        void Load();

        /// <summary>
        /// <paramref name="onCompleted"/> gets an empty string after the ad closed, the reason it was
        /// not shown otherwise. The close button appears after <paramref name="closeAfterSeconds"/>,
        /// or after <see cref="FsnSettings.closeAfterSeconds"/> when it is negative.
        /// </summary>
        void Show(int closeAfterSeconds, Action<string> onCompleted);
    }
}
