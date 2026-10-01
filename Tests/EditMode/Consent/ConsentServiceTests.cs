namespace ThirdPartyService.Tests.EditMode.Consent
{
    using System;
    using System.Threading.Tasks;
    using Cysharp.Threading.Tasks;
    using NUnit.Framework;
    using ThirdPartyService.Core.ConsentService;
    using ThirdPartyService.ServiceImplementation.ConsentService;
    using UnityEngine;
    using VContainer;

    public class ConsentServiceTests
    {
        private sealed class AppIdProvider : IConsentAppIdProvider
        {
            public string AppId { get; set; } = "ca-app-pub-test~test";
        }

        private sealed class Bridge : IConsentBridge
        {
            public int UpdateCalls;
            public int FormCalls;
            public int PrivacyCalls;
            public bool IsPrivacyOptionsRequired { get; set; }
            public Action UpdateSuccess;
            public Action UpdateFailure;
            public Action FormComplete;
            public Action PrivacyComplete;

            public void RequestConsentInfoUpdate(Action success, Action failure)
            {
                this.UpdateCalls++;
                this.UpdateSuccess = success;
                this.UpdateFailure = failure;
            }
            public void LoadAndShowConsentFormIfRequired(Action complete) { this.FormCalls++; this.FormComplete = complete; }
            public void ShowPrivacyOptionsForm(Action complete) { this.PrivacyCalls++; this.PrivacyComplete = complete; }
        }

        // The Android registration resolves ConsentService through VContainer. With two public
        // constructors and none marked, VContainer took the timer overload and failed on device
        // (No such registration of type Func<UniTask>), which stopped the Loading scene.
        [Test]
        public void TheAndroidRegistration_ResolvesThroughVContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance<IConsentAppIdProvider>(new AppIdProvider());
            builder.RegisterInstance<IConsentBridge>(new Bridge());
            builder.Register<ConsentService>(Lifetime.Singleton).AsSelf().As<IConsentService>();

            using var container = builder.Build();

            Assert.That(container.Resolve<IConsentService>(), Is.InstanceOf<ConsentService>());
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task SuccessfulFlow_WaitsForForm_AndReportsPrivacyRequirement(bool required)
        {
            var bridge = new Bridge { IsPrivacyOptionsRequired = required };
            var service = new ConsentService(bridge, new AppIdProvider());
            var gathering = service.GatherConsentAsync().AsTask();
            bridge.UpdateSuccess();
            Assert.That(bridge.FormCalls, Is.EqualTo(1));
            Assert.That(service.IsGathered, Is.False);
            bridge.FormComplete();
            await gathering;
            Assert.That(service.IsGathered, Is.True);
            Assert.That(service.IsPrivacyOptionsRequired, Is.EqualTo(required));

            var privacy = service.ShowPrivacyOptionsAsync().AsTask();
            Assert.That(bridge.PrivacyCalls, Is.EqualTo(required ? 1 : 0));
            if (required) bridge.PrivacyComplete();
            await privacy;
        }

        [Test]
        public async Task Error_FinishesUnknown_WithoutShowingForm()
        {
            var bridge = new Bridge { IsPrivacyOptionsRequired = true };
            var service = new ConsentService(bridge, new AppIdProvider());
            var gathering = service.GatherConsentAsync().AsTask();
            bridge.UpdateFailure();
            await gathering;
            Assert.That(service.IsGathered, Is.True);
            Assert.That(service.IsPrivacyOptionsRequired, Is.False);
            Assert.That(bridge.FormCalls, Is.Zero);
        }

        [Test]
        public async Task Timeout_FinishesUnknown_AndIgnoresLateCallback()
        {
            var bridge = new Bridge();
            var service = new ConsentService(bridge, new AppIdProvider(), () => UniTask.CompletedTask);
            await service.GatherConsentAsync().AsTask();
            bridge.UpdateSuccess();
            Assert.That(service.IsGathered, Is.True);
            Assert.That(bridge.FormCalls, Is.Zero);
        }

        [Test]
        public async Task DefaultTimeout_ExpiresWhileTimeScaleIsPaused()
        {
            var previousTimeScale = Time.timeScale;
            Time.timeScale = 0f;
            try
            {
                var bridge = new Bridge();
                var service = new ConsentService(bridge, new AppIdProvider());
                var gathering = service.GatherConsentAsync().AsTask();
                var finished = await Task.WhenAny(gathering, Task.Delay(TimeSpan.FromSeconds(7)));
                Assert.That(finished, Is.SameAs(gathering), "The 5 s update cap must use real time when the game is paused.");
                await gathering;
                Assert.That(service.IsGathered, Is.True);
                Assert.That(bridge.FormCalls, Is.Zero);
            }
            finally
            {
                Time.timeScale = previousTimeScale;
            }
        }

        // No AdMob App ID yet (the first release ships without one): the flow must finish at once,
        // touch nothing of Google's, and leave Privacy options hidden. Null is a missing asset.
        [TestCase("")]
        [TestCase("   ")]
        [TestCase(null)]
        public async Task EmptyAppId_DoesNotCallBridge(string appId)
        {
            var bridge = new Bridge();
            var service = new ConsentService(bridge, new AppIdProvider { AppId = appId });
            await service.GatherConsentAsync().AsTask();
            Assert.That(service.IsGathered, Is.True);
            Assert.That(service.IsPrivacyOptionsRequired, Is.False);
            Assert.That(service.HasCurrentConsent, Is.False);
            Assert.That(bridge.UpdateCalls, Is.Zero);
            Assert.That(bridge.FormCalls, Is.Zero);

            await service.ShowPrivacyOptionsAsync().AsTask();
            Assert.That(bridge.PrivacyCalls, Is.Zero);
        }

        [Test]
        public async Task RepeatedCalls_ShareOneFlow_AndDoNotRepeatNextTime()
        {
            var bridge = new Bridge();
            var service = new ConsentService(bridge, new AppIdProvider());
            var first = service.GatherConsentAsync().AsTask();
            var second = service.GatherConsentAsync().AsTask();
            Assert.That(bridge.UpdateCalls, Is.EqualTo(1));
            bridge.UpdateSuccess();
            bridge.FormComplete();
            await Task.WhenAll(first, second);
            await service.GatherConsentAsync().AsTask();
            Assert.That(bridge.UpdateCalls, Is.EqualTo(1));
        }

        [Test]
        public async Task EditorDummy_BecomesGathered_AndNeverRequiresPrivacyOptions()
        {
            var service = new DummyConsentService();
            Assert.That(service.IsGathered, Is.False);
            await service.GatherConsentAsync().AsTask();
            await service.ShowPrivacyOptionsAsync().AsTask();
            Assert.That(service.IsGathered, Is.True);
            Assert.That(service.IsPrivacyOptionsRequired, Is.False);
        }
    }
}
