namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.RewardedAds;

    // Only LevelPlay's reward callback earns a reward. Reward and close arrive in either order; the
    // attempt completes once, true when both happened, false when the ad closed and no reward came
    // within the 3 second grace window, or when the show failed.
    public class LevelPlayRewardedAdsTests
    {
        private AdsTestRig           rig;
        private LevelPlayRewardedAds rewarded;
        private List<bool>           results;

        [SetUp]
        public void SetUp()
        {
            this.rig      = new AdsTestRig();
            this.rewarded = this.rig.NewRewarded();
            this.results  = new List<bool>();
            this.rewarded.Initialize();
        }

        private FakeRewardedAdUnit Unit => this.rig.Sdk.Rewardeds.Single();

        private void ShowLoadedAd()
        {
            this.Unit.CompleteLoad();
            this.rewarded.ShowAd(this.results.Add, "test");
            this.Unit.Display();
        }

        [Test]
        public void Initialize_CreatesTheRewardedAdOnTheSettingsAdUnit_AndStartsLoading()
        {
            Assert.That(this.Unit.AdUnitId, Is.EqualTo("rewarded-unit"));
            Assert.That(this.Unit.LoadCalls, Is.EqualTo(1));
        }

        [Test]
        public void ShowBeforeReady_CompletesFalse_AndNeverReachesTheSdk()
        {
            this.rewarded.ShowAd(this.results.Add, "test");

            Assert.That(this.results, Is.EqualTo(new[] { false }));
            Assert.That(this.Unit.ShowPlacements, Is.Empty);
        }

        [Test]
        public void RewardThenClose_CompletesTrueOnce()
        {
            this.ShowLoadedAd();

            this.Unit.Reward();
            Assert.That(this.results, Is.Empty, "the reward is held until the ad closes");
            this.Unit.Close();

            Assert.That(this.results, Is.EqualTo(new[] { true }));
        }

        [Test]
        public void CloseThenReward_CompletesTrueOnce()
        {
            this.ShowLoadedAd();

            this.Unit.Close();
            Assert.That(this.results, Is.Empty, "close alone is not a verdict");
            this.Unit.Reward();

            Assert.That(this.results, Is.EqualTo(new[] { true }));
            Assert.That(this.rig.Scheduler.Pending.Where(e => e.Delay == 3f), Is.Empty, "the grace window is cancelled");
        }

        [Test]
        public void CloseWithoutReward_CompletesFalseOnlyWhenTheGraceWindowEnds()
        {
            this.ShowLoadedAd();

            this.Unit.Close();

            Assert.That(this.results, Is.Empty);
            Assert.That(this.rig.Scheduler.Pending.Single().Delay, Is.EqualTo(3f));

            this.rig.Scheduler.RunPending();

            Assert.That(this.results, Is.EqualTo(new[] { false }));
        }

        [Test]
        public void RewardAfterTheGraceWindow_GrantsNothing()
        {
            this.ShowLoadedAd();
            this.Unit.Close();
            this.rig.Scheduler.RunPending();

            this.Unit.Reward();

            Assert.That(this.results, Is.EqualTo(new[] { false }));
        }

        [Test]
        public void DuplicateRewardCallbacks_CompleteOnce()
        {
            this.ShowLoadedAd();

            this.Unit.Reward();
            this.Unit.Reward();
            this.Unit.Close();
            this.Unit.Reward();

            Assert.That(this.results, Is.EqualTo(new[] { true }));
        }

        [Test]
        public void ClickAndClose_WithoutReward_NeverCompleteTrue()
        {
            this.ShowLoadedAd();

            this.Unit.Click();
            this.Unit.Close();
            this.rig.Scheduler.RunPending();

            Assert.That(this.results, Is.EqualTo(new[] { false }));
        }

        [Test]
        public void DisplayFailure_CompletesFalseAtOnce_AndLoadsTheNextAd()
        {
            this.Unit.CompleteLoad();
            this.rewarded.ShowAd(this.results.Add, "test");

            this.Unit.FailDisplay();

            Assert.That(this.results, Is.EqualTo(new[] { false }));
            Assert.That(this.Unit.LoadCalls, Is.EqualTo(2));
        }

        [Test]
        public void Close_LoadsTheNextAd_WhileTheRewardCanStillArrive()
        {
            this.ShowLoadedAd();

            this.Unit.Close();

            Assert.That(this.Unit.LoadCalls, Is.EqualTo(2));
            this.Unit.CompleteLoad();
            this.Unit.Reward();
            Assert.That(this.results, Is.EqualTo(new[] { true }));
        }

        [Test]
        public void ShowWhileAnAttemptIsStillOpen_CompletesTheNewRequestFalse()
        {
            this.ShowLoadedAd();
            this.Unit.Close();
            this.Unit.CompleteLoad();
            var second = new List<bool>();

            this.rewarded.ShowAd(second.Add, "again");

            Assert.That(second, Is.EqualTo(new[] { false }));
            Assert.That(this.Unit.ShowPlacements, Is.EqualTo(new[] { "test" }));
            Assert.That(this.results, Is.Empty, "the first attempt is still waiting for its reward");
        }

        [Test]
        public void RewardIsLoggedAsTheOnlyRewardEvent()
        {
            this.ShowLoadedAd();

            this.Unit.Reward();
            this.Unit.Close();

            Assert.That(this.rig.Count(AdFormat.Rewarded, AdEventKind.Rewarded), Is.EqualTo(1));
            Assert.That(this.rig.Count(AdFormat.Rewarded, AdEventKind.Closed), Is.EqualTo(1));
            Assert.That(this.rig.Count(AdFormat.Rewarded, AdEventKind.NotRewarded), Is.EqualTo(0));
        }

        [Test]
        public void Priority_OutranksTheThirdPartyServiceDummy()
        {
            var dummy = new ThirdPartyService.ServiceImplementation.AdsService.DummyAds.RewardedAds.DummyRewardedAds();

            Assert.That(this.rewarded.GetPriority(), Is.GreaterThan(dummy.GetPriority()));
        }
    }
    #endif
}
