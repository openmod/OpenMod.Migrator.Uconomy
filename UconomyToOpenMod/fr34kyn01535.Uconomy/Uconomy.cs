#region

using System;
using OpenMod.Extensions.Economy.Abstractions;
using Rocket.API.Collections;
using Rocket.Core.Plugins;
using Rocket.Unturned.Player;
using Steamworks;

#endregion

namespace fr34kyn01535.Uconomy
{
    // ReSharper disable once UnusedMember.Global
    public sealed class Uconomy : RocketPlugin<UconomyConfiguration>
    {
        public delegate void PlayerBalanceCheck(UnturnedPlayer player, decimal balance);

        public delegate void PlayerBalanceUpdate(UnturnedPlayer player, decimal amt);

        public delegate void PlayerPay(UnturnedPlayer sender, string receiver, decimal amt);

        public static Uconomy Instance;

        public static string MessageColor;
        public static IEconomyProvider EconomyProvider;

        public DatabaseManager Database;

        public Uconomy()
        {
            Instance = this;
            Database = new DatabaseManager(EconomyProvider);
            MessageColor = Configuration.Instance.MessageColor;
        }

        public override TranslationList DefaultTranslations =>
            new TranslationList
            {
                {"command_balance_show", "Your current balance is: {0} {1} {2}"},
                {"command_balance_error_player_not_found", "Failed to find player!"},
                {"command_balance_check_noPermissions", "Insufficent Permissions!"},
                {"command_balance_show_otherPlayer", "{0}'s current balance is: {0} {1} {2}"},
                {"command_pay_invalid", "Invalid arguments"},
                {"command_pay_error_pay_self", "You cant pay yourself"},
                {"command_pay_error_invalid_amount", "Invalid amount"},
                {"command_pay_error_cant_afford", "Your balance does not allow this payment"},
                {"command_pay_error_player_not_found", "Failed to find player"},
                {"command_pay_private", "You paid {0} {1} {2}"},
                {"command_pay_console", "You received a payment of {0} {1} "},
                {"command_pay_other_private", "You received a payment of {0} {1} from {2}"}
            };

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