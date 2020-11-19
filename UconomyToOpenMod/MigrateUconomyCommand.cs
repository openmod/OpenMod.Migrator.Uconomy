#region

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using OpenMod.API.Commands;
using OpenMod.Core.Commands;
using OpenMod.Core.Console;
using OpenMod.Core.Users;
using OpenMod.Extensions.Economy.Abstractions;

#endregion

namespace RG.UconomyToOpenMod
{
    // ReSharper disable once UnusedMember.Global
    [Command("MigrateUconomy")]
    [CommandActor(typeof(ConsoleActor))]
    [CommandDescription("Migrates data from Uconomy to Openmod. (Console only)")]
    [CommandSyntax("[DeleteAfterMigrate]")]
    public class MigrateUconomyCommand : Command
    {
        private readonly IConfiguration m_Configuration;
        private readonly IEconomyProvider m_EconomyProvider;
        private readonly ILogger<MigrateUconomyCommand> m_Logger;

        public MigrateUconomyCommand(IConfiguration configuration, IEconomyProvider economyProvider, ILogger<MigrateUconomyCommand> logger,
            IServiceProvider serviceProvider) : base(
            serviceProvider)
        {
            m_Configuration = configuration;
            m_EconomyProvider = economyProvider;
            m_Logger = logger;
        }

        protected override async Task OnExecuteAsync()
        {
            var shouldDelete = false;
            if (Context.Parameters.Length > 0)
                shouldDelete = await Context.Parameters.GetAsync<bool>(0);

            await Context.Actor.PrintMessageAsync("Starting data migration, this may take some time.");

            try
            {
                var port = m_Configuration.GetSection("DatabasePort").Get<ushort>();
                if (port == 0)
                    port = 3306;

                var mySqlConnection = new MySqlConnection(
                    $"SERVER={m_Configuration.GetSection("DatabaseAddress").Get<ushort>()};" +
                    $"DATABASE={m_Configuration.GetSection("DatabaseName").Get<ushort>()};" +
                    $"UID={m_Configuration.GetSection("DatabaseUsername").Get<ushort>()};" +
                    $"PASSWORD={m_Configuration.GetSection("DatabasePassword").Get<string>()};" +
                    $"PORT={port};");

                await using var command = mySqlConnection.CreateCommand();
                var table = m_Configuration.GetSection("DatabaseTableName").Get<string>();
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

                await using var reader = await command.ExecuteReaderAsync();
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