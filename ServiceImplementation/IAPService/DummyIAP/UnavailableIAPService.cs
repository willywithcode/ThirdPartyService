namespace ThirdPartyService.ServiceImplementation.IAPService.DummyIAP
{
    using System;
    using System.Collections.Generic;
    using ThirdPartyService.Core.IAPService;

    /// <summary>
    /// A build without Unity IAP (neither UNITY_PURCHASING nor UNITY_PURCHASING_V5): IIAPService stays
    /// resolvable, but nothing can be bought, restored or owned. Unlike <see cref="DummyIAPService"/>,
    /// which the Editor uses and which reports every purchase as successful, this never grants anything.
    /// </summary>
    public sealed class UnavailableIAPService : IIAPService
    {
        public const string Reason = "In-app purchases are not in this build (no UNITY_PURCHASING or UNITY_PURCHASING_V5).";

        public bool IsInitialized => false;

        public void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production") { }

        public void BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null) => onFailed?.Invoke(Reason);

        public string GetPriceById(string productId, string defaultPrice) => defaultPrice;

        public void RestorePurchases(Action onComplete) => onComplete?.Invoke();

        public bool IsProductOwned(string productId) => false;

        public bool IsProductAvailable(string productId) => false;

        // As the Unity IAP services do for an unknown product: no price, no currency, never null.
        public ProductData GetProductData(string productId) => new() { Id = productId };
    }
}
