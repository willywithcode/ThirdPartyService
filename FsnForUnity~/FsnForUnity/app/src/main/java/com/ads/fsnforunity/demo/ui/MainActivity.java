package com.ads.fsnforunity.demo.ui;

import android.os.Bundle;
import android.util.Log;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import androidx.appcompat.app.AppCompatActivity;

import com.ads.fsnforunity.demo.R;
import com.ads.fsnforunity.fsnlib.FsnAd;
import com.ads.fsnforunity.fsnlib.OnFsnLoadListener;

public class MainActivity extends AppCompatActivity {

    Button loadAd;
    Button showAd;
    TextView tv_ad_status;

    FsnAd fsnAd;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        fsnAd = new FsnAd();
        fsnAd.Init("ca-app-pub-3940256099942544/2247696110", 5, true, false, true);
        fsnAd.SetFsnListener(new OnFsnLoadListener() {
            @Override
            public void OnLoadingCompleted(int errorCode, String errorMessage) {
                tv_ad_status.setText(errorCode == 0 ? "Load success" : "Load failed - errorMessage = " + errorMessage);
            }

            @Override
            public void OnLoadingStarted() {
                tv_ad_status.setText("Loading Ad ...");
            }

            @Override
            public void OnAdPaid(String adSource, String adUnitId, double valueMicros, String currencyCode) {
                Log.e("AAAA", "OnAdPaid " + (valueMicros / 1000000f) + " " + currencyCode);
            }
        });

        loadAd = findViewById(R.id.loadAd);
        showAd = findViewById(R.id.showAd);
        tv_ad_status = findViewById(R.id.tv_ad_status);

        loadAd.setOnClickListener(view -> {
            fsnAd.LoadAd(MainActivity.this);
        });

        showAd.setOnClickListener(view -> {
            fsnAd.ShowAd(MainActivity.this, errorMessage -> Toast.makeText(MainActivity.this, "ShowAd completed, errorMessage = " + errorMessage, Toast.LENGTH_LONG).show());
        });

    }
}