namespace ThirdPartyService.ServiceImplementation.Analytics.Appsflyer.Blueprints
{
    using GameFoundation.Scripts.Addressable;
    using GameFoundation.Scripts.Blueprints.ScriptableObject.Attributes;
    using GameFoundation.Scripts.Blueprints.ScriptableObject.Services;
    using UnityEngine;

    [SOBlueprintAttributes(nameof(AppsflyerAnalyticsBlueprint))]
    public class AppsflyerBlueprintService : BaseSOBlueprintService<AppsflyerAnalyticsBlueprint>
    {
        public AppsflyerBlueprintService(IAssetsManager assetsManager) : base(assetsManager) { }
    }
    
    [CreateAssetMenu(fileName = "AppsflyerAnalyticsBlueprint", menuName = "ThirdParty/ServiceImplementation/Analytics/Appsflyer/AppsflyerAnalyticsBlueprint")]
    public class AppsflyerAnalyticsBlueprint : ScriptableObject
    {
        public string androidDevKey;
        public string iosDevKey;
        public string appId;
    }
}