using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RankPointsPlugin;

public class PluginConfig
{
    public int PointsPerKill { get; set; } = 5; 
    public int PointsPerDeath { get; set; } = -5; 
    public int PointsPerAssist { get; set; } = 1;
    public int PointsPerSuicide { get; set; } = -6;
    public int PointsPerHeadshot { get; set; } = 1;
    public int PointsPerRoundWin { get; set; } = 2;
    public int PointsPerRoundLoss { get; set; } = -2;
    public int PointsPerMVP { get; set; } = 3;
    public int PointsPerNoScope { get; set; } = 2;
    public int MinPlayersForExperience { get; set; } = 4;
}

public class RankPointsPlugin : BasePlugin
{    private Dictionary<string, int> playerKills;
    private Dictionary<string, int> playerDeaths;
    private Dictionary<string, bool> playerReachedMaxRank = new Dictionary<string, bool>();
    private string? topPlayersFilePath;
    private List<TopPlayer>? topPlayersList;  
    private Dictionary<string, int> playerPoints;
    private Dictionary<string, string> playerRanks;
    private string? dataFilePath;
    private static PluginConfig _config = new PluginConfig();
    private string? ranksFilePath;
    private List<Rank> ranks;
    private bool isActiveRoundForPoints;
    public override string ModuleName => "Rank Points (by ABKAM)";
    public override string ModuleVersion => "1.0.0";

