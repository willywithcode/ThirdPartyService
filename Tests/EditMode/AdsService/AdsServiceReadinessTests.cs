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

    // How the aggregator picks one provider out of several for the two waterfall formats: the
    // highest-priority provider that is READY serves, so a starved top network falls through to the
    // next instead of reporting "no ads" over a filled one. On a device that means LevelPlay
    // (priority 10) first and AdMob (5) behind it; in the Editor only the Dummies are registered.
    public class AdsServiceReadinessTests
    {
        private sealed class Rewarded : IRewardedAdsService
        {
            public  int               Priority;
            public  bool              Ready;
            public  int               Shows;
            private UnityAction<bool> onComplete;

            public int  GetPriority() => this.Priority;
            public void Initialize()  { }
            public bool IsAdReady()   => this.Ready;

            public void ShowAd(UnityAction<bool> callback, string where)
            {
                this.Shows++;
                this.onComplete = callback;
            }

            public void Complete(bool rewarded) => this.onComplete?.Invoke(rewarded);
        }

        private sealed class Interstitial : IInterstitialAdsService
        {
            public int  Priority;
            public bool Ready;
            public int  Shows;

            public int  GetPriority()         => this.Priority;
            public void Initialize()          { }
            public bool IsInitialized()       => true;
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

        private static AdsLocalDataService NotRemoved()
        {
            var local = new AdsLocalDataService();
            local.Data.IsRemovedAds = false; // in memory only; whatever the editor saved is untouched
            return local;
        }

        [Test]
        public void RewardedWaterfall_PrefersTheHighestPriorityReadyService()
        {
            var high    = new Rewarded { Priority = 10, Ready = true };
            var low     = new Rewarded { Priority = 5, Ready  = true };
            var service = NewService(NotRemoved(), new Interstitial[0], new[] { low, high });

            Assert.That(service.IsRewardedAdReady(), Is.True);
            service.ShowRewardedAd(_ => { }, "test");

            Assert.That(high.Shows, Is.EqualTo(1));
            Assert.That(low.Shows, Is.Zero, "the lower-priority network must not be asked while the top one has an ad");
        }

        [Test]
        public void RewardedWaterfall_FallsThroughWhenTheTopServiceIsStarved()
        {
            var high    = new Rewarded { Priority = 10, Ready = false };
            var low     = new Rewarded { Priority = 5, Ready  = true };
            var service = NewService(NotRemoved(), new Interstitial[0], new[] { low, high });

            Assert.That(service.IsRewardedAdReady(), Is.True, "an ad is available, just not on the top network");
            service.ShowRewardedAd(_ => { }, "test");

            Assert.That(low.Shows, Is.EqualTo(1));
            Assert.That(high.Shows, Is.Zero);
        }

        [Test]
        public void RewardedWaterfall_WithNoReadyService_CompletesFalseWithoutShowing()
        {
            var high    = new Rewarded { Priority = 10, Ready = false };
            var low     = new Rewarded { Priority = 5, Ready  = false };
            var service = NewService(NotRemoved(), new Interstitial[0], new[] { low, high });
            var results = new List<bool>();

            Assert.That(service.IsRewardedAdReady(), Is.False);
            service.ShowRewardedAd(results.Add, "test");

            Assert.That(results, Is.EqualTo(new[] { false }));
            Assert.That(high.Shows, Is.Zero);
            Assert.That(low.Shows, Is.Zero);
        }

        [Test]
        public void RewardedWaterfall_TheChosenServiceOwnsTheWholeAttempt()
        {
            var high    = new Rewarded { Priority = 10, Ready = false };
            var low     = new Rewarded { Priority = 5, Ready  = true };
            var service = NewService(NotRemoved(), new Interstitial[0], new[] { low, high });
            var results = new List<bool>();

            service.ShowRewardedAd(results.Add, "test");
            low.Complete(true);

            Assert.That(results, Is.EqualTo(new[] { true }), "the chosen service's answer is the caller's answer");
            Assert.That(high.Shows, Is.Zero, "a failed or unrewarded show is not retried on another network");
        }

        [Test]
        public void Rewarded_KeepsServingAfterRemoveAds()
        {
            // docs/product/ads.md: "Remove Ads leaves rewarded ads on."
            var only  = new Rewarded { Priority = 10, Ready = true };
            var local = new AdsLocalDataService();
            local.Data.IsRemovedAds = true;
            var service = NewService(local, new Interstitial[0], new[] { only });

            Assert.That(service.IsRewardedAdReady(), Is.True);
            service.ShowRewardedAd(_ => { }, "test");

            Assert.That(only.Shows, Is.EqualTo(1));
        }

        [Test]
        public void InterstitialReadiness_UsesAnyReadyService_UnlessAdsWereRemoved()
        {
            var high  = new Interstitial { Priority = 10, Ready = false };
            var low   = new Interstitial { Priority = 1, Ready  = true };
            var local = NotRemoved();
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
            var service = NewService(NotRemoved(), new Interstitial[0], new Rewarded[0]);
            Assert.That(service.IsRewardedAdReady(), Is.False);
            Assert.That(service.IsInterstitialAdReady(), Is.False);
        }
    }
}
