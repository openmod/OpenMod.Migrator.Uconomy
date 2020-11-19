#region

using System;
using Microsoft.Extensions.DependencyInjection;
using Rocket.Unturned.Player;
using Steamworks;

#endregion

namespace fr34kyn01535.Uconomy
{
    // ReSharper disable once UnusedMember.Global
    public sealed class Uconomy
    {
        public delegate void PlayerBalanceCheck(UnturnedPlayer player, decimal balance);

        public delegate void PlayerBalanceUpdate(UnturnedPlayer player, decimal amt);

        public delegate void PlayerPay(UnturnedPlayer sender, string receiver, decimal amt);

        public static Uconomy Instance;

        public DatabaseManager Database;

        public Uconomy(IServiceProvider serviceProvider)
        {
            Instance = this;
            Database = ActivatorUtilities.CreateInstance<DatabaseManager>(serviceProvider);
        }

        public event PlayerBalanceUpdate OnBalanceUpdate;
        public event PlayerBalanceCheck OnBalanceCheck;
#pragma warning disable 67
        public event PlayerPay OnPlayerPay;
#pragma warning restore 67

        // ReSharper disable once InconsistentNaming
        internal void BalanceUpdated(string SteamID, decimal amt)
        {
            if (OnBalanceUpdate == null) return;
            var player = UnturnedPlayer.FromCSteamID(new CSteamID(Convert.ToUInt64(SteamID)));
            OnBalanceUpdate(player, amt);
        }

        // ReSharper disable once InconsistentNaming
        internal void OnBalanceChecked(string SteamID, decimal balance)
        {
            if (OnBalanceCheck == null) return;
            var player = UnturnedPlayer.FromCSteamID(new CSteamID(Convert.ToUInt64(SteamID)));
            OnBalanceCheck(player, balance);
        }
    }
}