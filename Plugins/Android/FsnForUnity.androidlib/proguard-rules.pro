# Unity reaches these through JNI (AndroidJavaObject / AndroidJavaProxy) by name.
-keep class com.ads.fsnforunity.fsnlib.FsnAd { public *; }
-keep interface com.ads.fsnforunity.fsnlib.OnFsnLoadListener { *; }
-keep interface com.ads.fsnforunity.fsnlib.OnFsnCompletedListener { *; }
