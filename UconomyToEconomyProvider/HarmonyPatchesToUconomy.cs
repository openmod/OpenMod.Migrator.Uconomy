using fr34kyn01535.Uconomy;
using HarmonyLib;
using Nito.AsyncEx;
using Steamworks;
using System.Threading.Tasks;

namespace RG.UconomyToEconomyProvider
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static class HarmonyPatchesToUconomy
    {
        internal delegate Task<decimal> GetBalance(string id);
        internal static event GetBalance OnGetBalance;

        internal static bool GetBalancePatch(ref decimal __result, string id)
        {
            if (OnGetBalance != null)
            {
                __result = AsyncContext.Run(async () => await OnGetBalance(id));
                return false;
            }
            else
                return true;
        }


        internal delegate Task<decimal> IncreaseBalance(string id, decimal increaseBy);
        internal static event IncreaseBalance OnIncreaseBalance;

        internal static bool IncreaseBalancePatch(ref decimal __result, string id, decimal increaseBy)
        {
            if (OnIncreaseBalance != null)
            {
                __result = AsyncContext.Run(async () => await OnIncreaseBalance(id, increaseBy));
                return false;
            }
            else
                return true;
        }


        #region Patched to be ignored

        internal static bool CheckSetupAccountPatch(CSteamID id)
        {
            return false;
        }

        internal static bool CheckSchemaPatch()
        {
            return false;
        }

        #endregion
    }
}
