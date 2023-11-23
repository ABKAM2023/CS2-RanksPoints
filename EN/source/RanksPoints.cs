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
    public bool AwardPointsForBots { get; set; } = false; 
    public string MinPlayersForExperienceMessage { get; set; } = "";
    public string MvpAwardMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Your experience: {LightYellow} {POINTS} [+{MVP_POINTS} for MVP]";
    public string GetActivePlayerCountMsg { get; set; } = "{White}[ {Red}RanksPoints {White}] At least {Red}{MIN_PLAYERS} {White}players required for gaining experience.";
    public string RoundWinMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Your experience: {Green}{POINTS} [+{ROUND_WIN_POINTS} for round win]";
    public string RoundLossMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Your experience: {Red}{POINTS} [{ROUND_LOSS_POINTS} for round loss]";
    public string SuicideMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Your experience: {Red}{POINTS} [{SUICIDE_POINTS} for suicide]";
    public string NoScopeKillMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Your experience: {Blue}{POINTS} [+{NOSCOPE_POINTS} for no-scope kill]";
    public string KillMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Your experience: {Green}{POINTS} [+{KILL_POINTS} for kill]";
    public string HeadshotMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Your experience: {Yellow}{POINTS} [+{HEADSHOT_POINTS} for headshot]";
    public string AssistMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Your experience: {Blue}{POINTS} [+{ASSIST_POINTS} for assist]";
    public string DeathMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Your experience: {Red}{POINTS} [{DEATH_POINTS} for death]";
    public string NoRankMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] You don't have a rank yet.";
    public string CurrentRankMessage { get; set; } = "{White}[ {Red}RanksPoints {White}] Your current rank: {Yellow}{RANK_NAME}{White}.";
    public string NextRankMessage { get; set; } = "{White}To the next rank {Yellow}{NEXT_RANK_NAME}{White}, you need {Green}{POINTS_TO_NEXT_RANK} {White}experience points.";
    public string MaxRankMessage { get; set; } = "{White}Congratulations, you have achieved the {Yellow}{RANK_NAME}{White} rank!";
    public string StatsMessage { get; set; } = "{White}Total experience: {Green}{POINTS}{White} Position: {Yellow}{RANK_POSITION}/{TOTAL_PLAYERS} {White}Kills: {Green}{KILLS}{White} Deaths: {Red}{DEATHS} {White}K/D Ratio: {Yellow}{KDRATIO}";
    public string TopCommandIntroMessage { get; set; } = "{White}[ {Red}Top Players {White}]";
    public string TopPlayerMessage { get; set; } = "{White}{POSITION}. {Grey}{NICKNAME}{White} - {Green}{POINTS} points";
    public string TopKillsIntroMessage { get; set; } = "{White}[ {Red}Top in Kills {White}]";
    public string TopKillsPlayerMessage { get; set; } = "{White}{POSITION}. {Grey}{NICKNAME}{White} - {Green}{KILLS} kills";    
    public string TopDeathsIntroMessage { get; set; } = "{White}[ {Red}Top in Deaths {White}]";
    public string TopDeathsPlayerMessage { get; set; } = "{POSITION}. {Grey}{NICKNAME}{White} - {Green}{DEATHS} deaths";
    public string TopKdrIntroMessage { get; set; } = "{White}[ {Red}Top KDR {White}]";
    public string TopKdrPlayerMessage { get; set; } = "{White}{POSITION}. {Grey}{NICKNAME}{White} - KDR: {Yellow}{KDR}";    
    public string RankUpMessage { get; set; } = "Congratulations! Your new rank: {RANK}.";
    public string RankDownMessage { get; set; } = "Your rank has been decreased to: {RANK}.";
}
public class RankPointsPlugin : BasePlugin
{    
    private Dictionary<string, int> playerKills;
    private Dictionary<string, int> playerDeaths;
    private Dictionary<string, bool> playerReachedMaxRank = new Dictionary<string, bool>();
    private string? topPlayersFilePath;
    private List<TopPlayer>? topPlayersList;  
    private Dictionary<string, int> playerPoints;
    private Dictionary<string, string> playerRanks;
    private string? dataFilePath;
    private static PluginConfig _config = new PluginConfig();
    private string? ranksFilePath;
    private string? topKillsFilePath;
    private string? topDeathsFilePath;
    private List<Rank> ranks;
    private List<TopPlayer> topKillersList = new List<TopPlayer>();
    private List<TopPlayer> topDeathsList = new List<TopPlayer>();
    private List<TopPlayerKDR> topKDRList = new List<TopPlayerKDR>();
    private bool isActiveRoundForPoints;
    public override string ModuleName => "Rank Points (by ABKAM)";
    public override string ModuleVersion => "1.0.1";
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
    private class TopPlayerKDR {
        public string? SteamID { get; set; }
        public string? Nickname { get; set; }
        public double KDR { get; set; }
    }
    private class TopPlayer
    {
        public string? SteamID { get; set; }
        public string? Nickname { get; set; }
        public int Points { get; set; }
    }  

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