    public RankPointsPlugin()
    {
        playerPoints = new Dictionary<string, int>();
        playerRanks = new Dictionary<string, string>();
        ranks = new List<Rank>();
        playerKills = new Dictionary<string, int>();
        playerDeaths = new Dictionary<string, int>();

        SavePlayerPoints();
    }
    public void SaveConfig(PluginConfig config, string filePath)
    {
        var stringBuilder = new StringBuilder();

        stringBuilder.AppendLine("# Конфигурационный файл для RankPoints");
        stringBuilder.AppendLine("# Очки за убийство - количество очков, добавляемое игроку за убийство противника.");
        stringBuilder.AppendLine($"PointsPerKill: {config.PointsPerKill}");
        stringBuilder.AppendLine("# Очки отнимаемые за смерть - количество очков, вычитаемое у игрока за смерть.");
        stringBuilder.AppendLine($"PointsPerDeath: {config.PointsPerDeath}");
        stringBuilder.AppendLine("# Очки за помощь - количество очков, добавляемое игроку за помощь в убийстве.");
        stringBuilder.AppendLine($"PointsPerAssist: {config.PointsPerAssist}");
        stringBuilder.AppendLine("# Очки за самоубийство - количество очков, вычитаемое у игрока за самоубийство.");
        stringBuilder.AppendLine($"PointsPerSuicide: {config.PointsPerSuicide}");
        stringBuilder.AppendLine("# Очки за выстрел в голову - дополнительные очки за убийство с выстрелом в голову.");
        stringBuilder.AppendLine($"PointsPerHeadshot: {config.PointsPerHeadshot}");
        stringBuilder.AppendLine("# Очки за победу в раунде - количество очков, добавляемое игроку за победу его команды в раунде.");
        stringBuilder.AppendLine($"PointsPerRoundWin: {config.PointsPerRoundWin}");
        stringBuilder.AppendLine("# Очки за проигрыш в раунде - количество очков, вычитаемое у игрока за проигрыш его команды в раунде.");
        stringBuilder.AppendLine($"PointsPerRoundLoss: {config.PointsPerRoundLoss}");
        stringBuilder.AppendLine("# Очки за MVP - количество очков, добавляемое игроку за получение звания MVP раунда.");
        stringBuilder.AppendLine($"PointsPerMVP: {config.PointsPerMVP}");
        stringBuilder.AppendLine("# Очки за убийство без прицела (no-scope) - дополнительные очки за убийство без использования прицела.");
        stringBuilder.AppendLine($"PointsPerNoScope: {config.PointsPerNoScope}");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("# Минимальное количество игроков для начисления опыта - игрокам начисляется опыт только если на сервере играет минимум это количество игроков.");
        stringBuilder.AppendLine($"MinPlayersForExperience: {config.MinPlayersForExperience}");

        File.WriteAllText(filePath, stringBuilder.ToString());
    }
    public PluginConfig LoadConfig(string filePath)
    {
        if (!File.Exists(filePath))
        {
            var config = new PluginConfig();
            SaveConfig(config, filePath);
            return config;
        }
        else
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yaml = File.ReadAllText(filePath);
            return deserializer.Deserialize<PluginConfig>(yaml);
        }
    }



    public PluginConfig LoadOrCreateConfig(string filePath)
    {
        if (!File.Exists(filePath))
        {
            var defaultConfig = new PluginConfig();

            var serializer = new SerializerBuilder().Build();
            var yaml = serializer.Serialize(defaultConfig);

            File.WriteAllText(filePath, "# Конфигурационный файл для RankPointsPlugin\n" + yaml);

            return defaultConfig;
        }
        else
        {
            var deserializer = new DeserializerBuilder().Build();
            var yaml = File.ReadAllText(filePath);
            return deserializer.Deserialize<PluginConfig>(yaml);
        }
    }

    

    public class User
    {
        public string? SteamId { get; set; }
        public int Experience { get; set; }
        public int Score { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int Assists { get; set; }
        public int Damage { get; set; }
        public int Mvp { get; set; }
        public int HeadshotKills { get; set; }
        public double PercentageHeadshot { get; set; }
        public float Kdr { get; set; }
    }

    private class TopPlayer
    {
        public string? SteamID { get; set; }
        public string? Nickname { get; set; }
        public int Points { get; set; }
    }    
    private User?[] _usersArray = new User?[65];

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);
        topPlayersFilePath = Path.Combine(ModuleDirectory, "stats_topPlayers.json");
        dataFilePath = Path.Combine(ModuleDirectory, "stats_playerPoints.json");
        ranksFilePath = Path.Combine(ModuleDirectory, "settings_ranks.yaml");
        var configFilePath = Path.Combine(ModuleDirectory, "Config.yaml");

        _config = LoadOrCreateConfig(configFilePath);
        SaveConfig(_config, configFilePath);

        InitializeRanks();  
        LoadPlayerPoints(); 
        LoadTopPlayers();
        LoadStatistics();
        RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
        RegisterEventHandler<EventRoundMvp>(OnPlayerMVP);
        RegisterEventHandler<EventRoundStart>(OnRoundStart);
        RegisterEventHandler<EventPlayerConnect>(OnPlayerConnect);
        RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);


        if (!File.Exists(dataFilePath))
        {
            SavePlayerPoints(); 
        }
        Console.WriteLine($"{ModuleName} v{ModuleVersion} by ABKAM loaded.");
    }
    public void Unload()
    {
        SavePlayerPoints(); 
        var configFilePath = Path.Combine(ModuleDirectory, "Config.yaml");
        SaveConfig(_config, configFilePath);
    }   

    private void InitializeRanks()
    {
        if (!File.Exists(ranksFilePath))
        {
            ranks = new List<Rank>
            {
                new Rank { Name = "Серебро - I", PointsRequired = 0 },
                new Rank { Name = "Серебро - II", PointsRequired = 10 },
                new Rank { Name = "Серебро - III", PointsRequired = 25 },
                new Rank { Name = "Серебро - IV", PointsRequired = 50 },
                new Rank { Name = "Серебро Элита", PointsRequired = 75 },
                new Rank { Name = "Серебро - Великий Магистр", PointsRequired = 100 },
                new Rank { Name = "Золотая Звезда - I", PointsRequired = 150 },
                new Rank { Name = "Золотая Звезда - II", PointsRequired = 200 },
                new Rank { Name = "Золотая Звезда - III", PointsRequired = 300 },      
                new Rank { Name = "Золотая Звезда - Магистр", PointsRequired = 500 },
                new Rank { Name = "Магистр-хранитель - I", PointsRequired = 750 },          
                new Rank { Name = "Магистр-хранитель - II", PointsRequired = 1000 },     
                new Rank { Name = "Магистр-хранитель - Элита", PointsRequired = 1500 },     
                new Rank { Name = "Заслуженный Магистр-хранитель", PointsRequired = 2000 },      
                new Rank { Name = "Легендарный Беркут", PointsRequired = 3000 },       
                new Rank { Name = "Легендарный Беркут-магистр", PointsRequired = 5000 },    
                new Rank { Name = "Великий Магистр - Высшего Ранга", PointsRequired = 7500 },   
                new Rank { Name = "Всемирная Элита", PointsRequired = 10000 },                                                                                                                                                                                                                       
            };
            SaveRanks(); 
        }
        else
        {
            LoadRanks();
        }
    }
    private void LoadTopPlayers()
    {
        if (File.Exists(topPlayersFilePath))
        {
            string json = File.ReadAllText(topPlayersFilePath);
            topPlayersList = JsonConvert.DeserializeObject<List<TopPlayer>>(json) ?? new List<TopPlayer>();
        }
        else
        {
            topPlayersList = new List<TopPlayer>();
        }
    }

    private void SaveRanks()
    {
        var serializer = new SerializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();
        
        var yaml = serializer.Serialize(ranks);

        var yamlWithComments = "# Это файл конфигурации рангов для RankPoints\n" + yaml;
        if (ranksFilePath != null)
        {
            File.WriteAllText(ranksFilePath, yamlWithComments);
        }
    }

    private void LoadRanks()
    {
        if (!File.Exists(ranksFilePath))
        {
            ranks = new List<Rank>();
            return;
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        var yaml = File.ReadAllText(ranksFilePath);

        ranks = deserializer.Deserialize<List<Rank>>(yaml);
    }

    private class Rank
    {
        public string? Name { get; set; }
        public int PointsRequired { get; set; }
    }

    private HookResult OnRoundStart(EventRoundStart roundStartEvent, GameEventInfo info)
    {
        isActiveRoundForPoints = GetActivePlayerCount() >= _config.MinPlayersForExperience;

        if (!isActiveRoundForPoints)
        {
            string message = $"{ChatColors.White}[ {ChatColors.Grey}Ранг {ChatColors.White}] {ChatColors.Yellow}Необходимо минимум {_config.MinPlayersForExperience} игрока для начисления опыта.";
            BroadcastToPlayers(message);
        }

        return HookResult.Continue;
    }

    private void BroadcastToPlayers(string message)
    {
        foreach (var player in Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller"))
        {
            if (player != null && player.IsValid && !player.IsBot && player.TeamNum != (int)CsTeam.Spectator)
            {
                player.PrintToChat(message);
            }
        }
    }

    private void SaveStatistics()
    {
        File.WriteAllText(Path.Combine(ModuleDirectory, "stats_playerKills.json"), JsonConvert.SerializeObject(playerKills, Formatting.Indented));
        File.WriteAllText(Path.Combine(ModuleDirectory, "stats_playerDeaths.json"), JsonConvert.SerializeObject(playerDeaths, Formatting.Indented));
    }

    private void LoadStatistics()
    {
        var killsFilePath = Path.Combine(ModuleDirectory, "stats_playerKills.json");
        var deathsFilePath = Path.Combine(ModuleDirectory, "stats_playerDeaths.json");

        if (File.Exists(killsFilePath))
        {
            var killsJson = File.ReadAllText(killsFilePath);
            playerKills = JsonConvert.DeserializeObject<Dictionary<string, int>>(killsJson) ?? new Dictionary<string, int>();
        }

        if (File.Exists(deathsFilePath))
        {
            var deathsJson = File.ReadAllText(deathsFilePath);
            playerDeaths = JsonConvert.DeserializeObject<Dictionary<string, int>>(deathsJson) ?? new Dictionary<string, int>();
        }
    }
    private int GetActivePlayerCount()
    {
        return _usersArray.Count(user => user != null);
    }
    private HookResult OnPlayerConnect(EventPlayerConnect connectEvent, GameEventInfo info)
    {
        try
        {
            if (connectEvent == null)
            {
                return HookResult.Continue;
            }
            if (connectEvent.Userid == null)
            {
                return HookResult.Continue;
            }
            if (_usersArray == null)
            {
                return HookResult.Continue;
            }

            uint steamId = ConvertXuidToUint(connectEvent.Xuid);

            int slot = MapSteamIdToSlot(steamId);
            if (slot < 0 || slot >= _usersArray.Length)
            {
                return HookResult.Continue;
            }

            _usersArray[slot] = CreateUser(connectEvent, steamId);

        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RankPointsPlugin] An exception occurred in OnPlayerConnect: {ex}");
        }

        return HookResult.Continue;
    }

    private uint ConvertXuidToUint(ulong xuid)
    {
        return (uint)xuid;
    }   

    private User CreateUser(EventPlayerConnect connectEvent, uint steamId)
    {
        return new User
        {
            SteamId = steamId.ToString(),
            Experience = 0,
            Score = 0,
            Kills = 0,
            Deaths = 0,
            Assists = 0,
            Damage = 0,
            Mvp = 0,
            HeadshotKills = 0,
            PercentageHeadshot = 0.0,
            Kdr = 0.0f
        };
    }

    private HookResult OnPlayerDisconnect(EventPlayerDisconnect disconnectEvent, GameEventInfo info)
    {
        if (disconnectEvent?.Userid != null)
        {
            uint steamId = (uint)disconnectEvent.Userid.SteamID;
            if (steamIdToSlotMap.TryGetValue(steamId, out int slot))
            {
                if (slot >= 0 && slot < _usersArray.Length)
                {
                    _usersArray[slot] = null;
                    steamIdToSlotMap.Remove(steamId); 
                }
                else
                {
                    Console.WriteLine($"[RankPointsPlugin] Ошибка: Игрок с SteamID {steamId} не найден в допустимых слотах.");
                }
            }
        }
        else
        {
            Console.WriteLine("[RankPointsPlugin] Ошибка: disconnectEvent или disconnectEvent.Userid равен null.");
        }
        return HookResult.Continue;
    }

    private Dictionary<uint, int> steamIdToSlotMap = new Dictionary<uint, int>();

    private int MapSteamIdToSlot(uint steamId)
    {
        if (steamIdToSlotMap.TryGetValue(steamId, out int existingSlot))
        {
            return existingSlot;
        }

        int newSlot = FindAvailableSlotForSteamId(steamId);

        if (newSlot >= 0)
        {
            steamIdToSlotMap.Add(steamId, newSlot);
        }

        return newSlot;
    }  

    private int FindAvailableSlotForSteamId(uint steamId)
    {
        for (int i = 0; i < _usersArray.Length; i++)
        {
            if (_usersArray[i] == null)
            {
                return i;
            }
        }
        
        return -1;
    }     
    private HookResult OnPlayerMVP(EventRoundMvp mvpEvent, GameEventInfo info)
    {
        if (GetActivePlayerCount() < _config.MinPlayersForExperience)
        {
            return HookResult.Continue;
        }        
        CCSPlayerController playerController = mvpEvent.Userid;

        if (playerController != null && playerController.IsValid && !playerController.IsBot && playerController.TeamNum != (int)CsTeam.Spectator)
        {
            var steamID = playerController.SteamID.ToString();
            AddPoints(steamID, _config.PointsPerMVP);
            playerController.PrintToChat($"{ChatColors.White}[ {ChatColors.Grey}Ранг {ChatColors.White}] Ваш опыт:{ChatColors.LightYellow} {playerPoints[steamID]} [+{_config.PointsPerMVP} за MVP]");
        }

        return HookResult.Continue;
    }

    private HookResult OnRoundEnd(EventRoundEnd roundEndEvent, GameEventInfo info)
    {
        if (GetActivePlayerCount() < _config.MinPlayersForExperience)
        {
            return HookResult.Continue;
        }        
        CsTeam winnerTeam = (CsTeam)roundEndEvent.Winner;

        for (int playerIndex = 0; playerIndex <= Server.MaxPlayers; playerIndex++)
        {
            CCSPlayerController playerController = Utilities.GetPlayerFromUserid(playerIndex);

            if (playerController != null && playerController.IsValid && !playerController.IsBot && playerController.TeamNum != (int)CsTeam.Spectator)
            {
                CsTeam playerTeam = (CsTeam)playerController.TeamNum;

                var steamID = playerController.SteamID.ToString();

                if (playerTeam == winnerTeam)
                {
                    AddPoints(steamID, _config.PointsPerRoundWin);
                    playerController.PrintToChat($"{ChatColors.White}[ {ChatColors.Grey}Ранг {ChatColors.White}] Ваш опыт:{ChatColors.Green} {playerPoints[steamID]} [+{_config.PointsPerRoundWin} за победу в раунде]");
                }
                else
                {
                    AddPoints(steamID, _config.PointsPerRoundLoss);
                    playerController.PrintToChat($"{ChatColors.White}[ {ChatColors.Grey}Ранг {ChatColors.White}] Ваш опыт:{ChatColors.Red} {playerPoints[steamID]} [{_config.PointsPerRoundLoss} за проигрыш в раунде]");
                }
            }
        }

        return HookResult.Continue;
    }
    private HookResult OnPlayerDeath(EventPlayerDeath deathEvent, GameEventInfo info)
    {
        if (GetActivePlayerCount() < _config.MinPlayersForExperience)
        {
            return HookResult.Continue;
        }        

        var killerSteamID = deathEvent.Attacker?.SteamID.ToString();
        var victimSteamID = deathEvent.Userid.SteamID.ToString();

        playerDeaths[victimSteamID] = playerDeaths.TryGetValue(victimSteamID, out var deaths) ? deaths + 1 : 1;
        
        if (killerSteamID != null && playerPoints.ContainsKey(killerSteamID))
        {
            playerKills[killerSteamID] = playerKills.TryGetValue(killerSteamID, out var kills) ? kills + 1 : 1;
        }

        SaveStatistics();

        if (killerSteamID == victimSteamID)
        {
            AddPoints(victimSteamID, _config.PointsPerSuicide);
            deathEvent.Userid.PrintToChat($"{ChatColors.White}[ {ChatColors.Grey}Ранг {ChatColors.White}]  Ваш опыт:{ChatColors.Red} {playerPoints[victimSteamID]} [{_config.PointsPerSuicide} за самоубийство]");
        }
        else
        {
            bool isHeadshot = deathEvent.Headshot;

            if (deathEvent.Attacker != null && deathEvent.Attacker.IsValid && !deathEvent.Attacker.IsBot)
            {
                int pointsForKill = _config.PointsPerKill;

                if (killerSteamID != null)
                {
                    if (playerPoints.TryGetValue(killerSteamID, out int killerPoints))
                    {
                        pointsForKill += _config.PointsPerNoScope;
                        deathEvent.Attacker.PrintToChat($"{ChatColors.White}[ {ChatColors.Grey}Ранг {ChatColors.White}] Ваш опыт:{ChatColors.Blue} {killerPoints} [+{_config.PointsPerNoScope} за убийство без прицела]");
                    }
                    else
                    {
                        // Обработка случая, когда ключ не найден в словаре.
                    }
                }
                
                if (killerSteamID != null)
                {
                    AddPoints(killerSteamID, pointsForKill);
                    deathEvent.Attacker.PrintToChat($"{ChatColors.White}[ {ChatColors.Grey}Ранг {ChatColors.White}] Ваш опыт:{ChatColors.Green} {playerPoints[killerSteamID]} [+{pointsForKill} за убийство]");
                }

                if (deathEvent.Headshot)
                {
                    if (killerSteamID != null)
                    {
                        AddPoints(killerSteamID, _config.PointsPerHeadshot);
                        deathEvent.Attacker.PrintToChat($"{ChatColors.White}[ {ChatColors.Grey}Ранг {ChatColors.White}] Ваш опыт:{ChatColors.Yellow} {playerPoints[killerSteamID]} [+{_config.PointsPerHeadshot} за выстрел в голову]");
                    }
                }
            }

            if (deathEvent.Assister != null && deathEvent.Assister.IsValid && !deathEvent.Assister.IsBot)
            {
                var assisterSteamID = deathEvent.Assister.SteamID.ToString();
                AddPoints(assisterSteamID, _config.PointsPerAssist);
                deathEvent.Assister.PrintToChat($"{ChatColors.White}[ {ChatColors.Grey}Ранг {ChatColors.White}] Ваш опыт:{ChatColors.Blue} {playerPoints[assisterSteamID]} [+{_config.PointsPerAssist} за помощь]");
            }

            if (victimSteamID != null && playerPoints.ContainsKey(victimSteamID))
            {
                AddPoints(victimSteamID, _config.PointsPerDeath);
                deathEvent.Userid.PrintToChat($"{ChatColors.White}[ {ChatColors.Grey}Ранг {ChatColors.White}] Ваш опыт:{ChatColors.Red} {playerPoints[victimSteamID]} [{_config.PointsPerDeath} за смерть]");
            }
        }

        return HookResult.Continue;
    }

    private void AddPoints(string steamID, int pointsToAdd)
    {
        var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
        int activePlayerCount = playerEntities.Count(p => !p.IsBot && p.TeamNum != (int)CsTeam.Spectator);

        if (activePlayerCount < _config.MinPlayersForExperience)
        {
            return; 
        }

        if (!playerPoints.TryGetValue(steamID, out int currentPoints))
        {
            currentPoints = 0;
        }

        currentPoints = Math.Max(0, currentPoints + pointsToAdd);
        playerPoints[steamID] = currentPoints;

        CheckAndUpdateRank(steamID, currentPoints);

        SavePlayerPoints();
        UpdateTopPlayersList();
    }

    private void UpdateTopPlayersList()
    {
        var updatedTopPlayers = playerPoints
            .OrderByDescending(kvp => kvp.Value)
            .Take(10)
            .ToList();

        topPlayersList = updatedTopPlayers.Select(kvp => new TopPlayer
        {
            SteamID = kvp.Key,
            Nickname = GetPlayerNickname(kvp.Key), 
            Points = kvp.Value
        }).ToList();

        SaveTopPlayers();
    }
    private void CheckAndUpdateRank(string steamID, int points)
    {
        var currentRank = playerRanks.TryGetValue(steamID, out var rank) ? rank : ranks[0].Name;
        var newRank = currentRank;
        var currentRankIndex = ranks.FindIndex(r => r.Name == currentRank);
        var newRankIndex = currentRankIndex;
        var rankUp = false;
        
        for (int i = 0; i < ranks.Count; i++)
        {
            if (points >= ranks[i].PointsRequired)
            {
                newRankIndex = i;
            }
        }

        if (newRankIndex != currentRankIndex)
        {
            newRank = ranks[newRankIndex].Name;
            rankUp = newRankIndex > currentRankIndex;
        }

        if (currentRank != newRank)
        {
            if (newRank != null)
            {
                playerRanks[steamID] = newRank;
            }
            else
            {
                // Обработка случая, когда newRank равно null.
            }
            
            var playerController = FindPlayerBySteamID(steamID);
            if (playerController != null)
            {
                string message;
                if (rankUp)
                {
                    message = $"Поздравляем! Ваше новое звание: {newRank}.";
                }
                else
                {
                    message = $"Ваше звание понизилось до: {newRank}.";
                }
                
                playerController.PrintToCenter(message);
            }
            else
            {
                Console.WriteLine($"[RankPointsPlugin] Player with SteamID {steamID} not found for rank update message.");
            }
        }
    }

    private bool IsMaxRank(string rank)
    {
        return rank == ranks.LastOrDefault()?.Name;
    } 

    private bool playerReachedMaxRankBefore(string steamID)
    {
        if (playerReachedMaxRank.TryGetValue(steamID, out bool reachedMaxRank))
        {
            return reachedMaxRank;
        }

        return false;
    }

    private void LoadPlayerPoints()
    {
        if (File.Exists(dataFilePath))
        {
            var json = File.ReadAllText(dataFilePath);
            playerPoints = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, int>>(json) ?? new Dictionary<string, int>();
        }
    }
    private void SavePlayerPoints()
    {
        var json = (playerPoints.Count > 0) ? JsonConvert.SerializeObject(playerPoints, Formatting.Indented) : "{}";

        try
        {
            var directory = Path.GetDirectoryName(dataFilePath) ?? Directory.GetCurrentDirectory();
            if (!Directory.Exists(directory))
            {;
                Directory.CreateDirectory(directory);
            }

            if (dataFilePath != null)
            {
                File.WriteAllText(dataFilePath, json);
            }
            else
            {
                // Обработка случая, когда dataFilePath равно null.
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RankPointsPlugin] Не удалось сохранить очки игроков: {ex.Message}");
        }
    }
    private string ConvertSteamID64ToSteamID(ulong steamID64)
    {
        const ulong SteamID64Identifier = 76561197960265728;
        const ulong universe = 1; 

        ulong accountIdLowBit = steamID64 & 1;
        ulong accountIdHighBits = (steamID64 - SteamID64Identifier - accountIdLowBit) / 2;

        return $"STEAM_{universe}:{accountIdLowBit}:{accountIdHighBits}";
    }

    private (string NextRankName, int PointsToNextRank) GetNextRankInfo(int currentPoints)
    {
        Rank nextRank = new Rank(); 
        int pointsNeeded = 0;

        foreach (var rank in ranks)
        {
            if (currentPoints < rank.PointsRequired)
            {
                nextRank = rank;
                pointsNeeded = rank.PointsRequired - currentPoints;
                break;
            }
        }

        if (nextRank == null)
        {
            return ("максимальное звание", 0);
        }

        string nextRankName = nextRank.Name ?? "неизвестный ранг";

        return (nextRankName, pointsNeeded);
    }

    private double CalculateKDRatio(int kills, int deaths)
    {
        return deaths > 0 ? (double)kills / deaths : kills;
    }

    [ConsoleCommand("rank", "Показывает ваше текущее звание и информацию о следующем")]
    public void OnRankCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            return;
        }

        var steamID = player.SteamID.ToString();
        if (!playerPoints.TryGetValue(steamID, out var points))
        {
            player.PrintToChat($"{ChatColors.White}[ {ChatColors.Grey}Ранг {ChatColors.White}] У вас еще нет звания.");
            return;
        }

        var sortedPlayers = playerPoints.OrderByDescending(kvp => kvp.Value).ToList();
        var rankPosition = sortedPlayers.FindIndex(kvp => kvp.Key == steamID) + 1; 
        var totalPlayers = sortedPlayers.Count;
        var kills = playerKills.TryGetValue(steamID, out var playerKillsCount) ? playerKillsCount : 0;
        var deaths = playerDeaths.TryGetValue(steamID, out var playerDeathsCount) ? playerDeathsCount : 0;    
        var kdRatio = CalculateKDRatio(kills, deaths);    
 
        string formattedKDRatio = kdRatio.ToString("0.00");

        var rankName = playerRanks.TryGetValue(steamID, out var rank) ? rank : "Нету";

        player.PrintToChat($"{ChatColors.White}[ {ChatColors.Grey}Ранг {ChatColors.White}] Ваше текущее звание: {ChatColors.Yellow}{rankName}{ChatColors.White}.");

        var (nextRankName, pointsToNextRank) = GetNextRankInfo(points);

        if (pointsToNextRank > 0)
        {
            player.PrintToChat($"{ChatColors.White}До следующего звания {ChatColors.Yellow}{nextRankName}{ChatColors.White} вам необходимо {ChatColors.Green}{pointsToNextRank} {ChatColors.White}опыта.");
        }
        else
        {
            player.PrintToChat($"{ChatColors.White}Поздравляем, вы достигли {ChatColors.Yellow}{nextRankName}{ChatColors.White}!");
        }
        player.PrintToChat($"{ChatColors.White}Всего опыта: {ChatColors.Green}{points}{ChatColors.White} Позиция: {ChatColors.Yellow}{rankPosition}/{totalPlayers} {ChatColors.White}Убийств: {ChatColors.Green}{kills}{ChatColors.White} Смертей: {ChatColors.Red}{deaths} {ChatColors.White}K/D Ratio: {ChatColors.Yellow}{formattedKDRatio}");
    }

   
    [ConsoleCommand("top", "Показывает топ-10 игроков по очкам")]
    public void OnTopCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            Console.WriteLine("Эту команду может использовать только игрок.");
            return;
        }


        var topPlayers = playerPoints
            .OrderByDescending(kvp => kvp.Value)
            .Take(10)
            .ToList();


        topPlayersList = topPlayers.Select(kvp => new TopPlayer
        {
            SteamID = kvp.Key,
            Nickname = GetPlayerNickname(kvp.Key),
            Points = kvp.Value
        }).ToList();

        SaveTopPlayers();

        player.PrintToChat($"{ChatColors.White}[ {ChatColors.Grey}Топ игроков {ChatColors.White}]");
        for (int i = 0; i < topPlayers.Count; i++)
        {
            var topPlayerInfo = topPlayersList[i];
            player.PrintToChat($"{ChatColors.White}{i + 1}. {ChatColors.Yellow}{topPlayerInfo.Nickname}{ChatColors.White} - {ChatColors.Green}{topPlayerInfo.Points} очков");
        }
    }
    private void SaveTopPlayers()
    {
        string json = JsonConvert.SerializeObject(topPlayersList, Formatting.Indented);
        if (topPlayersFilePath != null)
        {
            File.WriteAllText(topPlayersFilePath, json);
        }
        else
        {
            // Обработка случая, когда topPlayersFilePath равно null.
        }
    }    

    private CCSPlayerController? FindPlayerBySteamID(string steamID)
    {
        var playerEntities = Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller");
        foreach (var player in playerEntities)
        {
            if (player.SteamID.ToString() == steamID)
            {
                return player;
            }
        }
        return null;
    }

    private string GetPlayerNickname(string steamID)
    {
        var player = FindPlayerBySteamID(steamID);

        if (player != null)
        {
            var topPlayer = topPlayersList?.FirstOrDefault(p => p.SteamID == steamID);
            if (topPlayer != null)
            {
                topPlayer.Nickname = player.PlayerName;
            }
            return player.PlayerName;
        }

        var savedTopPlayer = topPlayersList?.FirstOrDefault(p => p.SteamID == steamID);
        
        if (savedTopPlayer != null)
        {
            return savedTopPlayer.Nickname ?? string.Empty;
        }

        return "Игрок не найден";
    }



}
