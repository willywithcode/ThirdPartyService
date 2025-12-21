namespace ThirdPartyService.Core.RemoteConfig
{
    public interface IRemoteConfig
    {
        public bool IsReady();
        public T GetValue<T>(string key, T defaultValue = default(T));
    }
}