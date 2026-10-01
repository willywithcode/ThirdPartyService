namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using System.Collections.Generic;
    using System.Linq;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using NUnit.Framework;
    using ThirdPartyService.Core.AdsService.AOA;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using ThirdPartyService.Core.AdsService.MRECAds;
    using ThirdPartyService.Core.AdsService.NativeAds;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using ThirdPartyService.ServiceImplementation.AdsService.DummyAds.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.DummyAds.RewardedAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.RewardedAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LocalDatas;
    using AdsService = ThirdPartyService.ServiceImplementation.AdsService.AdsService;

    // What the ThirdPartyService AdsService aggregator does with the LevelPlay wrappers next to a
    // second provider - the Dummies stand in for one here. Both waterfall formats take the
    // highest-priority READY service, so an unready LevelPlay falls through for interstitial AND for
    // rewarded. A LevelPlay close reaches the caller as onShowSuccess and a show failure as
    // onShowFail, now that AdsService.ShowInterstitialAd hands its callbacks over in
    // ShowInterstitial(where, onAdClosed, onAdFailedToShow) order.
    public class AdsServiceWithLevelPlayTests
    {
        private AdsTestRig               rig;
        private LevelPlayInterstitialAds interstitial;
        private LevelPlayRewardedAds     rewarded;
        private AdsService               adsService;
        private int                      onShowFail;
        private int                      onShowSuccess;

        [SetUp]
        public void SetUp()
        {
            this.rig          = new AdsTestRig();
            this.interstitial = this.rig.NewInterstitial();
            this.rewarded     = this.rig.NewRewarded();
            this.interstitial.Initialize();
            this.rewarded.Initialize();

            var localData = new AdsLocalDataService();
            localData.Data.IsRemovedAds = false; // in memory only; whatever the editor saved is untouched

            this.adsService = new AdsService(
                localData,
                new List<IAOAAdsService>(),
                new List<IBannerAdsService>(),
                new List<IInterstitialAdsService> { new DummyInterstitialAds(), this.interstitial },
                new List<IMRECAdsService>(),
                new List<INativeAdsService>(),
                new List<IRewardedAdsService> { new DummyRewardedAds(), this.rewarded },
                new SignalBus());
            this.onShowFail    = 0;
            this.onShowSuccess = 0;
        }

        private FakeFullscreenAdUnit InterstitialUnit => this.rig.Sdk.Interstitials.Single();

        private void ShowInterstitialThroughAdsService() =>
            this.adsService.ShowInterstitialAd("test", () => this.onShowFail++, () => this.onShowSuccess++);

        [Test]
        public void UnreadyLevelPlayInterstitial_FallsThroughToTheDummy()
        {
            this.ShowInterstitialThroughAdsService();

            Assert.That(this.InterstitialUnit.ShowPlacements, Is.Empty, "LevelPlay was skipped");
            // The Dummy "closes" at once, which the caller receives as a successful show.
            Assert.That(this.onShowSuccess, Is.EqualTo(1));
            Assert.That(this.onShowFail, Is.EqualTo(0));
        }

        [Test]
        public void ReadyLevelPlayInterstitial_IsChosen_AndItsCloseArrivesAsOnShowSuccess()
        {
            this.InterstitialUnit.CompleteLoad();

            this.ShowInterstitialThroughAdsService();
            this.InterstitialUnit.Display();
            this.InterstitialUnit.Close();

            Assert.That(this.InterstitialUnit.ShowPlacements, Is.EqualTo(new[] { "test" }));
            Assert.That(this.onShowSuccess, Is.EqualTo(1));
            Assert.That(this.onShowFail, Is.EqualTo(0));
        }

        [Test]
        public void ReadyLevelPlayInterstitial_ItsDisplayFailureArrivesAsOnShowFail()
        {
            this.InterstitialUnit.CompleteLoad();

            this.ShowInterstitialThroughAdsService();
            this.InterstitialUnit.FailDisplay();

            Assert.That(this.onShowFail, Is.EqualTo(1));
            Assert.That(this.onShowSuccess, Is.EqualTo(0));
        }

        [Test]
        public void UnreadyLevelPlayRewarded_FallsThroughToTheOtherProvider()
        {
            var unit    = this.rig.Sdk.Rewardeds.Single();
            var results = new List<bool>();

            this.adsService.ShowRewardedAd(results.Add, "test");

            Assert.That(unit.ShowPlacements, Is.Empty, "LevelPlay had no ad, so it was skipped");
            // The stand-in provider is always ready and pays at once.
            Assert.That(results, Is.EqualTo(new[] { true }));
        }

        [Test]
        public void ReadyLevelPlayRewarded_IsChosenOverTheOtherProvider()
        {
            var unit    = this.rig.Sdk.Rewardeds.Single();
            var results = new List<bool>();
            unit.CompleteLoad();

            this.adsService.ShowRewardedAd(results.Add, "test");

            Assert.That(unit.ShowPlacements, Is.EqualTo(new[] { "test" }), "LevelPlay outranks the other provider when both are ready");
            Assert.That(results, Is.Empty, "the attempt is still running: nothing is paid until LevelPlay rewards");
        }

        [Test]
        public void Rewarded_ThroughAdsService_GrantsOnlyOnLevelPlaysReward()
        {
            var unit    = this.rig.Sdk.Rewardeds.Single();
            var results = new List<bool>();
            unit.CompleteLoad();

            this.adsService.ShowRewardedAd(results.Add, "test");
            unit.Display();
            unit.Close();
            unit.Reward();

            Assert.That(results, Is.EqualTo(new[] { true }));
        }
    }
    #endif
}
