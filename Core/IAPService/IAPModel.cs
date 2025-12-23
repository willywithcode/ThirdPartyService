namespace ThirdPartyService.Core.IAPService
{
    public class IAPModel
    {
        public string      Id          { get; set; }
        public ProductType ProductType { get; set; }
        public IAPModel(string id, ProductType productType)
        {
            this.Id          = id;
            this.ProductType = productType;
        }
    }

    public enum ProductType
    {
        Consumable,
        NonConsumable,
        Subscription
    }
}