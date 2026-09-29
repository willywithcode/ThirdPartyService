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
    public class FirebaseAnalytics : IAnalyticsService, IAdRevenueService, IInitializable
    {
        // Resolving dependencies takes several frames, and events fired in the meantime are real:
        // anything reported at startup would otherwise be dropped every single run. They wait here
        // and go out once Firebase is up.
        private const int MAX_PENDING_EVENTS = 64;

        private readonly List<(string EventName, Dictionary<string, string> EventParams)> pendingEvents = new();
        private readonly List<AdImpression> pendingImpressions = new();

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