        stringBuilder.AppendLine("# Configuration file for RankPoints");
        stringBuilder.AppendLine("# Points per kill - the number of points added to the player for killing an opponent.");
        stringBuilder.AppendLine($"PointsPerKill: {config.PointsPerKill}");
        stringBuilder.AppendLine("# Points deducted for death - the number of points subtracted from the player for dying.");
        stringBuilder.AppendLine($"PointsPerDeath: {config.PointsPerDeath}");
        stringBuilder.AppendLine("# Points for assists - the number of points added to the player for assisting in a kill.");
        stringBuilder.AppendLine($"PointsPerAssist: {config.PointsPerAssist}");
        stringBuilder.AppendLine("# Points for suicides - the number of points subtracted from the player for committing suicide.");
        stringBuilder.AppendLine($"PointsPerSuicide: {config.PointsPerSuicide}");
        stringBuilder.AppendLine("# Points for headshots - additional points for killing with a headshot.");
        stringBuilder.AppendLine($"PointsPerHeadshot: {config.PointsPerHeadshot}");
        stringBuilder.AppendLine("# Points for round wins - the number of points added to the player for winning a round for their team.");
        stringBuilder.AppendLine($"PointsPerRoundWin: {config.PointsPerRoundWin}");
        stringBuilder.AppendLine("# Points for round losses - the number of points subtracted from the player for losing a round for their team.");
        stringBuilder.AppendLine($"PointsPerRoundLoss: {config.PointsPerRoundLoss}");
        stringBuilder.AppendLine("# Points for MVP - the number of points added to the player for receiving the MVP award of the round.");
        stringBuilder.AppendLine($"PointsPerMVP: {config.PointsPerMVP}");
        stringBuilder.AppendLine("# Points for no-scope kills - additional points for killing without using a scope.");
        stringBuilder.AppendLine($"PointsPerNoScope: {config.PointsPerNoScope}");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("# Experience points for bots");
        stringBuilder.AppendLine($"AwardPointsForBots: {config.AwardPointsForBots}");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("# Minimum number of players for awarding experience - players are awarded experience only if there are at least this many players on the server.");
        stringBuilder.AppendLine($"MinPlayersForExperience: {config.MinPlayersForExperience}");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("# Event messages");
        stringBuilder.AppendLine($"MvpAwardMessage: \"{EscapeMessage(config.MvpAwardMessage)}\"");
        stringBuilder.AppendLine($"RoundWinMessage: \"{EscapeMessage(config.RoundWinMessage)}\"");
        stringBuilder.AppendLine($"RoundLossMessage: \"{EscapeMessage(config.RoundLossMessage)}\"");
        stringBuilder.AppendLine($"SuicideMessage: \"{EscapeMessage(config.SuicideMessage)}\"");
        stringBuilder.AppendLine($"NoScopeKillMessage: \"{EscapeMessage(config.NoScopeKillMessage)}\"");
        stringBuilder.AppendLine($"KillMessage: \"{EscapeMessage(config.KillMessage)}\"");
        stringBuilder.AppendLine($"HeadshotMessage: \"{EscapeMessage(config.HeadshotMessage)}\"");
        stringBuilder.AppendLine($"AssistMessage: \"{EscapeMessage(config.AssistMessage)}\"");
        stringBuilder.AppendLine($"DeathMessage: \"{EscapeMessage(config.DeathMessage)}\"");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("# Message for insufficient player count");
        stringBuilder.AppendLine($"GetActivePlayerCountMsg: \"{EscapeMessage(config.GetActivePlayerCountMsg)}\"");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("# !rank command messages");
        stringBuilder.AppendLine($"NoRankMessage: \"{EscapeMessage(config.NoRankMessage)}\"");
        stringBuilder.AppendLine($"CurrentRankMessage: \"{EscapeMessage(config.CurrentRankMessage)}\"");
        stringBuilder.AppendLine($"NextRankMessage: \"{EscapeMessage(config.NextRankMessage)}\"");
        stringBuilder.AppendLine($"MaxRankMessage: \"{EscapeMessage(config.MaxRankMessage)}\"");
        stringBuilder.AppendLine($"StatsMessage: \"{EscapeMessage(config.StatsMessage)}\"");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("# !top command messages");
        stringBuilder.AppendLine($"TopCommandIntroMessage: \"{EscapeMessage(config.TopCommandIntroMessage)}\"");
        stringBuilder.AppendLine($"TopPlayerMessage: \"{EscapeMessage(config.TopPlayerMessage)}\"");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("# !topdeaths command messages");
        stringBuilder.AppendLine($"TopDeathsIntroMessage: \"{EscapeMessage(config.TopDeathsIntroMessage)}\"");
        stringBuilder.AppendLine($"TopDeathsPlayerMessage: \"{EscapeMessage(config.TopDeathsPlayerMessage)}\"");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("# !topkdr command messages");
        stringBuilder.AppendLine($"TopKdrIntroMessage: \"{EscapeMessage(config.TopKdrIntroMessage)}\"");
        stringBuilder.AppendLine($"TopKdrPlayerMessage: \"{EscapeMessage(config.TopKdrPlayerMessage)}\"");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("# !topkills command messages");
        stringBuilder.AppendLine($"TopKillsIntroMessage: \"{EscapeMessage(config.TopKillsIntroMessage)}\"");
        stringBuilder.AppendLine($"TopKillsPlayerMessage: \"{EscapeMessage(config.TopKillsPlayerMessage)}\"");

