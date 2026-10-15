namespace ThirdPartyService.ServiceImplementation.IAPService.DummyIAP
{
    using System;
    using System.Collections.Generic;
    using ThirdPartyService.Core.IAPService;

    public class DummyIAPService : IIAPService, IIAPRestoreResultService
    {
        private Dictionary<string, IAPModel> catalog = new();
        private readonly HashSet<string> owned = new();
        public bool IsInitialized => true;

        public void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production")
        {
            this.catalog = iapPack ?? new Dictionary<string, IAPModel>();
        }

        public void BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            foreach (var product in this.catalog.Values)
                if (product.Id == productId && product.ProductType == ProductType.NonConsumable) this.owned.Add(productId);
            onComplete?.Invoke(productId);
        }

        public string GetPriceById(string productId, string defaultPrice)
        {
            // Return a dummy price
            return defaultPrice;
        }

        public void RestorePurchases(Action onComplete)
        {
            this.RestorePurchasesWithResult(_ => onComplete?.Invoke());
        }
        public void RestorePurchasesWithResult(Action<IAPRestoreResult> onComplete)
            => onComplete?.Invoke(new IAPRestoreResult(true, new List<string>(this.owned)));

        public bool IsProductOwned(string productId)
        {
            return this.owned.Contains(productId);
        }

        public bool IsProductAvailable(string productId)
        {
            // Simulate that all products are available
            return true;
        }

        public ProductData GetProductData(string productId)
        {
            // Return dummy product data
            return new()
            {
                Id           = productId,
                Price        = 0.0m,
                CurrencyCode = "USD"
            };
        }
    }
}
