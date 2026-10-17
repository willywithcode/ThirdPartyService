package com.ads.fsnforunity.fsnlib;

import android.Manifest;
import android.annotation.SuppressLint;
import android.app.Activity;
import android.app.Application;
import android.app.Dialog;
import android.content.Context;
import android.graphics.Color;
import android.graphics.drawable.ColorDrawable;
import android.os.Bundle;
import android.os.Handler;
import android.os.Looper;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import android.view.WindowManager;
import android.widget.ImageView;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.annotation.RequiresPermission;
import androidx.core.graphics.Insets;
import androidx.core.view.ViewCompat;
import androidx.core.view.WindowCompat;
import androidx.core.view.WindowInsetsCompat;
import androidx.core.view.WindowInsetsControllerCompat;

import com.google.android.gms.ads.AdListener;
import com.google.android.gms.ads.AdLoader;
import com.google.android.gms.ads.AdRequest;
import com.google.android.gms.ads.AdapterResponseInfo;
import com.google.android.gms.ads.LoadAdError;
import com.google.android.gms.ads.ResponseInfo;
import com.google.android.gms.ads.VideoOptions;
import com.google.android.gms.ads.nativead.MediaView;
import com.google.android.gms.ads.nativead.NativeAd;
import com.google.android.gms.ads.nativead.NativeAdOptions;
import com.google.android.gms.ads.nativead.NativeAdView;

public class FsnAd extends Application {

    private String adUnitId = "";
    private int countDownSec = 3;
    private boolean loadNewAfterClose = true;
    private boolean startMuteVideo = false;
    private boolean autoDismissWhenAdClicked = false;

    private NativeAd nativeAd;
    private long lastLoadTimeMs = 0;

    private boolean isAdLoading = false;

    private OnFsnLoadListener onFsnLoadListener;

    private AdDialog dialog;

    private final Handler mainHandler = new Handler(Looper.getMainLooper());

    public void Init(String adUnitId, int countDownSec, boolean loadNewAfterClose, boolean startMuteVideo, boolean autoDismissWhenAdClicked) {
        this.adUnitId = adUnitId;
        this.countDownSec = countDownSec;
        this.loadNewAfterClose = loadNewAfterClose;
        this.startMuteVideo = startMuteVideo;
        this.autoDismissWhenAdClicked = autoDismissWhenAdClicked;
    }

    public void SetFsnListener(OnFsnLoadListener onFsnLoadListener) {
        this.onFsnLoadListener = onFsnLoadListener;
    }

    @RequiresPermission(Manifest.permission.INTERNET)
    public void LoadAd(Activity activity) {
        mainHandler.post(() -> DoLoadAd(activity));
    }

    @RequiresPermission(Manifest.permission.INTERNET)
    private void DoLoadAd(Activity activity) {
        if (IsAdReady() || isAdLoading) {
            return;
        }

        Context applicationContext = activity.getApplicationContext();
        isAdLoading = true;

        if (onFsnLoadListener != null) {
            onFsnLoadListener.OnLoadingStarted();
        }

        executeOnNewThread(() -> {
            AdLoader adLoader = new AdLoader.Builder(applicationContext, adUnitId)
                    .forNativeAd(nativeAd -> mainHandler.post(() -> {
                        this.nativeAd = nativeAd;
                        this.lastLoadTimeMs = System.currentTimeMillis();

                        nativeAd.setOnPaidEventListener(adValue -> executeOnNewThread(() -> {
                            long valueMicros = adValue.getValueMicros();
                            String currencyCode = adValue.getCurrencyCode();
                            String adSource = getAdSource(nativeAd);

                            if (onFsnLoadListener != null) {
                                onFsnLoadListener.OnAdPaid(adSource, adUnitId, valueMicros, currencyCode);
                            }
                        }));

                        isAdLoading = false;
                        if (onFsnLoadListener != null) {
                            onFsnLoadListener.OnLoadingCompleted(0, null);
                        }
                    }))
                    .withAdListener(new AdListener() {
                        @Override
                        public void onAdFailedToLoad(@NonNull LoadAdError adError) {
                            mainHandler.post(() -> {
                                isAdLoading = false;
                                if (onFsnLoadListener != null) {
                                    onFsnLoadListener.OnLoadingCompleted(adError.getCode(), adError.getMessage());
                                }
                            });
                        }

                        @Override
                        public void onAdClicked() {
                            if (dialog != null && autoDismissWhenAdClicked) {
                                dialog.setDismissAdOnNextResume(true);
                            }
                        }
                    })
                    .withNativeAdOptions(new NativeAdOptions.Builder()
                            .setRequestMultipleImages(true)
                            .setVideoOptions(new VideoOptions.Builder()
                                    .setStartMuted(startMuteVideo)
                                    .build())
                            .setAdChoicesPlacement(NativeAdOptions.ADCHOICES_TOP_RIGHT)
                            .build())
                    .build();

//            Bundle extras = new Bundle();
//            extras.putBoolean("request_full_screen_native_content", true);
//            extras.putBoolean("enable_native_video_blur_effect", true);

            adLoader.loadAd(new AdRequest.Builder()
//                    .addNetworkExtrasBundle(AdMobAdapter.class, extras)
                    .build());
        });
    }

