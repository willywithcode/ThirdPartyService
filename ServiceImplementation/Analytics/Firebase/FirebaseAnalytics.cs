namespace ThirdPartyService.ServiceImplementation.Analytics.Firebase
{
    using System.Collections.Generic;
    using global::Firebase;
    using global::Firebase.Analytics;
    using global::Firebase.Extensions;
    using ThirdPartyService.Core.Analytics;
    using UnityEngine;
    using VContainer.Unity;
    public class FirebaseAnalytics : IAnalyticsService, IInitializable
    {
        private bool initFirebase = false;
        public void Initialize()
        {
            // if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.OSXEditor)
            // {
                Debug.Log("Init firebase load");
                FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
                {
                    var dependencyStatus = task.Result;
                    if (dependencyStatus == DependencyStatus.Available)
                    {
                        this.initFirebase = true;
                        Debug.Log("Init firebase");
                    } else
                    {
                        Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    }
                });
            // }
        }

        public void SendEvent(string eventName, Dictionary<string, string> eventParams)
        {
            if (!this.initFirebase)
            {
                Debug.LogWarning("Firebase Analytics not initialized.");
                return;
            }
            var firebaseParams = new Parameter[eventParams.Count];
            var index          = 0;
            foreach (var param in eventParams)
            {
                firebaseParams[index++] = new(param.Key, param.Value);
            }
            global::Firebase.Analytics.FirebaseAnalytics.LogEvent(eventName, firebaseParams);
        }
    }
}