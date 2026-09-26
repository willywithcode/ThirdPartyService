namespace ThirdPartyService.ServiceImplementation
{
    #if FIREBASE_REMOTE_CONFIG || FIREBASE_ANALYTICS
    using System.Threading.Tasks;
    using global::Firebase;

    /// <summary>
    /// The one Firebase dependency check every Firebase service here waits on. Firebase throws if a
    /// second CheckAndFixDependenciesAsync starts while one is running, and the services initialize
    /// in the same frame, so each calling it would fail all but the first.
    /// </summary>
    internal static class FirebaseDependencies
    {
        private static Task<DependencyStatus> check;

        // A check that threw or was cancelled is started again, so the next caller can retry it.
        public static Task<DependencyStatus> CheckAndFixAsync()
        {
            if (check == null || check.IsFaulted || check.IsCanceled) check = FirebaseApp.CheckAndFixDependenciesAsync();
            return check;
        }
    }
    #endif
}
