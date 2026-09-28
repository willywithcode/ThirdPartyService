namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using System.Linq;
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;

    // One SDK initialization per app lifetime, from the settings' app key, retried with backoff on
    // failure; no ad object exists before it succeeds.
    public class LevelPlaySdkSessionTests
    {
        private AdsTestRig rig;

        [SetUp]
        public void SetUp() => this.rig = new AdsTestRig(initialized: false);

        [Test]
        public void Start_InitializesOnceWithTheSettingsAppKey()
        {
            this.rig.Session.Start();
            this.rig.Session.Start();

            Assert.That(this.rig.Sdk.InitAppKeys, Is.EqualTo(new[] { "app-key" }));
            Assert.That(this.rig.Session.IsInitialized, Is.False);

            this.rig.Sdk.SucceedInit();

            Assert.That(this.rig.Session.IsInitialized, Is.True);
        }

        [Test]
        public void WrapperInitializedBeforeTheSdk_CreatesNoAdUntilTheSdkSucceeds()
        {
            var interstitial = this.rig.NewInterstitial();
            var rewarded     = this.rig.NewRewarded();
            var banner       = this.rig.NewBanner();
            interstitial.Initialize();
            rewarded.Initialize();
            banner.Initialize();
            this.rig.Session.Start();

            Assert.That(this.rig.Sdk.Interstitials, Is.Empty);
            Assert.That(this.rig.Sdk.Rewardeds, Is.Empty);
            Assert.That(this.rig.Sdk.Banners, Is.Empty);

            this.rig.Sdk.SucceedInit();

            Assert.That(this.rig.Sdk.Interstitials.Single().LoadCalls, Is.EqualTo(1));
            Assert.That(this.rig.Sdk.Rewardeds.Single().LoadCalls, Is.EqualTo(1));
            Assert.That(this.rig.Sdk.Banners.Single().LoadCalls, Is.EqualTo(1));
        }

        [Test]
        public void ShowWhileTheSdkIsNotInitialized_FailsCleanly()
        {
            var interstitial = this.rig.NewInterstitial();
            var rewarded     = this.rig.NewRewarded();
            interstitial.Initialize();
            rewarded.Initialize();
            var interstitialFailed = 0;
            bool? reward           = null;

            interstitial.ShowInterstitial("test", null, () => interstitialFailed++);
            rewarded.ShowAd(r => reward = r, "test");

            Assert.That(interstitialFailed, Is.EqualTo(1));
            Assert.That(reward, Is.False);
            Assert.That(interstitial.IsInterstitialReady(), Is.False);
            Assert.That(rewarded.IsAdReady(), Is.False);
        }

        [Test]
        public void InitFailure_RetriesAfterBackoff_NotAtOnce()
        {
            this.rig.Session.Start();

            this.rig.Sdk.FailInit("no network");

            Assert.That(this.rig.Sdk.InitAppKeys.Count, Is.EqualTo(1));
            Assert.That(this.rig.Scheduler.Pending.Single().Delay, Is.EqualTo(2f));

            this.rig.Scheduler.RunPending();
            this.rig.Sdk.FailInit("no network");

            Assert.That(this.rig.Sdk.InitAppKeys.Count, Is.EqualTo(2));
            Assert.That(this.rig.Scheduler.Pending.Single().Delay, Is.EqualTo(4f));

            this.rig.Scheduler.RunPending();
            this.rig.Sdk.SucceedInit();

            Assert.That(this.rig.Session.IsInitialized, Is.True);
            Assert.That(this.rig.Scheduler.Pending, Is.Empty);
        }

        [Test]
        public void MissingAppKey_NeverCallsTheSdk()
        {
            this.rig.Settings.Current.appKey = "";

            this.rig.Session.Start();

            Assert.That(this.rig.Sdk.InitAppKeys, Is.Empty);
            Assert.That(this.rig.Count(AdFormat.Sdk, AdEventKind.InitFailed), Is.EqualTo(1));
        }

        [Test]
        public void MissingSettings_NeverCallsTheSdk()
        {
            this.rig.Settings.Current = null;

            this.rig.Session.Start();

            Assert.That(this.rig.Sdk.InitAppKeys, Is.Empty);
        }

        [Test]
        public void Setup_InitializesEveryWrapper_AndStartsTheSdkInTheEditor()
        {
            var interstitial = this.rig.NewInterstitial();
            var rewarded     = this.rig.NewRewarded();
            var banner       = this.rig.NewBanner();
            var mrec         = this.rig.NewMrec();
            var setup        = new Setup(this.rig.Session, this.rig.Settings, banner, mrec, interstitial, rewarded);

            setup.Start();
            this.rig.Sdk.SucceedInit();

            Assert.That(this.rig.Sdk.InitAppKeys, Is.EqualTo(new[] { "app-key" }));
            Assert.That(this.rig.Sdk.Interstitials.Count, Is.EqualTo(1));
            Assert.That(this.rig.Sdk.Rewardeds.Count, Is.EqualTo(1));
            Assert.That(this.rig.Sdk.Banners.Count, Is.EqualTo(2), "banner and MREC");
        }
    }
    #endif
}
