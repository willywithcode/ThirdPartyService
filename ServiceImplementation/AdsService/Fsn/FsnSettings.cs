namespace ThirdPartyService.ServiceImplementation.AdsService.Fsn
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    [Serializable]
    public class FsnPlacement
    {
        public string placement;

        [Tooltip("AdMob native ad unit ids, tried in this order. Put a _high unit first.")]
        public List<string> adUnitIds = new();
    }

    /// <summary>
    /// The game's full screen native placements, loaded from the Addressable asset at the address
    /// <see cref="Address"/>. Only read when FSN is registered (define FSN, Android build).
    /// </summary>
    [CreateAssetMenu(fileName = "FsnSettings", menuName = "ThirdParty/ServiceImplementation/AdsService/Fsn/FsnSettings")]
    public class FsnSettings : ScriptableObject
    {
        public const string Address = "FsnSettings";

        [Tooltip("Seconds before the close button appears.")]
        public int closeAfterSeconds = 3;

        public bool startMuted;

        [Tooltip("Close the ad when the player returns from tapping it.")]
        public bool closeWhenAdClicked = true;

        [Tooltip("Load the next ad on a unit as soon as its ad is closed.")]
        public bool reloadAfterClose = true;

        public List<FsnPlacement> placements = new();

        public IReadOnlyList<string> AdUnitsOf(string placement)
        {
            foreach (var entry in this.placements)
            {
                if (entry != null && entry.placement == placement) return entry.adUnitIds;
            }
            return Array.Empty<string>();
        }
    }
}
