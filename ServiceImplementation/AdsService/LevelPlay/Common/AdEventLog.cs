namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Common
{
    #if LevelPlay
    using System;
    using UnityEngine;

    public enum AdFormat
    {
        Sdk,
        Banner,
        MREC,
        Interstitial,
        Rewarded,
    }

    public enum AdEventKind
    {
        InitRequested,
        InitSucceeded,
        InitFailed,
        LoadRequested,
        Loaded,
        LoadFailed,
        RetryScheduled,
        ShowRequested,
        Shown,
        ShowFailed,
        Clicked,
        Closed,
        Rewarded,
        NotRewarded,
        Hidden,
        Destroyed,
        Expanded,
        Collapsed,
        LeftApplication,
    }

    public readonly struct AdEvent
    {
        public AdFormat    Format { get; }
        public AdEventKind Kind   { get; }
        public string      Detail { get; }

        public AdEvent(AdFormat format, AdEventKind kind, string detail)
        {
            this.Format = format;
            this.Kind   = kind;
            this.Detail = detail;
        }

        public override string ToString() =>
            string.IsNullOrEmpty(this.Detail) ? $"{this.Format} {this.Kind}" : $"{this.Format} {this.Kind}: {this.Detail}";
    }

    // One stream of every SDK and wrapper event. It writes each to the Unity log, which is where the
    // device check reads them, and the development panel lists them as they arrive.
    public class AdEventLog
    {
        public event Action<AdEvent> Recorded;

        public void Record(AdFormat format, AdEventKind kind, string detail = null)
        {
            var adEvent = new AdEvent(format, kind, detail);
            var line    = "[LevelPlay] " + adEvent;
            if (kind is AdEventKind.InitFailed or AdEventKind.LoadFailed or AdEventKind.ShowFailed)
                Debug.LogWarning(line);
            else
                Debug.Log(line);
            this.Recorded?.Invoke(adEvent);
        }
    }
    #endif
}
