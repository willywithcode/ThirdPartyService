namespace ThirdPartyService.ServiceImplementation.ConsentService
{
    using System;
    using Cysharp.Threading.Tasks;
    using ThirdPartyService.Core.ConsentService;
    using UnityEngine;

    // Native calls are isolated so the flow can be exercised without an Android device.
    public interface IConsentBridge
    {
        void RequestConsentInfoUpdate(Action onSuccess, Action onFailure);
        void LoadAndShowConsentFormIfRequired(Action onComplete);
        void ShowPrivacyOptionsForm(Action onComplete);
        bool IsPrivacyOptionsRequired { get; }
    }

    public interface IConsentAppIdProvider
    {
        string AppId { get; }
    }

    public interface ICcpaConsentReader
    {
        // true: opted out of sale/share; false: explicitly did not opt out; null: unknown.
        bool? ReadOptOut();
    }

    public interface IConsentUpdates
    {
        event Action PrivacyOptionsCompleted;
    }

    public sealed class ConsentService : IConsentService, IConsentUpdates
    {
        private readonly IConsentBridge bridge;
        private readonly IConsentAppIdProvider appId;
        private readonly Func<UniTask> infoUpdateTimeout;
        private UniTaskCompletionSource gatherCompletion;

        public ConsentService(IConsentBridge bridge, IConsentAppIdProvider appId)
            : this(bridge, appId, () => UniTask.Delay(TimeSpan.FromSeconds(5), DelayType.Realtime)) { }

        // A supplied timer makes timeout behavior deterministic in EditMode.
        public ConsentService(IConsentBridge bridge, IConsentAppIdProvider appId, Func<UniTask> infoUpdateTimeout)
        {
            this.bridge = bridge;
            this.appId = appId;
            this.infoUpdateTimeout = infoUpdateTimeout;
        }

        public bool IsGathered { get; private set; }
        public bool IsPrivacyOptionsRequired { get; private set; }
        public bool HasCurrentConsent { get; private set; }
        public event Action PrivacyOptionsCompleted;

        public UniTask GatherConsentAsync()
        {
            if (this.gatherCompletion != null) return this.gatherCompletion.Task;
            this.gatherCompletion = new UniTaskCompletionSource();
            this.RunFlowAsync().Forget();
            return this.gatherCompletion.Task;
        }

        private async UniTaskVoid RunFlowAsync()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(this.appId.AppId)) return;

                var update = new UniTaskCompletionSource<bool>();
                this.bridge.RequestConsentInfoUpdate(() => update.TrySetResult(true), () => update.TrySetResult(false));
                var (winner, succeeded) = await UniTask.WhenAny(update.Task, this.infoUpdateTimeout());
                if (!winner || !succeeded) return;

                var form = new UniTaskCompletionSource();
                this.bridge.LoadAndShowConsentFormIfRequired(() => form.TrySetResult());
                // The player controls how long the form stays open; it must have no timer.
                await form.Task;
                this.HasCurrentConsent = true;
                this.IsPrivacyOptionsRequired = this.bridge.IsPrivacyOptionsRequired;
            }
            catch (Exception error)
            {
                Debug.LogWarning($"Consent unavailable this launch: {error.Message}");
            }
            finally
            {
                this.IsGathered = true;
                this.gatherCompletion.TrySetResult();
            }
        }

        public async UniTask ShowPrivacyOptionsAsync()
        {
            if (!this.IsPrivacyOptionsRequired) return;
            try
            {
                var shown = new UniTaskCompletionSource();
                this.bridge.ShowPrivacyOptionsForm(() => shown.TrySetResult());
                await shown.Task;
                this.IsPrivacyOptionsRequired = this.bridge.IsPrivacyOptionsRequired;
                this.PrivacyOptionsCompleted?.Invoke();
            }
            catch (Exception error)
            {
                Debug.LogWarning($"Privacy options unavailable: {error.Message}");
            }
        }
    }

    public sealed class DummyConsentService : IConsentService, ICcpaConsentReader
    {
        public bool IsGathered { get; private set; }
        public bool IsPrivacyOptionsRequired => false;
        public UniTask GatherConsentAsync() { this.IsGathered = true; return UniTask.CompletedTask; }
        public UniTask ShowPrivacyOptionsAsync() => UniTask.CompletedTask;
        public bool? ReadOptOut() => null;
    }
}
