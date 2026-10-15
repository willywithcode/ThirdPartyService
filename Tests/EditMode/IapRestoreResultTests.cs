namespace ThirdPartyService.Tests.EditMode
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using GameFoundation.Scripts.Patterns.SignalBus;
    using NUnit.Framework;
    using ThirdPartyService.Core.IAPService;
    using ThirdPartyService.ServiceImplementation.IAPService.DummyIAP;

    public sealed class IapRestoreResultTests
    {
        [Test] public void EmptySuccessfulDummyRestoreIsStoreSuccess()
        {
            IAPRestoreResult result = null; var service = new DummyIAPService();
            service.RestorePurchasesWithResult(value => result = value);
            Assert.That(result.Success, Is.True); Assert.That(result.RestoredProductIds, Is.Empty);
        }
        [Test] public void DummyRestoreReturnsOwnedNonConsumablesWithoutConsumableRewards()
        {
            var service = new DummyIAPService();
            service.InitIapServices(new Dictionary<string, IAPModel> {
                ["coins"] = new IAPModel("coins", ProductType.Consumable), ["noads"] = new IAPModel("noads", ProductType.NonConsumable) });
            service.BuyProductID("coins"); service.BuyProductID("noads"); IAPRestoreResult result = null;
            service.RestorePurchasesWithResult(value => result = value);
            Assert.That(result.RestoredProductIds, Is.EqualTo(new[] { "noads" })); Assert.That(service.IsProductOwned("noads"), Is.True);
        }
        [Test] public void LegacyDummyCompletionStillRunsOnce()
        { var count = 0; new DummyIAPService().RestorePurchases(() => count++); Assert.That(count, Is.EqualTo(1)); }
        [TestCase(true)] [TestCase(false)] public void V5RestoreForwardsSdkOutcomeAndCompletesCoalescedCallbacksOnce(bool success)
        {
            var type = typeof(IIAPService).Assembly.GetType("ThirdPartyService.ServiceImplementation.UnityIAP.IAPService.UnityIAPV5Service");
            if (type == null) Assert.Ignore("Unity Purchasing V5 backend is not enabled in this build.");
            var service = Activator.CreateInstance(type, new SignalBus());
            var callbacks = (List<Action<IAPRestoreResult>>)type.GetField("restoreCallbacks", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(service);
            var owned = (HashSet<string>)type.GetField("ownedProductIds", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(service); owned.Add("noads");
            var results = new List<IAPRestoreResult>(); callbacks.Add(results.Add); callbacks.Add(results.Add);
            var finish = type.GetMethod("FinishRestore", BindingFlags.Instance | BindingFlags.NonPublic);
            finish.Invoke(service, new object[] { success, success ? null : "store failure" });
            finish.Invoke(service, new object[] { success, null });
            Assert.That(results.Count, Is.EqualTo(2)); Assert.That(results[0].Success, Is.EqualTo(success));
            Assert.That(results[0].RestoredProductIds, success ? Is.EqualTo(new[] { "noads" }) : Is.Empty);
            Assert.That(results[0].Error, Is.EqualTo(success ? null : "store failure")); ((IDisposable)service).Dispose();
        }
    }
}
