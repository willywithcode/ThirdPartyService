namespace ThirdPartyService.Tests.EditMode.AdsService
{
    using System.Collections.Generic;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using NUnit.Framework;
    using ThirdPartyService.Core.AdsService.AOA;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using ThirdPartyService.Core.AdsService.MRECAds;
    using ThirdPartyService.Core.AdsService.NativeAds;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LocalDatas;
    using UnityEngine.Events;
    using AdsService = ThirdPartyService.ServiceImplementation.AdsService.AdsService;

    // AdsService.ShowInterstitialAd(where, onShowFail, onShowSuccess) must hand the chosen
    // IInterstitialAdsService its callbacks in ShowInterstitial(where, onAdClosed, onAdFailedToShow)
    // order: a close is a successful show, a show failure is a failed one.
    public class AdsServiceInterstitialCallbackTests
    {
        private FakeInterstitialAds interstitial;
        private AdsService          adsService;
        private int                 onShowFail;
        private int                 onShowSuccess;

        [SetUp]
        public void SetUp()
        {
            this.interstitial = new FakeInterstitialAds();

            var localData = new AdsLocalDataService();
            localData.Data.IsRemovedAds = false; // in memory only; whatever the editor saved is untouched

            this.adsService = new AdsService(
                localData,
                new List<IAOAAdsService>(),
                new List<IBannerAdsService>(),
                new List<IInterstitialAdsService> { this.interstitial },
                new List<IMRECAdsService>(),
                new List<INativeAdsService>(),
                new List<IRewardedAdsService>(),
                new SignalBus());
            this.onShowFail    = 0;
            this.onShowSuccess = 0;
        }

        private void ShowInterstitial() =>
            this.adsService.ShowInterstitialAd("test", () => this.onShowFail++, () => this.onShowSuccess++);

        [Test]
        public void ClosedInterstitial_ReachesOnShowSuccess()
        {
            this.ShowInterstitial();
            this.interstitial.Close();

            Assert.That(this.interstitial.ShowPlacements, Is.EqualTo(new[] { "test" }));
            Assert.That(this.onShowSuccess, Is.EqualTo(1));
            Assert.That(this.onShowFail, Is.EqualTo(0));
        }

        [Test]
        public void InterstitialShowFailure_ReachesOnShowFail()
        {
            this.ShowInterstitial();
            this.interstitial.FailToShow();

            Assert.That(this.onShowFail, Is.EqualTo(1));
            Assert.That(this.onShowSuccess, Is.EqualTo(0));
        }

        [Test]
        public void NoReadyInterstitial_ReachesOnShowFail_WithoutShowing()
        {
            this.interstitial.Ready = false;

            this.ShowInterstitial();

            Assert.That(this.interstitial.ShowPlacements, Is.Empty);
            Assert.That(this.onShowFail, Is.EqualTo(1));
            Assert.That(this.onShowSuccess, Is.EqualTo(0));
        }

        private sealed class FakeInterstitialAds : IInterstitialAdsService
        {
            public bool         Ready          { get; set; } = true;
            public List<string> ShowPlacements { get; }      = new();

            private UnityAction onAdClosed;
            private UnityAction onAdFailedToShow;

            public int  GetPriority()         => 10;
            public void Initialize()          { }
            public bool IsInitialized()       => true;
            public bool IsInterstitialReady() => this.Ready;

            public void ShowInterstitial(string where, UnityAction onAdClosed = null, UnityAction onAdFailedToShow = null)
            {
                this.ShowPlacements.Add(where);
                this.onAdClosed       = onAdClosed;
                this.onAdFailedToShow = onAdFailedToShow;
            }

            public void Close()      => this.onAdClosed?.Invoke();
            public void FailToShow() => this.onAdFailedToShow?.Invoke();
        }
    }
}
