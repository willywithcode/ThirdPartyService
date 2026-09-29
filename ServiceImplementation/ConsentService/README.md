# Android consent setup

`Resources/ThirdPartyService/ConsentSettings.asset` holds the Android AdMob **App ID**
(`ca-app-pub-…~…`). The ID is currently empty. Fill it after creating the AdMob app and
publishing its GDPR and US state messages. The build processor writes
`com.google.android.gms.ads.APPLICATION_ID` into the Unity library manifest only when this field
is nonempty. At runtime, an empty field skips UMP and reports consent gathered with no
privacy-options entry.

UMP is pinned to `com.google.android.ump:user-messaging-platform:4.0.0` by
`Editor/UmpDependencies.xml`. Google's Android setup guide names this version:
https://developers.google.com/admob/android/privacy

LevelPlay 9.5.1 documents automatic UMP GDPR/TCF passthrough, but no automatic US state
GPP handling. The Android GPP reader uses the IAB default SharedPreferences keys
`IABGPP_GppSID`, `IABGPP_USNAT_SaleOptOut`, `IABGPP_USNAT_SharingOptOut`, and the
corresponding `USFL` fields (Florida). An explicit `1` in sale or sharing means opted out;
both fields `2` mean no opt-out; missing or inapplicable fields leave CCPA unset. These
values are read only after a successful UMP info update and form completion.

Sources:
- https://docs.unity.com/en-us/grow/levelplay/sdk/unity/regulation-advanced-settings
- https://github.com/InteractiveAdvertisingBureau/Global-Privacy-Platform/blob/main/Core/CMP%20API%20Specification.md
- https://support.google.com/admob/answer/10862202
