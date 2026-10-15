namespace ThirdPartyService.Core.IAPService
{
    using System;
    using System.Collections.Generic;

    // Optional capability: legacy IIAPService callers keep their completion-only contract.
    public interface IIAPRestoreResultService { void RestorePurchasesWithResult(Action<IAPRestoreResult> onComplete); }
    public sealed class IAPRestoreResult
    {
        public bool Success { get; }
        public IReadOnlyList<string> RestoredProductIds { get; }
        public string Error { get; }
        public IAPRestoreResult(bool success, IReadOnlyList<string> restoredProductIds, string error = null)
        { Success = success; RestoredProductIds = restoredProductIds ?? Array.Empty<string>(); Error = error; }
    }
}
