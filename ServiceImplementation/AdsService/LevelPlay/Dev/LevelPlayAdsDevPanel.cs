namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Dev
{
    #if LevelPlay && (DEVELOPMENT_BUILD || UNITY_EDITOR)
    using System;
    using System.Collections.Generic;
    using ThirdPartyService.Core.AdsService;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.MRECAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.MRECAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.RewardedAds;
    using UnityEngine;
    using VContainer.Unity;
    using Object = UnityEngine.Object;

    // Development-only manual trigger for every LevelPlay format. Release builds do not compile it.
    // It hangs off the DI scope, not a game screen, so no game flow can open it; it appears as a
    // small "Ads" button on the left edge that expands into the panel.
    public class LevelPlayAdsDevPanelInstaller : IStartable, IDisposable
    {
        private readonly LevelPlaySdkSession      session;
        private readonly LevelPlayBannerAds       bannerAds;
        private readonly LevelPlayMRECAds         mrecAds;
        private readonly LevelPlayInterstitialAds interstitialAds;
        private readonly LevelPlayRewardedAds     rewardedAds;
        private readonly IAdsService              adsService;
        private readonly AdEventLog               log;

        private GameObject panelObject;

        public LevelPlayAdsDevPanelInstaller(
            LevelPlaySdkSession      session,
            LevelPlayBannerAds       bannerAds,
            LevelPlayMRECAds         mrecAds,
            LevelPlayInterstitialAds interstitialAds,
            LevelPlayRewardedAds     rewardedAds,
            IAdsService              adsService,
            AdEventLog               log
        )
        {
            this.session         = session;
            this.bannerAds       = bannerAds;
            this.mrecAds         = mrecAds;
            this.interstitialAds = interstitialAds;
            this.rewardedAds     = rewardedAds;
            this.adsService      = adsService;
            this.log             = log;
        }

        public void Start()
        {
            this.panelObject = new GameObject(nameof(LevelPlayAdsDevPanel));
            Object.DontDestroyOnLoad(this.panelObject);
            this.panelObject.AddComponent<LevelPlayAdsDevPanel>()
                .Construct(this.session, this.bannerAds, this.mrecAds, this.interstitialAds, this.rewardedAds, this.adsService, this.log);
        }

        public void Dispose()
        {
            if (this.panelObject != null) Object.Destroy(this.panelObject);
        }
    }

    public class LevelPlayAdsDevPanel : MonoBehaviour
    {
        private const int MaxLines = 40;

        private readonly List<string> lines = new();

        private LevelPlaySdkSession      session;
        private LevelPlayBannerAds       bannerAds;
        private LevelPlayMRECAds         mrecAds;
        private LevelPlayInterstitialAds interstitialAds;
        private LevelPlayRewardedAds     rewardedAds;
        private IAdsService              adsService;
        private AdEventLog               log;

        private bool    expanded;
        private Vector2 scroll;

        public void Construct(
            LevelPlaySdkSession      session,
            LevelPlayBannerAds       bannerAds,
            LevelPlayMRECAds         mrecAds,
            LevelPlayInterstitialAds interstitialAds,
            LevelPlayRewardedAds     rewardedAds,
            IAdsService              adsService,
            AdEventLog               log
        )
        {
            this.session         = session;
            this.bannerAds       = bannerAds;
            this.mrecAds         = mrecAds;
            this.interstitialAds = interstitialAds;
            this.rewardedAds     = rewardedAds;
            this.adsService      = adsService;
            this.log             = log;
            this.log.Recorded    += this.OnAdEvent;
        }

        private void OnDestroy()
        {
            if (this.log != null) this.log.Recorded -= this.OnAdEvent;
        }

        private void OnAdEvent(AdEvent adEvent) => this.AddLine(adEvent.ToString());

        // Callback results are logged here, next to the SDK events, so the device log shows which
        // callback each path delivered.
        private void Callback(string line)
        {
            Debug.Log("[LevelPlay] " + line);
            this.AddLine(line);
        }

        private void AddLine(string line)
        {
            this.lines.Add($"{DateTime.Now:HH:mm:ss} {line}");
            if (this.lines.Count > MaxLines) this.lines.RemoveAt(0);
            this.scroll.y = float.MaxValue;
        }

        private void OnGUI()
        {
            var unit = Mathf.Max(24f, Screen.height / 40f);
            GUI.skin.button.fontSize = (int)(unit * 0.6f);
            GUI.skin.label.fontSize  = (int)(unit * 0.55f);

            if (!this.expanded)
            {
                if (GUI.Button(new Rect(0, Screen.height * 0.45f, unit * 3f, unit * 1.5f), "Ads")) this.expanded = true;
                return;
            }

            var area = new Rect(Screen.safeArea.x, Screen.height - Screen.safeArea.yMax, Screen.safeArea.width, Screen.safeArea.height);
            GUI.Box(area, GUIContent.none);
            GUILayout.BeginArea(area);
            this.scroll = GUILayout.BeginScrollView(this.scroll);
            var height = GUILayout.Height(unit * 1.6f);

            GUILayout.BeginHorizontal();
            GUILayout.Label($"LevelPlay SDK: {(this.session.IsInitialized ? "initialized" : this.session.IsStarted ? "starting" : "not started")}");
            if (GUILayout.Button("Init SDK", height)) this.session.Start();
            if (GUILayout.Button("Close", height)) this.expanded = false;
            GUILayout.EndHorizontal();

            GUILayout.Label($"Banner created/shown: {this.bannerAds.IsInitialized()}/{this.bannerAds.IsShown()}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load", height)) this.bannerAds.Load();
            if (GUILayout.Button("Show bottom", height)) this.bannerAds.ShowBanner(BannerPosition.BottomCenter);
            if (GUILayout.Button("Show top", height)) this.bannerAds.ShowBanner(BannerPosition.TopCenter);
            if (GUILayout.Button("Hide", height)) this.bannerAds.HideBanner();
            if (GUILayout.Button("Destroy", height)) this.bannerAds.Destroy();
            GUILayout.EndHorizontal();

            GUILayout.Label($"MREC created/shown: {this.mrecAds.IsInitialized()}/{this.mrecAds.IsShown()}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load", height)) this.mrecAds.Load();
            if (GUILayout.Button("Show center", height)) this.mrecAds.ShowMREC(MRECAdsPosition.Centered);
            if (GUILayout.Button("Show bottom", height)) this.mrecAds.ShowMREC(MRECAdsPosition.BottomCenter);
            if (GUILayout.Button("Hide", height)) this.mrecAds.HideMREC();
            if (GUILayout.Button("Destroy", height)) this.mrecAds.Destroy();
            GUILayout.EndHorizontal();

            GUILayout.Label($"Interstitial ready: {this.interstitialAds.IsInterstitialReady()}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load", height)) this.interstitialAds.Load();
            if (GUILayout.Button("Show direct", height)) this.ShowInterstitialDirect();
            if (GUILayout.Button("Show via IAdsService", height)) this.ShowInterstitialThroughAdsService();
            GUILayout.EndHorizontal();

            GUILayout.Label($"Rewarded ready: {this.rewardedAds.IsAdReady()}");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load", height)) this.rewardedAds.Load();
            if (GUILayout.Button("Show direct", height)) this.rewardedAds.ShowAd(rewarded => this.Callback($"IRewardedAdsService onAdComplete({rewarded})"), "dev_panel");
            if (GUILayout.Button("Show via IAdsService", height)) this.adsService.ShowRewardedAd(rewarded => this.Callback($"IAdsService onComplete({rewarded})"), "dev_panel");
            GUILayout.EndHorizontal();

            foreach (var line in this.lines) GUILayout.Label(line);

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void ShowInterstitialDirect()
        {
            this.interstitialAds.ShowInterstitial(
                "dev_panel",
                onAdClosed: () => this.Callback("IInterstitialAdsService onAdClosed"),
                onAdFailedToShow: () => this.Callback("IInterstitialAdsService onAdFailedToShow"));
        }

        // The aggregator shows the highest-priority interstitial that is ready, so while LevelPlay is
        // not ready this path lands on the Dummy. A LevelPlay close arrives here as onShowSuccess and a
        // show failure as onShowFail. Both are logged as they happen.
        private void ShowInterstitialThroughAdsService()
        {
            var route = this.interstitialAds.IsInterstitialReady() ? "LevelPlay (ready)" : "not LevelPlay (LevelPlay not ready, falls through to the next ready service)";
            this.Callback($"IAdsService.ShowInterstitialAd -> expected route: {route}");
            this.adsService.ShowInterstitialAd(
                "dev_panel",
                onShowFail: () => this.Callback("IAdsService onShowFail"),
                onShowSuccess: () => this.Callback("IAdsService onShowSuccess"));
        }
    }
    #endif
}
