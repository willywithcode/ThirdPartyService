# ThirdPartyService

## Full screen native ads (FSN)

An AdMob native ad shown full screen, closable after a countdown.

- `Plugins/Android/FsnForUnity.androidlib`: the Android library, a copy of
  `FsnForUnity~/FsnForUnity/fsnlib/src/main/{java,res}`. The `~` keeps Unity from
  importing the Android Studio project; copy the sources again after changing it.
- `IFullScreenNativeAdsService`: `Load`, `IsReady` and `Show` by placement name.
  In the Editor it is `DummyFullScreenNativeAds`, always ready. An Android build with the
  define `FSN` gets `FsnFullScreenNativeAds`; any other build gets
  `UnavailableFullScreenNativeAds`, which never has an ad.
- `FsnSettings`: an Addressable ScriptableObject at the address `FsnSettings` listing each
  placement's ad units, tried in order.

A game that defines `FSN` must also:

1. put `com.google.android.gms:play-services-ads` in the Android build (the library only
   compiles against it), with the Google Mobile Ads Unity plugin or an androidlib of its own;
2. set its AdMob app id in `ServiceImplementation/ConsentService/Resources/ThirdPartyService/ConsentSettings.asset`,
   which `ConsentManifestPostprocessor` writes into the manifest;
3. skip `Show` after Remove Ads; the service does not check it.
