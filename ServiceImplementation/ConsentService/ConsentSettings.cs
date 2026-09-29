namespace ThirdPartyService.ServiceImplementation.ConsentService
{
    using UnityEngine;

    [CreateAssetMenu(fileName = "ConsentSettings", menuName = "ThirdParty/Consent Settings")]
    public sealed class ConsentSettings : ScriptableObject
    {
        [Tooltip("AdMob Android App ID (ca-app-pub-...~...). Empty disables UMP and manifest injection.")]
        public string androidAdMobAppId;
    }

    public sealed class ConsentAppIdProvider : IConsentAppIdProvider
    {
        public string AppId => Resources.Load<ConsentSettings>("ThirdPartyService/ConsentSettings")?.androidAdMobAppId;
    }
}
