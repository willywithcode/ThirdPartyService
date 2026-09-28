namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using ThirdPartyService.Core.AdsService.BannerAds;

    // Delayed work belongs to the attempt or ad object that scheduled it. If it still fires after
    // that attempt ended or that object was replaced or disposed (a late timer, a cancel that did not
    // take), it must do nothing. RunIncludingCancelled fires every entry, cancelled or not.
    public class StaleScheduledWorkTests
    {
        private AdsTestRig rig;

        [SetUp]
        public void SetUp() => this.rig = new AdsTestRig();

        [Test]
        public void StaleRewardGrace_DoesNotCompleteTheNextAttempt()
        {
            var rewarded = this.rig.NewRewarded();
            rewarded.Initialize();
            var unit  = this.rig.Sdk.Rewardeds.Single();
            var first = new List<bool>();
            unit.CompleteLoad();
            rewarded.ShowAd(first.Add, "first");
            unit.Display();
            unit.Close();
            unit.Reward();
            Assert.That(first, Is.EqualTo(new[] { true }));

            unit.CompleteLoad();
            var second = new List<bool>();
            rewarded.ShowAd(second.Add, "second");
            unit.Display();
            this.rig.Scheduler.RunIncludingCancelled();

            Assert.That(second, Is.Empty, "the first attempt's grace timer must not end the second attempt");
            unit.Reward();
            unit.Close();
            Assert.That(second, Is.EqualTo(new[] { true }));
        }

        [Test]
        public void StaleRetry_OnAReplacedBanner_DoesNotReloadTheDestroyedAd()
        {
            var banner = this.rig.NewBanner();
            banner.Initialize();
            var first = this.rig.Sdk.Banners.Single();
            first.FailLoad();

            banner.ShowBanner(BannerPosition.TopCenter);
            this.rig.Scheduler.RunIncludingCancelled();

            var second = this.rig.Sdk.Banners.Last();
            Assert.That(first.Disposed, Is.True);
            Assert.That(first.LoadCalls, Is.EqualTo(1), "the destroyed banner must not be asked to load again");
            Assert.That(second.LoadCalls, Is.EqualTo(1));
        }

        [Test]
        public void StaleRetry_AfterTheWrapperIsDisposed_DoesNothing()
        {
            var interstitial = this.rig.NewInterstitial();
            interstitial.Initialize();
            var unit = this.rig.Sdk.Interstitials.Single();
            unit.FailLoad();

            interstitial.Dispose();

            Assert.DoesNotThrow(() => this.rig.Scheduler.RunIncludingCancelled());
            Assert.That(unit.LoadCalls, Is.EqualTo(1));
        }

        [Test]
        public void StaleInitRetry_AfterTheSessionIsDisposed_DoesNothing()
        {
            var rig = new AdsTestRig(initialized: false);
            rig.Session.Start();
            rig.Sdk.FailInit("no network");

            rig.Session.Dispose();
            rig.Scheduler.RunIncludingCancelled();

            Assert.That(rig.Sdk.InitAppKeys.Count, Is.EqualTo(1));
        }
    }
    #endif
}
