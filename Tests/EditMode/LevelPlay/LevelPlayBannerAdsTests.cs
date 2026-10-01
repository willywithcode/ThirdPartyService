namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.Core.AdsService.MRECAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;

    // Banner and MREC both go through LevelPlay's banner API. The ad is preloaded hidden, shown on
    // request, and LevelPlay fixes its position at creation, so another position means a new ad.
    public class LevelPlayBannerAdsTests
    {
        private AdsTestRig rig;

        [SetUp]
        public void SetUp() => this.rig = new AdsTestRig();

        [Test]
        public void Initialize_PreloadsAHiddenBannerAtBottomCenter()
        {
            var banner = this.rig.NewBanner();

            banner.Initialize();

            var unit = this.rig.Sdk.Banners.Single();
            Assert.That(unit.AdUnitId, Is.EqualTo("banner-unit"));
            Assert.That(unit.Size, Is.EqualTo(BannerAdSize.Banner));
            Assert.That(unit.Position, Is.EqualTo(BannerPosition.BottomCenter));
            Assert.That(unit.LoadCalls, Is.EqualTo(1));
            Assert.That(unit.ShowCalls, Is.EqualTo(0));
            Assert.That(banner.IsShown(), Is.False);
        }

        [Test]
        public void ShowBeforeLoaded_ShowsAsSoonAsTheAdLoads()
        {
            var banner = this.rig.NewBanner();
            banner.Initialize();
            var unit = this.rig.Sdk.Banners.Single();

            banner.ShowBanner(BannerPosition.BottomCenter);
            Assert.That(unit.ShowCalls, Is.EqualTo(0));
            unit.CompleteLoad();

            Assert.That(unit.ShowCalls, Is.EqualTo(1));
            Assert.That(banner.IsShown(), Is.True);
            Assert.That(banner.GetBannerHeight(), Is.EqualTo(50f));
        }

        [Test]
        public void ShowAfterLoaded_ShowsAtOnce_AndHideHidesIt()
        {
            var banner = this.rig.NewBanner();
            banner.Initialize();
            var unit = this.rig.Sdk.Banners.Single();
            unit.CompleteLoad();

            banner.ShowBanner(BannerPosition.BottomCenter);
            banner.HideBanner();

            Assert.That(unit.ShowCalls, Is.EqualTo(1));
            Assert.That(unit.HideCalls, Is.EqualTo(1));
            Assert.That(banner.IsShown(), Is.False);
            Assert.That(banner.GetBannerHeight(), Is.EqualTo(0f));
            Assert.That(this.rig.Count(AdFormat.Banner, AdEventKind.Hidden), Is.EqualTo(1));
        }

        [Test]
        public void HideBeforeLoaded_KeepsTheAdHiddenWhenItLoads()
        {
            var banner = this.rig.NewBanner();
            banner.Initialize();
            var unit = this.rig.Sdk.Banners.Single();

            banner.ShowBanner();
            banner.HideBanner();
            unit.CompleteLoad();

            Assert.That(unit.ShowCalls, Is.EqualTo(0));
        }

        [Test]
        public void ShowAtAnotherPosition_ReplacesTheAdWithOneAtThatPosition()
        {
            var banner = this.rig.NewBanner();
            banner.Initialize();
            var first = this.rig.Sdk.Banners.Single();
            first.CompleteLoad();

            banner.ShowBanner(BannerPosition.TopCenter);

            Assert.That(first.Disposed, Is.True);
            var second = this.rig.Sdk.Banners.Last();
            Assert.That(second.Position, Is.EqualTo(BannerPosition.TopCenter));
            Assert.That(second.LoadCalls, Is.EqualTo(1));
            second.CompleteLoad();
            Assert.That(second.ShowCalls, Is.EqualTo(1));
        }

        [Test]
        public void LoadFailureBeforeTheFirstLoad_RetriesWithBackoff()
        {
            var banner = this.rig.NewBanner();
            banner.Initialize();
            var unit = this.rig.Sdk.Banners.Single();

            unit.FailLoad();
            Assert.That(unit.LoadCalls, Is.EqualTo(1));
            this.rig.Scheduler.RunPending();
            unit.FailLoad();

            Assert.That(this.rig.Scheduler.Scheduled.Select(e => e.Delay), Is.EqualTo(new[] { 2f, 4f }));
            Assert.That(unit.LoadCalls, Is.EqualTo(2));
        }

        [Test]
        public void RefreshFailureAfterALoad_LeavesReloadingToLevelPlay()
        {
            var banner = this.rig.NewBanner();
            banner.Initialize();
            var unit = this.rig.Sdk.Banners.Single();
            unit.CompleteLoad();

            unit.FailLoad();
            this.rig.Scheduler.RunPending();

            Assert.That(unit.LoadCalls, Is.EqualTo(1));
        }

        [Test]
        public void ShownClickedAndDisplayFailed_AreLogged_AndDisplayFailureMarksItNotShown()
        {
            var banner = this.rig.NewBanner();
            banner.Initialize();
            var unit = this.rig.Sdk.Banners.Single();
            unit.CompleteLoad();
            banner.ShowBanner();

            unit.Display();
            unit.Click();
            unit.Expand();
            unit.Collapse();
            unit.LeaveApplication();
            unit.FailDisplay();

            Assert.That(this.rig.Count(AdFormat.Banner, AdEventKind.Shown), Is.EqualTo(1));
            Assert.That(this.rig.Count(AdFormat.Banner, AdEventKind.Clicked), Is.EqualTo(1));
            Assert.That(this.rig.Count(AdFormat.Banner, AdEventKind.Expanded), Is.EqualTo(1));
            Assert.That(this.rig.Count(AdFormat.Banner, AdEventKind.Collapsed), Is.EqualTo(1));
            Assert.That(this.rig.Count(AdFormat.Banner, AdEventKind.LeftApplication), Is.EqualTo(1));
            Assert.That(this.rig.Count(AdFormat.Banner, AdEventKind.ShowFailed), Is.EqualTo(1));
            Assert.That(banner.IsShown(), Is.False);
        }

        // The game lifts its HUD only for a banner that is really on screen, so the signal waits for
        // Displayed - not the show request, not the load - and says so once however often LevelPlay
        // refreshes the ad.
        [Test]
        public void Visibility_IsAnnouncedOnDisplay_Once_WithTheBannersHeight()
        {
            var seen = new List<OnBannerVisibilityChangedSignal>();
            this.rig.Signals.Subscribe<OnBannerVisibilityChangedSignal>(seen.Add);
            var banner = this.rig.NewBanner();
            banner.Initialize();

            banner.ShowBanner();
            var unit = this.rig.Sdk.Banners.Single();
            unit.CompleteLoad();
            Assert.That(seen, Is.Empty, "a loaded banner that has not displayed yet takes no room");

            unit.Display();
            unit.Display();

            Assert.That(seen, Has.Count.EqualTo(1));
            Assert.That(seen[0].Visible, Is.True);
            Assert.That(seen[0].HeightPixels, Is.EqualTo(50f));
        }

        [Test]
        public void Visibility_IsWithdrawnOnHide_AndOnDisplayFailure()
        {
            var seen = new List<bool>();
            this.rig.Signals.Subscribe<OnBannerVisibilityChangedSignal>(signal => seen.Add(signal.Visible));
            var banner = this.rig.NewBanner();
            banner.Initialize();
            banner.ShowBanner();
            var unit = this.rig.Sdk.Banners.Single();
            unit.CompleteLoad();
            unit.Display();

            banner.HideBanner();
            banner.HideBanner();

            Assert.That(seen, Is.EqualTo(new[] { true, false }));

            banner.ShowBanner();
            unit.Display();
            unit.FailDisplay();

            Assert.That(seen, Is.EqualTo(new[] { true, false, true, false }));
        }

        [Test]
        public void Visibility_IsNeverAnnounced_WithoutFill()
        {
            var seen = new List<bool>();
            this.rig.Signals.Subscribe<OnBannerVisibilityChangedSignal>(signal => seen.Add(signal.Visible));
            var banner = this.rig.NewBanner();
            banner.Initialize();

            banner.ShowBanner();
            this.rig.Sdk.Banners.Single().FailLoad();

            Assert.That(seen, Is.Empty);
        }

        [Test]
        public void Mrec_UsesTheMediumRectangleSizeOnTheBannerUnit_WhenNoMrecUnitIsSet()
        {
            var mrec = this.rig.NewMrec();

            mrec.Initialize();

            var unit = this.rig.Sdk.Banners.Single();
            Assert.That(unit.Size, Is.EqualTo(BannerAdSize.MediumRectangle));
            Assert.That(unit.AdUnitId, Is.EqualTo("banner-unit"));
        }

        [Test]
        public void Mrec_UsesItsOwnUnit_WhenTheSettingsNameOne()
        {
            this.rig.Settings.Current.mrecAdUnitId = "mrec-unit";
            var mrec = this.rig.NewMrec();

            mrec.Initialize();

            Assert.That(this.rig.Sdk.Banners.Single().AdUnitId, Is.EqualTo("mrec-unit"));
        }

        [TestCase(MRECAdsPosition.TopLeft, BannerPosition.TopLeft)]
        [TestCase(MRECAdsPosition.TopCenter, BannerPosition.TopCenter)]
        [TestCase(MRECAdsPosition.TopRight, BannerPosition.TopRight)]
        [TestCase(MRECAdsPosition.Centered, BannerPosition.Centered)]
        [TestCase(MRECAdsPosition.CenterLeft, BannerPosition.CenterLeft)]
        [TestCase(MRECAdsPosition.CenterRight, BannerPosition.CenterRight)]
        [TestCase(MRECAdsPosition.BottomLeft, BannerPosition.BottomLeft)]
        [TestCase(MRECAdsPosition.BottomCenter, BannerPosition.BottomCenter)]
        [TestCase(MRECAdsPosition.BottomRight, BannerPosition.BottomRight)]
        public void Mrec_ShowsAtTheRequestedPosition(MRECAdsPosition requested, BannerPosition expected)
        {
            var mrec = this.rig.NewMrec();
            mrec.Initialize();

            mrec.ShowMREC(requested);
            this.rig.Sdk.Banners.Last().CompleteLoad();

            var shown = this.rig.Sdk.Banners.Last();
            Assert.That(shown.Position, Is.EqualTo(expected));
            Assert.That(shown.ShowCalls, Is.EqualTo(1));
            Assert.That(mrec.IsShown(), Is.True);
        }

        [Test]
        public void ShowBeforeTheSdkIsReady_IsRemembered_AndAppliedWhenTheAdIsCreated()
        {
            var rig    = new AdsTestRig(initialized: false);
            var banner = rig.NewBanner();
            banner.Initialize();

            banner.ShowBanner(BannerPosition.TopCenter);
            Assert.That(rig.Sdk.Banners, Is.Empty);
            rig.Session.Start();
            rig.Sdk.SucceedInit();

            var unit = rig.Sdk.Banners.Single();
            Assert.That(unit.Position, Is.EqualTo(BannerPosition.TopCenter));
            unit.CompleteLoad();
            Assert.That(unit.ShowCalls, Is.EqualTo(1));
            Assert.That(banner.IsShown(), Is.True);
        }

        [Test]
        public void MrecShowBeforeTheSdkIsReady_IsRemembered_AndAppliedWhenTheAdIsCreated()
        {
            var rig  = new AdsTestRig(initialized: false);
            var mrec = rig.NewMrec();
            mrec.Initialize();

            mrec.ShowMREC(MRECAdsPosition.Centered);
            rig.Session.Start();
            rig.Sdk.SucceedInit();

            var unit = rig.Sdk.Banners.Single();
            Assert.That(unit.Position, Is.EqualTo(BannerPosition.Centered));
            unit.CompleteLoad();
            Assert.That(unit.ShowCalls, Is.EqualTo(1));
            Assert.That(mrec.IsShown(), Is.True);
        }

        [Test]
        public void ShowThenHideBeforeTheSdkIsReady_LeavesTheAdHidden()
        {
            var rig    = new AdsTestRig(initialized: false);
            var banner = rig.NewBanner();
            banner.Initialize();

            banner.ShowBanner(BannerPosition.TopCenter);
            banner.HideBanner();
            rig.Session.Start();
            rig.Sdk.SucceedInit();
            rig.Sdk.Banners.Single().CompleteLoad();

            Assert.That(rig.Sdk.Banners.Single().ShowCalls, Is.EqualTo(0));
            Assert.That(banner.IsShown(), Is.False);
        }

        [Test]
        public void ShowWithoutAnEarlierInitialize_CreatesTheAdThere_AndShowsItOnLoad()
        {
            var banner = this.rig.NewBanner();

            banner.ShowBanner(BannerPosition.TopCenter);

            var unit = this.rig.Sdk.Banners.Single();
            Assert.That(unit.Position, Is.EqualTo(BannerPosition.TopCenter));
            unit.CompleteLoad();
            Assert.That(unit.ShowCalls, Is.EqualTo(1));
        }

        [Test]
        public void Priorities_OutrankTheThirdPartyServiceDummies()
        {
            var dummyMrec = new ThirdPartyService.ServiceImplementation.AdsService.DummyAds.MRECAds.DummyMRECAds();

            Assert.That(this.rig.NewBanner().GetPriority(), Is.GreaterThan(1), "DummyBannerAds.GetPriority() is 1");
            Assert.That(this.rig.NewMrec().GetPriority(), Is.GreaterThan(dummyMrec.GetPriority()));
        }
    }
    #endif
}
