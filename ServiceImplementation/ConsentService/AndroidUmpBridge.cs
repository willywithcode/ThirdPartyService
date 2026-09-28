namespace ThirdPartyService.ServiceImplementation.ConsentService
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;
    using UnityEngine;

    public sealed class AndroidUmpBridge : IConsentBridge
    {
        private SynchronizationContext mainThread;
        private AndroidJavaProxy successProxy;
        private AndroidJavaProxy failureProxy;
        private AndroidJavaProxy formProxy;
        private AndroidJavaObject consentInformation;
        private volatile bool privacyOptionsRequired;

        private static AndroidJavaObject Activity => new AndroidJavaClass("com.unity3d.player.UnityPlayer")
            .GetStatic<AndroidJavaObject>("currentActivity");

        // Public entry points run on Unity's player thread. At early startup there may be no
        // SynchronizationContext, so native callbacks can use the PlayerLoop instead.
        private void CaptureUnityContext() => this.mainThread ??= SynchronizationContext.Current;

        private void OnMain(Action action)
        {
            var context = this.mainThread;
            if (context != null) context.Post(_ => action(), null);
            else RunOnUnityThreadAsync(action).Forget();
        }

        private static async UniTaskVoid RunOnUnityThreadAsync(Action action)
        {
            await UniTask.SwitchToMainThread();
            action();
        }

        private void OnAndroidUiThread(Action<AndroidJavaObject> action, Action onFailure)
        {
            AndroidJavaObject activity = null;
            try
            {
                activity = Activity;
                var capturedActivity = activity;
                activity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    try { action(capturedActivity); }
                    catch (Exception error)
                    {
                        Debug.LogWarning($"UMP call failed: {error.Message}");
                        this.OnMain(onFailure);
                    }
                    finally { capturedActivity.Dispose(); }
                }));
            }
            catch (Exception error)
            {
                activity?.Dispose();
                Debug.LogWarning($"UMP UI dispatch failed: {error.Message}");
                this.OnMain(onFailure);
            }
        }

        public void RequestConsentInfoUpdate(Action onSuccess, Action onFailure)
        {
            this.CaptureUnityContext();
            this.OnAndroidUiThread(activity =>
            {
                using var ump = new AndroidJavaClass("com.google.android.ump.UserMessagingPlatform");
                this.consentInformation = ump.CallStatic<AndroidJavaObject>("getConsentInformation", activity);
                using var builder = new AndroidJavaObject("com.google.android.ump.ConsentRequestParameters$Builder");
                using var configuredBuilder = builder.Call<AndroidJavaObject>("setTagForUnderAgeOfConsent", false);
                using var parameters = builder.Call<AndroidJavaObject>("build");
                this.successProxy = new SuccessProxy(() => this.OnMain(onSuccess));
                this.failureProxy = new FailureProxy(() => this.OnMain(onFailure));
                this.consentInformation.Call("requestConsentInfoUpdate", activity, parameters, this.successProxy, this.failureProxy);
            }, onFailure);
        }

        public void LoadAndShowConsentFormIfRequired(Action onComplete) => this.Show("loadAndShowConsentFormIfRequired", onComplete);
        public void ShowPrivacyOptionsForm(Action onComplete) => this.Show("showPrivacyOptionsForm", onComplete);

        private void Show(string method, Action onComplete)
        {
            this.CaptureUnityContext();
            this.formProxy = new FormProxy(() => this.OnAndroidUiThread(_ =>
            {
                this.privacyOptionsRequired = this.ReadPrivacyOptionsRequired();
                this.OnMain(onComplete);
            }, onComplete));
            this.OnAndroidUiThread(activity =>
            {
                using var ump = new AndroidJavaClass("com.google.android.ump.UserMessagingPlatform");
                ump.CallStatic(method, activity, this.formProxy);
            }, onComplete);
        }

        public bool IsPrivacyOptionsRequired => this.privacyOptionsRequired;

        private bool ReadPrivacyOptionsRequired()
        {
            if (this.consentInformation == null) return false;
            using var status = this.consentInformation.Call<AndroidJavaObject>("getPrivacyOptionsRequirementStatus");
            return status.Call<string>("name") == "REQUIRED";
        }

        private sealed class SuccessProxy : AndroidJavaProxy
        {
            private readonly Action complete;
            public SuccessProxy(Action complete) : base("com.google.android.ump.ConsentInformation$OnConsentInfoUpdateSuccessListener") => this.complete = complete;
            public void onConsentInfoUpdateSuccess() => this.complete();
        }

        private sealed class FailureProxy : AndroidJavaProxy
        {
            private readonly Action complete;
            public FailureProxy(Action complete) : base("com.google.android.ump.ConsentInformation$OnConsentInfoUpdateFailureListener") => this.complete = complete;
            public void onConsentInfoUpdateFailure(AndroidJavaObject error) => this.complete();
        }

        private sealed class FormProxy : AndroidJavaProxy
        {
            private readonly Action complete;
            public FormProxy(Action complete) : base("com.google.android.ump.ConsentForm$OnConsentFormDismissedListener") => this.complete = complete;
            public void onConsentFormDismissed(AndroidJavaObject error) => this.complete();
        }
    }
    #endif
}
