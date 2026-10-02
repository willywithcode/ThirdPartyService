namespace ThirdPartyService.Tests.EditMode.Admob
{
    #if Admob
    using System.Collections.Generic;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using NUnit.Framework;
    using ThirdPartyService.Core.AdsService.Signals;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.RewardedAds;
    using ThirdPartyService.Tests.EditMode.AdsService;
    using UnityEngine;
    using Object = UnityEngine.Object;

    // An AdMob format with no ad unit id in the blueprint - the state the repository ships in until
    // the owner pastes the console values. Two things must hold, because the waterfall depends on
    // them: such a provider never claims to be ready, so the aggregator passes it over and the next
    // network gets the request; and a show attempted anyway still answers its caller, because
    // AdsGate awaits that answer before it unlocks a popup or loads the next level.
    //
    // A configured format cannot be exercised here: the wrappers reach GMA's static
    // InterstitialAd.Load / RewardedAd.Load, which need the native SDK. The loaded-and-shown paths
    // are proven on a device instead.
    public class AdmobUnconfiguredFormatTests
    {
        private AdmobSetting          setting;
        private SignalBus             signals;
        private AdmobInterstitialsAds interstitial;
        private AdmobRewardedAds      rewarded;
        private AdmobBannerAds        banner;

        [SetUp]
        public void SetUp()
        {
            this.setting = ScriptableObject.CreateInstance<AdmobSetting>();
            var blueprints = new AdmobSettingBlueprintService(new SingleAssetsManager(nameof(AdmobSetting), this.setting));
            blueprints.Initialize();
            this.signals      = new SignalBus();
            this.interstitial = new AdmobInterstitialsAds(blueprints, new SignalBus());
            this.rewarded     = new AdmobRewardedAds(blueprints, new SignalBus());
            this.banner       = new AdmobBannerAds(blueprints, this.signals);
        }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(this.setting);

        [Test]
        public void UnconfiguredInterstitial_InitializesWithoutTouchingTheSdk_AndIsNeverReady()
        {
            this.interstitial.Initialize();

            Assert.That(this.interstitial.IsInitialized(), Is.False);
            Assert.That(this.interstitial.IsInterstitialReady(), Is.False);
        }

        [Test]
        public void UnconfiguredInterstitial_FailsItsShowAtOnce_SoTheCallerNeverWaits()
        {
            this.interstitial.Initialize();
            var closed = 0;
            var failed = 0;

            this.interstitial.ShowInterstitial("test", () => closed++, () => failed++);

            Assert.That(failed, Is.EqualTo(1), "exactly one of the two callbacks must fire");
            Assert.That(closed, Is.Zero);
        }

        [Test]
        public void UnconfiguredRewarded_InitializesWithoutTouchingTheSdk_AndIsNeverReady()
        {
            this.rewarded.Initialize();

            Assert.That(this.rewarded.IsAdReady(), Is.False);
        }

        [Test]
        public void UnconfiguredRewarded_CompletesFalseAtOnce_SoThePopupUnlocks()
        {
            this.rewarded.Initialize();
            var results = new List<bool>();

            this.rewarded.ShowAd(results.Add, "test");

            Assert.That(results, Is.EqualTo(new[] { false }));
        }

        [Test]
        public void UnconfiguredBanner_InitializesWithoutTouchingTheSdk_AndNeverReportsLoaded()
        {
            var changes = 0;
            this.banner.BannerLoadStateChanged += () => changes++;

            this.banner.Initialize();

            Assert.That(this.banner.IsInitialized(), Is.False);
            Assert.That(this.banner.IsBannerLoaded(), Is.False, "the aggregator must keep the other network's banner");
            Assert.That(changes, Is.Zero);
        }

        [Test]
        public void UnconfiguredBanner_ShowAndHide_DoNothing_AndTakeNoRoom()
        {
            var seen = new List<OnBannerVisibilityChangedSignal>();
            this.signals.Subscribe<OnBannerVisibilityChangedSignal>(seen.Add);
            this.banner.Initialize();

            this.banner.ShowBanner();
            Assert.That(this.banner.GetBannerHeight(), Is.Zero);
            this.banner.HideBanner();

            Assert.That(seen, Is.Empty, "the gameplay bar must not make room for a banner that is not there");
            Assert.That(this.banner.IsShown(), Is.False);
        }

        [Test]
        public void Priorities_ComeFromTheBlueprint_SoTheyMoveWithoutABuild()
        {
            this.setting.priorityInterstitial = 5;
            this.setting.priorityRewarded     = 5;
            this.setting.priorityBanner       = 5;

            Assert.That(this.interstitial.GetPriority(), Is.EqualTo(5));
            Assert.That(this.rewarded.GetPriority(), Is.EqualTo(5));
            Assert.That(this.banner.GetPriority(), Is.EqualTo(5));
        }
    }
    #endif
}
