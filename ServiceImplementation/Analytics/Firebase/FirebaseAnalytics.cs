namespace ThirdPartyService.ServiceImplementation.Analytics.Firebase
{
    #if FIREBASE_ANALYTICS
    using System.Collections.Generic;
    using global::Firebase;
    using global::Firebase.Analytics;
    using global::Firebase.Extensions;
    using ThirdPartyService.Core.Analytics;
    using UnityEngine;
    using VContainer.Unity;
    public class FirebaseAnalytics : IAnalyticsService, IAdRevenueService, IUserPropertyService, IAnalyticsConsentService, IInitializable
    {
        // Resolving dependencies takes several frames, and events fired in the meantime are real:
        // anything reported at startup would otherwise be dropped every single run. They wait here
        // and go out once Firebase is up.
        private const int MAX_PENDING_EVENTS = 64;

        private readonly List<(string EventName, Dictionary<string, string> EventParams)> pendingEvents = new();
        private readonly List<AdImpression> pendingImpressions = new();

        // A property only ever needs its latest value, so a name set twice before init is kept once
        // and the map is bounded by the number of names rather than by MAX_PENDING_EVENTS.
        private readonly Dictionary<string, string> pendingUserProperties = new();

        // Only the latest consent matters, and it goes out before anything queued, so the events
        // fired during startup are collected under it.
        private AnalyticsConsent? pendingConsent;

        private bool initFirebase = false;
        private bool initFailed   = false;

        public void Initialize()
        {
            // if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.OSXEditor)
            // {
                Debug.Log("Init firebase load");
                FirebaseDependencies.CheckAndFixAsync().ContinueWithOnMainThread(task =>
                {
                    var dependencyStatus = task.Result;
                    if (dependencyStatus == DependencyStatus.Available)
                    {
                        this.initFirebase = true;
                        Debug.Log("Init firebase");
                        this.FlushPendingEvents();
                    } else
                    {
                        this.initFailed = true;
                        this.pendingEvents.Clear();
                        this.pendingImpressions.Clear();
                        this.pendingUserProperties.Clear();
                        this.pendingConsent = null;
                        Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    }
                });
            // }
        }

        public void SendEvent(string eventName, Dictionary<string, string> eventParams)
        {
            if (!this.initFirebase)
            {
                this.QueueEvent(eventName, eventParams);
                return;
            }

            LogEvent(eventName, eventParams);
        }

        public void SendAdImpression(AdImpression impression)
        {
            if (!this.initFirebase)
            {
                if (!this.initFailed && this.pendingEvents.Count + this.pendingImpressions.Count < MAX_PENDING_EVENTS)
                    this.pendingImpressions.Add(impression);
                return;
            }
            LogImpression(impression);
        }

        public void SetConsent(AnalyticsConsent consent)
        {
            if (!this.initFirebase)
            {
                if (!this.initFailed) this.pendingConsent = consent;
                return;
            }

            ApplyConsent(consent);
        }

        private static void ApplyConsent(AnalyticsConsent consent)
        {
            static ConsentStatus Status(bool granted) => granted ? ConsentStatus.Granted : ConsentStatus.Denied;

            global::Firebase.Analytics.FirebaseAnalytics.SetConsent(new Dictionary<ConsentType, ConsentStatus>
            {
                [ConsentType.AnalyticsStorage]  = Status(consent.AnalyticsStorage),
                [ConsentType.AdStorage]         = Status(consent.AdStorage),
                [ConsentType.AdUserData]        = Status(consent.AdUserData),
                [ConsentType.AdPersonalization] = Status(consent.AdPersonalization),
            });
        }

        // Properties are flushed before the queued events, so the events fired during startup are
        // already labelled with them.
        public void SetUserProperty(string name, string value)
        {
            if (!this.initFirebase)
            {
                if (!this.initFailed) this.pendingUserProperties[name] = value;
                return;
            }

            global::Firebase.Analytics.FirebaseAnalytics.SetUserProperty(name, value);
        }

        public static Parameter[] ToFirebaseParameters(AdImpression impression)
        {
            var mapped = AdImpressionFirebaseParameters.Map(impression);
            var parameters = new List<Parameter>(mapped.Count);
            foreach (var pair in mapped)
                parameters.Add(pair.Value is double amount ? new Parameter(pair.Key, amount) : new Parameter(pair.Key, (string)pair.Value));
            return parameters.ToArray();
        }

        private static void LogImpression(AdImpression impression) =>
            global::Firebase.Analytics.FirebaseAnalytics.LogEvent("ad_impression", ToFirebaseParameters(impression));

        // Copied, because callers are free to reuse or mutate the dictionary they handed over once
        // SendEvent returns, and this one is read later.
        private void QueueEvent(string eventName, Dictionary<string, string> eventParams)
        {
            if (this.initFailed) return;

            if (this.pendingEvents.Count + this.pendingImpressions.Count >= MAX_PENDING_EVENTS)
            {
                Debug.LogWarning($"Firebase Analytics still initializing; dropping '{eventName}'.");
                return;
            }

            this.pendingEvents.Add((eventName, new Dictionary<string, string>(eventParams)));
        }

        private void FlushPendingEvents()
        {
            if (this.pendingConsent is { } consent) ApplyConsent(consent);
            this.pendingConsent = null;

            foreach (var property in this.pendingUserProperties)
            {
                global::Firebase.Analytics.FirebaseAnalytics.SetUserProperty(property.Key, property.Value);
            }

            this.pendingUserProperties.Clear();
            foreach (var (eventName, eventParams) in this.pendingEvents)
            {
                LogEvent(eventName, eventParams);
            }

            this.pendingEvents.Clear();
            foreach (var impression in this.pendingImpressions) LogImpression(impression);
            this.pendingImpressions.Clear();
        }

        private static void LogEvent(string eventName, Dictionary<string, string> eventParams)
        {
            var firebaseParams = new Parameter[eventParams.Count];
            var index          = 0;
            foreach (var param in eventParams)
            {
                firebaseParams[index++] = new(param.Key, param.Value);
            }
            global::Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, firebaseParams);
        }
    }
#endif
}
