namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using System.Collections;
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;
    using UnityEngine;
    using UnityEngine.TestTools;

    // The real scheduler, on UniTask's editor player loop: disposing a handle must stop its action,
    // since the retry and reward-grace logic relies on it.
    public class RealtimeAdsSchedulerTests
    {
        [UnityTest]
        public IEnumerator DisposedHandle_NeverRuns_WhileAnUndisposedOneDoes()
        {
            var scheduler = new RealtimeAdsScheduler();
            var kept      = false;
            var disposed  = false;

            scheduler.Schedule(0.05f, () => kept = true);
            scheduler.Schedule(0.05f, () => disposed = true).Dispose();

            var deadline = Time.realtimeSinceStartup + 3f;
            while (!kept && Time.realtimeSinceStartup < deadline) yield return null;
            var settle = Time.realtimeSinceStartup + 0.2f;
            while (Time.realtimeSinceStartup < settle) yield return null;

            Assert.That(kept, Is.True, "the undisposed action must run, or this test proves nothing");
            Assert.That(disposed, Is.False);
        }
    }
    #endif
}
