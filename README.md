# ThirdPartyService

## Scripting defines (what each game turns on)

A provider is compiled, registered and put into the build only when the game defines its symbol
(Player Settings > Other Settings > Scripting Define Symbols, per platform). The Editor always gets the Dummies.

| Define | Turns on | Without it |
|---|---|---|
| `Admob`, `MAX`, `IronSource`, `LevelPlay` | that ad network's providers | not compiled or registered |
| `FSN` (Android) | full screen natives (`FsnFullScreenNativeAds`) and the `FsnForUnity.androidlib` module | `UnavailableFullScreenNativeAds`; `FsnGradleStripper` takes the androidlib out of the Gradle project, so no FSN code, resources or `FsnActivity` ship |
| `UMP` (Android) | the UMP consent flow (`ConsentService`, `AndroidUmpBridge`) and the UMP SDK, added to Gradle by `UmpGradlePostprocessor` unless another plugin already brings it | `DummyConsentService`, and no UMP SDK from this library. A game that declares an AdMob app ID without `UMP` gets a build warning: AdMob needs consent in the EEA and UK |
| `FIREBASE_ANALYTICS`, `APPSFLYER_ANALYTICS` | those analytics providers | `NullAnalyticsService` when neither is set |
| `FIREBASE_REMOTE_CONFIG` | Firebase Remote Config | `DefaultValuesRemoteConfig` |
| `UNITY_PURCHASING` / `UNITY_PURCHASING_V5` (the latter set automatically from the IAP package version) | Unity IAP | `UnavailableIAPService` in builds: nothing can be bought. The Editor uses `DummyIAPService`, where every purchase succeeds |

## AdMob app ID (each game declares its own)

The AdMob app ID (`ca-app-pub-...~...`) belongs to the game, never to this library, which ships no
ID. A game that uses AdMob declares it in **one** place in its own repository:

1. **With the Google Mobile Ads Unity plugin** (the usual case): in the plugin settings,
   *Assets > Google Mobile Ads > Settings > Android App ID*
   (`Assets/GoogleMobileAds/Resources/GoogleMobileAdsSettings.asset`). The plugin writes it into the
   manifest; nothing else is needed.
2. **Without that plugin:** create *Create > ThirdParty > Consent Settings* at
   `Assets/Resources/ThirdPartyService/ConsentSettings.asset` in the game and fill
   `androidAdMobAppId`. `ConsentManifestPostprocessor` writes it into the manifest.

At runtime `ConsentAppIdProvider` reads the ID back from the app's manifest
(`com.google.android.gms.ads.APPLICATION_ID`). When that is empty, the consent flow skips UMP as before.

Every Android build checks the declaration in `ConsentManifestPostprocessor` and **fails**, with a message saying where to set
the ID, when:
- the Google Mobile Ads plugin is present with an empty Android App ID;
- a `ConsentSettings` asset exists with an empty ID;
- both places hold IDs that differ;
- the build carries the AdMob SDK (`play-services-ads`) but no ID is declared anywhere.

A game with neither place filled and no AdMob SDK in its build is left alone.

### Updating the submodule past this change

Before this change the library carried one game's ID in
`ServiceImplementation/ConsentService/Resources/ThirdPartyService/ConsentSettings.asset`. That asset
is gone. After updating:

1. If the game uses the Google Mobile Ads plugin, check that *Google Mobile Ads > Settings > Android
   App ID* holds **this game's** ID. That is the only place to keep it.
2. If it does not use the plugin, create `Assets/Resources/ThirdPartyService/ConsentSettings.asset`
   in the game with **this game's** ID.
3. Delete any other copy of `ConsentSettings` the game still has with a different value; the build
   fails while two different IDs exist.
4. Build for Android and check the manifest:
   `aapt2 dump xmltree --file AndroidManifest.xml app.apk | grep -A1 APPLICATION_ID` shows one entry
   with this game's ID.


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

A game that defines `FSN` must also (without `FSN` the androidlib is stripped from the build):

1. put `com.google.android.gms:play-services-ads` in the Android build (the library only
   compiles against it), with the Google Mobile Ads Unity plugin or an androidlib of its own;
2. declare its AdMob app ID in its own repository (see "AdMob app ID" below);
3. skip `Show` after Remove Ads; the service does not check it.
