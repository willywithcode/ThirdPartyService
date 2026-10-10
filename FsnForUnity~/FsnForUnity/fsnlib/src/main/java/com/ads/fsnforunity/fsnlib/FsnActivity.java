package com.ads.fsnforunity.fsnlib;

import android.app.Activity;
import android.content.Intent;
import android.content.pm.ActivityInfo;
import android.os.Bundle;

import androidx.annotation.Nullable;

public class FsnActivity extends Activity {
    private static OnActivityLifecycleChange onActivityLifecycleChange;

    public static void start(Activity activity, OnActivityLifecycleChange onActivityLifecycleChange) {
        FsnActivity.onActivityLifecycleChange = onActivityLifecycleChange;
        activity.startActivity(new Intent(activity, FsnActivity.class));
    }

    @Override
    protected void onCreate(@Nullable Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);

        setRequestedOrientation(ActivityInfo.SCREEN_ORIENTATION_LOCKED);

        if (onActivityLifecycleChange == null) {
            finish();
            return;
        }

        onActivityLifecycleChange.onCreated(this);
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        if (onActivityLifecycleChange != null) {
            onActivityLifecycleChange.onDestroy(this);
            onActivityLifecycleChange = null;
        }
    }

    public interface OnActivityLifecycleChange {
        void onCreated(Activity activity);

        void onDestroy(Activity activity);
    }
}
