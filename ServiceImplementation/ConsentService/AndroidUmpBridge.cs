namespace ThirdPartyService.ServiceImplementation.ConsentService
{
    #if UNITY_ANDROID && !UNITY_EDITOR
    using System;
    using System.Threading;
    using UnityEngine;

    public sealed class AndroidUmpBridge : IConsentBridge
    {
        private readonly SynchronizationContext mainThread = SynchronizationContext.Current;
        private AndroidJavaProxy successProxy;
        private AndroidJavaProxy failureProxy;
        private AndroidJavaProxy formProxy;
        private AndroidJavaObject consentInformation;

        private static AndroidJavaObject Activity => new AndroidJavaClass("com.unity3d.player.UnityPlayer")
            .GetStatic<AndroidJavaObject>("currentActivity");

        private void OnMain(Action action) => this.mainThread.Post(_ => action(), null);

        public void RequestConsentInfoUpdate(Action onSuccess, Action onFailure)
        {
            using var activity = Activity;
            using var ump = new AndroidJavaClass("com.google.android.ump.UserMessagingPlatform");
            this.consentInformation = ump.CallStatic<AndroidJavaObject>("getConsentInformation", activity);
            using var builder = new AndroidJavaObject("com.google.android.ump.ConsentRequestParameters$Builder");
            builder.Call<AndroidJavaObject>("setTagForUnderAgeOfConsent", false);
            using var parameters = builder.Call<AndroidJavaObject>("build");
            this.successProxy = new SuccessProxy(() => this.OnMain(onSuccess));
            this.failureProxy = new FailureProxy(() => this.OnMain(onFailure));
            this.consentInformation.Call("requestConsentInfoUpdate", activity, parameters, this.successProxy, this.failureProxy);
        }

        public void LoadAndShowConsentFormIfRequired(Action onComplete) => this.Show("loadAndShowConsentFormIfRequired", onComplete);
        public void ShowPrivacyOptionsForm(Action onComplete) => this.Show("showPrivacyOptionsForm", onComplete);

        private void Show(string method, Action onComplete)
        {
            using var activity = Activity;
            using var ump = new AndroidJavaClass("com.google.android.ump.UserMessagingPlatform");
            this.formProxy = new FormProxy(() => this.OnMain(onComplete));
            ump.CallStatic(method, activity, this.formProxy);
        }

        public bool IsPrivacyOptionsRequired
        {
            get
            {
                if (this.consentInformation == null) return false;
                using var status = this.consentInformation.Call<AndroidJavaObject>("getPrivacyOptionsRequirementStatus");
                return status.Call<string>("name") == "REQUIRED";
            }
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
