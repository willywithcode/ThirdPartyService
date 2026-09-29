namespace ThirdPartyService.ServiceImplementation.ConsentService
{
    using System;
    #if UNITY_ANDROID && !UNITY_EDITOR
    using UnityEngine;
    #endif

    public static class GppCcpaConsent
    {
        // IAB GPP pre-parsed fields: 1 is opted out, 2 is did not opt out, 0 is N/A.
        // Google UMP writes US National (7), or Florida (13), in the app's default preferences.
        public static bool? FromValues(string sectionIds, string nationalSale, string nationalSharing, string floridaSale, string floridaSharing)
        {
            if (string.IsNullOrEmpty(sectionIds)) return null;
            foreach (var id in sectionIds.Split('_'))
            {
                if (id == "7") return Choice(nationalSale, nationalSharing);
                if (id == "13") return Choice(floridaSale, floridaSharing);
            }
            return null;
        }

        private static bool? Choice(string sale, string sharing)
        {
            if (sale == "1" || sharing == "1") return true;
            if (sale == "2" && sharing == "2") return false;
            return null;
        }
    }

    #if UNITY_ANDROID && !UNITY_EDITOR
    public sealed class AndroidGppCcpaConsentReader : ICcpaConsentReader
    {
        private readonly ConsentService consent;

        public AndroidGppCcpaConsentReader(ConsentService consent) => this.consent = consent;

        public bool? ReadOptOut()
        {
            if (!this.consent.HasCurrentConsent) return null;
            try
            {
                using var activity = new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity");
                using var preferences = new AndroidJavaClass("android.preference.PreferenceManager")
                    .CallStatic<AndroidJavaObject>("getDefaultSharedPreferences", activity);
                using var values = preferences.Call<AndroidJavaObject>("getAll");
                string Read(string key)
                {
                    using var value = values.Call<AndroidJavaObject>("get", key);
                    return value?.Call<string>("toString");
                }
                return GppCcpaConsent.FromValues(Read("IABGPP_GppSID"),
                    Read("IABGPP_USNAT_SaleOptOut"), Read("IABGPP_USNAT_SharingOptOut"),
                    Read("IABGPP_USFL_SaleOptOut"), Read("IABGPP_USFL_SharingOptOut"));
            }
            catch (Exception error)
            {
                Debug.LogWarning($"GPP choice unavailable: {error.Message}");
                return null;
            }
        }
    }
    #endif
}
