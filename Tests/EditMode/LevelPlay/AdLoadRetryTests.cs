namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using System.Linq;
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.InterstitialsAds;

    // After a failed load the next attempt waits: 2s, then doubling, never more than 64s apart, and
    // never two loads in flight. Driven through the interstitial and rewarded wrappers, which share
    // the loading rules.
    public class AdLoadRetryTests
    {
        private AdsTestRig               rig;
        private LevelPlayInterstitialAds interstitial;

        [SetUp]
        public void SetUp()
        {
            this.rig          = new AdsTestRig();
            this.interstitial = this.rig.NewInterstitial();
            this.interstitial.Initialize();
        }

        private FakeFullscreenAdUnit Unit => this.rig.Sdk.Interstitials.Single();

        [Test]
        public void LoadFailure_SchedulesOneRetryAfterTwoSeconds_WithoutReloadingAtOnce()
        {
            this.Unit.FailLoad();

            Assert.That(this.Unit.LoadCalls, Is.EqualTo(1));
            Assert.That(this.rig.Scheduler.Pending.Select(e => e.Delay), Is.EqualTo(new[] { 2f }));

            this.rig.Scheduler.RunPending();

            Assert.That(this.Unit.LoadCalls, Is.EqualTo(2));
        }

        [Test]
        public void RepeatedLoadFailures_BackOffExponentially_UpToSixtyFourSeconds()
        {
            for (var i = 0; i < 8; i++)
            {
                this.Unit.FailLoad();
                this.rig.Scheduler.RunPending();
            }

            Assert.That(this.rig.Scheduler.Scheduled.Select(e => e.Delay),
                Is.EqualTo(new[] { 2f, 4f, 8f, 16f, 32f, 64f, 64f, 64f }));
            Assert.That(this.Unit.LoadCalls, Is.EqualTo(9));
        }

        [Test]
        public void SuccessfulLoad_ResetsTheBackoff()
        {
            this.Unit.FailLoad();
            this.rig.Scheduler.RunPending();
            this.Unit.FailLoad();
            this.rig.Scheduler.RunPending();
            this.Unit.CompleteLoad();
            this.interstitial.ShowInterstitial("test");
            this.Unit.Display();
            this.Unit.Close();

            this.Unit.FailLoad();

            Assert.That(this.rig.Scheduler.Pending.Single().Delay, Is.EqualTo(2f));
        }

        [Test]
        public void LoadWhileALoadIsInFlight_DoesNotStartASecond()
        {
            this.interstitial.Load();
            this.interstitial.Load();

            Assert.That(this.Unit.LoadCalls, Is.EqualTo(1));
        }

        [Test]
        public void LoadWhileAnAdIsReady_DoesNothing()
        {
            this.Unit.CompleteLoad();

            this.interstitial.Load();

            Assert.That(this.Unit.LoadCalls, Is.EqualTo(1));
        }

        [Test]
        public void ShowWhileRetryIsPending_FailsWithoutCuttingTheWaitShort()
        {
            this.Unit.FailLoad();
            var failed = 0;

            this.interstitial.ShowInterstitial("test", null, () => failed++);

            Assert.That(failed, Is.EqualTo(1));
            Assert.That(this.Unit.LoadCalls, Is.EqualTo(1));
            Assert.That(this.rig.Scheduler.Pending.Count, Is.EqualTo(1));
        }

        [Test]
        public void ExplicitLoadDuringTheWait_LoadsNow_AndCancelsThePendingRetry()
        {
            this.Unit.FailLoad();

            this.interstitial.Load();
            this.rig.Scheduler.RunPending();

            Assert.That(this.Unit.LoadCalls, Is.EqualTo(2));
            Assert.That(this.rig.Scheduler.Pending, Is.Empty);
        }

        [Test]
        public void LoadFailureAfterADisplayFailure_StillBacksOff()
        {
            this.Unit.CompleteLoad();
            this.interstitial.ShowInterstitial("test");
            this.Unit.FailDisplay();
            Assert.That(this.Unit.LoadCalls, Is.EqualTo(2), "display failure loads the next ad");

            this.Unit.FailLoad();

            Assert.That(this.Unit.LoadCalls, Is.EqualTo(2));
            Assert.That(this.rig.Scheduler.Pending.Single().Delay, Is.EqualTo(2f));
        }

        [Test]
        public void RewardedLoadFailure_BacksOffTheSameWay()
        {
            var rewarded = this.rig.NewRewarded();
            rewarded.Initialize();
            var unit = this.rig.Sdk.Rewardeds.Single();

            unit.FailLoad();
            this.rig.Scheduler.RunPending();
            unit.FailLoad();

            Assert.That(this.rig.Scheduler.Scheduled.Select(e => e.Delay), Is.EqualTo(new[] { 2f, 4f }));
            Assert.That(unit.LoadCalls, Is.EqualTo(2));
        }

        [Test]
        public void Dispose_CancelsThePendingRetry_AndDestroysTheUnit()
        {
            this.Unit.FailLoad();

            this.interstitial.Dispose();

            Assert.That(this.rig.Scheduler.Pending, Is.Empty);
            Assert.That(this.Unit.Disposed, Is.True);
        }
    }
    #endif
}
