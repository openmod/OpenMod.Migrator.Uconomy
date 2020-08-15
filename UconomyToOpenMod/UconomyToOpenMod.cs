using System;
using System.Reflection;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using fr34kyn01535.Uconomy;
using HarmonyLib;
using OpenMod.API.Plugins;
using OpenMod.Core.Users;
using OpenMod.Extensions.Economy.Abstractions;
using OpenMod.Unturned.Plugins;

[assembly:
    PluginMetadata("UconomyToOpenMod", Author = "Rube200", Website = "https://github.com/rube200/UconomyToOpenMod")]

namespace RG.UconomyToOpenMod
{
    // ReSharper disable once UnusedMember.Global
    public class UconomyToOpenMod : OpenModUnturnedPlugin
    {
        private readonly IEconomyProvider m_EconomyProvider;

        private readonly MethodBase m_RiseBalanceChecked =
            typeof(Uconomy).GetMethod("OnBalanceChecked", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly MethodBase m_RiseBalanceUpdated =
            typeof(Uconomy).GetMethod("BalanceUpdated", BindingFlags.Instance | BindingFlags.NonPublic);

        public UconomyToOpenMod(IEconomyProvider economyProvider, IServiceProvider serviceProvider) : base(
            serviceProvider)
        {
            m_EconomyProvider = economyProvider;
        }

        protected override UniTask OnLoadAsync()
        {
            Harmony.Patch(m_OriginalGetBalance, m_TargetGetBalance);
            Harmony.Patch(m_OriginalIncreaseBalance, m_TargetIncreaseBalance);
            Harmony.Patch(m_OriginalCheckSchema, m_TargetCheckSchema);
            Harmony.Patch(m_OriginalCheckSetupAccount, m_TargetCheckSetupAccount);

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

        private async Task<decimal> OnIncreaseBalance(string id, decimal increaseBy)
        {
            var balance =
                await m_EconomyProvider.UpdateBalanceAsync(id, KnownActorTypes.Player, increaseBy, "UconomyBridge");
            m_RiseBalanceUpdated.Invoke(Uconomy.Instance, new object[] {id, increaseBy});
            return balance;
        }

        private Task<decimal> OnGetBalance(string id)
        {
            var balance = m_EconomyProvider.GetBalanceAsync(id, KnownActorTypes.Player);
            m_RiseBalanceChecked.Invoke(Uconomy.Instance, new object[] {id, balance});
            return balance;
        }

        #region Patches_Info

        private readonly MethodInfo m_OriginalGetBalance =
            typeof(DatabaseManager).GetMethod(nameof(DatabaseManager.GetBalance),
                BindingFlags.Instance | BindingFlags.Public);

        private readonly HarmonyMethod m_TargetGetBalance = new HarmonyMethod(typeof(HarmonyPatchesToUconomy),
            nameof(HarmonyPatchesToUconomy.GetBalancePatch));

        private readonly MethodInfo m_OriginalIncreaseBalance =
            typeof(DatabaseManager).GetMethod(nameof(DatabaseManager.IncreaseBalance),
                BindingFlags.Instance | BindingFlags.Public);

        private readonly HarmonyMethod m_TargetIncreaseBalance = new HarmonyMethod(typeof(HarmonyPatchesToUconomy),
            nameof(HarmonyPatchesToUconomy.IncreaseBalancePatch));

        private readonly MethodInfo m_OriginalCheckSchema =
            typeof(DatabaseManager).GetMethod("CheckSchema", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly HarmonyMethod m_TargetCheckSchema = new HarmonyMethod(typeof(HarmonyPatchesToUconomy),
            nameof(HarmonyPatchesToUconomy.CheckSchemaPatch));

        private readonly MethodInfo m_OriginalCheckSetupAccount =
            typeof(DatabaseManager).GetMethod(nameof(DatabaseManager.CheckSetupAccount),
                BindingFlags.Instance | BindingFlags.Public);

        private readonly HarmonyMethod m_TargetCheckSetupAccount = new HarmonyMethod(typeof(HarmonyPatchesToUconomy),
            nameof(HarmonyPatchesToUconomy.CheckSetupAccountPatch));

        #endregion
    }
}