        stringBuilder.AppendLine();
        stringBuilder.AppendLine("# Messages for rank promotion or demotion");
        stringBuilder.AppendLine($"RankUpMessage: \"{EscapeMessage(config.RankUpMessage)}\"");
        stringBuilder.AppendLine($"RankDownMessage: \"{EscapeMessage(config.RankDownMessage)}\"");



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
    private User?[] _usersArray = new User?[65];

    public override void Load(bool hotReload)
    {
        base.Load(hotReload);
        topPlayersFilePath = Path.Combine(ModuleDirectory, "stats_topPlayers.json");
        dataFilePath = Path.Combine(ModuleDirectory, "stats_playerPoints.json");
        ranksFilePath = Path.Combine(ModuleDirectory, "settings_ranks.yaml");   
        topKillsFilePath = Path.Combine(ModuleDirectory, "stats_TopKills.json");  
        topDeathsFilePath = Path.Combine(ModuleDirectory, "stats_TopDeaths.json");
        var configFilePath = Path.Combine(ModuleDirectory, "Config.yaml");

        _config = LoadOrCreateConfig(configFilePath);

        InitializeRanks();  
        LoadPlayerPoints(); 
        LoadTopPlayers();
        LoadStatistics();
        LoadTopKills();
        LoadTopDeaths();
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
                new Rank { Name = "Silver - I", PointsRequired = 0 },
                new Rank { Name = "Silver - II", PointsRequired = 10 },
                new Rank { Name = "Silver - III", PointsRequired = 25 },
                new Rank { Name = "Silver - IV", PointsRequired = 50 },
                new Rank { Name = "Silver Elite", PointsRequired = 75 },
                new Rank { Name = "Silver Master Elite", PointsRequired = 100 },
                new Rank { Name = "Gold Nova - I", PointsRequired = 150 },
                new Rank { Name = "Gold Nova - II", PointsRequired = 200 },
                new Rank { Name = "Gold Nova - III", PointsRequired = 300 },
                new Rank { Name = "Gold Nova Master", PointsRequired = 500 },
                new Rank { Name = "Master Guardian - I", PointsRequired = 750 },
                new Rank { Name = "Master Guardian - II", PointsRequired = 1000 },
                new Rank { Name = "Master Guardian Elite", PointsRequired = 1500 },
                new Rank { Name = "Distinguished Master Guardian", PointsRequired = 2000 },
                new Rank { Name = "Legendary Eagle", PointsRequired = 3000 },
                new Rank { Name = "Legendary Eagle Master", PointsRequired = 5000 },
                new Rank { Name = "Supreme Master First Class", PointsRequired = 7500 },
                new Rank { Name = "Global Elite", PointsRequired = 10000 },                                                                                                                                                                                                             
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

        var yamlWithComments = "# This is the rank configuration file for RankPoints\n" + yaml;
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
    private void LoadTopKills()
    {
        if (File.Exists(topKillsFilePath))
        {
            string json = File.ReadAllText(topKillsFilePath);
            topKillersList = JsonConvert.DeserializeObject<List<TopPlayer>>(json) ?? new List<TopPlayer>();
        }
        else
        {
            topKillersList = new List<TopPlayer>();
            SaveTopKills(); 
        }
    }
    private void SaveTopKills()
    {
        string json = JsonConvert.SerializeObject(topKillersList, Formatting.Indented);

        var directory = Path.GetDirectoryName(topKillsFilePath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (topKillsFilePath != null)
        {
            File.WriteAllText(topKillsFilePath, json);
        }
    }
    private void LoadTopDeaths()
    {
        if (File.Exists(topDeathsFilePath))
        {
            string json = File.ReadAllText(topDeathsFilePath);
            topDeathsList = JsonConvert.DeserializeObject<List<TopPlayer>>(json) ?? new List<TopPlayer>();
        }
        else
        {
            topDeathsList = new List<TopPlayer>();
            SaveTopDeaths(); 
        }
    }
    private void SaveTopDeaths()
    {
        string json = JsonConvert.SerializeObject(topDeathsList, Formatting.Indented);

        var directory = Path.GetDirectoryName(topDeathsFilePath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (topDeathsFilePath != null)
        {
            File.WriteAllText(topDeathsFilePath, json);
        }
    }
    private void SaveTopKDR()
    {
        string json = JsonConvert.SerializeObject(topKDRList, Formatting.Indented);
        var topKdrFilePath = Path.Combine(ModuleDirectory, "stats_TopKdr.json");

        var directory = Path.GetDirectoryName(topKdrFilePath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(topKdrFilePath, json);
    }
    private void LoadTopKDR()
    {
        var topKdrFilePath = Path.Combine(ModuleDirectory, "stats_TopKdr.json");
        if (File.Exists(topKdrFilePath))
        {
            string json = File.ReadAllText(topKdrFilePath);
            topKDRList = JsonConvert.DeserializeObject<List<TopPlayerKDR>>(json) ?? new List<TopPlayerKDR>();
        }
        else
        {
            topKDRList = new List<TopPlayerKDR>();
            SaveTopKDR();
        }
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
    private void UpdateTopKDRList()
    {
        var kdrCalculations = new Dictionary<string, double>();

        foreach (var kvp in playerKills)
        {
            string steamID = kvp.Key;
            int kills = kvp.Value;
            int deaths = playerDeaths.TryGetValue(steamID, out var deathCount) ? deathCount : 0;

            double kdr = CalculateKDRatio(kills, deaths);
            kdrCalculations.Add(steamID, kdr);
        }

        var sortedKDRs = kdrCalculations.OrderByDescending(kvp => kvp.Value)
                                        .Take(10);

        topKDRList = sortedKDRs.Select(kvp => new TopPlayerKDR
        {
            SteamID = kvp.Key,
            Nickname = GetPlayerNickname(kvp.Key),
            KDR = kvp.Value
        }).ToList();
    }
    private void UpdateTopKillsList()
    {
        var updatedTopKillers = playerKills
            .OrderByDescending(kvp => kvp.Value)
            .Take(10)
            .ToList();

        topKillersList = updatedTopKillers.Select(kvp => new TopPlayer
        {
            SteamID = kvp.Key,
            Nickname = GetPlayerNickname(kvp.Key),
            Points = kvp.Value  
        }).ToList();
    }
    private void UpdateTopDeathsList()
    {
        var updatedTopDeaths = playerDeaths
            .OrderByDescending(kvp => kvp.Value)
            .Take(10)
            .ToList();

        topDeathsList = updatedTopDeaths.Select(kvp => new TopPlayer
        {
            SteamID = kvp.Key,
            Nickname = GetPlayerNickname(kvp.Key),
            Points = kvp.Value  
        }).ToList();
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
                    Console.WriteLine($"[RankPointsPlugin] Error: Player with SteamID {steamId} not found in valid slots.");
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

                if (!playerPoints.ContainsKey(steamID))
                {
                    playerPoints[steamID] = 0; 
                }

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
        isActiveRoundForPoints = GetActivePlayerCount() >= _config.MinPlayersForExperience;

        if (!isActiveRoundForPoints)
        {
            return HookResult.Continue;
        }

        var killerSteamID = deathEvent.Attacker?.SteamID.ToString();
        var victimSteamID = deathEvent.Userid.SteamID.ToString();

        playerDeaths[victimSteamID] = playerDeaths.TryGetValue(victimSteamID, out var deaths) ? deaths + 1 : 1;

        if (killerSteamID == victimSteamID)
        {
            AddPointsForNonBotPlayer(victimSteamID, _config.PointsPerSuicide, deathEvent.Userid, _config.SuicideMessage, "{SUICIDE_POINTS}");
        }
        else
        {
            if (deathEvent.Attacker != null && deathEvent.Attacker.IsValid && !deathEvent.Attacker.IsBot &&
                (!deathEvent.Userid.IsBot || _config.AwardPointsForBots))
            {
                playerKills[killerSteamID] = playerKills.TryGetValue(killerSteamID, out var kills) ? kills + 1 : 1;

                int pointsForKill = _config.PointsPerKill;
                if (deathEvent.Noscope)
                {
                    AddPointsForNonBotPlayer(killerSteamID, _config.PointsPerNoScope, deathEvent.Attacker, _config.NoScopeKillMessage, "{NOSCOPE_POINTS}");
                }

                AddPointsForNonBotPlayer(killerSteamID, pointsForKill, deathEvent.Attacker, _config.KillMessage, "{KILL_POINTS}");

                if (deathEvent.Headshot)
                {
                    AddPointsForNonBotPlayer(killerSteamID, _config.PointsPerHeadshot, deathEvent.Attacker, _config.HeadshotMessage, "{HEADSHOT_POINTS}");
                }
            }

            if (deathEvent.Assister != null && deathEvent.Assister.IsValid && !deathEvent.Assister.IsBot)
            {
                var assisterSteamID = deathEvent.Assister.SteamID.ToString();
                AddPointsForNonBotPlayer(assisterSteamID, _config.PointsPerAssist, deathEvent.Assister, _config.AssistMessage, "{ASSIST_POINTS}");
            }

            if (victimSteamID != null && playerPoints.ContainsKey(victimSteamID) && !deathEvent.Userid.IsBot)
            {
                AddPointsForNonBotPlayer(victimSteamID, _config.PointsPerDeath, deathEvent.Userid, _config.DeathMessage, "{DEATH_POINTS}");
            }
        }

        SaveStatistics();
        UpdateTopKillsList();
        UpdateTopDeathsList();
        UpdateTopKDRList();
        SaveTopDeaths();        
        SaveTopKills();    
        SaveTopKDR();    

        return HookResult.Continue;
    }
    private void AddPointsForNonBotPlayer(string steamID, int pointsToAdd, CCSPlayerController playerController, string messageTemplate, string pointsPlaceholder)
    {
        if (playerController != null)
        {
            if (playerController.IsBot && !_config.AwardPointsForBots)
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

            string formattedMessage = messageTemplate
                .Replace("{POINTS}", playerPoints[steamID].ToString())
                .Replace(pointsPlaceholder, pointsToAdd.ToString());
            formattedMessage = ReplaceColorPlaceholders(formattedMessage);

            playerController.PrintToChat(formattedMessage);
        }
        else
        {

        }
    }
    private void AddPoints(string steamID, int pointsToAdd)
    {
        var playerController = FindPlayerBySteamID(steamID);

        if (playerController != null && playerController.IsBot && !_config.AwardPointsForBots)
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

            if (newRank != null)
            {
                playerRanks[steamID] = newRank;

                string messageTemplate = rankUp ? _config.RankUpMessage : _config.RankDownMessage;
                string message = messageTemplate.Replace("{RANK}", newRank);

                var rankUpdatePlayerController = FindPlayerBySteamID(steamID);
                if (rankUpdatePlayerController != null)
                {
                    rankUpdatePlayerController.PrintToCenter(message);
                }
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

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[RankPointsPlugin] Failed to save player points: {ex.Message}");
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
            return ("maximum rank", 0);
        }

        string nextRankName = nextRank.Name ?? "unknown rank";

        return (nextRankName, pointsNeeded);
    }

    private double CalculateKDRatio(int kills, int deaths)
    {
        return deaths > 0 ? (double)kills / deaths : kills;
    }

    [ConsoleCommand("rank", "Displays your current rank and information about the next one")]
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

        var rankName = playerRanks.TryGetValue(steamID, out var rank) ? rank : "Not available";

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

   
    [ConsoleCommand("top", "Shows the top 10 players by points")]
    public void OnTopCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            Console.WriteLine("This command can only be used by a player.");
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
    [ConsoleCommand("topkills", "Displays the top 10 players by kills")]
    public void OnTopKillsCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            Console.WriteLine("This command can only be used by a player.");
            return;
        }

        var topKillsPlayers = playerKills
            .OrderByDescending(kvp => kvp.Value)
            .Take(10)
            .ToList();

        string introMessage = ReplaceColorPlaceholders(_config.TopKillsIntroMessage);
        player.PrintToChat(introMessage);

        for (int i = 0; i < topKillsPlayers.Count; i++)
        {
            var topPlayerInfo = topKillsPlayers[i];
            string playerMessage = ReplaceColorPlaceholders(
                _config.TopKillsPlayerMessage.Replace("{POSITION}", (i + 1).ToString())
                                            .Replace("{NICKNAME}", GetPlayerNickname(topPlayerInfo.Key))
                                            .Replace("{KILLS}", topPlayerInfo.Value.ToString()));
            player.PrintToChat(playerMessage);
        }
    }
    [ConsoleCommand("topdeaths", "Displays the top 10 players by deaths")]
    public void OnTopDeathsCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            Console.WriteLine("This command can only be used by a player.");
            return;
        }

        var topDeathsPlayers = playerDeaths
            .OrderByDescending(kvp => kvp.Value)
            .Take(10)
            .ToList();

        string introMessage = ReplaceColorPlaceholders(_config.TopDeathsIntroMessage);
        player.PrintToChat(introMessage);

        for (int i = 0; i < topDeathsPlayers.Count; i++)
        {
            var topPlayerInfo = topDeathsPlayers[i];
            string playerMessage = ReplaceColorPlaceholders(
                _config.TopDeathsPlayerMessage.Replace("{POSITION}", (i + 1).ToString())
                                            .Replace("{NICKNAME}", GetPlayerNickname(topPlayerInfo.Key))
                                            .Replace("{DEATHS}", topPlayerInfo.Value.ToString()));
            player.PrintToChat(playerMessage);
        }
    }  
    [ConsoleCommand("topkdr", "Displays the top 10 players by KDR")]
    public void OnTopKDRCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            Console.WriteLine("This command can only be used by a player.");
            return;
        }

