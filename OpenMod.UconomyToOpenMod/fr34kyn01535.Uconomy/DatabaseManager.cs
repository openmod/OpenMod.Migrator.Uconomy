#region

using Nito.AsyncEx;
using OpenMod.Core.Users;
using OpenMod.Extensions.Economy.Abstractions;

#endregion

namespace fr34kyn01535.Uconomy
{
    public class DatabaseManager
    {
        private readonly IEconomyProvider m_EconomyProvider;

        public DatabaseManager(IEconomyProvider economyProvider)
        {
            m_EconomyProvider = economyProvider;
        }

        public decimal GetBalance(string playerId)
        {
            var balance = AsyncContext.Run(async () =>
                await m_EconomyProvider.GetBalanceAsync(playerId, KnownActorTypes.Player));
            Uconomy.Instance.OnBalanceChecked(playerId, balance);
            return balance;
        }

        // ReSharper disable once UnusedMember.Global
        public decimal IncreaseBalance(string playerId, decimal increaseBy)
        {
            if (increaseBy == 0)
                return GetBalance(playerId);

            var balance = AsyncContext.Run(async () =>
                await m_EconomyProvider.UpdateBalanceAsync(playerId, KnownActorTypes.Player, increaseBy,
                    "UconomyBridge"));
            Uconomy.Instance.BalanceUpdated(playerId, increaseBy);
            return balance;
        }
    }
}