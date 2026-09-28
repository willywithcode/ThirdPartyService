namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Banner;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.InterstitialsAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.MRECAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.RewardedAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;

    // Stands in for LevelPlay: records what the wrappers ask of it and lets a test raise the SDK's
    // callbacks in any order, with no network.
    internal sealed class FakeAdsSdk : IAdsSdk
    {
        private Action         initSuccess;
        private Action<string> initFailed;

        public readonly List<string>               InitAppKeys   = new();
        public readonly List<FakeFullscreenAdUnit> Interstitials = new();
        public readonly List<FakeRewardedAdUnit>   Rewardeds     = new();
        public readonly List<FakeBannerAdUnit>     Banners       = new();

        public void Init(string appKey, Action onSuccess, Action<string> onFailed)
        {
            this.InitAppKeys.Add(appKey);
            this.initSuccess = onSuccess;
            this.initFailed  = onFailed;
        }

        public void SucceedInit() => this.initSuccess();
        public void FailInit(string error) => this.initFailed(error);

        public IFullscreenAdUnit CreateInterstitial(string adUnitId)
        {
            var unit = new FakeFullscreenAdUnit(adUnitId);
            this.Interstitials.Add(unit);
            return unit;
        }

        public IRewardedAdUnit CreateRewarded(string adUnitId)
        {
            var unit = new FakeRewardedAdUnit(adUnitId);
            this.Rewardeds.Add(unit);
            return unit;
        }

        public IBannerAdUnit CreateBanner(string adUnitId, BannerAdSize size, BannerPosition position)
        {
            var unit = new FakeBannerAdUnit(adUnitId, size, position);
            this.Banners.Add(unit);
            return unit;
        }
    }

    internal class FakeFullscreenAdUnit : IFullscreenAdUnit
    {
        public FakeFullscreenAdUnit(string adUnitId) => this.AdUnitId = adUnitId;

        public readonly string       AdUnitId;
        public readonly List<string> ShowPlacements = new();
        public int  LoadCalls { get; private set; }
        public bool Disposed  { get; private set; }

        public event Action         Loaded;
        public event Action<string> LoadFailed;
        public event Action         Displayed;
        public event Action<string> DisplayFailed;
        public event Action         Clicked;
        public event Action         Closed;

        public bool IsReady { get; private set; }

        public void Load() => this.LoadCalls++;
        public void Show(string placement) => this.ShowPlacements.Add(placement);
        public void Dispose() => this.Disposed = true;

        public void CompleteLoad()
        {
            this.IsReady = true;
            this.Loaded?.Invoke();
        }

        public void FailLoad(string error = "no fill") => this.LoadFailed?.Invoke(error);

        public void Display()
        {
            this.IsReady = false;
            this.Displayed?.Invoke();
        }

        public void FailDisplay(string error = "display failed")
        {
            this.IsReady = false;
            this.DisplayFailed?.Invoke(error);
        }

        public void Click() => this.Clicked?.Invoke();
        public void Close() => this.Closed?.Invoke();
    }

    internal sealed class FakeRewardedAdUnit : FakeFullscreenAdUnit, IRewardedAdUnit
    {
        public FakeRewardedAdUnit(string adUnitId) : base(adUnitId) { }

        public event Action<string> Rewarded;

        public void Reward() => this.Rewarded?.Invoke("coins x1");
    }

    internal sealed class FakeBannerAdUnit : IBannerAdUnit
    {
        public FakeBannerAdUnit(string adUnitId, BannerAdSize size, BannerPosition position)
        {
            this.AdUnitId = adUnitId;
            this.Size     = size;
            this.Position = position;
        }

        public readonly string       AdUnitId;
        public readonly BannerAdSize Size;
        public int  LoadCalls { get; private set; }
        public int  ShowCalls { get; private set; }
        public int  HideCalls { get; private set; }
        public bool Disposed  { get; private set; }

        public event Action         Loaded;
        public event Action<string> LoadFailed;
        public event Action         Displayed;
        public event Action<string> DisplayFailed;
        public event Action         Clicked;
        public event Action         Expanded;
        public event Action         Collapsed;
        public event Action         LeftApplication;

        public BannerPosition Position     { get; }
        public float          HeightPixels => this.Size == BannerAdSize.MediumRectangle ? 250f : 50f;

        public void Load() => this.LoadCalls++;
        public void Show() => this.ShowCalls++;
        public void Hide() => this.HideCalls++;
        public void Dispose() => this.Disposed = true;

        public void CompleteLoad() => this.Loaded?.Invoke();
        public void FailLoad(string error = "no fill") => this.LoadFailed?.Invoke(error);
        public void Display() => this.Displayed?.Invoke();
        public void FailDisplay(string error = "display failed") => this.DisplayFailed?.Invoke(error);
        public void Click() => this.Clicked?.Invoke();
        public void Expand() => this.Expanded?.Invoke();
        public void Collapse() => this.Collapsed?.Invoke();
        public void LeaveApplication() => this.LeftApplication?.Invoke();
    }

    // Runs nothing on its own: a test inspects what was scheduled and fires it when it chooses.
    internal sealed class FakeAdsScheduler : IAdsScheduler
    {
        internal sealed class Entry : IDisposable
        {
            public float  Delay;
            public Action Action;
            public bool   Cancelled;
            public bool   Ran;

            public void Dispose() => this.Cancelled = true;
        }

        public readonly List<Entry> Scheduled = new();

        public List<Entry> Pending => this.Scheduled.Where(e => !e.Cancelled && !e.Ran).ToList();

        public IDisposable Schedule(float delaySeconds, Action action)
        {
            var entry = new Entry { Delay = delaySeconds, Action = action };
            this.Scheduled.Add(entry);
            return entry;
        }

        // Fires every entry that has not run, cancelled or not: a timer whose cancel did not take.
        public void RunIncludingCancelled()
        {
            foreach (var entry in this.Scheduled.Where(e => !e.Ran).ToList())
            {
                entry.Ran = true;
                entry.Action();
            }
        }

        public void RunPending()
        {
            foreach (var entry in this.Pending)
            {
                if (entry.Cancelled) continue;
                entry.Ran = true;
                entry.Action();
            }
        }
    }

    internal sealed class StubLevelPlaySettings : ILevelPlaySettingsProvider
    {
        public LevelPlayPlatformSettings Current { get; set; } = new()
        {
            appKey               = "app-key",
            bannerAdUnitId       = "banner-unit",
            interstitialAdUnitId = "interstitial-unit",
            rewardedAdUnitId     = "rewarded-unit",
        };

        public bool StartSdkInReleaseBuilds { get; set; } = LevelPlaySettings.DefaultStartSdkInReleaseBuilds;
    }

    // The collaborators every wrapper test builds; by default the SDK is already initialized.
    internal sealed class AdsTestRig
    {
        public readonly FakeAdsSdk            Sdk       = new();
        public readonly FakeAdsScheduler      Scheduler = new();
        public readonly StubLevelPlaySettings Settings  = new();
        public readonly AdEventLog            Log       = new();
        public readonly List<AdEvent>         Events    = new();
        public readonly LevelPlaySdkSession   Session;

        public AdsTestRig(bool initialized = true)
        {
            this.Log.Recorded += this.Events.Add;
            this.Session      =  new LevelPlaySdkSession(this.Sdk, this.Settings, this.Scheduler, this.Log);
            if (!initialized) return;
            this.Session.Start();
            this.Sdk.SucceedInit();
        }

        public LevelPlayInterstitialAds NewInterstitial() => new(this.Session, this.Sdk, this.Settings, this.Scheduler, this.Log);
        public LevelPlayRewardedAds     NewRewarded()     => new(this.Session, this.Sdk, this.Settings, this.Scheduler, this.Log);
        public LevelPlayBannerAds       NewBanner()       => new(this.Session, this.Sdk, this.Settings, this.Scheduler, this.Log);
        public LevelPlayMRECAds         NewMrec()         => new(this.Session, this.Sdk, this.Settings, this.Scheduler, this.Log);

        public int Count(AdFormat format, AdEventKind kind) => this.Events.Count(e => e.Format == format && e.Kind == kind);
    }
    #endif
}
