namespace ThirdPartyService.ServiceImplementation.IAPService.DummyIAP
{
    using System;
    using System.Collections.Generic;
    using ThirdPartyService.Core.IAPService;

    public class DummyIAPService : IIAPService
    {
        public bool IsInitialized => true;

        public void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production")
        {
            // Dummy initialization logic (if any)
        }

        public void BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            // Simulate a successful purchase
            onComplete?.Invoke(productId);
        }

        public string GetPriceById(string productId, string defaultPrice)
        {
            // Return a dummy price
            return defaultPrice;
        }

        public void RestorePurchases(Action onComplete)
        {
            // Simulate restoring purchases
            onComplete?.Invoke();
        }

        public bool IsProductOwned(string productId)
        {
            // Simulate that no products are owned
            return false;
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