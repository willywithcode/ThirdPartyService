namespace ThirdPartyService.Tests.EditMode.Admob
{
    #if Admob
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Common;
    using ThirdPartyService.ServiceImplementation.Analytics.Firebase;

    // What an AdMob OnAdPaid becomes in Firebase's ad_impression. AdMob pays in micros of the
    // currency it names, so a mistake here logs revenue a million times too high, or in the wrong
    // currency; and it must say AdMob, so its revenue is not counted as LevelPlay's.
    public class AdmobImpressionTests
    {
        [Test]
        public void ValueIsConvertedFromMicros()
        {
            var impression = AdmobImpression.From(AdmobImpression.Rewarded, "unit", "AdMob Network", 12_345, "USD");

            Assert.That(impression.Revenue, Is.EqualTo(0.012345d).Within(1e-12));
        }

        [Test]
        public void FirebaseMapping_SaysAdMob_AndKeepsTheReportedCurrency()
        {
            var mapped = AdImpressionFirebaseParameters.Map(
                AdmobImpression.From(AdmobImpression.Interstitial, "ca-app-pub-1/2", "AdMob Network", 2_500_000, "EUR"));

            Assert.That(mapped["ad_platform"], Is.EqualTo("AdMob"));
            Assert.That(mapped["ad_source"], Is.EqualTo("AdMob Network"));
            Assert.That(mapped["ad_format"], Is.EqualTo("interstitial"));
            Assert.That(mapped["ad_unit_name"], Is.EqualTo("ca-app-pub-1/2"));
            Assert.That(mapped["currency"], Is.EqualTo("EUR"));
            Assert.That(mapped["value"], Is.TypeOf<double>().And.EqualTo(2.5d));
        }

        [Test]
        public void MissingSourceOrUnit_LogsEmptyStrings_NotNull()
        {
            var mapped = AdImpressionFirebaseParameters.Map(AdmobImpression.From(AdmobImpression.Banner, null, null, 0, "USD"));

            Assert.That(mapped["ad_source"], Is.EqualTo(""));
            Assert.That(mapped["ad_unit_name"], Is.EqualTo(""));
            Assert.That(mapped["value"], Is.EqualTo(0d));
        }
    }
    #endif
}
