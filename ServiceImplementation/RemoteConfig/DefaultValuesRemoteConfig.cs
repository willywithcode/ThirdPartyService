namespace ThirdPartyService.ServiceImplementation.RemoteConfig
{
    using ThirdPartyService.Core.RemoteConfig;

    /// <summary>
    /// The remote config of a build without a remote config SDK (define FIREBASE_REMOTE_CONFIG
    /// off): never fetched, every key answers the caller's default.
    /// </summary>
    public sealed class DefaultValuesRemoteConfig : IRemoteConfig
    {
        public bool IsReady() => false;

        public T GetValue<T>(string key, T defaultValue = default) => defaultValue;
    }
}
