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

    public class AdsServiceReadinessTests
    {
        private sealed class Rewarded : IRewardedAdsService
        {
            public int Priority;
            public bool Ready;
            public int Shows;
            public int GetPriority() => this.Priority;
            public void Initialize() { }
            public bool IsAdReady() => this.Ready;
            public void ShowAd(UnityAction<bool> callback, string where) => this.Shows++;
        }

        private sealed class Interstitial : IInterstitialAdsService
        {
            public int Priority;
            public bool Ready;
            public int Shows;
            public int GetPriority() => this.Priority;
            public void Initialize() { }
            public bool IsInitialized() => true;
            public bool IsInterstitialReady() => this.Ready;
            public void ShowInterstitial(string where, UnityAction onAdClosed = null, UnityAction onAdFailedToShow = null) => this.Shows++;
        }

        private static AdsService NewService(AdsLocalDataService localData, IEnumerable<Interstitial> interstitials, IEnumerable<Rewarded> rewardeds)
        {
            var i = new List<IInterstitialAdsService>(interstitials);
            var r = new List<IRewardedAdsService>(rewardeds);
            return new AdsService(localData, new List<IAOAAdsService>(), new List<IBannerAdsService>(), i,
                new List<IMRECAdsService>(), new List<INativeAdsService>(), r, new SignalBus());
        }

        [Test]
        public void RewardedReadiness_UsesHighestPriorityServiceEvenIfLowerOneIsReady()
        {
            var high = new Rewarded { Priority = 10, Ready = false };
            var low = new Rewarded { Priority = 1, Ready = true };
            var service = NewService(new AdsLocalDataService(), new Interstitial[0], new[] { low, high });
            Assert.That(service.IsRewardedAdReady(), Is.False);
            service.ShowRewardedAd(_ => { }, "test");
            Assert.That(high.Shows, Is.EqualTo(1));
            Assert.That(low.Shows, Is.Zero);
            high.Ready = true;
            Assert.That(service.IsRewardedAdReady(), Is.True);
        }

        [Test]
        public void InterstitialReadiness_UsesAnyReadyService_UnlessAdsWereRemoved()
        {
            var high = new Interstitial { Priority = 10, Ready = false };
            var low = new Interstitial { Priority = 1, Ready = true };
            var local = new AdsLocalDataService();
            local.Data.IsRemovedAds = false;
            var service = NewService(local, new[] { low, high }, new Rewarded[0]);
            Assert.That(service.IsInterstitialAdReady(), Is.True);
            service.ShowInterstitialAd("test");
            Assert.That(low.Shows, Is.EqualTo(1));
            Assert.That(high.Shows, Is.Zero);
            local.Data.IsRemovedAds = true;
            Assert.That(service.IsInterstitialAdReady(), Is.False);
        }

        [Test]
        public void EmptyServiceLists_AreNotReady()
        {
            var service = NewService(new AdsLocalDataService(), new Interstitial[0], new Rewarded[0]);
            Assert.That(service.IsRewardedAdReady(), Is.False);
            Assert.That(service.IsInterstitialAdReady(), Is.False);
        }
    }
}
