namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using System.Linq;
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.InterstitialsAds;

    // IInterstitialAdsService as LevelPlay serves it: callbacks keep the interface's own parameter
    // meaning (onAdClosed on close, onAdFailedToShow on failure), and the next ad is always on its way.
    public class LevelPlayInterstitialAdsTests
    {
        private AdsTestRig               rig;
        private LevelPlayInterstitialAds interstitial;
        private int                      closed;
        private int                      failed;

        [SetUp]
        public void SetUp()
        {
            this.rig          = new AdsTestRig();
            this.interstitial = this.rig.NewInterstitial();
            this.closed       = 0;
            this.failed       = 0;
        }

        private FakeFullscreenAdUnit Unit => this.rig.Sdk.Interstitials.Single();

        private void Show() => this.interstitial.ShowInterstitial("test", () => this.closed++, () => this.failed++);

        [Test]
        public void Initialize_CreatesTheInterstitialOnTheSettingsAdUnit_AndStartsLoading()
        {
            this.interstitial.Initialize();

            Assert.That(this.Unit.AdUnitId, Is.EqualTo("interstitial-unit"));
            Assert.That(this.Unit.LoadCalls, Is.EqualTo(1));
            Assert.That(this.interstitial.IsInitialized(), Is.True);
        }

        [Test]
        public void ShowBeforeReady_FailsThroughOnAdFailedToShow_AndNeverReachesTheSdk()
        {
            this.interstitial.Initialize();

            this.Show();

            Assert.That(this.failed, Is.EqualTo(1));
            Assert.That(this.closed, Is.EqualTo(0));
            Assert.That(this.Unit.ShowPlacements, Is.Empty);
        }

        [Test]
        public void ShowBeforeInitialize_FailsThroughOnAdFailedToShow()
        {
            this.Show();

            Assert.That(this.failed, Is.EqualTo(1));
            Assert.That(this.rig.Sdk.Interstitials, Is.Empty);
        }

        [Test]
        public void ShowWhenReady_ShowsOnTheSdkWithThePlacement()
        {
            this.interstitial.Initialize();
            this.Unit.CompleteLoad();

            this.Show();

            Assert.That(this.Unit.ShowPlacements, Is.EqualTo(new[] { "test" }));
            Assert.That(this.closed + this.failed, Is.EqualTo(0), "nothing completes until the SDK reports back");
        }

        [Test]
        public void Close_InvokesOnlyOnAdClosed_AndLoadsTheNextAd()
        {
            this.interstitial.Initialize();
            this.Unit.CompleteLoad();
            this.Show();
            this.Unit.Display();

            this.Unit.Close();

            Assert.That(this.closed, Is.EqualTo(1));
            Assert.That(this.failed, Is.EqualTo(0));
            Assert.That(this.Unit.LoadCalls, Is.EqualTo(2));
        }

        [Test]
        public void DisplayFailure_InvokesOnlyOnAdFailedToShow_AndLoadsTheNextAd()
        {
            this.interstitial.Initialize();
            this.Unit.CompleteLoad();
            this.Show();

            this.Unit.FailDisplay();

            Assert.That(this.failed, Is.EqualTo(1));
            Assert.That(this.closed, Is.EqualTo(0));
            Assert.That(this.Unit.LoadCalls, Is.EqualTo(2));
        }

        [Test]
        public void SecondShowWhileOneIsOnScreen_FailsAtOnce_AndLeavesTheFirstShowsCallbacksAlone()
        {
            this.interstitial.Initialize();
            this.Unit.CompleteLoad();
            this.Show();
            var secondFailed = 0;

            this.interstitial.ShowInterstitial("again", null, () => secondFailed++);
            this.Unit.Display();
            this.Unit.Close();

            Assert.That(secondFailed, Is.EqualTo(1));
            Assert.That(this.closed, Is.EqualTo(1));
            Assert.That(this.Unit.ShowPlacements, Is.EqualTo(new[] { "test" }));
        }

        [Test]
        public void Click_IsLogged()
        {
            this.interstitial.Initialize();
            this.Unit.CompleteLoad();
            this.Show();
            this.Unit.Display();

            this.Unit.Click();

            Assert.That(this.rig.Count(AdFormat.Interstitial, AdEventKind.Clicked), Is.EqualTo(1));
            Assert.That(this.rig.Count(AdFormat.Interstitial, AdEventKind.Shown), Is.EqualTo(1));
        }

        [Test]
        public void Priority_OutranksTheThirdPartyServiceDummy()
        {
            var dummy = new ThirdPartyService.ServiceImplementation.AdsService.DummyAds.InterstitialsAds.DummyInterstitialAds();

            Assert.That(this.interstitial.GetPriority(), Is.GreaterThan(dummy.GetPriority()));
        }
    }
    #endif
}
