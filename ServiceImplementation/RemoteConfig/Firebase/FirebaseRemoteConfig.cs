namespace ThirdPartyService.ServiceImplementation.RemoteConfig.Firebase
{
    using System;
    using System.Threading.Tasks;
    using global::Firebase;
    using ThirdPartyService.Core.RemoteConfig;
    using UnityEngine;
    using RemoteConfig = global::Firebase.RemoteConfig.FirebaseRemoteConfig;

    public class FirebaseRemoteConfig : IRemoteConfig
    {
        private bool         isReady;
        private RemoteConfig remoteConfig;

        public FirebaseRemoteConfig()
        {
            this.InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();
                if (dependencyStatus == DependencyStatus.Available)
                {
                    this.remoteConfig = RemoteConfig.DefaultInstance;
                    await this.FetchAndActivateAsync();
                    this.isReady = true;
                    Debug.Log("[FirebaseRemoteConfig] Initialized successfully");
                } else
                {
                    Debug.LogError($"[FirebaseRemoteConfig] Could not resolve dependencies: {dependencyStatus}");
                }
            } catch (Exception e)
            {
                Debug.LogError($"[FirebaseRemoteConfig] Initialization failed: {e.Message}");
            }
        }

        private async Task FetchAndActivateAsync()
        {
            try
            {
                await this.remoteConfig.FetchAsync(TimeSpan.Zero);
                await this.remoteConfig.ActivateAsync();
                Debug.Log("[FirebaseRemoteConfig] Fetch and activate completed");
            } catch (Exception e)
            {
                Debug.LogError($"[FirebaseRemoteConfig] Fetch failed: {e.Message}");
            }
        }

        public bool IsReady()
        {
            return this.isReady;
        }

        public T GetValue<T>(string key, T defaultValue = default)
        {
            if (!this.isReady || this.remoteConfig == null)
            {
                Debug.LogWarning($"[FirebaseRemoteConfig] Not ready, returning default value for key: {key}");
                return defaultValue;
            }

            try
            {
                var configValue = this.remoteConfig.GetValue(key);

                if (typeof(T) == typeof(string))
                {
                    return (T)(object)configValue.StringValue;
                }
                if (typeof(T) == typeof(bool))
                {
                    return (T)(object)configValue.BooleanValue;
                }
                if (typeof(T) == typeof(long))
                {
                    return (T)(object)configValue.LongValue;
                }
                if (typeof(T) == typeof(int))
                {
                    return (T)(object)(int)configValue.LongValue;
                }
                if (typeof(T) == typeof(double))
                {
                    return (T)(object)configValue.DoubleValue;
                }
                if (typeof(T) == typeof(float))
                {
                    return (T)(object)(float)configValue.DoubleValue;
                }

                Debug.LogWarning($"[FirebaseRemoteConfig] Unsupported type {typeof(T)} for key: {key}");
                return defaultValue;
            } catch (Exception e)
            {
                Debug.LogError($"[FirebaseRemoteConfig] Error getting value for key {key}: {e.Message}");
                return defaultValue;
            }
        }
    }
}