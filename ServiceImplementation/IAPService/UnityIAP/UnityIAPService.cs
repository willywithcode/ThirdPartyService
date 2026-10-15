namespace ThirdPartyService.ServiceImplementation.UnityIAP.IAPService
{
    #if UNITY_PURCHASING
    using System;
    using System.Collections.Generic;
    using ThirdPartyService.Core.IAPService;
    using Unity.Services.Core;
    using Unity.Services.Core.Environments;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Security;
    using ProductType = ThirdPartyService.Core.IAPService.ProductType;

    public class UnityIAPService : IIAPService, IIAPRestoreResultService, IDetailedStoreListener
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

            // Begin to configure our connection to Purchasing
            try
            {
                var options = new InitializationOptions()
                    .SetEnvironmentName(environment);

                await UnityServices.InitializeAsync(options);
            }
            catch (Exception exception)
            {
                // An error occurred during services initialization.
            }

            this.InitializePurchasing();
        }

        private void InitializePurchasing()
        {
            if (this.IsInitialized)
            {
                return;
            }

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
            this.AddAllProduct(builder);
            UnityPurchasing.Initialize(this, builder);
        }

        private void AddAllProduct(ConfigurationBuilder builder)
        {
            foreach (var iapPack in this.iapPacks.Values)
            {
                builder.AddProduct(iapPack.Id, this.ConvertToUnityProductType(iapPack.ProductType));
            }
        }

        private UnityEngine.Purchasing.ProductType ConvertToUnityProductType(ProductType productType)
        {
            return productType switch
            {
                ProductType.Consumable    => UnityEngine.Purchasing.ProductType.Consumable,
                ProductType.Subscription  => UnityEngine.Purchasing.ProductType.Subscription,
                ProductType.NonConsumable => UnityEngine.Purchasing.ProductType.NonConsumable,
                _                         => UnityEngine.Purchasing.ProductType.Consumable
            };
        }

        public void BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            if (this.IsInitialized)
            {
                // this.signalBus.Fire<OnStartDoingIAPSignal>();

                var product = this.mStoreController.products.WithID(productId);

                if (product is { availableToPurchase: true })
                {
                    // this.logger.Log($"Purchasing product asychronously: '{product.definition.id}'");

                    this.onPurchaseComplete = onComplete;
                    this.mStoreController.InitiatePurchase(product);
                }
                else
                {
                    this.onPurchaseFailed = onFailed;
                    // this.logger.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                this.InitializePurchasing();
                // this.logger.Log("BuyProductID FAIL. Not initialized.");
            }
        }

        public string GetPriceById(string productId, string defaultPrice)
        {
            var s = defaultPrice;

            if (!this.IsInitialized) return s;

            try
            {
                s = this.mStoreController.products.WithID(productId).metadata.localizedPriceString;

                if (string.IsNullOrWhiteSpace(s))
                {
                    s = defaultPrice;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return s;
        }

        public void RestorePurchases(Action onComplete)
            => this.RestorePurchasesWithResult(_ => onComplete?.Invoke());

        public void RestorePurchasesWithResult(Action<IAPRestoreResult> onComplete)
        {
            if (!this.IsInitialized)
            { onComplete?.Invoke(new IAPRestoreResult(false, null, "IAP is not initialized.")); return; }
            void Complete(bool success, string error)
            {
                var owned = new List<string>();
                if (success) foreach (var pack in this.iapPacks.Values)
                    if (pack.ProductType == ProductType.NonConsumable && this.IsProductOwned(pack.Id)) owned.Add(pack.Id);
                onComplete?.Invoke(new IAPRestoreResult(success, owned, error));
            }
            #if FAKE_RESTORE_PURCHASE
            Complete(true, null); return;
            #endif
            if (Application.platform is RuntimePlatform.IPhonePlayer or RuntimePlatform.OSXPlayer)
                this.mStoreExtensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions(Complete);
            else Complete(true, null); // Legacy Google ownership is restored at initialization.
        }

        public bool IsProductOwned(string productId)
        {
            if (!this.IsInitialized) return false;

            if (string.IsNullOrEmpty(productId)) return false;

            var pd = this.mStoreController.products.WithID(productId);

            if (!pd.hasReceipt) return false;
            // presume validity if not validate receipt.
            #if !UNITY_EDITOR
            var isValid = this.ValidateReceipt(pd.receipt, out var purchaseReceipts);

            return isValid;
            #endif
            return true;
        }

        public bool IsProductAvailable(string productId)
        {
            if (!this.IsInitialized) return false;

            var product = this.mStoreController.products.WithID(productId);

            return product != null && product.availableToPurchase;
        }

        public ProductData GetProductData(string productId)
        {
            var product = this.mStoreController.products.WithID(productId);

            return new ProductData()
            {
                Id           = productId,
                Price        = product.metadata.localizedPrice,
                CurrencyCode = product.metadata.isoCurrencyCode
            };
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            this.mStoreController        = controller;
            this.mStoreExtensionProvider = extensions;
            this.isInitialized           = true;
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message = null)
        {
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            if (this.onPurchaseComplete == null)
            {
                // this.signalBus.Fire(new OnRestorePurchaseCompleteSignal(args.purchasedProduct.definition.id));
            }
            else
            {
                // this.signalBus.Fire(new OnIAPPurchaseSuccessSignal() { ProductId = args.purchasedProduct.definition.id });
                // this.signalBus.Fire(new OnUnityIAPPurchaseSuccessSignal(args.purchasedProduct) );
            }

            this.onPurchaseComplete?.Invoke(args.purchasedProduct.definition.id);
            this.onPurchaseComplete = null;

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            this.onPurchaseFailed?.Invoke(product.definition.storeSpecificId);
            this.onPurchaseFailed = null;
            // this.signalBus.Fire(new OnIAPPurchaseFailedSignal() { ProductId = product.definition.storeSpecificId });
            // this.logger.Log($"OnPurchaseFailed: FAIL. Product: '{product.definition.storeSpecificId}', PurchaseFailureReason: {failureDescription}");
        }

        private bool ValidateReceipt(string receipt, out IPurchaseReceipt[] purchaseReceipts, bool logReceiptContent = true)
        {
            // default the out parameter to an empty array
            purchaseReceipts = Array.Empty<IPurchaseReceipt>();

            // Does the receipt has some content?
            if (string.IsNullOrEmpty(receipt))
            {
                return false;
            }

            var isValidReceipt = true; // presume validity for platforms with no receipt validation.
            // Unity IAP's receipt validation is only available for Apple app stores and Google Play store.
            #if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS
            byte[] googlePlayTangleData = null;
            byte[] appleTangleData = null;

            // Here we populate the secret keys for each platform.
            // Note that the code is disabled in the editor for it to not stop the EM editor code (due to ClassNotFound error)
            // from recreating the dummy AppleTangle and GoogleTangle classes if they were inadvertently removed.

// #if UNITY_ANDROID && !UNITY_EDITOR
            // googlePlayTangleData = GooglePlayTangle.Data();
// #endif

#if (UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) && !UNITY_EDITOR
            appleTangleData = AppleTangle.Data();
#endif

            // Prepare the validator with the secrets we prepared in the Editor obfuscation window.
            var validator = new CrossPlatformValidator(googlePlayTangleData, appleTangleData, Application.identifier);

            try
            {
                // On Google Play, result has a single product ID.
                // On Apple stores, receipts contain multiple products.
                var result = validator.Validate(receipt);

                // If the validation is successful, the result won't be null.
                if (result == null)
                {
                    isValidReceipt = false;
                }
                else
                {
                    purchaseReceipts = result;

                    // For informational purposes, we list the receipt(s)
                    if (logReceiptContent)
                    {
                        // this.logger.Log("Receipt contents:");

                        foreach (var productReceipt in result)
                        {
                            if (productReceipt == null) continue;
                            // this.logger.Log(productReceipt.productID);
                            // this.logger.Log(productReceipt.purchaseDate.ToString(CultureInfo.InvariantCulture));
                            // this.logger.Log(productReceipt.transactionID);
                        }
                    }
                }
            }
            catch (IAPSecurityException)
            {
                isValidReceipt = false;
            }
            #endif

            return isValidReceipt;
        }
    }
    #endif
}