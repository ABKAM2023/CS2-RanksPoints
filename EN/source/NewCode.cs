using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using Dapper;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RanksPointsNamespace;

public partial class RanksPoints
{


    private async Task GetPlayerConnectionAsync(string steamId, string playerName, long currentTime, uint client, CCSPlayerController player)
    {
        try
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var insertQuery = $"INSERT INTO {dbConfig.Name} (steam, name, lastconnect) VALUES (@SteamID, @Name, @LastConnect) ON DUPLICATE KEY UPDATE lastconnect = @LastConnect,name = @Name;";
                await connection.ExecuteAsync(insertQuery, new { SteamID = steamId, Name = playerName, LastConnect = currentTime });
                string query = $"select * from `{dbConfig.Name}` where  steam  = '{steamId}'";

                var playerdata = await connection.QueryFirstOrDefaultAsync(query);

                if (playerdata != null)
                {
                    g_Player[client].value = playerdata.value;
                    g_Player[client].rank = playerdata.rank;
                }
                var ranksConfig = LoadRanksConfig();
                RankConfig? defaultRank = ranksConfig.FirstOrDefault(r => r.Id == 0);
                RankConfig? currentRank = ranksConfig.FirstOrDefault(r => r.Id == g_Player[client].rank);
                var rank = currentRank ?? defaultRank;

                if (rank != null && !string.IsNullOrEmpty(rank.ClanTag))
                {
                    g_Player[client].ClanTag = rank.ClanTag;
                    player.Clan = rank.ClanTag;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in UpdatePlayerConnectionAsync: {ex.Message}");
        }
    }
}