    @NonNull
    private static String getAdSource(NativeAd nativeAd) {
        String adSource = "";
        if (nativeAd != null) {
            ResponseInfo responseInfo = nativeAd.getResponseInfo();
            if (responseInfo != null) {
                AdapterResponseInfo loadedAdapterResponseInfo = responseInfo.getLoadedAdapterResponseInfo();
                if (loadedAdapterResponseInfo != null) {
                    adSource = loadedAdapterResponseInfo.getAdSourceName();
                }
            }
        }
        return adSource;
    }

    @RequiresPermission(Manifest.permission.INTERNET)
    public void ShowAd(Activity activity, OnFsnCompletedListener onFsnCompletedListener) {
        mainHandler.post(() -> {
            if (dialog != null) {
                if (onFsnCompletedListener != null) {
                    onFsnCompletedListener.OnAdCompleted("Another ad is showing");
                }
                return;
            }

            if (IsAdReady()) {
                FsnActivity.start(activity, new FsnActivity.OnActivityLifecycleChange() {
                    @RequiresPermission(Manifest.permission.INTERNET)
                    @Override
                    public void onCreated(Activity fsnActivity) {
                        dialog = new AdDialog(fsnActivity);
                        dialog.setData(nativeAd, countDownSec);
                        dialog.setOnDismissListener(dialogInterface -> {
                            dialog = null;

                            if (!fsnActivity.isDestroyed() && !fsnActivity.isFinishing()) {
                                fsnActivity.finish();
                            }

                            if (loadNewAfterClose) {
                                LoadAd(activity);
                            }

                            if (onFsnCompletedListener != null) {
                                onFsnCompletedListener.OnAdCompleted("");
                            }
                        });
                        dialog.show();

                        nativeAd = null;
                    }

                    @Override
                    public void onDestroy(Activity activity) {
                        if (dialog != null) {
                            dialog.dismiss();
                            dialog = null;
                        }
                    }
                });
            } else {
                LoadAd(activity);

                if (onFsnCompletedListener != null) {
                    onFsnCompletedListener.OnAdCompleted("No ad available");
                }
            }
        });
    }

    public boolean IsAdReady() {
        return nativeAd != null && System.currentTimeMillis() - lastLoadTimeMs < 60 * 60 * 1000L;
    }

    public boolean IsAdLoading() {
        return isAdLoading;
    }

    private void executeOnNewThread(Runnable runnable) {
        new Thread(runnable).start();
    }

    private static class AdDialog extends Dialog {

        private NativeAd nativeAd;
        private int countDownSec;
        private long currentCountDownMs = 0L;
        private boolean dismissAdOnNextResume = false;

        private final Handler countDownHandler = new Handler(Looper.getMainLooper());
        private final Handler dismissHandler = new Handler(Looper.getMainLooper());

        public AdDialog(@NonNull Context context) {
            super(context, android.R.style.Theme_Black_NoTitleBar_Fullscreen);
        }

        @Override
        public void onDetachedFromWindow() {
            super.onDetachedFromWindow();
            countDownHandler.removeCallbacksAndMessages(null);
            dismissHandler.removeCallbacksAndMessages(null);
        }

        @Override
        protected void onCreate(Bundle savedInstanceState) {
            super.onCreate(savedInstanceState);
            setContentView(R.layout.layout_fsn_dialog);

            Window window = getWindow();
            if (window != null) {
                window.setBackgroundDrawable(new ColorDrawable(Color.TRANSPARENT));
                window.setLayout(ViewGroup.LayoutParams.MATCH_PARENT, ViewGroup.LayoutParams.MATCH_PARENT);
            }
            setCancelable(false);

            View safeRootView = findViewById(R.id.safeRootView);

            int closeBtnSize = getContext().getResources().getDimensionPixelSize(R.dimen.fsn_close_btn_size);
            int adChoiceSize = getContext().getResources().getDimensionPixelSize(R.dimen.fsn_ad_choice_size);

            ViewCompat.setOnApplyWindowInsetsListener(safeRootView, (view, windowInsets) -> {
                Insets cutout = windowInsets.getInsets(WindowInsetsCompat.Type.displayCutout());
                view.setPadding(0, Math.max(adChoiceSize, cutout.top - closeBtnSize / 2), 0, 0);
                return windowInsets;
            });

            findViewById(R.id.btnClose).setVisibility(View.GONE);
            findViewById(R.id.iv_close).setVisibility(View.GONE);
            findViewById(R.id.tv_countdown).setVisibility(View.VISIBLE);

            currentCountDownMs = countDownSec * 1000L;
            populateData();

            findViewById(R.id.btnClose).setVisibility(View.VISIBLE);
            findViewById(R.id.tv_countdown).setVisibility(View.VISIBLE);
            doCountDown();
        }

