using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using fr34kyn01535.Uconomy;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Core.Users;
using OpenMod.Extensions.Economy.Abstractions;
using Rocket.API;

namespace RG.UconomyToOpenMod
{
    // ReSharper disable once UnusedMember.Global
    [Command("MigrateUconomy")]
    [CommandActor(typeof(ConsoleActor))]
    [CommandDescription("Migrates data from Uconomy to Openmod. (Console only)")]
    [CommandSyntax("[DeleteAfterMigrate]")]
    public class MigrateUconomyCommand : Command
    {
        private readonly MethodInfo m_CreateConnectionMethod =
            typeof(DatabaseManager).GetMethod("CreateConnection", BindingFlags.Instance | BindingFlags.NonPublic);

        private readonly IEconomyProvider m_EconomyProvider;
        private readonly ILogger<MigrateUconomyCommand> m_Logger;

        public MigrateUconomyCommand(IEconomyProvider economyProvider, ILogger<MigrateUconomyCommand> logger,
            IServiceProvider serviceProvider) : base(
            serviceProvider)
        {
            m_EconomyProvider = economyProvider;
            m_Logger = logger;
        }

        protected override async Task OnExecuteAsync()
        {
            var shouldDelete = false;
            if (Context.Parameters.Length > 0)
                shouldDelete = await Context.Parameters.GetAsync<bool>(0);

            if (Uconomy.Instance == null || Uconomy.Instance.State != PluginState.Loaded)
                throw new UserFriendlyException("Uconomy needs to be loaded!");
            await Context.Actor.PrintMessageAsync("Starting data migration, this may take some time.");

            try
            {
                if (!(m_CreateConnectionMethod.Invoke(Uconomy.Instance.Database, null) is MySqlConnection
                    mySqlConnection))
                    throw new UserFriendlyException("It was not possible to create a mysql connection.");

                using var command = mySqlConnection.CreateCommand();
                var table = Uconomy.Instance.Configuration.Instance.DatabaseTableName;
                command.CommandText = $"SHOW TABLES LIKE '{table}';";

                await mySqlConnection.OpenAsync();
                if (await command.ExecuteScalarAsync() == null)
                {
                    await Context.Actor.PrintMessageAsync("No uconomy data was found.");
                    return;
                }

                var balanceData = new Dictionary<string, decimal>();
                command.CommandText = $"SELECT * FROM `{table}`;";

                await Context.Actor.PrintMessageAsync("Reading old data...");

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var steamId = reader.GetString(reader.GetOrdinal("steamId"));
                    var balance = reader.GetDecimal(reader.GetOrdinal("balance"));

                    m_Logger.LogTrace($"Reading data: {steamId}:{balance}");
                    balanceData.Add(steamId, balance);
                }

                await Context.Actor.PrintMessageAsync("Writing new data...");

                foreach (var data in balanceData)
                {
                    m_Logger.LogTrace($"Reading data: {data.Key}:{data.Value}");
                    await m_EconomyProvider.SetBalanceAsync(data.Key, KnownActorTypes.Player, data.Value);
                }

                if (shouldDelete)
                {
                    m_Logger.LogTrace("Erasing old data...");
                    command.CommandText = $"DROP TABLE `{table}`;";
                    await command.ExecuteNonQueryAsync();
                }

                await Context.Actor.PrintMessageAsync(
                    $"Data has been successfully migrated. Amount: {balanceData.Count}");
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception ex)
            {
                m_Logger.LogError(ex, "Failed to migrate data from uconomy to openmod.");
            }
        }
    }
}