        UpdateTopKDRList(); 

        string introMessage = ReplaceColorPlaceholders(_config.TopKdrIntroMessage);
        player.PrintToChat(introMessage);

        foreach (var topPlayerKDR in topKDRList.Take(10))
        {
            string playerMessage = ReplaceColorPlaceholders(
                _config.TopKdrPlayerMessage.Replace("{POSITION}", (topKDRList.IndexOf(topPlayerKDR) + 1).ToString())
                                        .Replace("{NICKNAME}", topPlayerKDR.Nickname)
                                        .Replace("{KDR}", topPlayerKDR.KDR.ToString("0.00")));
            player.PrintToChat(playerMessage);
        }
    }

    [ConsoleCommand("rp_reloadconfig", "Reloads the configuration file Config.yml")]
    public void ReloadConfigCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            try
            {
                var configFilePath = Path.Combine(ModuleDirectory, "Config.yaml");

                _config = LoadOrCreateConfig(configFilePath);

                Console.WriteLine("[RankPointsPlugin] Configuration successfully reloaded.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RankPointsPlugin] Error reloading configuration: {ex.Message}");
            }
        }
        else
        {
            player.PrintToChat("This command is only available from the server console.");
        }
    }

    [ConsoleCommand("rp_reloadranks", "Reloads the configuration file settings_ranks.yaml")]
    public void ReloadRanksCommand(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            try
            {
                var ranksFilePath = Path.Combine(ModuleDirectory, "settings_ranks.yaml");

                LoadRanks(ranksFilePath);

                Console.WriteLine("[RankPointsPlugin] Rank configuration successfully reloaded.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RankPointsPlugin] Error reloading rank configuration: {ex.Message}");
            }
        }
        else
        {
            player.PrintToChat("{Red}This command is only available from the server console.");
        }
    }
    private void LoadRanks(string ranksFilePath)
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
    private void SaveTopPlayers()
    {
        string json = JsonConvert.SerializeObject(topPlayersList, Formatting.Indented);
        if (topPlayersFilePath != null)
        {
            File.WriteAllText(topPlayersFilePath, json);
        }
        else
        {

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

        return "Player not found";
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
