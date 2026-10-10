namespace ThirdPartyService.ServiceImplementation.AdsService.Fsn
{
    using System;
    using System.Collections.Generic;
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using ThirdPartyService.Core.AdsService.NativeAds;
    using ThirdPartyService.Core.AdsService.Signals;
    using UnityEngine;
    using UnityEngine.Events;
    using VContainer;

    /// <summary>
    /// Full screen native ads through the FSN Android library. A placement lists its ad units in
    /// <see cref="FsnSettings"/>; loading loads every one of them and showing takes the first that is
    /// ready, so a _high unit listed first wins whenever it has an ad. One ad is on screen at a time.
    /// </summary>
    public sealed class FsnFullScreenNativeAds : IFullScreenNativeAdsService
    {
        private const string Platform = "Admob";

        private readonly Func<FsnSettings>                        loadSettings;
        private readonly Func<string, FsnSettings, IFsnAdUnit>    createUnit;
        private readonly SignalBus                                signalBus;
        private readonly Dictionary<string, IFsnAdUnit>           units = new();

        private FsnSettings settings;
        private bool        settingsLoaded;
        private bool        showing;
        private string      shownPlacement = string.Empty;

        [Inject]
        public FsnFullScreenNativeAds(IAssetsManager assetsManager, SignalBus signalBus)
            : this(() => assetsManager.LoadAsset<FsnSettings>(FsnSettings.Address), (id, settings) => new AndroidFsnAdUnit(id, settings), signalBus)
        {
        }

        public FsnFullScreenNativeAds(Func<FsnSettings> loadSettings, Func<string, FsnSettings, IFsnAdUnit> createUnit, SignalBus signalBus)
        {
            this.loadSettings = loadSettings;
            this.createUnit   = createUnit;
            this.signalBus    = signalBus;
        }

        public void Load(string placement)
        {
            foreach (var unit in this.UnitsOf(placement))
            {
                if (!unit.IsReady && !unit.IsLoading) unit.Load();
            }
        }

        public bool IsReady(string placement) => this.FirstReady(placement) != null;

        public void Show(string placement, UnityAction<bool> onClosed)
        {
            var unit = this.showing ? null : this.FirstReady(placement);
            if (unit == null)
            {
                this.Load(placement);
                onClosed?.Invoke(false);
                return;
            }

            this.showing        = true;
            this.shownPlacement = placement;
            this.signalBus.Fire<OnNativeShowSignal>(new(Platform, placement));
            unit.Show(error =>
            {
                this.showing = false;
                var shown = string.IsNullOrEmpty(error);
                if (shown) this.signalBus.Fire<OnNativeAdHiddenEventSignal>(new(Platform, placement));
                else Debug.LogWarning($"[FSN] {placement} was not shown: {error}");
                onClosed?.Invoke(shown);
            });
        }

        private IFsnAdUnit FirstReady(string placement)
        {
            foreach (var unit in this.UnitsOf(placement))
            {
                if (unit.IsReady) return unit;
            }
            return null;
        }

        private IEnumerable<IFsnAdUnit> UnitsOf(string placement)
        {
            var current = this.Settings();
            if (current == null) yield break;
            foreach (var adUnitId in current.AdUnitsOf(placement))
            {
                if (string.IsNullOrEmpty(adUnitId)) continue;
                if (!this.units.TryGetValue(adUnitId, out var unit))
                {
                    unit = this.CreateUnit(adUnitId, current);
                    this.units[adUnitId] = unit;
                }
                yield return unit;
            }
        }

        private IFsnAdUnit CreateUnit(string adUnitId, FsnSettings current)
        {
            var unit = this.createUnit(adUnitId, current);
            unit.LoadCompleted += error =>
            {
                if (error == null) this.signalBus.Fire<OnNativeAdLoadedEventSignal>(new(Platform, adUnitId));
                else this.signalBus.Fire<OnNativeAdLoadFailedEventSignal>(new(Platform, error));
            };
            unit.Paid += (adSource, revenue, currency) =>
                this.signalBus.Fire<OnNativeAdRevenuePaidEventSignal>(new(Platform, this.shownPlacement, revenue, currency));
            return unit;
        }

        private FsnSettings Settings()
        {
            if (this.settingsLoaded) return this.settings;
            this.settingsLoaded = true;
            this.settings       = this.loadSettings();
            if (this.settings == null) Debug.LogError($"[FSN] No FsnSettings at the Addressable address '{FsnSettings.Address}'; full screen native ads are off.");
            return this.settings;
        }
    }
}
