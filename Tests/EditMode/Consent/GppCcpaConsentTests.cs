namespace ThirdPartyService.Tests.EditMode.Consent
{
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.ConsentService;

    public class GppCcpaConsentTests
    {
        [TestCase("7", "1", "2", null, null, true)]
        [TestCase("7", "2", "1", null, null, true)]
        [TestCase("7", "2", "2", null, null, false)]
        [TestCase("13", null, null, "1", "2", true)]
        [TestCase("13", null, null, "2", "2", false)]
        [TestCase("7", "0", "2", null, null, null)]
        [TestCase("2", "1", "1", null, null, null)]
        [TestCase(null, "1", "1", null, null, null)]
        public void ExplicitApplicableGppChoice_Only(string ids, string sale, string sharing, string floridaSale, string floridaSharing, bool? expected)
        {
            Assert.That(GppCcpaConsent.FromValues(ids, sale, sharing, floridaSale, floridaSharing), Is.EqualTo(expected));
        }
    }
}
