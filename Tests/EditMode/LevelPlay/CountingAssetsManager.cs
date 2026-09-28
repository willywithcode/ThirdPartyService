namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Addressable;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Blueprints;
    using UnityEngine.AddressableAssets;
    using Object = UnityEngine.Object;

    // Serves Settings at the LevelPlaySettings blueprint address, null for every other key, and
    // records each key it was asked for.
    internal sealed class CountingAssetsManager : IAssetsManager
    {
        public readonly List<string> LoadedKeys = new();
        public          LevelPlaySettings Settings;

        public T LoadAsset<T>(string key) where T : Object
        {
            this.LoadedKeys.Add(key);
            return key == "LevelPlaySettings" ? this.Settings as T : null;
        }

        public UniTask<T> LoadAssetAsync<T>(string key, IProgress<float> progress = null) where T : Object => throw new NotSupportedException();
        public UniTask<T> LoadAssetAsync<T>(AssetReferenceT<T> reference, IProgress<float> progress = null) where T : Object => throw new NotSupportedException();
        public UniTask<Dictionary<string, T>> LoadAssetsAsync<T>(IEnumerable<string> keys, IProgress<float> progress = null) where T : Object => throw new NotSupportedException();
        public bool TryGet<T>(string key, out T asset) where T : Object => throw new NotSupportedException();
        public bool IsLoaded(string key) => throw new NotSupportedException();
        public int GetRefCount(string key) => throw new NotSupportedException();
        public void Release(string key) => throw new NotSupportedException();
        public void ReleaseAll() => throw new NotSupportedException();
        public UniTask<long> GetDownloadSizeAsync(IEnumerable<object> keysOrLabels) => throw new NotSupportedException();
        public UniTask<long> GetDownloadSizeForAllAsync() => throw new NotSupportedException();
        public UniTask<Guid> DownloadDependenciesAsync(IEnumerable<object> keysOrLabels, bool autoReleaseOnComplete = false) => throw new NotSupportedException();
        public UniTask<Guid> DownloadAllAsync(bool autoReleaseOnComplete = false) => throw new NotSupportedException();
        public bool TryGetDownloadStatus(Guid handleId, out DownloadInfo info) => throw new NotSupportedException();
        public UniTask<DownloadInfo> AwaitDownloadAsync(Guid handleId, bool autoRelease = true) => throw new NotSupportedException();
        public void ReleaseDownload(Guid handleId) => throw new NotSupportedException();
        public UniTask<bool> CheckCatalogUpdatesAsync() => throw new NotSupportedException();
        public UniTask<int> UpdateCatalogsAsync() => throw new NotSupportedException();
        public void Dispose() { }

        public event Action<string, Object>    OnAssetLoaded     { add { } remove { } }
        public event Action<string, Exception> OnAssetLoadFailed { add { } remove { } }
        public event Action                    OnCatalogUpdated  { add { } remove { } }
    }
    #endif
}
