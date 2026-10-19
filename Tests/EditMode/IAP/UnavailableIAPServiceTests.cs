namespace ThirdPartyService.Tests.EditMode.IAP
{
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.IAPService.DummyIAP;

    public sealed class UnavailableIAPServiceTests
    {
        [Test]
        public void Buying_Fails_AndNeverGrantsTheProduct()
        {
            var iap       = new UnavailableIAPService();
            string failed = null;
            var granted   = false;

            iap.BuyProductID("no_ads", _ => granted = true, reason => failed = reason);

            Assert.IsFalse(granted, "A build without Unity IAP must not hand out purchases (the Editor Dummy does).");
            Assert.AreEqual(UnavailableIAPService.Reason, failed);
        }

        [Test]
        public void NothingIsAvailableOrOwned_AndQueriesStillAnswer()
        {
            var iap = new UnavailableIAPService();

            Assert.IsFalse(iap.IsInitialized);
            Assert.IsFalse(iap.IsProductAvailable("no_ads"));
            Assert.IsFalse(iap.IsProductOwned("no_ads"));
            Assert.AreEqual("$1.99", iap.GetPriceById("no_ads", "$1.99"));
            Assert.AreEqual("no_ads", iap.GetProductData("no_ads").Id);

            var restored = false;
            iap.RestorePurchases(() => restored = true);
            Assert.IsTrue(restored, "Restore completes with nothing restored.");
        }
    }
}
