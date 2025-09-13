namespace ThirdPartyService.Core.IAPService
{
    using System;
    using System.Collections.Generic;

    public interface IIAPService
    {
        bool        IsInitialized { get; }
        void        InitIapServices(Dictionary<string, IAPModel> iapPack,   string         environment = "production");
        void        BuyProductID(string                          productId, Action<string> onComplete  = null, Action<string> onFailed = null);
        string      GetPriceById(string                          productId, string         defaultPrice);
        void        RestorePurchases(Action                      onComplete);
        bool        IsProductOwned(string                        productId);
        bool        IsProductAvailable(string                    productId);
        ProductData GetProductData(string                        productId);
    }
}