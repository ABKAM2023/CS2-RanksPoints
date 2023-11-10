using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Reflection;
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
    public string MinPlayersForExperienceMessage { get; set; } = "";
    public string MvpAwardMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Ваш опыт:{LightYellow} {POINTS} [+{MVP_POINTS} за MVP]";
    public string GetActivePlayerCountMsg { get; set; } = "{White}[ {Red}RanksPoints {White}] Необходимо минимум {Red}{MIN_PLAYERS} {White}игрока для начисления опыта.";
    public string RoundWinMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Green} {POINTS} [+{ROUND_WIN_POINTS} за победу в раунде]";
    public string RoundLossMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Red} {POINTS} [{ROUND_LOSS_POINTS} за проигрыш в раунде]";
    public string SuicideMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Red} {POINTS} [{SUICIDE_POINTS} за самоубийство]";
    public string NoScopeKillMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Blue} {POINTS} [+{NOSCOPE_POINTS} за убийство без прицела]";
    public string KillMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Green} {POINTS} [+{KILL_POINTS} за убийство]";
    public string HeadshotMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Yellow} {POINTS} [+{HEADSHOT_POINTS} за выстрел в голову]";    
    public string AssistMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Blue} {POINTS} [+{ASSIST_POINTS} за помощь]";
    public string DeathMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Red} {POINTS} [{DEATH_POINTS} за смерть]";   
    public string NoRankMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] У вас еще нет звания.";
    public string CurrentRankMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Ваше текущее звание: {Yellow}{RANK_NAME}{White}.";
    public string NextRankMessage { get; set; } = "{White}До следующего звания {Yellow}{NEXT_RANK_NAME}{White} вам необходимо {Green}{POINTS_TO_NEXT_RANK} {White}опыта.";
    public string MaxRankMessage { get; set; } = "{White}Поздравляем, вы достигли {Yellow}{RANK_NAME}{White}!";
    public string StatsMessage { get; set; } = "{White}Всего опыта: {Green}{POINTS}{White} Позиция: {Yellow}{RANK_POSITION}/{TOTAL_PLAYERS} {White}Убийств: {Green}{KILLS}{White} Смертей: {Red}{DEATHS} {White}K/D Ratio: {Yellow}{KDRATIO}";     
    public string TopCommandIntroMessage { get; set; } = "{White}[ {Red}Топ игроков {White}]";
    public string TopPlayerMessage { get; set; } = "{White}{POSITION}. {Grey}{NICKNAME}{White} - {Green}{POINTS} очков";
    public string RankUpMessage { get; set; } = "Поздравляем! Ваше новое звание: {RANK}.";
    public string RankDownMessage { get; set; } = "Ваше звание понизилось до: {RANK}.";
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
    
    private string EscapeMessage(string message)
    {
        return message.Replace("\"", "\\\"").Replace("\n", "\\n");
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

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("# Сообщения событий");            
        string escapedMvpMessage = config.MvpAwardMessage.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"MvpAwardMessage: \"{escapedMvpMessage}\"");    
        string escapedRoundWinMessage = config.RoundWinMessage.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"RoundWinMessage: \"{escapedRoundWinMessage}\"");
        string escapedRoundLossMessage = config.RoundLossMessage.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"RoundLossMessage: \"{escapedRoundLossMessage}\"");
        string escapedSuicideMessage = config.SuicideMessage.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"SuicideMessage: \"{escapedSuicideMessage}\"");      
        string escapedNoScopeMessage = config.NoScopeKillMessage.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"NoScopeKillMessage: \"{escapedNoScopeMessage}\"");
        string escapedKillMessage = config.KillMessage.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"KillMessage: \"{escapedKillMessage}\"");
        string escapedHeadshotMessage = config.HeadshotMessage.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"HeadshotMessage: \"{escapedHeadshotMessage}\"");      
        string escapedAssistMessage = config.AssistMessage.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"AssistMessage: \"{escapedAssistMessage}\"");
        string escapedDeathMessage = config.DeathMessage.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"DeathMessage: \"{escapedDeathMessage}\"");   
        
        stringBuilder.AppendLine(); 
        stringBuilder.AppendLine("# Сообщение, если не хватает необходимого количества игроков.");       
        string escapedGetActivePlayerCountMsg = config.GetActivePlayerCountMsg.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"GetActivePlayerCountMsg: \"{escapedGetActivePlayerCountMsg}\"");

        stringBuilder.AppendLine(); 
        stringBuilder.AppendLine("# Сообщения команды !rank");
        stringBuilder.AppendLine($"NoRankMessage: \"{EscapeMessage(config.NoRankMessage)}\"");
        stringBuilder.AppendLine($"CurrentRankMessage: \"{EscapeMessage(config.CurrentRankMessage)}\"");
        stringBuilder.AppendLine($"NextRankMessage: \"{EscapeMessage(config.NextRankMessage)}\"");
        stringBuilder.AppendLine($"MaxRankMessage: \"{EscapeMessage(config.MaxRankMessage)}\"");
        stringBuilder.AppendLine($"StatsMessage: \"{EscapeMessage(config.StatsMessage)}\"");

        stringBuilder.AppendLine(); 
        stringBuilder.AppendLine("# Сообщения команды !top");
        string escapedTopCommandIntroMessage = config.TopCommandIntroMessage.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"TopCommandIntroMessage: \"{escapedTopCommandIntroMessage}\"");
        string escapedTopPlayerMessage = config.TopPlayerMessage.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"TopPlayerMessage: \"{escapedTopPlayerMessage}\"");
        
        stringBuilder.AppendLine();      
        stringBuilder.AppendLine("# Сообщения при повышении или понижении звания");
        string escapedRankUpMessage = config.RankUpMessage.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"RankUpMessage: \"{escapedRankUpMessage}\"");
        string escapedRankDownMessage = config.RankDownMessage.Replace("\"", "\\\"").Replace("\n", "\\n");
        stringBuilder.AppendLine($"RankDownMessage: \"{escapedRankDownMessage}\"");        


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
    }   

    public PluginConfig LoadOrCreateConfig(string filePath)
    {
        if (!File.Exists(filePath))
        {
            var defaultConfig = new PluginConfig();
            SaveConfig(defaultConfig, filePath);
            return defaultConfig;
        }
        else
        {
            var deserializer = new DeserializerBuilder().Build();
            var yaml = File.ReadAllText(filePath);
            return deserializer.Deserialize<PluginConfig>(yaml);
        }
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
            string formattedMessage = _config.GetActivePlayerCountMsg
                .Replace("{MIN_PLAYERS}", _config.MinPlayersForExperience.ToString());
            formattedMessage = ReplaceColorPlaceholders(formattedMessage);
            BroadcastToPlayers(formattedMessage);
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

            string formattedMessage = _config.MvpAwardMessage
                .Replace("{POINTS}", playerPoints[steamID].ToString())
                .Replace("{MVP_POINTS}", _config.PointsPerMVP.ToString());
            formattedMessage = ReplaceColorPlaceholders(formattedMessage);

            playerController.PrintToChat(formattedMessage);
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
                    string formattedMessage = _config.RoundWinMessage
                        .Replace("{POINTS}", playerPoints[steamID].ToString())
                        .Replace("{ROUND_WIN_POINTS}", _config.PointsPerRoundWin.ToString());
                    formattedMessage = ReplaceColorPlaceholders(formattedMessage);
                    playerController.PrintToChat(formattedMessage);
                }
                else
                {
                    AddPoints(steamID, _config.PointsPerRoundLoss);
                    string formattedMessage = _config.RoundLossMessage
                        .Replace("{POINTS}", playerPoints[steamID].ToString())
                        .Replace("{ROUND_LOSS_POINTS}", _config.PointsPerRoundLoss.ToString());
                    formattedMessage = ReplaceColorPlaceholders(formattedMessage);
                    playerController.PrintToChat(formattedMessage);
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
            string formattedMessage = _config.SuicideMessage
                .Replace("{POINTS}", playerPoints[victimSteamID].ToString())
                .Replace("{SUICIDE_POINTS}", _config.PointsPerSuicide.ToString());
            formattedMessage = ReplaceColorPlaceholders(formattedMessage);
            deathEvent.Userid.PrintToChat(formattedMessage);
        }
        else
        {
            bool isHeadshot = deathEvent.Headshot;

            if (deathEvent.Attacker != null && deathEvent.Attacker.IsValid && !deathEvent.Attacker.IsBot)
            {
                int pointsForKill = _config.PointsPerKill;

                if (deathEvent.Noscope)
                {
                    AddPoints(killerSteamID, _config.PointsPerNoScope);
                    string formattedNoScopeMessage = _config.NoScopeKillMessage
                        .Replace("{POINTS}", playerPoints[killerSteamID].ToString())
                        .Replace("{NOSCOPE_POINTS}", _config.PointsPerNoScope.ToString());
                    formattedNoScopeMessage = ReplaceColorPlaceholders(formattedNoScopeMessage);
                    deathEvent.Attacker.PrintToChat(formattedNoScopeMessage);
                }
                
                if (killerSteamID != null)
                {
                    AddPoints(killerSteamID, pointsForKill);
                    string formattedKillMessage = _config.KillMessage
                        .Replace("{POINTS}", playerPoints[killerSteamID].ToString())
                        .Replace("{KILL_POINTS}", pointsForKill.ToString());
                    formattedKillMessage = ReplaceColorPlaceholders(formattedKillMessage);
                    deathEvent.Attacker.PrintToChat(formattedKillMessage);
                }

                if (deathEvent.Headshot)
                {
                    if (killerSteamID != null)
                    {
                        AddPoints(killerSteamID, _config.PointsPerHeadshot);
                        string formattedHeadshotMessage = _config.HeadshotMessage
                            .Replace("{POINTS}", playerPoints[killerSteamID].ToString())
                            .Replace("{HEADSHOT_POINTS}", _config.PointsPerHeadshot.ToString());
                        formattedHeadshotMessage = ReplaceColorPlaceholders(formattedHeadshotMessage);
                        deathEvent.Attacker.PrintToChat(formattedHeadshotMessage);
                    }
                }
            }
            if (deathEvent.Assister != null && deathEvent.Assister.IsValid && !deathEvent.Assister.IsBot)
            {
                var assisterSteamID = deathEvent.Assister.SteamID.ToString();
                AddPoints(assisterSteamID, _config.PointsPerAssist);
                string formattedAssistMessage = _config.AssistMessage
                    .Replace("{POINTS}", playerPoints[assisterSteamID].ToString())
                    .Replace("{ASSIST_POINTS}", _config.PointsPerAssist.ToString());
                formattedAssistMessage = ReplaceColorPlaceholders(formattedAssistMessage);
                deathEvent.Assister.PrintToChat(formattedAssistMessage);
            }

            if (victimSteamID != null && playerPoints.ContainsKey(victimSteamID))
            {
                AddPoints(victimSteamID, _config.PointsPerDeath);
                string formattedDeathMessage = _config.DeathMessage
                    .Replace("{POINTS}", playerPoints[victimSteamID].ToString())
                    .Replace("{DEATH_POINTS}", _config.PointsPerDeath.ToString());
                formattedDeathMessage = ReplaceColorPlaceholders(formattedDeathMessage);
                deathEvent.Userid.PrintToChat(formattedDeathMessage);
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
            
            if (currentRank != newRank)
            {
                string messageTemplate = rankUp ? _config.RankUpMessage : _config.RankDownMessage;
                string message = messageTemplate.Replace("{RANK}", newRank);

                var rankUpdatePlayerController = FindPlayerBySteamID(steamID);
                if (rankUpdatePlayerController != null)
                {
                    rankUpdatePlayerController.PrintToCenter(message);
                }
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
        if (player == null) return;

        var steamID = player.SteamID.ToString();
        if (!playerPoints.TryGetValue(steamID, out var points))
        {
            player.PrintToChat(ReplaceColorPlaceholders(_config.NoRankMessage));
            return;
        }

        var sortedPlayers = playerPoints.OrderByDescending(kvp => kvp.Value).ToList();
        var rankPosition = sortedPlayers.FindIndex(kvp => kvp.Key == steamID) + 1; 
        var totalPlayers = sortedPlayers.Count;
        var kills = playerKills.TryGetValue(steamID, out var playerKillsCount) ? playerKillsCount : 0;
        var deaths = playerDeaths.TryGetValue(steamID, out var playerDeathsCount) ? playerDeathsCount : 0;    
        var kdRatio = CalculateKDRatio(kills, deaths);    
        var (nextRankName, pointsToNextRank) = GetNextRankInfo(points);
 
        string formattedKDRatio = kdRatio.ToString("0.00");

        var rankName = playerRanks.TryGetValue(steamID, out var rank) ? rank : "Нету";

        string currentRankMessage = ReplaceColorPlaceholders(_config.CurrentRankMessage.Replace("{RANK_NAME}", rankName));
        player.PrintToChat(currentRankMessage);

        if (pointsToNextRank > 0)
        {
            string nextRankMessage = ReplaceColorPlaceholders(_config.NextRankMessage.Replace("{NEXT_RANK_NAME}", nextRankName).Replace("{POINTS_TO_NEXT_RANK}", pointsToNextRank.ToString()));
            player.PrintToChat(nextRankMessage);
        }
        else
        {
            string maxRankMessage = ReplaceColorPlaceholders(_config.MaxRankMessage.Replace("{RANK_NAME}", nextRankName));
            player.PrintToChat(maxRankMessage);
        }

        string statsMessage = ReplaceColorPlaceholders(_config.StatsMessage.Replace("{POINTS}", points.ToString()).Replace("{RANK_POSITION}", rankPosition.ToString()).Replace("{TOTAL_PLAYERS}", totalPlayers.ToString()).Replace("{KILLS}", kills.ToString()).Replace("{DEATHS}", deaths.ToString()).Replace("{KDRATIO}", formattedKDRatio));
        player.PrintToChat(statsMessage);
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

        string introMessage = ReplaceColorPlaceholders(_config.TopCommandIntroMessage);
        player.PrintToChat(introMessage);

        for (int i = 0; i < topPlayers.Count; i++)
        {
            var topPlayerInfo = topPlayersList[i];
            string playerMessage = ReplaceColorPlaceholders(
                _config.TopPlayerMessage.Replace("{POSITION}", (i + 1).ToString())
                                        .Replace("{NICKNAME}", topPlayerInfo.Nickname)
                                        .Replace("{POINTS}", topPlayerInfo.Points.ToString()));
            player.PrintToChat(playerMessage);
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

    private string ReplaceColorPlaceholders(string message)
    {
        if (message.Contains('{'))
        {
            string modifiedValue = message;
            foreach (FieldInfo field in typeof(ChatColors).GetFields())
            {
                string pattern = $"{{{field.Name}}}";
                if (message.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    modifiedValue = modifiedValue.Replace(pattern, field.GetValue(null).ToString(), StringComparison.OrdinalIgnoreCase);
                }
            }
            return modifiedValue;
        }

        return message;
    }

}
