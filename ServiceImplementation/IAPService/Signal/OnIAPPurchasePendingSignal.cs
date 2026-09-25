namespace ThirdPartyService.Core.IAPService
{
    /// <summary>
    /// A paid item awaiting fulfillment, including orders recovered on startup.
    /// The reward service must synchronously persist the reward and (TransactionId, ProductId)
    /// before returning. The IAP service confirms immediately after signal dispatch.
    /// Repeated delivery must be deduplicated by the reward service.
    /// </summary>
    public readonly struct OnIAPPurchasePendingSignal
    {
        public string ProductId { get; }
        public string TransactionId { get; }

        public OnIAPPurchasePendingSignal(string productId, string transactionId)
        {
            this.ProductId     = productId;
            this.TransactionId = transactionId;
        }
    }
}
