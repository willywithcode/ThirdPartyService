namespace ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk
{
    #if LevelPlay
    using System;
    using System.Threading;
    using Cysharp.Threading.Tasks;

    // Real-time delays, so retries and the reward grace window still run while a fullscreen ad has
    // the game paused or timeScale at zero.
    public class RealtimeAdsScheduler : IAdsScheduler
    {
        public IDisposable Schedule(float delaySeconds, Action action)
        {
            var handle = new Handle();
            Run(delaySeconds, action, handle.Token).Forget();
            return handle;
        }

        private static async UniTaskVoid Run(float delaySeconds, Action action, CancellationToken token)
        {
            var cancelled = await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds), DelayType.Realtime, cancellationToken: token)
                .SuppressCancellationThrow();
            if (!cancelled && !token.IsCancellationRequested) action();
        }

        // Dispose cancels first: disposing a CancellationTokenSource on its own leaves its token
        // uncancelled and the delay running.
        private sealed class Handle : IDisposable
        {
            private readonly CancellationTokenSource source = new();
            private          bool                    disposed;

            public CancellationToken Token => this.source.Token;

            public void Dispose()
            {
                if (this.disposed) return;
                this.disposed = true;
                this.source.Cancel();
                this.source.Dispose();
            }
        }
    }
    #endif
}
