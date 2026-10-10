package com.ads.fsnforunity.fsnlib;

public interface OnFsnLoadListener {
    void OnLoadingCompleted(int errorCode, String errorMessage);

    void OnLoadingStarted();

    void OnAdPaid(String adSource, String adUnitId, double valueMicros, String currencyCode);
}