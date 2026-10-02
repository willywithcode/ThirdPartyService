namespace ThirdPartyService.Tests.EditMode.AdsService
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using NUnit.Framework;
    using ThirdPartyService.Core.AdsService.AOA;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.InterstitialsAds;
    using ThirdPartyService.Core.AdsService.MRECAds;
    using ThirdPartyService.Core.AdsService.NativeAds;
    using ThirdPartyService.Core.AdsService.RewardedAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LocalDatas;
    using AdsService = ThirdPartyService.ServiceImplementation.AdsService.AdsService;

    // How the aggregator picks the banner when several networks offer one: the highest-priority banner
    // that has an ad loaded is on screen while the game wants a banner, and a later load on a higher
    // network takes the screen. On a device that is AdMob (20) first and LevelPlay (10) behind it.
    public class AdsServiceBannerWaterfallTests
    {
        private sealed class Banner : IBannerAdsService, IBannerLoadState
        {
            private readonly string       name;
            private readonly List<string> calls;
            private          bool         loaded;

            public Banner(string name, int priority, List<string> calls)
            {
                this.name     = name;
                this.Priority = priority;
                this.calls    = calls;
            }

            public int  Priority;
            public bool Shown;

            public event Action BannerLoadStateChanged;

            public bool IsBannerLoaded() => this.loaded;

            public void SetLoaded(bool value)
            {
                this.loaded = value;
                this.BannerLoadStateChanged?.Invoke();
            }

            public int   GetPriority()     => this.Priority;
            public void  Initialize()      { }
            public float GetBannerHeight() => this.Shown ? this.Priority : 0f;
            public bool  IsInitialized()   => true;
            public bool  IsShown()         => this.Shown;

            public void ShowBanner(BannerPosition position = BannerPosition.BottomCenter)
            {
                this.Shown = true;
                this.calls.Add("show " + this.name);
            }

            public void HideBanner()
            {
                this.Shown = false;
                this.calls.Add("hide " + this.name);
            }
        }

        // A banner that cannot report load state, like the Dummy or another game's MAX wrapper.
        private sealed class LegacyBanner : IBannerAdsService
        {
            public int  Priority;
            public bool Shown;

            public int   GetPriority()     => this.Priority;
            public void  Initialize()      { }
            public float GetBannerHeight() => 0f;
            public bool  IsInitialized()   => true;
            public bool  IsShown()         => this.Shown;
            public void  ShowBanner(BannerPosition position = BannerPosition.BottomCenter) => this.Shown = true;
            public void  HideBanner()      => this.Shown = false;
        }

        // RemoveAds() saves; this keeps the purchase in memory so the editor's own save is untouched.
        private sealed class InMemoryAdsLocalData : AdsLocalDataService
        {
            public override void Save() { }
        }

        private List<string>        calls;
        private AdsLocalDataService local;

        [SetUp]
        public void SetUp()
        {
            this.calls = new List<string>();
            this.local = new InMemoryAdsLocalData();
            this.local.Data.IsRemovedAds = false;
        }

        private AdsService NewService(params IBannerAdsService[] banners) =>
            new(this.local, new List<IAOAAdsService>(), new List<IBannerAdsService>(banners), new List<IInterstitialAdsService>(),
                new List<IMRECAdsService>(), new List<INativeAdsService>(), new List<IRewardedAdsService>(), new SignalBus());

        [Test]
        public void BothLoaded_TheHigherPriorityBannerShows()
        {
            var admob     = new Banner("admob", 20, this.calls);
            var levelPlay = new Banner("levelplay", 10, this.calls);
            admob.SetLoaded(true);
            levelPlay.SetLoaded(true);
            var service = this.NewService(levelPlay, admob);

            service.ShowBannerAd();

            Assert.That(this.calls, Is.EqualTo(new[] { "show admob" }));
            Assert.That(service.GetBannerAdHeight(), Is.EqualTo(20f));
        }

        [Test]
        public void TopStarved_TheLowerLoadedBannerShows()
        {
            var admob     = new Banner("admob", 20, this.calls);
            var levelPlay = new Banner("levelplay", 10, this.calls);
            levelPlay.SetLoaded(true);
            var service = this.NewService(levelPlay, admob);

            service.ShowBannerAd();

            Assert.That(this.calls, Is.EqualTo(new[] { "show levelplay" }));
            Assert.That(service.IsShowingBannerAd(), Is.True);
        }

        [Test]
        public void NothingLoaded_NothingShows_UntilABannerLoads()
        {
            var admob     = new Banner("admob", 20, this.calls);
            var levelPlay = new Banner("levelplay", 10, this.calls);
            var service   = this.NewService(levelPlay, admob);

            service.ShowBannerAd();
            Assert.That(this.calls, Is.Empty);
            Assert.That(service.IsShowingBannerAd(), Is.True, "the request stands, so the game does not ask again every level");

            levelPlay.SetLoaded(true);

            Assert.That(this.calls, Is.EqualTo(new[] { "show levelplay" }));
        }

        [Test]
        public void TopLoadsLater_ItTakesTheScreen_HidingTheOldBannerFirst()
        {
            var admob     = new Banner("admob", 20, this.calls);
            var levelPlay = new Banner("levelplay", 10, this.calls);
            levelPlay.SetLoaded(true);
            var service = this.NewService(levelPlay, admob);
            service.ShowBannerAd();

            admob.SetLoaded(true);

            Assert.That(this.calls, Is.EqualTo(new[] { "show levelplay", "hide levelplay", "show admob" }),
                "AdsGate keeps one on-screen flag, so the hidden report must come before the visible one");
            Assert.That(service.GetBannerAdHeight(), Is.EqualTo(20f));
        }

        [Test]
        public void LowerLoadsLater_TheTopBannerStays()
        {
            var admob     = new Banner("admob", 20, this.calls);
            var levelPlay = new Banner("levelplay", 10, this.calls);
            admob.SetLoaded(true);
            var service = this.NewService(levelPlay, admob);
            service.ShowBannerAd();

            levelPlay.SetLoaded(true);

            Assert.That(this.calls, Is.EqualTo(new[] { "show admob" }));
        }

        [Test]
        public void ALoadWhileNoBannerIsWanted_ShowsNothing()
        {
            var admob   = new Banner("admob", 20, this.calls);
            var service = this.NewService(admob);

            admob.SetLoaded(true);
            Assert.That(this.calls, Is.Empty, "the game has not reached the banner level yet");

            service.ShowBannerAd();
            service.HideBannerAd();
            admob.SetLoaded(true);

            Assert.That(this.calls, Is.EqualTo(new[] { "show admob", "hide admob" }));
            Assert.That(service.IsShowingBannerAd(), Is.False);
        }

        [Test]
        public void RemoveAds_HidesTheBannerOnScreen_AndLaterLoadsStayHidden()
        {
            var admob     = new Banner("admob", 20, this.calls);
            var levelPlay = new Banner("levelplay", 10, this.calls);
            levelPlay.SetLoaded(true);
            var service = this.NewService(levelPlay, admob);
            service.ShowBannerAd();

            service.RemoveAds();
            admob.SetLoaded(true);
            service.ShowBannerAd();

            Assert.That(this.calls, Is.EqualTo(new[] { "show levelplay", "hide levelplay" }));
            Assert.That(service.IsShowingBannerAd(), Is.False);
            Assert.That(service.GetBannerAdHeight(), Is.Zero);
        }

        [Test]
        public void ABannerWithoutLoadState_IsPickedByPriorityAlone()
        {
            var dummy   = new LegacyBanner { Priority = 1 };
            var service = this.NewService(dummy);

            service.ShowBannerAd();

            Assert.That(dummy.Shown, Is.True, "the Editor's Dummy banner and other games' wrappers keep today's behaviour");
        }

        [Test]
        public void NoBannerRegistered_TheRequestDoesNotStand()
        {
            var service = this.NewService();

            service.ShowBannerAd();

            Assert.That(service.IsShowingBannerAd(), Is.False);
        }
    }
}
