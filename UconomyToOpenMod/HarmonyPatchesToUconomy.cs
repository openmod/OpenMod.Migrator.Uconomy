using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Steamworks;

namespace RG.UconomyToOpenMod
{
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
    public static class HarmonyPatchesToUconomy
    {
        internal static event GetBalance OnGetBalance;

        // ReSharper disable once InconsistentNaming
        internal static bool GetBalancePatch(ref decimal __result, string id)
        {
            if (OnGetBalance == null)
                return true;

            __result = AsyncContext.Run(async () => await OnGetBalance(id));
            return false;
        }

        internal static event IncreaseBalance OnIncreaseBalance;

        // ReSharper disable once InconsistentNaming
        internal static bool IncreaseBalancePatch(ref decimal __result, string id, decimal increaseBy)
        {
            if (OnIncreaseBalance == null)
                return true;

            __result = AsyncContext.Run(async () => await OnIncreaseBalance(id, increaseBy));
            return false;
        }

        internal delegate Task<decimal> GetBalance(string id);


        internal delegate Task<decimal> IncreaseBalance(string id, decimal increaseBy);


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