namespace ThirdPartyService.Tests.EditMode.Admob
{
    #if Admob
    using NUnit.Framework;
    using ThirdPartyService.ServiceImplementation.AdsService.Admob.Common;

    // The retry schedule AdMob uses after a failed load. It must match LevelPlay's, so a player on
    // either network waits the same amount before the next attempt.
    public class AdmobLoadBackoffTests
    {
        [TestCase(0, 2f)]
        [TestCase(1, 2f)]
        [TestCase(2, 4f)]
        [TestCase(3, 8f)]
        [TestCase(4, 16f)]
        [TestCase(5, 32f)]
        [TestCase(6, 64f)]
        public void DelayDoubles_FromTwoSeconds(int consecutiveFailures, float expected)
        {
            Assert.That(AdmobLoadBackoff.DelaySeconds(consecutiveFailures), Is.EqualTo(expected));
        }

        [Test]
        public void DelayIsCapped_SoANeverFillingUnitKeepsTryingAboutOnceAMinute()
        {
            Assert.That(AdmobLoadBackoff.DelaySeconds(7), Is.EqualTo(AdmobLoadBackoff.MaxDelaySeconds));
            Assert.That(AdmobLoadBackoff.DelaySeconds(50), Is.EqualTo(AdmobLoadBackoff.MaxDelaySeconds));
        }

        #if LevelPlay
        [Test]
        public void AdmobAndLevelPlay_AgreeOnEveryDelay()
        {
            // The two copies exist only because LevelPlay's lives behind its own define. If one is
            // ever retuned, this fails instead of letting the pair drift apart unnoticed.
            for (var failures = 0; failures <= 10; failures++)
            {
                Assert.That(
                    AdmobLoadBackoff.DelaySeconds(failures),
                    Is.EqualTo(ServiceImplementation.AdsService.LevelPlay.Common.RetryBackoff.DelaySeconds(failures)),
                    $"the two providers disagree after {failures} failures");
            }
        }
        #endif
    }
    #endif
}
