namespace ThirdPartyService.Tests.EditMode.Fsn
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.AdsService.Fsn;
    using UnityEngine;

    public sealed class FsnFullScreenNativeAdsTests
    {
        private sealed class FakeUnit : IFsnAdUnit
        {
            public FakeUnit(string adUnitId) => this.AdUnitId = adUnitId;

            public string AdUnitId  { get; }
            public bool   IsReady   { get; set; }
            public bool   IsLoading { get; set; }
            public int    LoadCount { get; private set; }
            public int    ShowCount { get; private set; }
            public int    LastCloseAfterSeconds { get; private set; }

            public Action<string> PendingClose;

            public event Action<string>                 LoadCompleted;
            public event Action<string, double, string> Paid;

            public void Load()
            {
                this.LoadCount++;
                this.IsLoading = true;
            }

            public void Show(int closeAfterSeconds, Action<string> onCompleted)
            {
                this.ShowCount++;
                this.LastCloseAfterSeconds = closeAfterSeconds;
                this.IsReady      = false;
                this.PendingClose = onCompleted;
            }

            public void RaiseLoaded(string error) => this.LoadCompleted?.Invoke(error);
            public void RaisePaid() => this.Paid?.Invoke("AdMob Network", 0.01, "USD");
        }

        private FsnSettings                       settings;
        private Dictionary<string, FakeUnit>      created;
        private FsnFullScreenNativeAds            ads;

        [SetUp]
        public void SetUp()
        {
            this.settings = ScriptableObject.CreateInstance<FsnSettings>();
            this.settings.placements.Add(new FsnPlacement { placement = "intro", adUnitIds = new List<string> { "high", "normal" } });
            this.settings.placements.Add(new FsnPlacement { placement = "after_inter", adUnitIds = new List<string> { "normal" } });
            this.created = new Dictionary<string, FakeUnit>();
            this.ads = new FsnFullScreenNativeAds(() => this.settings, (id, _) => this.created[id] = new FakeUnit(id), new SignalBus());
        }

        [TearDown]
        public void TearDown() => UnityEngine.Object.DestroyImmediate(this.settings);

        [Test]
        public void Load_LoadsEveryIdleUnitOfThePlacement()
        {
            this.ads.Load("intro");

            Assert.AreEqual(1, this.created["high"].LoadCount);
            Assert.AreEqual(1, this.created["normal"].LoadCount);

            this.ads.Load("intro");
            Assert.AreEqual(1, this.created["high"].LoadCount, "A loading unit is not asked again.");
        }

        [Test]
        public void Show_PrefersTheFirstListedReadyUnit()
        {
            this.ads.Load("intro");
            this.created["high"].IsReady   = true;
            this.created["normal"].IsReady = true;

            bool? result = null;
            this.ads.Show("intro", shown => result = shown);

            Assert.AreEqual(1, this.created["high"].ShowCount);
            Assert.AreEqual(0, this.created["normal"].ShowCount);
            Assert.IsNull(result, "Closed is reported only when the ad closes.");

            this.created["high"].PendingClose(string.Empty);
            Assert.AreEqual(true, result);
        }

        [Test]
        public void Show_PassesTheCloseDelayToTheUnit_NegativeMeaningTheConfiguredOne()
        {
            this.ads.Load("intro");
            this.created["high"].IsReady = true;
            this.ads.Show("intro", 7, _ => { });
            Assert.AreEqual(7, this.created["high"].LastCloseAfterSeconds);

            this.created["high"].PendingClose(string.Empty);
            this.created["high"].IsReady = true;
            this.ads.Show("intro", _ => { });
            Assert.AreEqual(-1, this.created["high"].LastCloseAfterSeconds);
        }

        [Test]
        public void Show_FallsBackToTheNextUnitWithAnAd()
        {
            this.ads.Load("intro");
            this.created["normal"].IsReady = true;

            this.ads.Show("intro", _ => { });

            Assert.AreEqual(0, this.created["high"].ShowCount);
            Assert.AreEqual(1, this.created["normal"].ShowCount);
        }

        [Test]
        public void Show_WithoutAnAd_ReportsFalseAndLoads()
        {
            bool? result = null;
            this.ads.Show("intro", shown => result = shown);

            Assert.AreEqual(false, result);
            Assert.AreEqual(1, this.created["high"].LoadCount);
            Assert.AreEqual(1, this.created["normal"].LoadCount);
        }

        [Test]
        public void Show_WhileAnotherIsOpen_ReportsFalse()
        {
            this.ads.Load("intro");
            this.created["high"].IsReady   = true;
            this.created["normal"].IsReady = true;
            this.ads.Show("intro", _ => { });

            bool? second = null;
            this.ads.Show("after_inter", shown => second = shown);
            Assert.AreEqual(false, second);
            Assert.AreEqual(0, this.created["normal"].ShowCount);

            this.created["high"].PendingClose(string.Empty);
            this.ads.Show("after_inter", shown => second = shown);
            Assert.AreEqual(1, this.created["normal"].ShowCount, "The next ad may show once the first closed.");
        }

        [Test]
        public void Show_ThatTheLibraryRefuses_ReportsFalse()
        {
            this.ads.Load("after_inter");
            this.created["normal"].IsReady = true;

            bool? result = null;
            this.ads.Show("after_inter", shown => result = shown);
            this.created["normal"].PendingClose("No ad available");

            Assert.AreEqual(false, result);
        }

        [Test]
        public void PlacementsSharingAnAdUnit_ShareOneUnit()
        {
            this.ads.Load("intro");
            var normal = this.created["normal"];
            this.ads.Load("after_inter");

            Assert.AreSame(normal, this.created["normal"]);
            Assert.AreEqual(1, normal.LoadCount);
        }

        [Test]
        public void UnitEvents_DoNotThrow()
        {
            this.ads.Load("intro");
            Assert.DoesNotThrow(() => this.created["high"].RaiseLoaded(null));
            Assert.DoesNotThrow(() => this.created["high"].RaiseLoaded("3 No fill"));
            Assert.DoesNotThrow(() => this.created["high"].RaisePaid());
        }

        [Test]
        public void UnknownPlacementOrMissingSettings_NeverShows()
        {
            bool? result = null;
            this.ads.Show("nowhere", shown => result = shown);
            Assert.AreEqual(false, result);

            UnityEngine.TestTools.LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("No FsnSettings"));
            var noSettings = new FsnFullScreenNativeAds(() => null, (id, _) => new FakeUnit(id), new SignalBus());
            result = null;
            noSettings.Show("intro", shown => result = shown);
            Assert.AreEqual(false, result);
            Assert.IsFalse(noSettings.IsReady("intro"));
        }
    }
}
