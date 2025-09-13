namespace ThirdPartyService.ServiceImplementation.UnityIAP.IAPService
{
    using System;
    using System.Collections.Generic;
    using Codice.Client.Common.WebApi;
    using ThirdPartyService.Core.IAPService;
    using UnityEngine.Purchasing;

    public class UnityIAPService : IIAPService
    {
        private Action<string>               onPurchaseComplete, onPurchaseFailed;
        private IStoreController             mStoreController;
        private IExtensionProvider           mStoreExtensionProvider;
        private Dictionary<string, IAPModel> iapPacks;
        private bool                         isInitialized = false;
        public  bool                         IsInitialized => this.isInitialized;

        public async void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production")
        {
            if (this.mStoreController != null) return;
            this.iapPacks = iapPack;

        }

        public void BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            throw new NotImplementedException();
        }

        public string GetPriceById(string productId, string defaultPrice)
        {
            throw new NotImplementedException();
        }

        public void RestorePurchases(Action onComplete)
        {
            throw new NotImplementedException();
        }

        public bool IsProductOwned(string productId)
        {
            throw new NotImplementedException();
        }

        public bool IsProductAvailable(string productId)
        {
            throw new NotImplementedException();
        }

        public ProductData GetProductData(string productId)
        {
            throw new NotImplementedException();
        }
    }
}