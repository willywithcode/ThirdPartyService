namespace ThirdPartyService.ServiceImplementation.AdsService.Fsn
{
    using System;
    using UnityEngine;
#if UNITY_ANDROID && !UNITY_EDITOR
    using Cysharp.Threading.Tasks;
    using UnityEngine.Scripting;
#endif

    /// <summary>
    /// Drives <c>com.ads.fsnforunity.fsnlib.FsnAd</c> from Plugins/Android/FsnForUnity.androidlib.
    /// The library calls back on Android threads; every callback is posted to the Unity main thread.
    /// Off an Android device a unit never has an ad.
    /// </summary>
    public sealed class AndroidFsnAdUnit : IFsnAdUnit
    {
        public string AdUnitId { get; }

#pragma warning disable CS0067 // Never raised off Android.
        public event Action<string>                 LoadCompleted;
        public event Action<string, double, string> Paid;
#pragma warning restore CS0067

#if UNITY_ANDROID && !UNITY_EDITOR
        private const string FsnAdClass = "com.ads.fsnforunity.fsnlib.FsnAd";

        // Google's sample native ad unit: development builds request it instead of the game's, so
        // testing never counts as invalid traffic.
        private const string TestNativeAdUnit = "ca-app-pub-3940256099942544/2247696110";

        private static bool mobileAdsInitialized;

        private readonly AndroidJavaObject ad;

        public AndroidFsnAdUnit(string adUnitId, FsnSettings settings)
        {
            this.AdUnitId = adUnitId;
            InitializeMobileAds();
            this.ad = new AndroidJavaObject(FsnAdClass);
            this.ad.Call("Init", Debug.isDebugBuild ? TestNativeAdUnit : adUnitId, settings.closeAfterSeconds, settings.reloadAfterClose, settings.startMuted, settings.closeWhenAdClicked);
            this.ad.Call("SetFsnListener", new LoadListener(this));
        }

        public bool IsReady   => this.ad.Call<bool>("IsAdReady");
        public bool IsLoading => this.ad.Call<bool>("IsAdLoading");

        public void Load()
        {
            using var activity = CurrentActivity();
            this.ad.Call("LoadAd", activity);
        }

        public void Show(Action<string> onCompleted)
        {
            using var activity = CurrentActivity();
            this.ad.Call("ShowAd", activity, new CompletedListener(onCompleted));
        }

        private static AndroidJavaObject CurrentActivity()
        {
            using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            return player.GetStatic<AndroidJavaObject>("currentActivity");
        }

        // FsnAd loads without it, but mediation adapters only start through MobileAds.initialize.
        // Repeated calls are harmless, so the Google Mobile Ads Unity plugin may call it too.
        private static void InitializeMobileAds()
        {
            if (mobileAdsInitialized) return;
            mobileAdsInitialized = true;
            using var activity  = CurrentActivity();
            using var mobileAds = new AndroidJavaClass("com.google.android.gms.ads.MobileAds");
            mobileAds.CallStatic("initialize", activity);
        }

        private sealed class LoadListener : AndroidJavaProxy
        {
            private readonly AndroidFsnAdUnit unit;

            public LoadListener(AndroidFsnAdUnit unit) : base("com.ads.fsnforunity.fsnlib.OnFsnLoadListener") => this.unit = unit;

            [Preserve]
            public void OnLoadingStarted() { }

            [Preserve]
            public void OnLoadingCompleted(int errorCode, string errorMessage)
            {
                var error = errorCode == 0 ? null : $"{errorCode} {errorMessage}";
                UniTask.Post(() => this.unit.LoadCompleted?.Invoke(error));
            }

            [Preserve]
            public void OnAdPaid(string adSource, string adUnitId, double valueMicros, string currencyCode)
            {
                UniTask.Post(() => this.unit.Paid?.Invoke(adSource, valueMicros / 1_000_000d, currencyCode));
            }
        }

        private sealed class CompletedListener : AndroidJavaProxy
        {
            private readonly Action<string> onCompleted;

            public CompletedListener(Action<string> onCompleted) : base("com.ads.fsnforunity.fsnlib.OnFsnCompletedListener") => this.onCompleted = onCompleted;

            [Preserve]
            public void OnAdCompleted(string errorMessage) => UniTask.Post(() => this.onCompleted?.Invoke(errorMessage ?? string.Empty));
        }
#else
        public AndroidFsnAdUnit(string adUnitId, FsnSettings settings) => this.AdUnitId = adUnitId;

        public bool IsReady   => false;
        public bool IsLoading => false;

        public void Load() { }

        public void Show(Action<string> onCompleted) => onCompleted?.Invoke("FSN ads need an Android device");
#endif
    }
}
