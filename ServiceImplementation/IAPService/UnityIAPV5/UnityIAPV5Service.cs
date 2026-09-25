namespace ThirdPartyService.ServiceImplementation.UnityIAP.IAPService
{
    #if UNITY_PURCHASING_V5
    using System;
    using System.Collections.Generic;
    using Cysharp.Threading.Tasks;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using ThirdPartyService.Core.IAPService;
    using Unity.Services.Core;
    using Unity.Services.Core.Environments;
    using UnityEngine;
    using UnityEngine.Purchasing;
    using CoreProductType = ThirdPartyService.Core.IAPService.ProductType;
    using UnityProductType = UnityEngine.Purchasing.ProductType;

    /// <summary>
    /// Publishes new and recovered paid orders through OnIAPPurchasePendingSignal.
    /// Confirms immediately after synchronous signal dispatch returns.
    /// An in-session BuyProductID onComplete callback handles fulfillment when supplied;
    /// recovered purchases and calls without that callback use the pending-purchase signal.
    /// </summary>
    public class UnityIAPV5Service : IIAPService, IDisposable
    {
        private sealed class PurchaseRequest
        {
            public string ProductId;
            public string TransactionId;
            public Action<string> Complete;
            public Action<string> Failed;
            public bool CompleteInvoked;
        }

        private StoreController                     storeController;
        private Dictionary<string, IAPModel>        iapPacks;
        private readonly HashSet<string>            ownedProductIds          = new HashSet<string>();
        private readonly Dictionary<string, PendingOrder> pendingOrders = new();
        private readonly HashSet<string> confirmingOrders = new();
        private readonly HashSet<string> confirmedTransactions = new();
        private readonly SignalBus signalBus;
        private readonly List<Action> restoreCallbacks = new();
        private PurchaseRequest                     purchaseRequest;
        private bool                                isConnected;
        private bool                                productsFetched;
        private bool                                purchasesFetched;
        private bool                                initializing;
        private bool                                connecting;
        private bool                                connectionFailed;
        private bool                                fetchingPurchases;
        private bool                                restoring;
        private bool                                disposed;

        public bool IsInitialized => !this.disposed && this.isConnected && this.productsFetched && this.purchasesFetched;

        public UnityIAPV5Service(SignalBus signalBus)
        {
            this.signalBus = signalBus ?? throw new ArgumentNullException(nameof(signalBus));
        }

        public void InitIapServices(Dictionary<string, IAPModel> iapPack, string environment = "production")
        {
            if (this.disposed) throw new ObjectDisposedException(nameof(UnityIAPV5Service));
            if (this.initializing || this.IsInitialized) return;
            if (iapPack == null) throw new ArgumentNullException(nameof(iapPack));

            // Normalize by product ID; callers need not use product IDs as dictionary keys.
            var catalog = new Dictionary<string, IAPModel>();
            foreach (var model in iapPack.Values)
            {
                if (model == null || string.IsNullOrEmpty(model.Id))
                    throw new ArgumentException("Each IAP product must have an ID.", nameof(iapPack));
                catalog.Add(model.Id, new IAPModel(model.Id, model.ProductType));
            }

            this.iapPacks = catalog;
            this.initializing = true;
            this.InitializeAsync(environment).Forget();
        }

        private async UniTask InitializeAsync(string environment)
        {
            try
            {
                try
                {
                    await UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName(environment));
                }
                catch (Exception exception)
                {
                    // UGS is optional for native IAP, so the store can still be connected.
                    Debug.LogWarning($"[UnityIAPV5Service] UnityServices initialization failed: {exception}");
                }

                if (this.disposed) return;
                if (this.storeController == null)
                {
                    this.storeController = UnityIAPServices.StoreController();
                    this.RegisterCallbacks();
                    // Explicitly route fetched pending orders through the same handler below.
                    this.storeController.ProcessPendingOrdersOnPurchasesFetched(false);
                }

                await this.ConnectAndFetchAsync();
            }
            catch (Exception exception)
            {
                this.initializing = false;
                Debug.LogError($"[UnityIAPV5Service] Initialization failed: {exception}");
            }
        }

        private async UniTask ConnectAndFetchAsync()
        {
            try
            {
                this.productsFetched = false;
                this.purchasesFetched = false;
                this.connecting = true;
                this.connectionFailed = false;
                await this.storeController.Connect();
                this.connecting = false;
                if (this.disposed) return;
                // IAP 5.0.1 may complete Connect normally after OnStoreDisconnected.
                if (this.connectionFailed)
                {
                    this.initializing = false;
                    return;
                }
                this.isConnected = true;
                this.storeController.FetchProducts(this.BuildProductDefinitions());
            }
            catch (Exception exception)
            {
                this.isConnected = false;
                this.connecting = false;
                this.initializing = false;
                Debug.LogError($"[UnityIAPV5Service] Store connection failed: {exception}");
            }
        }

        private void RegisterCallbacks()
        {
            this.storeController.OnStoreDisconnected    += this.OnStoreDisconnected;
            this.storeController.OnProductsFetched      += this.OnProductsFetched;
            this.storeController.OnProductsFetchFailed  += this.OnProductsFetchFailed;
            this.storeController.OnPurchasePending      += this.OnPurchasePending;
            this.storeController.OnPurchaseConfirmed    += this.OnPurchaseConfirmed;
            this.storeController.OnPurchaseFailed       += this.OnPurchaseFailed;
            this.storeController.OnPurchaseDeferred     += this.OnPurchaseDeferred;
            this.storeController.OnPurchasesFetched     += this.OnPurchasesFetched;
            this.storeController.OnPurchasesFetchFailed += this.OnPurchasesFetchFailed;
            if (this.storeController.AppleStoreExtendedPurchaseService != null)
                this.storeController.AppleStoreExtendedPurchaseService.OnEntitlementRevoked += this.OnEntitlementRevoked;
        }

        private List<ProductDefinition> BuildProductDefinitions()
        {
            var definitions = new List<ProductDefinition>();

            foreach (var iapPack in this.iapPacks.Values)
            {
                definitions.Add(new ProductDefinition(iapPack.Id, this.ConvertToUnityProductType(iapPack.ProductType)));
            }

            return definitions;
        }

        private UnityProductType ConvertToUnityProductType(CoreProductType productType)
        {
            return productType switch
            {
                CoreProductType.Consumable    => UnityProductType.Consumable,
                CoreProductType.Subscription  => UnityProductType.Subscription,
                CoreProductType.NonConsumable => UnityProductType.NonConsumable,
                _                             => UnityProductType.Consumable
            };
        }

        public void BuyProductID(string productId, Action<string> onComplete = null, Action<string> onFailed = null)
        {
            if (!this.IsInitialized || string.IsNullOrEmpty(productId))
            {
                Debug.LogWarning("[UnityIAPV5Service] BuyProductID called before initialization completed.");
                onFailed?.Invoke(productId);
                return;
            }

            var product = this.storeController.GetProductById(productId);

            if (product is not { availableToPurchase: true })
            {
                Debug.LogWarning($"[UnityIAPV5Service] Product '{productId}' not found or not available to purchase.");
                onFailed?.Invoke(productId);
                return;
            }

            if (this.purchaseRequest != null || this.HasPendingProduct(productId))
            {
                Debug.LogWarning($"[UnityIAPV5Service] A purchase is still pending; cannot buy '{productId}'.");
                onFailed?.Invoke(productId);
                return;
            }

            this.purchaseRequest = new PurchaseRequest { ProductId = productId, Complete = onComplete, Failed = onFailed };
            try
            {
                this.storeController.PurchaseProduct(product);
            }
            catch (Exception exception)
            {
                this.FinishPurchaseRequest(productId, false);
                Debug.LogException(exception);
            }
        }

        public string GetPriceById(string productId, string defaultPrice)
        {
            if (!this.IsInitialized) return defaultPrice;

            var price = this.storeController.GetProductById(productId)?.metadata?.localizedPriceString;

            return string.IsNullOrWhiteSpace(price) ? defaultPrice : price;
        }

        public void RestorePurchases(Action onComplete)
        {
            #if FAKE_RESTORE_PURCHASE
            onComplete?.Invoke();
            return;
            #endif

            if (!this.IsInitialized)
            {
                Debug.LogWarning("[UnityIAPV5Service] RestorePurchases called but IAP not initialized.");
                onComplete?.Invoke();
                return;
            }

            if (onComplete != null) this.restoreCallbacks.Add(onComplete);
            if (this.restoring) return;
            this.restoring = true;

            // Complete restore only after our own ownership snapshot has been refreshed.
            if (Application.platform is RuntimePlatform.IPhonePlayer or RuntimePlatform.OSXPlayer)
            {
                this.storeController.RestoreTransactions((success, error) =>
                {
                    if (!success)
                    {
                        Debug.LogWarning($"[UnityIAPV5Service] Apple restore transactions failed: {error}");
                        this.FinishRestore();
                        return;
                    }

                    if (!this.disposed) this.FetchPurchases();
                });
            }
            // Google Play and others restore automatically; refresh the cache, then report on fetch.
            else
            {
                this.FetchPurchases();
            }
        }

        public bool IsProductOwned(string productId)
        {
            if (!this.IsInitialized || string.IsNullOrEmpty(productId)) return false;

            return this.ownedProductIds.Contains(productId);
        }

        public bool IsProductAvailable(string productId)
        {
            if (!this.IsInitialized) return false;

            var product = this.storeController.GetProductById(productId);

            return product is { availableToPurchase: true };
        }

        public ProductData GetProductData(string productId)
        {
            var metadata = this.storeController?.GetProductById(productId)?.metadata;

            return new ProductData
            {
                Id           = productId,
                Price        = metadata?.localizedPrice ?? 0m,
                CurrencyCode = metadata?.isoCurrencyCode
            };
        }

        #region Store event handlers

        private void OnProductsFetched(List<Product> products)
        {
            if (this.disposed || !this.isConnected) return;
            this.productsFetched = true;
            this.FetchPurchases();
        }

        private void OnProductsFetchFailed(ProductFetchFailed failure)
        {
            if (!this.productsFetched) this.initializing = false;
            Debug.LogWarning($"[UnityIAPV5Service] Products fetch failed: {failure}");
        }

        private void OnPurchasePending(PendingOrder order)
        {
            if (this.disposed) return;
            var transactionId = order?.Info?.TransactionID;
            var items = order?.CartOrdered?.Items();
            if (string.IsNullOrEmpty(transactionId) || items == null || items.Count != 1)
            {
                Debug.LogError("[UnityIAPV5Service] Expected a transaction ID and exactly one product. Leaving the order pending.");
                return;
            }

            if (this.confirmedTransactions.Contains(transactionId)) return;
            // Keep the latest SDK object for retries after a refetch/reconnection.
            this.pendingOrders[transactionId] = order;
            if (!this.confirmingOrders.Add(transactionId)) return;
            try
            {
                var productId = items[0].Product?.definition?.id;
                if (string.IsNullOrEmpty(productId))
                    throw new InvalidOperationException("The order contains a product without an ID.");
                if (this.purchaseRequest != null && this.purchaseRequest.TransactionId == null &&
                    this.purchaseRequest.ProductId == productId)
                    this.purchaseRequest.TransactionId = transactionId;
                if (this.purchaseRequest is { Complete: not null } request &&
                    request.TransactionId == transactionId)
                {
                    if (!request.CompleteInvoked)
                    {
                        request.Complete(productId);
                        request.CompleteInvoked = true;
                    }
                }
                else
                {
                    this.signalBus.Fire(new OnIAPPurchasePendingSignal(productId, transactionId));
                }

                if (!this.disposed) this.storeController.ConfirmPurchase(order);
            }
            catch (Exception exception)
            {
                this.confirmingOrders.Remove(transactionId);
                // A synchronous subscriber exception prevents confirmation; refetch can replay.
                Debug.LogError($"[UnityIAPV5Service] Pending purchase processing failed for '{transactionId}': {exception}");
            }
        }

        private void OnPurchaseConfirmed(Order order)
        {
            var transactionId = order?.Info?.TransactionID;
            if (transactionId != null) this.confirmingOrders.Remove(transactionId);
            // Fires with a ConfirmedOrder on success or a FailedOrder if confirmation failed.
            if (order is FailedOrder failedOrder)
            {
                Debug.LogWarning($"[UnityIAPV5Service] Purchase confirmation failed: {GetProductId(order)}, {failedOrder.FailureReason}, {failedOrder.Details}");
                return;
            }

            if (order is not ConfirmedOrder) return;
            if (!string.IsNullOrEmpty(transactionId))
            {
                this.confirmedTransactions.Add(transactionId);
                this.pendingOrders.Remove(transactionId);
            }
            foreach (var item in order.CartOrdered.Items())
            {
                var productId = item.Product.definition.id;
                this.CacheIfNonConsumable(productId);
                if (this.purchaseRequest?.TransactionId == transactionId)
                    this.FinishPurchaseRequest(productId, true);
            }
        }

        private void OnPurchaseFailed(FailedOrder order)
        {
            var productId = GetProductId(order);

            this.FinishPurchaseRequest(productId, false);

            Debug.LogWarning($"[UnityIAPV5Service] Purchase failed: {productId}, {order.FailureReason}, {order.Details}");
        }

        private void OnPurchaseDeferred(DeferredOrder order)
        {
            // e.g. iOS Ask-to-Buy: awaiting external approval. Nothing to grant yet.
            Debug.Log($"[UnityIAPV5Service] Purchase deferred: {GetProductId(order)}");
        }

        private void OnPurchasesFetched(Orders orders)
        {
            if (this.disposed || !this.isConnected) return;
            this.fetchingPurchases = false;
            this.ownedProductIds.Clear();
            foreach (var confirmed in orders.ConfirmedOrders)
            {
                // Already confirmed orders restore ownership, never consumable rewards.
                this.OnPurchaseConfirmed(confirmed);
            }

            foreach (var pending in orders.PendingOrders)
            {
                foreach (var item in pending.CartOrdered.Items()) this.CacheIfNonConsumable(item.Product.definition.id);
                this.OnPurchasePending(pending);
            }

            this.purchasesFetched = true;
            this.initializing = false;
            this.FinishRestore();
        }

        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription failure)
        {
            Debug.LogWarning($"[UnityIAPV5Service] Purchases fetch failed: {failure}");

            this.fetchingPurchases = false;
            this.initializing = false;
            this.FinishRestore();
        }

        private void OnStoreDisconnected(StoreConnectionFailureDescription description)
        {
            this.connectionFailed = true;
            this.isConnected = false;
            this.purchasesFetched = false;
            this.fetchingPurchases = false;
            this.confirmingOrders.Clear();
            this.FinishRestore();
            Debug.LogWarning($"[UnityIAPV5Service] Store disconnected: {description}");
            // One reconnect attempt; a failed attempt remains retryable via InitIapServices.
            if (!this.disposed && !this.connecting)
            {
                this.initializing = true;
                this.ConnectAndFetchAsync().Forget();
            }
        }

        #endregion

        #region Helpers

        private void OnEntitlementRevoked(string productId) => this.ownedProductIds.Remove(productId);

        private void FetchPurchases()
        {
            if (this.disposed || this.fetchingPurchases) return;
            this.fetchingPurchases = true;
            try
            {
                this.storeController.FetchPurchases();
            }
            catch (Exception exception)
            {
                this.fetchingPurchases = false;
                this.initializing = false;
                this.FinishRestore();
                Debug.LogException(exception);
            }
        }

        private bool HasPendingProduct(string productId)
        {
            foreach (var order in this.pendingOrders.Values)
                foreach (var item in order.CartOrdered.Items())
                    if (item.Product.definition.id == productId) return true;
            return false;
        }

        private void FinishPurchaseRequest(string productId, bool success)
        {
            if (this.purchaseRequest == null || this.purchaseRequest.ProductId != productId) return;
            var request = this.purchaseRequest;
            this.purchaseRequest = null;
            try
            {
                if (!success) request.Failed?.Invoke(productId);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private void FinishRestore()
        {
            this.restoring = false;
            var callbacks = this.restoreCallbacks.ToArray();
            this.restoreCallbacks.Clear();
            foreach (var callback in callbacks)
            {
                try { callback(); }
                catch (Exception exception) { Debug.LogException(exception); }
            }
        }

        public void Dispose()
        {
            if (this.disposed) return;
            this.disposed = true;
            if (this.storeController != null)
            {
                this.storeController.OnStoreDisconnected    -= this.OnStoreDisconnected;
                this.storeController.OnProductsFetched      -= this.OnProductsFetched;
                this.storeController.OnProductsFetchFailed  -= this.OnProductsFetchFailed;
                this.storeController.OnPurchasePending      -= this.OnPurchasePending;
                this.storeController.OnPurchaseConfirmed    -= this.OnPurchaseConfirmed;
                this.storeController.OnPurchaseFailed       -= this.OnPurchaseFailed;
                this.storeController.OnPurchaseDeferred     -= this.OnPurchaseDeferred;
                this.storeController.OnPurchasesFetched     -= this.OnPurchasesFetched;
                this.storeController.OnPurchasesFetchFailed -= this.OnPurchasesFetchFailed;
                if (this.storeController.AppleStoreExtendedPurchaseService != null)
                    this.storeController.AppleStoreExtendedPurchaseService.OnEntitlementRevoked -= this.OnEntitlementRevoked;
            }
            this.purchaseRequest = null;
            this.restoreCallbacks.Clear();
            this.pendingOrders.Clear();
            this.confirmingOrders.Clear();
            this.confirmedTransactions.Clear();
            this.ownedProductIds.Clear();
        }

        private void CacheIfNonConsumable(string productId)
        {
            if (productId != null && this.IsNonConsumable(productId))
            {
                this.ownedProductIds.Add(productId);
            }
        }

        private bool IsNonConsumable(string productId)
        {
            return this.iapPacks != null
                && this.iapPacks.TryGetValue(productId, out var model)
                && model.ProductType != CoreProductType.Consumable;
        }

        private static string GetProductId(Order order)
        {
            var items = order?.CartOrdered?.Items();

            if (items == null || items.Count == 0) return null;

            return items[0].Product?.definition?.id;
        }

        #endregion
    }
    #endif
}
