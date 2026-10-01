namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using NUnit.Framework;
    using ThirdPartyService.Core.Analytics;
    using ThirdPartyService.Core.ConsentService;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay;
    using ThirdPartyService.ServiceImplementation.ConsentService;
    using ThirdPartyService.ServiceImplementation.Analytics.Firebase;

    public class LevelPlayConsentRevenueTests
    {
        private sealed class Consent : IConsentService, IConsentUpdates
        {
            private readonly UniTaskCompletionSource completion = new();
            public bool IsGathered { get; private set; }
            public bool IsPrivacyOptionsRequired => false;
            public event System.Action PrivacyOptionsCompleted;
            public UniTask GatherConsentAsync() => this.completion.Task;
            public UniTask ShowPrivacyOptionsAsync() => UniTask.CompletedTask;
            public void Complete() { this.IsGathered = true; this.completion.TrySetResult(); }
            public void CompletePrivacyOptions() => this.PrivacyOptionsCompleted?.Invoke();
        }

        private sealed class Ccpa : ICcpaConsentReader
        {
            public bool? Value;
            public bool? ReadOptOut() => this.Value;
        }

        private sealed class Analytics : IAdRevenueService
        {
            public readonly List<AdImpression> Impressions = new();
            public void SendAdImpression(AdImpression impression) => this.Impressions.Add(impression);
        }

        [TestCase(true, "CCPA:True")]
        [TestCase(false, "CCPA:False")]
        [TestCase(null, null)]
        public async Task Init_WaitsForConsent_ThenSetsKnownPrivacyBeforeInit(bool? optedOut, string ccpaCall)
        {
            var rig = new AdsTestRig(initialized: false);
            var consent = new Consent();
            var setup = new Setup(rig.Session, rig.Settings, rig.NewBanner(), rig.NewMrec(), rig.NewInterstitial(), rig.NewRewarded(),
                consent, new Ccpa { Value = optedOut }, rig.Sdk, new Analytics());
            setup.Start();
            Assert.That(rig.Sdk.InitAppKeys, Is.Empty);
            consent.Complete();
            await Task.Yield();
            Assert.That(rig.Sdk.PrivacyCalls[0], Is.EqualTo("COPPA:False"));
            if (ccpaCall != null) Assert.That(rig.Sdk.PrivacyCalls[1], Is.EqualTo(ccpaCall));
            Assert.That(rig.Sdk.PrivacyCalls[rig.Sdk.PrivacyCalls.Count - 1], Is.EqualTo("Init"));
            Assert.That(rig.Sdk.InitAppKeys, Is.EqualTo(new[] { "app-key" }));
            setup.Dispose();
        }

        // Any call into Google's UMP fails the test.
        private sealed class ForbiddenBridge : IConsentBridge
        {
            public bool IsPrivacyOptionsRequired => throw new AssertionException("UMP was asked for privacy options.");
            public void RequestConsentInfoUpdate(System.Action onSuccess, System.Action onFailure) => throw new AssertionException("UMP was called.");
            public void LoadAndShowConsentFormIfRequired(System.Action onComplete) => throw new AssertionException("UMP was called.");
            public void ShowPrivacyOptionsForm(System.Action onComplete) => throw new AssertionException("UMP was called.");
        }

        private sealed class NoAppId : IConsentAppIdProvider
        {
            public string AppId => "";
        }

        // The first release ships without an AdMob App ID: the real consent service must finish
        // without touching UMP, and LevelPlay must still start, with no CCPA flag.
        [Test]
        public async Task WithoutAnAdMobAppId_LevelPlayStartsWithoutTouchingUmp()
        {
            var rig     = new AdsTestRig(initialized: false);
            var consent = new ConsentService(new ForbiddenBridge(), new NoAppId());
            var setup = new Setup(rig.Session, rig.Settings, rig.NewBanner(), rig.NewMrec(), rig.NewInterstitial(), rig.NewRewarded(),
                consent, new Ccpa { Value = null }, rig.Sdk, new Analytics());

            setup.Start();
            await Task.Yield();

            Assert.That(consent.IsGathered, Is.True);
            Assert.That(rig.Sdk.PrivacyCalls, Is.EqualTo(new[] { "COPPA:False", "Init" }));
            Assert.That(rig.Sdk.InitAppKeys, Is.EqualTo(new[] { "app-key" }));
            setup.Dispose();
        }

        [Test]
        public void ImpressionCallback_ForwardsEachTypedImpression()
        {
            var rig = new AdsTestRig(initialized: false);
            var consent = new Consent();
            var analytics = new Analytics();
            var setup = new Setup(rig.Session, rig.Settings, rig.NewBanner(), rig.NewMrec(), rig.NewInterstitial(), rig.NewRewarded(),
                consent, new Ccpa(), rig.Sdk, analytics);
            setup.Start();
            rig.Sdk.EmitImpression(new AdImpression("network", "rewarded", "unit", 0.12345d));
            Assert.That(analytics.Impressions.Count, Is.EqualTo(1));
            Assert.That(analytics.Impressions[0].Revenue, Is.EqualTo(0.12345d));
            setup.Dispose();
            rig.Sdk.EmitImpression(new AdImpression("other", "banner", "other unit", 0.1d));
            Assert.That(analytics.Impressions.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task PrivacyOptionsChange_UpdatesKnownCcpaChoice()
        {
            var rig = new AdsTestRig(initialized: false);
            var consent = new Consent();
            var ccpa = new Ccpa { Value = true };
            var setup = new Setup(rig.Session, rig.Settings, rig.NewBanner(), rig.NewMrec(), rig.NewInterstitial(), rig.NewRewarded(),
                consent, ccpa, rig.Sdk, new Analytics());
            setup.Start();
            consent.Complete();
            await Task.Yield();
            ccpa.Value = false;
            consent.CompletePrivacyOptions();
            Assert.That(rig.Sdk.PrivacyCalls[rig.Sdk.PrivacyCalls.Count - 1], Is.EqualTo("CCPA:False"));
            setup.Dispose();
        }

        [Test]
        public void FirebaseMapping_KeepsDoubleAndUsd_AndOmitsUnknownValue()
        {
            var mapped = AdImpressionFirebaseParameters.Map(new AdImpression("network", "rewarded", "unit", 0.12345d));
            Assert.That(mapped["ad_platform"], Is.EqualTo("ironSource"));
            Assert.That(mapped["ad_source"], Is.EqualTo("network"));
            Assert.That(mapped["ad_format"], Is.EqualTo("rewarded"));
            Assert.That(mapped["ad_unit_name"], Is.EqualTo("unit"));
            Assert.That(mapped["currency"], Is.EqualTo("USD"));
            Assert.That(mapped["value"], Is.TypeOf<double>().And.EqualTo(0.12345d));
            Assert.That(AdImpressionFirebaseParameters.Map(new AdImpression("network", "banner", "unit", null)).ContainsKey("value"), Is.False);
        }
    }
    #endif
}
