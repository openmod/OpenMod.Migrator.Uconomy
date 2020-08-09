using Cysharp.Threading.Tasks;
using fr34kyn01535.Uconomy;
using HarmonyLib;
using OpenMod.API.Plugins;
using OpenMod.Core.Users;
using OpenMod.Extensions.Economy.Abstractions;
using OpenMod.Unturned.Plugins;
using System;
using System.Reflection;
using System.Threading.Tasks;

[assembly: PluginMetadata("UconomyToEconomyProvider", Author = "Rube200", Website = "https://github.com/rube200/UconomyToEconomyProvider")]
namespace RG.UconomyToEconomyProvider
{
    public class UconomyToEconomyProvider : OpenModUnturnedPlugin
    {
        #region Patches_Info
        private readonly MethodInfo OriginalGetBalance = typeof(DatabaseManager).GetMethod(nameof(DatabaseManager.GetBalance), BindingFlags.Instance | BindingFlags.Public);
        private readonly HarmonyMethod TargetGetBalance = new HarmonyMethod(typeof(HarmonyPatchesToUconomy), nameof(HarmonyPatchesToUconomy.GetBalancePatch));

        private readonly MethodInfo OriginalIncreaseBalance = typeof(DatabaseManager).GetMethod(nameof(DatabaseManager.IncreaseBalance), BindingFlags.Instance | BindingFlags.Public);
        private readonly HarmonyMethod TargetIncreaseBalance = new HarmonyMethod(typeof(HarmonyPatchesToUconomy), nameof(HarmonyPatchesToUconomy.IncreaseBalancePatch));

        private readonly MethodInfo OriginalCheckSchema = typeof(DatabaseManager).GetMethod("CheckSchema", BindingFlags.Instance | BindingFlags.NonPublic);
        private readonly HarmonyMethod TargetCheckSchema = new HarmonyMethod(typeof(HarmonyPatchesToUconomy), nameof(HarmonyPatchesToUconomy.CheckSchemaPatch));

        private readonly MethodInfo OriginalCheckSetupAccount = typeof(DatabaseManager).GetMethod(nameof(DatabaseManager.CheckSetupAccount), BindingFlags.Instance | BindingFlags.Public);
        private readonly HarmonyMethod TargetCheckSetupAccount = new HarmonyMethod(typeof(HarmonyPatchesToUconomy), nameof(HarmonyPatchesToUconomy.CheckSetupAccountPatch));
        #endregion

        private readonly IEconomyProvider m_EconomyProvider;

        public UconomyToEconomyProvider(IEconomyProvider economyProvider, IServiceProvider serviceProvider) : base(serviceProvider)
        {
            m_EconomyProvider = economyProvider;
        }

        protected override UniTask OnLoadAsync()
        {
            Harmony.Patch(OriginalGetBalance, TargetGetBalance);
            Harmony.Patch(OriginalIncreaseBalance, TargetIncreaseBalance);
            Harmony.Patch(OriginalCheckSchema, TargetCheckSchema);
            Harmony.Patch(OriginalCheckSetupAccount, TargetCheckSetupAccount);

            HarmonyPatchesToUconomy.OnIncreaseBalance += OnIncreaseBalance;
            HarmonyPatchesToUconomy.OnGetBalance += OnGetBalance;
            return UniTask.CompletedTask;
        }

        protected override UniTask OnUnloadAsync()
        {
            //Auto unpatch
            HarmonyPatchesToUconomy.OnGetBalance -= OnGetBalance;
            HarmonyPatchesToUconomy.OnIncreaseBalance -= OnIncreaseBalance;
            return UniTask.CompletedTask;
        }


        private MethodBase RiseBalanceChecked => typeof(Uconomy).GetMethod("OnBalanceChecked", BindingFlags.Instance | BindingFlags.NonPublic);
        private MethodBase RiseBalanceUpdated => typeof(Uconomy).GetMethod("BalanceUpdated", BindingFlags.Instance | BindingFlags.NonPublic);

        private async Task<decimal> OnIncreaseBalance(string id, decimal increaseBy)
        {
            var balance = await m_EconomyProvider.UpdateBalanceAsync(id, KnownActorTypes.Player, increaseBy);
            RiseBalanceUpdated.Invoke(Uconomy.Instance, new object[] { id, increaseBy});
            return balance;
        }

        private Task<decimal> OnGetBalance(string id)
        {
            var balance = m_EconomyProvider.GetBalanceAsync(id, KnownActorTypes.Player);
            RiseBalanceChecked.Invoke(Uconomy.Instance, new object[] { id, balance });
            return balance;
        }
    }
}