        private void registerCloseBtnClick() {
            findViewById(R.id.btnClose).setOnClickListener(view -> {
                if (currentCountDownMs > 0) return;
                dismiss();
            });
        }

        @SuppressLint("SetTextI18n")
        private void doCountDown() {
            View ivClose = findViewById(R.id.iv_close);
            TextView tvCountDown = findViewById(R.id.tv_countdown);
            View btnClose = findViewById(R.id.btnClose);
            tvCountDown.setText("" + currentCountDownMs / 1000L);

            countDownHandler.removeCallbacksAndMessages(null);
            countDownHandler.postDelayed(() -> {
                currentCountDownMs -= 1000L;
                if (currentCountDownMs <= -1000) {
                    btnClose.setVisibility(View.VISIBLE);
                    ivClose.setVisibility(View.VISIBLE);
                    registerCloseBtnClick();
                }
                else if (currentCountDownMs <= 0) {
                    btnClose.setVisibility(View.GONE);
                    tvCountDown.setVisibility(View.GONE);
                    doCountDown();
                } else {
                    doCountDown();
                }
            }, 1000L);
        }

        private void populateData() {
            if (nativeAd == null) return;

            String headline = nativeAd.getHeadline();
            String cta = nativeAd.getCallToAction();
            NativeAd.Image icon = nativeAd.getIcon();

            NativeAdView nativeAdView = findViewById(R.id.native_ad_view);
            TextView callToActionView = findViewById(R.id.cta);
            MediaView mediaView = findViewById(R.id.media_view);
            ImageView iconView = findViewById(R.id.icon);
            TextView headlineView = findViewById(R.id.headline);

            mediaView.setMediaContent(nativeAd.getMediaContent());
            nativeAdView.setMediaView(mediaView);
//            if (nativeAd.getMediaContent() != null && nativeAd.getMediaContent().hasVideoContent()) {
//                if (mediaView.getLayoutParams() != null) {
//                    mediaView.getLayoutParams().height = getContext().getResources().getDimensionPixelSize(R.dimen.fsn_ad_media_video_max_height);
//                    mediaView.requestLayout();
//                }
//            }

            headlineView.setText(headline);
            nativeAdView.setHeadlineView(headlineView);

            TextView bodyView = findViewById(R.id.body);
            if (bodyView != null) {
                if (nativeAd.getBody() != null) {
                    bodyView.setText(nativeAd.getBody());
                    nativeAdView.setBodyView(bodyView);
                } else {
                    bodyView.setVisibility(View.GONE);
                }
            }

            callToActionView.setText(cta);
            nativeAdView.setCallToActionView(callToActionView);

            if (icon != null) {
                iconView.setVisibility(View.VISIBLE);
                iconView.setImageDrawable(icon.getDrawable());
                nativeAdView.setIconView(iconView);
            } else {
                iconView.setVisibility(View.GONE);
            }

            nativeAdView.setNativeAd(nativeAd);
        }

        @Override
        public void onWindowFocusChanged(boolean hasFocus) {
            super.onWindowFocusChanged(hasFocus);
            if (hasFocus) {
                setupFullScreenDialog();

                if (dismissAdOnNextResume) {
                    dismissAdOnNextResume = false;
                    dismissHandler.postDelayed(() -> {
                        if (isShowing()) {
                            dismiss();
                        }
                    }, 500L);
                }
            }
        }

        private void setupFullScreenDialog() {
            Window window = getWindow();
            if (window != null) {
                WindowCompat.setDecorFitsSystemWindows(window, false);

                WindowInsetsControllerCompat controller =
                        new WindowInsetsControllerCompat(window, window.getDecorView());

                controller.hide(WindowInsetsCompat.Type.systemBars());

                controller.setSystemBarsBehavior(
                        WindowInsetsControllerCompat.BEHAVIOR_SHOW_TRANSIENT_BARS_BY_SWIPE
                );

                if (android.os.Build.VERSION.SDK_INT >= android.os.Build.VERSION_CODES.P) {
                    WindowManager.LayoutParams lp = window.getAttributes();
                    lp.layoutInDisplayCutoutMode =
                            WindowManager.LayoutParams.LAYOUT_IN_DISPLAY_CUTOUT_MODE_SHORT_EDGES;
                    window.setAttributes(lp);
                }
            }
        }

        public void setData(NativeAd nativeAd, int countDownSec) {
            this.nativeAd = nativeAd;
            this.countDownSec = countDownSec;
        }

        public void setDismissAdOnNextResume(boolean dismissAdOnNextResume) {
            this.dismissAdOnNextResume = dismissAdOnNextResume;
        }
    }
}
