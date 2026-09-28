namespace ThirdPartyService.Tests.EditMode.LevelPlay
{
    #if LevelPlay
    using NUnit.Framework;
    using ThirdPartyService.Core.AdsService.BannerAds;
    using ThirdPartyService.ServiceImplementation.AdsService.LevelPlay.Sdk;
    using global::Unity.Services.LevelPlay;

    // The adapter's translations into LevelPlay's own presets.
    public class LevelPlaySdkAdapterTests
    {
        [Test]
        public void EveryBannerPosition_MapsToTheMatchingLevelPlayPreset()
        {
            Assert.That(LevelPlaySdkAdapter.ToLevelPlayPosition(BannerPosition.TopLeft), Is.SameAs(LevelPlayBannerPosition.TopLeft));
            Assert.That(LevelPlaySdkAdapter.ToLevelPlayPosition(BannerPosition.TopCenter), Is.SameAs(LevelPlayBannerPosition.TopCenter));
            Assert.That(LevelPlaySdkAdapter.ToLevelPlayPosition(BannerPosition.TopRight), Is.SameAs(LevelPlayBannerPosition.TopRight));
            Assert.That(LevelPlaySdkAdapter.ToLevelPlayPosition(BannerPosition.CenterLeft), Is.SameAs(LevelPlayBannerPosition.CenterLeft));
            Assert.That(LevelPlaySdkAdapter.ToLevelPlayPosition(BannerPosition.Centered), Is.SameAs(LevelPlayBannerPosition.Center));
            Assert.That(LevelPlaySdkAdapter.ToLevelPlayPosition(BannerPosition.CenterRight), Is.SameAs(LevelPlayBannerPosition.CenterRight));
            Assert.That(LevelPlaySdkAdapter.ToLevelPlayPosition(BannerPosition.BottomLeft), Is.SameAs(LevelPlayBannerPosition.BottomLeft));
            Assert.That(LevelPlaySdkAdapter.ToLevelPlayPosition(BannerPosition.BottomCenter), Is.SameAs(LevelPlayBannerPosition.BottomCenter));
            Assert.That(LevelPlaySdkAdapter.ToLevelPlayPosition(BannerPosition.BottomRight), Is.SameAs(LevelPlayBannerPosition.BottomRight));
        }

        [Test]
        public void MediumRectangle_UsesLevelPlaysMrecSize()
        {
            Assert.That(LevelPlaySdkAdapter.ToLevelPlaySize(BannerAdSize.MediumRectangle), Is.SameAs(LevelPlayAdSize.MEDIUM_RECTANGLE));
            Assert.That(LevelPlaySdkAdapter.ToLevelPlaySize(BannerAdSize.Banner), Is.SameAs(LevelPlayAdSize.BANNER));
        }
    }
    #endif
}
