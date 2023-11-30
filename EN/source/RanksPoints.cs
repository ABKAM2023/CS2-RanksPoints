using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using System.Reflection;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using MySqlConnector;
using Dapper;

namespace RanksPointsNamespace
{    
    public class RanksPoints : BasePlugin
    {
        private const string PluginAuthor = "ABKAM";
        private const string PluginName = "[RanksPoints] by ABKAM";
        private const string PluginVersion = "2.0";
        private const string DbConfigFileName = "dbconfig.json";
        private DatabaseConfig? dbConfig;
        private PluginConfig config;  
        private bool isActiveRoundForPoints;       
        private HashSet<string> activePlayers = new HashSet<string>();  
        private Dictionary<string, PlayerResetInfo> playerResetTimes = new Dictionary<string, PlayerResetInfo>();
        public class PluginConfig
        {
            public int MinPlayersForExperience { get; set; } = 4; 
            public int PointsPerRoundWin { get; set; } = 2; 
            public int PointsPerRoundLoss { get; set; } = -2; 
            public int PointsPerMVP { get; set; } = 3;
            public int PointsForSuicide { get; set; } = -6; 
            public int PointsForKill { get; set; } = 5; 
            public int PointsForDeath { get; set; } = -5;
            public int PointsForAssist { get; set; } = 1; 
            public int PointsForNoScopeAWP { get; set; } = 1; 
            public int PointsForHeadshot { get; set; } = 1;
            public int PointsForBombDefusal { get; set; } = 2; 
            public int PointsForBombPlanting { get; set; } = 2; 
            public string GetActivePlayerCountMsg { get; set; } = "[ {Yellow}RanksPoints {White}] At least {Red}{MIN_PLAYERS} {White}players are required to earn experience.";
            public string PointsChangeMessage { get; set; } = "[ {Yellow}RanksPoints{White} ] Your experience:{COLOR} {POINTS} [{SIGN}{CHANGE_POINTS} for {REASON}]";
            public string SuicideMessage { get; set; } = "suicide"; 
            public string SuicideMessageColor { get; set; } = "{Red}"; 
            public string DeathMessage { get; set; } = "death"; 
            public string DeathMessageColor { get; set; } = "{Red}"; 
            public string KillMessage { get; set; } = "kill"; 
            public string KillMessageColor { get; set; } = "{Green}";  
            public string NoScopeAWPMessage { get; set; } = "kill with AWP without a scope";
            public string NoScopeAWPMessageColor { get; set; } = "{Blue}";
            public string HeadshotMessage { get; set; } = "headshot"; 
            public string HeadshotMessageColor { get; set; } = "{Yellow}";
            public string AssistMessage { get; set; } = "assist"; 
            public string AssistMessageColor { get; set; } = "{Blue}";  
            public string RoundWinMessage { get; set; } = "round win";
            public string RoundWinMessageColor { get; set; } = "{Green}";           
            public string RoundLossMessage { get; set; } = "round loss"; 
            public string RoundLossMessageColor { get; set; } = "{Red}";   
            public string MVPMessage { get; set; } = "MVP"; 
            public string MVPMessageColor { get; set; } = "{Gold}";    
            public string BombDefusalMessage { get; set; } = "bomb defusal";          
            public string BombDefusalMessageColor { get; set; } = "{Green}";     
            public string BombPlantingMessage { get; set; } = "bomb planting";             
            public string BombPlantingMessageColor { get; set; } = "{Green}";          
            public string RankCommandMessage { get; set; } = "[ {Yellow}RanksPoints {White}] Rank: {Green}{RANK_NAME} {White}| Position: {Blue}{PLACE}/{TOTAL_PLAYERS} {White}| Experience: {Gold}{POINTS} {White}| Kills: {Green}{KILLS} {White}| Deaths: {Red}{DEATHS} {White}| KDR: {Yellow}{KDR} {White}| Time on server: {Gold}{PLAY_TIME}";                                                            
            public string TimeFormat { get; set; } = "{0}d {1}h {2}min";   
            public string TopCommandIntroMessage { get; set; } = "[ {Blue}Top players{White} ]"; 
            public string TopCommandPlayerMessage { get; set; } = "{INDEX}. {Grey}{NAME} - {Blue}{POINTS} points{White}"; 
            public string TopCommandNoDataMessage { get; set; } = "[ {Red}Error{White} ] No data on top players."; 
            public string TopCommandErrorMessage { get; set; } = "[ {Red}Error{White} ] An error occurred while executing the command."; 
            public string TopKillsCommandIntroMessage { get; set; } = "[ {Green}Top players by kills{White} ]";
            public string TopKillsCommandPlayerMessage { get; set; } = "{INDEX}. {Grey}{NAME} - {Green}{KILLS} kills{White}";
            public string TopKillsCommandNoDataMessage { get; set; } = "[ {Red}Error{White} ] No data on top players by kills.";
            public string TopKillsCommandErrorMessage { get; set; } = "[ {Red}Error{White} ] An error occurred while executing the command.";
            public string TopDeathsCommandIntroMessage { get; set; } = "[ {Red}Top players by deaths{White} ]";
            public string TopDeathsCommandPlayerMessage { get; set; } = "{INDEX}. {Grey}{NAME}{White} - {Red}{DEATHS} deaths{White}";
            public string TopDeathsCommandNoDataMessage { get; set; } = "[ {Red}Error{White} ] No data on top players by deaths.";
            public string TopDeathsCommandErrorMessage { get; set; } = "[ {Red}Error{White} ] An error occurred while executing the command."; 
            public string TopKDRCommandIntroMessage { get; set; } = "[ {Yellow}Top players by KDR{White} ]";
            public string TopKDRCommandPlayerMessage { get; set; } = "{INDEX}. {Grey}{NAME}{White} - {Yellow}KDR: {KDR}";
            public string TopKDRCommandNoDataMessage { get; set; } = "[ {Red}Error{White} ] No data on top players by KDR.";
            public string TopKDRCommandErrorMessage { get; set; } = "[ {Red}Error{White} ] An error occurred while executing the command.";
            public string TopTimeCommandIntroMessage { get; set; } = "[ {Gold}Top players by server time{White} ]";
            public string TopTimeCommandPlayerMessage { get; set; } = "{INDEX}. {Grey}{NAME} - {Gold}{TIME}{White}";
            public string TopTimeCommandNoDataMessage { get; set; } = "[ {Red}Error{White} ] No data on top players by server time.";
            public string TopTimeCommandErrorMessage { get; set; } = "[ {Red}Error{White} ] An error occurred while executing the command.";
            public string TopTimeFormat { get; set; } = "{0}d {1}h {2}min";
            public string ResetStatsCooldownMessage { get; set; } = "[ {Red}RanksPoints {White}] You can reset your statistics only once every 3 hours.";
            public string ResetStatsSuccessMessage { get; set; } = "[ {Yellow}RanksPoints {White}] Your statistics have been reset.";
            public double ResetStatsCooldownHours { get; set; } = 3.0; 
            public string RanksCommandIntroMessage { get; set; } = "[ {Gold}List of ranks{White} ]";
            public string RanksCommandRankMessage { get; set; } = "{NAME} - {Green}{EXPERIENCE} experience{White}";
            public string RanksCommandNoDataMessage { get; set; } = "[ {Red}Error{White} ] No data on ranks.";
            public string RanksCommandErrorMessage { get; set; } = "[ {Red}Error{White} ] An error occurred while executing the command.";
            public string LvlCommandIntroMessage { get; set; } = "[ {Gold}List of available commands{White} ]"; 
            public string RankCommandDescription { get; set; } = "- {Green}!rank {White}- Displays your current rank and statistics";
            public string TopCommandDescription { get; set; } = "- {Green}!top {White}- Displays the top-10 players by points";
            public string TopKillsCommandDescription { get; set; } = "- {Green}!topkills {White}- Displays the top-10 players by kills";  
            public string TopDeathsCommandDescription { get; set; } = "- {Green}!topdeaths {White}- Displays the top-10 players by deaths";
            public string TopKDRCommandDescription { get; set; } = "- {Green}!topkdr {White}- Displays the top-10 players by KDR";
            public string TopTimeCommandDescription { get; set; } = "- {Green}!toptime {White}- Displays the top-10 players by server time";
            public string ResetStatsCommandDescription { get; set; } = "- {Green}!resetstats {White}- Reset your statistics (can be used once every 3 hours)";
            public string RanksCommandDescription { get; set; } = "- {Green}!ranks {White}- Displays a list of all ranks and the experience required to obtain them"; 
            public string RankUpMessage { get; set; } = "Your rank has been increased to {RANK_NAME}!";
            public string RankDownMessage { get; set; } = "Your rank has been decreased to {RANK_NAME}.";
  
        } 
        public class RankConfig
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public int MinExperience { get; set; } 
        }
        public void SaveConfig(PluginConfig config, string filePath)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("# Configuration file for RankPointsPlugin");

            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForKill), config.PointsForKill, "Points for kill - the number of points added to the player for killing an opponent.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForDeath), config.PointsForDeath, "Points deducted for death - the number of points subtracted from the player for dying.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForAssist), config.PointsForAssist, "Points for assist - the number of points added to the player for assisting in a kill.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForSuicide), config.PointsForSuicide, "Points for suicide - the number of points subtracted from the player for committing suicide.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForHeadshot), config.PointsForHeadshot, "Points for headshot - additional points for killing with a headshot.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsPerRoundWin), config.PointsPerRoundWin, "Points for round win - the number of points added to the player for their team's victory in a round.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsPerRoundLoss), config.PointsPerRoundLoss, "Points for round loss - the number of points subtracted from the player for their team's loss in a round.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsPerMVP), config.PointsPerMVP, "Points for MVP - the number of points added to the player for receiving the MVP title of the round.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForNoScopeAWP), config.PointsForNoScopeAWP, "Points for AWP no-scope kill - additional points for a kill without using the scope.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForBombDefusal), config.PointsForBombDefusal, "Points for bomb defusal");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForBombPlanting), config.PointsForBombPlanting, "Points for bomb planting");
            AppendConfigValueWithComment(stringBuilder, nameof(config.RankUpMessage), config.RankUpMessage, "Message for rank up.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.RankDownMessage), config.RankDownMessage, "Message for rank down.");


            stringBuilder.AppendLine("# Minimum number of players for experience calculation - players receive experience only if at least this number of players is on the server.");
            stringBuilder.AppendLine($"GetActivePlayerCountMsg: \"{EscapeMessage(config.GetActivePlayerCountMsg)}\"");
            AppendConfigValue(stringBuilder, nameof(config.MinPlayersForExperience), config.MinPlayersForExperience);

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# Messages upon receiving experience");         
            stringBuilder.AppendLine($"PointsChangeMessage: \"{EscapeMessage(config.PointsChangeMessage)}\"");
            stringBuilder.AppendLine("# Events");
            stringBuilder.AppendLine($"SuicideMessage: \"{EscapeMessage(config.SuicideMessage)}\"");
            stringBuilder.AppendLine($"SuicideMessageColor: \"{EscapeMessage(config.SuicideMessageColor)}\"");
            stringBuilder.AppendLine($"DeathMessage: \"{EscapeMessage(config.DeathMessage)}\"");
            stringBuilder.AppendLine($"DeathMessageColor: \"{EscapeMessage(config.DeathMessageColor)}\"");
            stringBuilder.AppendLine($"KillMessage: \"{EscapeMessage(config.KillMessage)}\"");
            stringBuilder.AppendLine($"KillMessageColor: \"{EscapeMessage(config.KillMessageColor)}\"");     
            stringBuilder.AppendLine($"NoScopeAWPMessage: \"{EscapeMessage(config.NoScopeAWPMessage)}\"");
            stringBuilder.AppendLine($"NoScopeAWPMessageColor: \"{EscapeMessage(config.NoScopeAWPMessageColor)}\"");            
            stringBuilder.AppendLine($"HeadshotMessage: \"{EscapeMessage(config.HeadshotMessage)}\"");
            stringBuilder.AppendLine($"HeadshotMessageColor: \"{EscapeMessage(config.HeadshotMessageColor)}\"");     
            stringBuilder.AppendLine($"AssistMessage: \"{EscapeMessage(config.AssistMessage)}\"");
            stringBuilder.AppendLine($"AssistMessageColor: \"{EscapeMessage(config.AssistMessageColor)}\"");            
            stringBuilder.AppendLine($"RoundWinMessage: \"{EscapeMessage(config.RoundWinMessage)}\"");
            stringBuilder.AppendLine($"RoundWinMessageColor: \"{EscapeMessage(config.RoundWinMessageColor)}\"");     
            stringBuilder.AppendLine($"RoundLossMessage: \"{EscapeMessage(config.RoundLossMessage)}\"");
            stringBuilder.AppendLine($"RoundLossMessageColor: \"{EscapeMessage(config.RoundLossMessageColor)}\"");     
            stringBuilder.AppendLine($"MVPMessage: \"{EscapeMessage(config.MVPMessage)}\"");
            stringBuilder.AppendLine($"MVPMessageColor: \"{EscapeMessage(config.MVPMessageColor)}\"");     
            stringBuilder.AppendLine($"BombDefusalMessage: \"{EscapeMessage(config.BombDefusalMessage)}\"");
            stringBuilder.AppendLine($"BombDefusalMessageColor: \"{EscapeMessage(config.BombDefusalMessageColor)}\"");   
            stringBuilder.AppendLine($"BombPlantingMessage: \"{EscapeMessage(config.BombPlantingMessage)}\"");
            stringBuilder.AppendLine($"BombPlantingMessageColor: \"{EscapeMessage(config.BombPlantingMessageColor)}\"");       
            
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !rank");
            stringBuilder.AppendLine($"RankCommandMessage : \"{EscapeMessage(config.RankCommandMessage)}\"");   
            stringBuilder.AppendLine($"TimeFormat: \"{EscapeMessage(config.TimeFormat)}\"");               
            
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !top");
            stringBuilder.AppendLine($"TopCommandIntroMessage : \"{EscapeMessage(config.TopCommandIntroMessage)}\"");   
            stringBuilder.AppendLine($"TopCommandPlayerMessage: \"{EscapeMessage(config.TopCommandPlayerMessage)}\"");       
            stringBuilder.AppendLine($"TopCommandNoDataMessage: \"{EscapeMessage(config.TopCommandNoDataMessage)}\"");     
            stringBuilder.AppendLine($"TopCommandErrorMessage: \"{EscapeMessage(config.TopCommandErrorMessage)}\"");     

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !topkills");       
            stringBuilder.AppendLine($"TopKillsCommandIntroMessage: \"{EscapeMessage(config.TopKillsCommandIntroMessage)}\"");
            stringBuilder.AppendLine($"TopKillsCommandPlayerMessage: \"{EscapeMessage(config.TopKillsCommandPlayerMessage)}\"");
            stringBuilder.AppendLine($"TopKillsCommandNoDataMessage: \"{EscapeMessage(config.TopKillsCommandNoDataMessage)}\"");
            stringBuilder.AppendLine($"TopKillsCommandErrorMessage: \"{EscapeMessage(config.TopKillsCommandErrorMessage)}\"");

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !topdeaths");              
            stringBuilder.AppendLine($"TopDeathsCommandIntroMessage: \"{EscapeMessage(config.TopDeathsCommandIntroMessage)}\"");
            stringBuilder.AppendLine($"TopDeathsCommandPlayerMessage: \"{EscapeMessage(config.TopDeathsCommandPlayerMessage)}\"");
            stringBuilder.AppendLine($"TopDeathsCommandNoDataMessage: \"{EscapeMessage(config.TopDeathsCommandNoDataMessage)}\"");
            stringBuilder.AppendLine($"TopDeathsCommandErrorMessage: \"{EscapeMessage(config.TopDeathsCommandErrorMessage)}\"");

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !topkdr");              
            stringBuilder.AppendLine($"TopKDRCommandIntroMessage: \"{EscapeMessage(config.TopKDRCommandIntroMessage)}\"");
            stringBuilder.AppendLine($"TopKDRCommandPlayerMessage: \"{EscapeMessage(config.TopKDRCommandPlayerMessage)}\"");
            stringBuilder.AppendLine($"TopKDRCommandNoDataMessage: \"{EscapeMessage(config.TopKDRCommandNoDataMessage)}\"");
            stringBuilder.AppendLine($"TopKDRCommandErrorMessage: \"{EscapeMessage(config.TopKDRCommandErrorMessage)}\"");  

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !toptime");              
            stringBuilder.AppendLine($"TopTimeCommandIntroMessage: \"{EscapeMessage(config.TopTimeCommandIntroMessage)}\"");
            stringBuilder.AppendLine($"TopTimeCommandPlayerMessage: \"{EscapeMessage(config.TopTimeCommandPlayerMessage)}\"");
            stringBuilder.AppendLine($"TopTimeCommandNoDataMessage : \"{EscapeMessage(config.TopTimeCommandNoDataMessage)}\"");
            stringBuilder.AppendLine($"TopTimeCommandErrorMessage: \"{EscapeMessage(config.TopTimeCommandErrorMessage)}\"");   
            stringBuilder.AppendLine($"TopTimeFormat: \"{EscapeMessage(config.TopTimeFormat)}\"");   
            
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !resetstats");              
            stringBuilder.AppendLine($"ResetStatsCooldownMessage: \"{EscapeMessage(config.ResetStatsCooldownMessage)}\"");
            stringBuilder.AppendLine($"ResetStatsSuccessMessage: \"{EscapeMessage(config.ResetStatsSuccessMessage)}\""); 
            stringBuilder.AppendLine($"ResetStatsCooldownHours: \"{EscapeMessage(config.ResetStatsCooldownHours.ToString())}\"");   

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !ranks");              
            stringBuilder.AppendLine($"RanksCommandIntroMessage: \"{EscapeMessage(config.RanksCommandIntroMessage)}\"");
            stringBuilder.AppendLine($"RanksCommandRankMessage: \"{EscapeMessage(config.RanksCommandRankMessage)}\""); 
            stringBuilder.AppendLine($"RanksCommandNoDataMessage: \"{EscapeMessage(config.RanksCommandNoDataMessage)}\"");   
            stringBuilder.AppendLine($"RanksCommandErrorMessage: \"{EscapeMessage(config.RanksCommandErrorMessage)}\"");     

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !lvl");    
            stringBuilder.AppendLine($"LvlCommandIntroMessage: \"{EscapeMessage(config.LvlCommandIntroMessage)}\"");                      
            stringBuilder.AppendLine($"RankCommandDescription: \"{EscapeMessage(config.RankCommandDescription)}\"");
            stringBuilder.AppendLine($"TopCommandDescription: \"{EscapeMessage(config.TopCommandDescription)}\""); 
            stringBuilder.AppendLine($"TopKillsCommandDescription: \"{EscapeMessage(config.TopKillsCommandDescription)}\"");   
            stringBuilder.AppendLine($"TopDeathsCommandDescription: \"{EscapeMessage(config.TopDeathsCommandDescription)}\"");   
            stringBuilder.AppendLine($"TopKDRCommandDescription: \"{EscapeMessage(config.TopKDRCommandDescription)}\"");
            stringBuilder.AppendLine($"TopTimeCommandDescription: \"{EscapeMessage(config.TopTimeCommandDescription)}\""); 
            stringBuilder.AppendLine($"ResetStatsCommandDescription: \"{EscapeMessage(config.ResetStatsCommandDescription)}\"");   
            stringBuilder.AppendLine($"RanksCommandDescription: \"{EscapeMessage(config.RanksCommandDescription)}\"");                                                                

            File.WriteAllText(filePath, stringBuilder.ToString());
        }

        private string EscapeMessage(string message)
        {
            return message.Replace("\"", "\\\"").Replace("\n", "\\n");
        }

        private void AppendConfigValueWithComment(StringBuilder sb, string key, object value, string comment)
        {
            sb.AppendLine($"# {comment}");
            sb.AppendLine($"{key}: {value}");
            sb.AppendLine();
        }
        private void AppendConfigValue(StringBuilder sb, string key, object value)
        {
            sb.AppendLine($"{key}: {value}");
        }        
        private List<RankConfig> LoadRanksConfig()
        {
            var filePath = Path.Combine(ModuleDirectory, "settings_ranks.yml");

            if (!File.Exists(filePath))
            {
                var defaultRanks = new List<RankConfig>
                {
                    new RankConfig { Id = 1, Name = "Silver - I", MinExperience = 0 },
                    new RankConfig { Id = 2, Name = "Silver - II", MinExperience = 10 },
                    new RankConfig { Id = 3, Name = "Silver - III", MinExperience = 25 },
                    new RankConfig { Id = 4, Name = "Silver - IV", MinExperience = 50 },
                    new RankConfig { Id = 5, Name = "Silver Elite", MinExperience = 75 },
                    new RankConfig { Id = 6, Name = "Silver - Grand Master", MinExperience = 100 },
                    new RankConfig { Id = 7, Name = "Gold Star - I", MinExperience = 150 },
                    new RankConfig { Id = 8, Name = "Gold Star - II", MinExperience = 200 },
                    new RankConfig { Id = 9, Name = "Gold Star - III", MinExperience = 300 },
                    new RankConfig { Id = 10, Name = "Gold Star - Master", MinExperience = 500 },
                    new RankConfig { Id = 11, Name = "Master Guardian - I", MinExperience = 750 },
                    new RankConfig { Id = 12, Name = "Master Guardian - II", MinExperience = 1000 },
                    new RankConfig { Id = 13, Name = "Master Guardian - Elite", MinExperience = 1500 },
                    new RankConfig { Id = 14, Name = "Distinguished Master Guardian", MinExperience = 2000 },
                    new RankConfig { Id = 15, Name = "Legendary Eagle", MinExperience = 3000 },
                    new RankConfig { Id = 16, Name = "Legendary Eagle Master", MinExperience = 5000 },
                    new RankConfig { Id = 17, Name = "Supreme Master - Highest Rank", MinExperience = 7500 },
                    new RankConfig { Id = 18, Name = "Global Elite", MinExperience = 10000 }
                };

                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var yaml = serializer.Serialize(defaultRanks);
                File.WriteAllText(filePath, yaml);

                Console.WriteLine("Rank configuration file created with default settings.");

                return defaultRanks;
            }

            try
            {
                var yaml = File.ReadAllText(filePath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();
                var ranksConfig = deserializer.Deserialize<List<RankConfig>>(yaml);

                return ranksConfig ?? new List<RankConfig>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading the rank configuration file: {ex.Message}");
                return new List<RankConfig>();
            }
        }


        public static string ConvertSteamID64ToSteamID(string steamId64)
        {
            if (ulong.TryParse(steamId64, out var communityId) && communityId > 76561197960265728)
            {
                var authServer = (communityId - 76561197960265728) % 2;
                var authId = (communityId - 76561197960265728 - authServer) / 2;
                return $"STEAM_0:{authServer}:{authId}";
            }
            return null; 
        }
        public override void Load(bool hotReload)
        {
            base.Load(hotReload);
            CreateDbConfigIfNotExists();
            dbConfig = DatabaseConfig.ReadFromJsonFile(Path.Combine(ModuleDirectory, DbConfigFileName));
            RegisterListener<Listeners.OnClientConnected>(OnClientConnected);
            RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
            RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
            RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
            RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
            RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
            RegisterEventHandler<EventRoundMvp>(OnPlayerMVP);
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
            RegisterEventHandler<EventBombExploded>(OnBombExploded);
            RegisterEventHandler<EventBombDefused>(OnBombDefused);
            isActiveRoundForPoints = true; 
            CreateTable();
            config = LoadOrCreateConfig();
            LoadRanksConfig();

            CreateDbConfigIfNotExists();
            dbConfig = DatabaseConfig.ReadFromJsonFile(Path.Combine(ModuleDirectory, DbConfigFileName));            
        }
        private void OnClientConnected(int playerSlot)
        {
            var player = Utilities.GetPlayerFromSlot(playerSlot);
            if (player != null && !player.IsBot)
            {
                var steamId64 = player.SteamID.ToString();
                var steamId = ConvertSteamID64ToSteamID(steamId64); 
                var playerName = GetPlayerNickname(steamId64);
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    var updateQuery = "UPDATE lvl_base SET lastconnect = @LastConnect WHERE steam = @SteamID;";
                    var insertQuery = "INSERT INTO lvl_base (steam, name, lastconnect) VALUES (@SteamID, @Name, @LastConnect) ON DUPLICATE KEY UPDATE lastconnect = @LastConnect;";
                    connection.Execute(insertQuery, new { SteamID = steamId, Name = playerName, LastConnect = currentTime });
                }

                activePlayers.Add(steamId);  
            }
        }
        private HookResult OnRoundStart(EventRoundStart roundStartEvent, GameEventInfo info)
        {
            isActiveRoundForPoints = GetActivePlayerCount() >= config.MinPlayersForExperience;

            if (!isActiveRoundForPoints)
            {
                string message = config.GetActivePlayerCountMsg
                    .Replace("{MIN_PLAYERS}", config.MinPlayersForExperience.ToString());
                message = ReplaceColorPlaceholders(message); 
                BroadcastToPlayers(message); 
            }

            return HookResult.Continue;
        }
        private HookResult OnBombExploded(EventBombExploded eventBombPlanted, GameEventInfo info)
        {      
            var planterSteamId64 = eventBombPlanted.Userid.SteamID.ToString();
            var planterSteamId = ConvertSteamID64ToSteamID(planterSteamId64);

            if (config.PointsForBombPlanting != 0)
            {
                string BombPlantingMessageColor = ReplaceColorPlaceholders(config.BombPlantingMessageColor);       
                AddOrRemovePoints(planterSteamId, config.PointsForBombPlanting, eventBombPlanted.Userid, config.BombPlantingMessage, BombPlantingMessageColor);
            }    

            return HookResult.Continue;
        }
        private HookResult OnBombDefused(EventBombDefused eventBombDefused, GameEventInfo info)
        {
            if (GetActivePlayerCount() < config.MinPlayersForExperience)
            {
                return HookResult.Continue;
            }   

            var defuserSteamId64 = eventBombDefused.Userid.SteamID.ToString();
            var defuserSteamId = ConvertSteamID64ToSteamID(defuserSteamId64);

            if (config.PointsForBombDefusal != 0)
            {
                string BombDefusalMessageColor = ReplaceColorPlaceholders(config.BombDefusalMessageColor);  
                AddOrRemovePoints(defuserSteamId, config.PointsForBombDefusal, eventBombDefused.Userid, config.BombDefusalMessage, BombDefusalMessageColor);
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
        private PluginConfig LoadOrCreateConfig()
        {
            var filePath = Path.Combine(ModuleDirectory, "Config.yml");
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

        private int GetActivePlayerCount()
        {
            return activePlayers.Count;
        }
        private HookResult OnWeaponFire(EventWeaponFire fireEvent, GameEventInfo info)
        {
            var shooterSteamId64 = fireEvent.Userid.SteamID.ToString();
            var shooterSteamId = ConvertSteamID64ToSteamID(shooterSteamId64);

            UpdateShoots(shooterSteamId);

            return HookResult.Continue;
        }

        private HookResult OnPlayerHurt(EventPlayerHurt hurtEvent, GameEventInfo info)
        {
            if (hurtEvent.Attacker != null && IsValidPlayer(hurtEvent.Attacker))
            {
                var attackerSteamId64 = hurtEvent.Attacker.SteamID.ToString();
                var attackerSteamId = ConvertSteamID64ToSteamID(attackerSteamId64);

                UpdateHits(attackerSteamId);
            }

            return HookResult.Continue;
        }
        private void UpdateShoots(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var updateQuery = "UPDATE lvl_base SET shoots = shoots + 1 WHERE steam = @SteamID;";
                connection.Execute(updateQuery, new { SteamID = steamId });
            }
        }

        private void UpdateHits(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var updateQuery = "UPDATE lvl_base SET hits = hits + 1 WHERE steam = @SteamID;";
                connection.Execute(updateQuery, new { SteamID = steamId });
            }
        }


        private HookResult OnPlayerDisconnect(EventPlayerDisconnect disconnectEvent, GameEventInfo info)
        {
            if (disconnectEvent?.Userid != null && !disconnectEvent.Userid.IsBot)
            {
                var steamId64 = disconnectEvent.Userid.SteamID.ToString();
                var steamId = ConvertSteamID64ToSteamID(steamId64);  

                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();

                    var playerData = connection.QueryFirstOrDefault("SELECT lastconnect, playtime FROM lvl_base WHERE steam = @SteamID", new { SteamID = steamId });

                    if (playerData != null)
                    {
                        var sessionTime = currentTime - playerData.lastconnect;
                        var newPlaytime = playerData.playtime + sessionTime;

                        var updateQuery = "UPDATE lvl_base SET playtime = @Playtime WHERE steam = @SteamID;";
                        connection.Execute(updateQuery, new { SteamID = steamId, Playtime = newPlaytime });
                    }
                }
            }
            if (disconnectEvent?.Userid != null && !disconnectEvent.Userid.IsBot)
            {
                var steamId64 = disconnectEvent.Userid.SteamID.ToString();
                var steamId = ConvertSteamID64ToSteamID(steamId64);
                activePlayers.Remove(steamId);
                Console.WriteLine($"Player disconnected: {steamId}. Active players now: {activePlayers.Count}");

                foreach (var player in activePlayers)
                {
                    Console.WriteLine("Remaining active player: " + player);
                }
            }

            return HookResult.Continue;
        }
        private HookResult OnRoundEnd(EventRoundEnd roundEndEvent, GameEventInfo info)
        {
            if (GetActivePlayerCount() < config.MinPlayersForExperience)
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
                    var steamId = ConvertSteamID64ToSteamID(steamID);

                    if (playerTeam == winnerTeam)
                    {
                        if (config.PointsPerRoundWin != 0)
                        {                          
                            string RoundWinMessageColor = ReplaceColorPlaceholders(config.RoundWinMessageColor);   
                            AddOrRemovePoints(steamId, config.PointsPerRoundWin, playerController, config.RoundWinMessage, RoundWinMessageColor);
                        }
                        UpdateRoundResult(steamId, true);
                    }
                    else
                    {
                        if (config.PointsPerRoundLoss != 0)
                        {                            
                            string RoundLossMessageColor = ReplaceColorPlaceholders(config.RoundLossMessageColor);    
                            AddOrRemovePoints(steamId, config.PointsPerRoundLoss, playerController, config.RoundLossMessage, RoundLossMessageColor);
                        }
                        UpdateRoundResult(steamId, false); 
                    }
                }
            }

            return HookResult.Continue;
        }
        private HookResult OnPlayerMVP(EventRoundMvp mvpEvent, GameEventInfo info)
        {
            if (GetActivePlayerCount() < config.MinPlayersForExperience)
            {
                return HookResult.Continue;
            }       
            var mvpPlayerSteamId64 = mvpEvent.Userid.SteamID.ToString();
            var mvpPlayerSteamId = ConvertSteamID64ToSteamID(mvpPlayerSteamId64);

            if (config.PointsPerMVP != 0)
            {
                string MVPMessageColor = ReplaceColorPlaceholders(config.MVPMessageColor);  
                AddOrRemovePoints(mvpPlayerSteamId, config.PointsPerMVP, mvpEvent.Userid, config.MVPMessage, MVPMessageColor);
            }

            return HookResult.Continue;
        }
        private void UpdateRoundResult(string steamId, bool isWin)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                string columnToUpdate = isWin ? "round_win" : "round_lose";
                var updateQuery = $"UPDATE lvl_base SET {columnToUpdate} = {columnToUpdate} + 1 WHERE steam = @SteamID;";
                connection.Execute(updateQuery, new { SteamID = steamId });
            }
        }
        private HookResult OnPlayerDeath(EventPlayerDeath deathEvent, GameEventInfo info)
        {
            if (GetActivePlayerCount() < config.MinPlayersForExperience)
            {
                return HookResult.Continue;
            }
            try
            {
                var victimSteamId64 = deathEvent.Userid.SteamID.ToString();
                var victimSteamId = ConvertSteamID64ToSteamID(victimSteamId64);

                if (deathEvent.Attacker != null && deathEvent.Attacker == deathEvent.Userid)
                {
                    if (config.PointsForSuicide != 0)
                    {
                        string suicideMessageColor = ReplaceColorPlaceholders(config.SuicideMessageColor);
                        AddOrRemovePoints(victimSteamId, config.PointsForSuicide, deathEvent.Userid, config.SuicideMessage, suicideMessageColor);
                    }
                }
                else
                {
                    if (config.PointsForDeath != 0)
                    {
                        string DeathMessageColor = ReplaceColorPlaceholders(config.DeathMessageColor);            
                        AddOrRemovePoints(victimSteamId, config.PointsForDeath, deathEvent.Userid, config.DeathMessage, DeathMessageColor);
                    }
                    UpdateKillsOrDeaths(victimSteamId, false);

                    if (deathEvent.Attacker != null && IsValidPlayer(deathEvent.Attacker))
                    {
                        var killerSteamId64 = deathEvent.Attacker.SteamID.ToString();
                        var killerSteamId = ConvertSteamID64ToSteamID(killerSteamId64);

                        if (config.PointsForKill != 0)
                        {
                            string KillMessageColor = ReplaceColorPlaceholders(config.KillMessageColor);                                   
                            AddOrRemovePoints(killerSteamId, config.PointsForKill, deathEvent.Attacker, config.KillMessage, KillMessageColor);
                        }
                        UpdateKillsOrDeaths(killerSteamId, true);

                        if (deathEvent.Weapon == "awp" && deathEvent.Noscope && config.PointsForNoScopeAWP != 0)
                        {
                            string NoScopeAWPMessageColor = ReplaceColorPlaceholders(config.NoScopeAWPMessageColor);   
                            AddOrRemovePoints(killerSteamId, config.PointsForNoScopeAWP, deathEvent.Attacker, config.NoScopeAWPMessage, NoScopeAWPMessageColor);
                        }
                        if (deathEvent.Headshot && config.PointsForHeadshot != 0)
                        {
                            string HeadshotMessageColor = ReplaceColorPlaceholders(config.HeadshotMessageColor);  
                            AddOrRemovePoints(killerSteamId, config.PointsForHeadshot, deathEvent.Attacker, config.HeadshotMessage, HeadshotMessageColor);
                            UpdateHeadshots(killerSteamId);
                        }  
                    }
                    if (deathEvent.Assister != null && IsValidPlayer(deathEvent.Assister) && config.PointsForAssist != 0)
                    {
                        var assisterSteamId64 = deathEvent.Assister.SteamID.ToString();
                        var assisterSteamId = ConvertSteamID64ToSteamID(assisterSteamId64);

                        string AssistMessageColor = ReplaceColorPlaceholders(config.AssistMessageColor);  
                        AddOrRemovePoints(assisterSteamId, config.PointsForAssist, deathEvent.Assister, config.AssistMessage, AssistMessageColor);
                        UpdateAssists(assisterSteamId);
                    }                                      
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in OnPlayerDeath: " + ex.Message);
            }
            return HookResult.Continue;
        }
        private bool IsValidPlayer(CCSPlayerController player)
        {
            return player != null && player.IsValid && !player.IsBot;
        }        

        private void UpdateHeadshots(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var updateQuery = "UPDATE lvl_base SET headshots = headshots + 1 WHERE steam = @SteamID;";
                connection.Execute(updateQuery, new { SteamID = steamId });
            }
        }

        private void UpdateAssists(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                var updateQuery = "UPDATE lvl_base SET assists = assists + 1 WHERE steam = @SteamID;";
                connection.Execute(updateQuery, new { SteamID = steamId });
            }
        }

        private void UpdateKillsOrDeaths(string steamId, bool isKill)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                string columnToUpdate = isKill ? "kills" : "deaths";
                var updateQuery = $"UPDATE lvl_base SET {columnToUpdate} = {columnToUpdate} + 1 WHERE steam = @SteamID;";
                connection.Execute(updateQuery, new { SteamID = steamId });
            }
        }
       private int AddOrRemovePoints(string steamId, int points, CCSPlayerController playerController, string reason, string messageColor)
        {
            int updatedPoints = 0;

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();

                try
                {
                    var currentPointsQuery = "SELECT value FROM lvl_base WHERE steam = @SteamID;";
                    var currentPoints = connection.ExecuteScalar<int>(currentPointsQuery, new { SteamID = steamId }, transaction);

                    updatedPoints = currentPoints + points;

                    if (updatedPoints < 0)
                    {
                        updatedPoints = 0; 
                    }

                    var updateQuery = "UPDATE lvl_base SET value = @NewPoints WHERE steam = @SteamID;";
                    connection.Execute(updateQuery, new { NewPoints = updatedPoints, SteamID = steamId }, transaction);

                    transaction.Commit();

                    string sign = points >= 0 ? "+" : "-";
                    string rawMessage = config.PointsChangeMessage
                        .Replace("{COLOR}", messageColor)
                        .Replace("{POINTS}", updatedPoints.ToString())
                        .Replace("{SIGN}", sign)
                        .Replace("{CHANGE_POINTS}", Math.Abs(points).ToString())
                        .Replace("{REASON}", reason);

                    string formattedMessage = ReplaceColorPlaceholders(rawMessage);

                    if (playerController != null && playerController.IsValid && !playerController.IsBot)
                    {
                        playerController.PrintToChat(formattedMessage);
                    }
                    
                    CheckAndUpdateRank(steamId, updatedPoints);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    Console.WriteLine("Exception in AddOrRemovePoints: " + ex.Message);
                }
            }

            return updatedPoints;
        }


        private bool CheckAndUpdateRank(string steamId, int updatedPoints)
        {

            var ranksConfig = LoadRanksConfig();
            var newRankIndex = 0;

            for (int i = 0; i < ranksConfig.Count; i++)
            {
                if (updatedPoints >= ranksConfig[i].MinExperience)
                {
                    newRankIndex = i;
               }
            }

            var newRank = ranksConfig[newRankIndex];

            if (newRank != null)
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    var currentRankQuery = "SELECT rank FROM lvl_base WHERE steam = @SteamID;";
                    var currentRankId = connection.ExecuteScalar<int>(currentRankQuery, new { SteamID = steamId });

                    if (currentRankId != newRank.Id)
                    {
                        var updateRankQuery = "UPDATE lvl_base SET rank = @NewRankId WHERE steam = @SteamID;";
                        connection.Execute(updateRankQuery, new { NewRankId = newRank.Id, SteamID = steamId });

                        bool isRankUp = newRank.Id > currentRankId;
                        NotifyPlayerOfRankChange(steamId, newRank.Name, isRankUp);
                        return true;
                    }
                }
            }
            return false;
        }
        private void NotifyPlayerOfRankChange(string steamId, string newRankName, bool isRankUp)
        {
            string steamId64 = ConvertSteamIDToSteamID64(steamId);
            string message = isRankUp ? config.RankUpMessage.Replace("{RANK_NAME}", newRankName) 
                                    : config.RankDownMessage.Replace("{RANK_NAME}", newRankName);

            foreach (var player in Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller"))
            {
                if (player != null && player.IsValid && !player.IsBot && player.SteamID.ToString() == steamId64)
                {
                    player.PrintToCenter(message);
                    break;
                }
            }
        }



        private string ConvertSteamIDToSteamID64(string steamID)
        {
            if (string.IsNullOrEmpty(steamID) || !steamID.StartsWith("STEAM_"))
            {
                return null;
            }

            try
            {
                string[] split = steamID.Replace("STEAM_", "").Split(':');
                long steamID64 = 76561197960265728 + Convert.ToInt64(split[2]) * 2 + Convert.ToInt64(split[1]);
                return steamID64.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ConvertSteamIDToSteamID64] Error converting SteamID: {ex.Message}");
                return null;
            }
        }
        private RankConfig? GetCurrentRank(string steamID64)
        {
            var steamID = ConvertSteamID64ToSteamID(steamID64);
            if (steamID == null)
            {
                Console.WriteLine("Invalid SteamID64 format.");
                return null;
            }

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var query = "SELECT rank FROM lvl_base WHERE steam = @SteamID;";
                var rankId = connection.QueryFirstOrDefault<int>(query, new { SteamID = steamID });

                if (rankId == 0)
                {
                    return new RankConfig { Id = 1, Name = "Silver - I", MinExperience = 0 };
                }

                var ranksConfig = LoadRanksConfig();
                return ranksConfig.FirstOrDefault(r => r.Id == rankId);
            }
        }



        private string GetPlayerNickname(string steamID)
        {
            var player = FindPlayerBySteamID(steamID);
            if (player != null)
            {
                return player.PlayerName;
            }
            return "Unkown";
        }

        private CCSPlayerController FindPlayerBySteamID(string steamID)
        {
            foreach (var player in Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller"))
            {
                if (player != null && player.IsValid && !player.IsBot && player.SteamID.ToString() == steamID)
                {
                    return player;
                }
            }
            return null;
        }


        private void CreateDbConfigIfNotExists()
        {
            string configFilePath = Path.Combine(ModuleDirectory, DbConfigFileName);
            if (!File.Exists(configFilePath))
            {
                var config = new DatabaseConfig
                {
                    DbHost = "YourHost",
                    DbUser = "YourUser",
                    DbPassword = "YourPassword",
                    DbName = "YourDatabase"
                };

                string jsonConfig = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(configFilePath, jsonConfig);
                Console.WriteLine("Database configuration file created.");
            }
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

        private PlayerStats GetPlayerStats(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var playerData = connection.QueryFirstOrDefault(@"
                    SELECT p.rank, p.value as points, p.kills, p.deaths, p.playtime,
                        (SELECT COUNT(*) FROM lvl_base WHERE value > p.value) + 1 as place,
                        (SELECT COUNT(*) FROM lvl_base) as totalPlayers
                    FROM lvl_base p
                    WHERE p.steam = @SteamID;", new { SteamID = steamId });


                if (playerData == null)
                {
                    throw new InvalidOperationException("Player data not found for the given SteamID.");
                }

                var ranksConfig = LoadRanksConfig();
                var rankConfig = ranksConfig.FirstOrDefault(r => r.Id == Convert.ToInt32(playerData.rank));
                if (rankConfig == null)
                {
                    throw new InvalidOperationException("Rank configuration not found for the given rank ID.");
                }

                var kdr = (playerData.deaths > 0) ? (double)playerData.kills / playerData.deaths : playerData.kills;
                
                int place = Convert.ToInt32(playerData.place);
                int totalPlayers = Convert.ToInt32(playerData.totalPlayers);

                return new PlayerStats
                {
                    RankName = rankConfig.Name,
                    Place = place,
                    TotalPlayers = totalPlayers,
                    Points = Convert.ToInt32(playerData.points),
                    Kills = playerData.kills,
                    PlayTime = playerData.playtime,
                    Deaths = playerData.deaths,
                    KDR = kdr
                };
            }
        }
        public class PlayerResetInfo
        {
            public DateTime LastResetTime { get; set; }
        }        
        public class PlayerStats
        {
            public string RankName { get; set; }
            public int Place { get; set; }
            public int TotalPlayers { get; set; }
            public int Points { get; set; }
            public int Kills { get; set; }
            public int PlayTime { get; set; }
            public int Deaths { get; set; }
            public double KDR { get; set; }
        }

        private string FormatTime(int playTimeSeconds)
        {
            TimeSpan timePlayed = TimeSpan.FromSeconds(playTimeSeconds);
            return string.Format(config.TimeFormat, timePlayed.Days, timePlayed.Hours, timePlayed.Minutes);
        }

        [ConsoleCommand("rank", "Показывает ваше текущее звание и статистику")]
        public void OnRankCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;

            var steamID64 = player.SteamID.ToString();
            var steamID = ConvertSteamID64ToSteamID(steamID64); 
            var stats = GetPlayerStats(steamID); 

            string message = config.RankCommandMessage
                .Replace("{RANK_NAME}", stats.RankName)
                .Replace("{PLACE}", stats.Place.ToString())
                .Replace("{TOTAL_PLAYERS}", stats.TotalPlayers.ToString())
                .Replace("{POINTS}", stats.Points.ToString())
                .Replace("{KILLS}", stats.Kills.ToString())
                .Replace("{DEATHS}", stats.Deaths.ToString())
                .Replace("{KDR}", stats.KDR.ToString("F2"))
                .Replace("{PLAY_TIME}", FormatTime(stats.PlayTime));

            message = ReplaceColorPlaceholders(message);
            player.PrintToChat(message);
        }
        [ConsoleCommand("top", "Displays the top 10 players by points")]
        public void OnTopCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("This command can only be used by players.");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    var topPlayersQuery = @"
                        SELECT steam, name, value
                        FROM lvl_base
                        ORDER BY value DESC
                        LIMIT 10;";

                    var topPlayers = connection.Query(topPlayersQuery).ToList();

                    if (topPlayers.Any())
                    {
                        string introMessage = ReplaceColorPlaceholders(config.TopCommandIntroMessage);
                        player.PrintToChat(introMessage);

                        for (int i = 0; i < topPlayers.Count; i++)
                        {
                            var topPlayerInfo = topPlayers[i];
                            string playerMessage = config.TopCommandPlayerMessage
                                .Replace("{INDEX}", (i + 1).ToString())
                                .Replace("{NAME}", topPlayerInfo.name)
                                .Replace("{POINTS}", topPlayerInfo.value.ToString());
                            playerMessage = ReplaceColorPlaceholders(playerMessage);
                            player.PrintToChat(playerMessage);
                        }
                    }
                    else
                    {
                        string noDataMessage = ReplaceColorPlaceholders(config.TopCommandNoDataMessage);
                        player.PrintToChat(noDataMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in OnTopCommand: " + ex.Message);
                string errorMessage = ReplaceColorPlaceholders(config.TopCommandErrorMessage);
                player.PrintToChat(errorMessage);
            }
        }

        [ConsoleCommand("topkills", "Displays the top 10 players by kills")]
        public void OnTopKillsCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("This command can only be used by players.");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    var topPlayersQuery = @"
                        SELECT steam, name, kills
                        FROM lvl_base
                        ORDER BY kills DESC
                        LIMIT 10;";

                    var topPlayers = connection.Query(topPlayersQuery).ToList();

                    if (topPlayers.Any())
                    {
                        string introMessage = ReplaceColorPlaceholders(config.TopKillsCommandIntroMessage);
                        player.PrintToChat(introMessage);

                        for (int i = 0; i < topPlayers.Count; i++)
                        {
                            var topPlayerInfo = topPlayers[i];
                            string playerMessage = ReplaceColorPlaceholders(config.TopKillsCommandPlayerMessage)
                                .Replace("{INDEX}", (i + 1).ToString())
                                .Replace("{NAME}", topPlayerInfo.name)
                                .Replace("{KILLS}", topPlayerInfo.kills.ToString());
                            player.PrintToChat(playerMessage);
                        }
                    }
                    else
                    {
                        player.PrintToChat(ReplaceColorPlaceholders(config.TopKillsCommandNoDataMessage));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in OnTopKillsCommand: " + ex.Message);
                player.PrintToChat(ReplaceColorPlaceholders(config.TopKillsCommandErrorMessage));
            }
        }

        [ConsoleCommand("topdeaths", "Displays the top 10 players by deaths")]
        public void OnTopDeathsCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("This command can only be used by players.");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    var topPlayersQuery = @"
                        SELECT steam, name, deaths
                        FROM lvl_base
                        ORDER BY deaths DESC
                        LIMIT 10;";

                    var topPlayers = connection.Query(topPlayersQuery).ToList();

                    if (topPlayers.Any())
                    {
                        string introMessage = ReplaceColorPlaceholders(config.TopDeathsCommandIntroMessage);
                        player.PrintToChat(introMessage);

                        for (int i = 0; i < topPlayers.Count; i++)
                        {
                            var topPlayerInfo = topPlayers[i];
                            string playerMessage = ReplaceColorPlaceholders(config.TopDeathsCommandPlayerMessage)
                                .Replace("{INDEX}", (i + 1).ToString())
                                .Replace("{NAME}", topPlayerInfo.name)
                                .Replace("{DEATHS}", topPlayerInfo.deaths.ToString());
                            player.PrintToChat(playerMessage);
                        }
                    }
                    else
                    {
                        player.PrintToChat(ReplaceColorPlaceholders(config.TopDeathsCommandNoDataMessage));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in OnTopDeathsCommand: " + ex.Message);
                player.PrintToChat(ReplaceColorPlaceholders(config.TopDeathsCommandErrorMessage));
            }
        }

        [ConsoleCommand("topkdr", "Displays the top 10 players by KDR")]
        public void OnTopKDRCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("This command can only be used by players.");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    var topPlayersQuery = @"
                        SELECT steam, name, kills, deaths, IF(deaths = 0, kills, kills/deaths) AS kdr
                        FROM lvl_base
                        ORDER BY kdr DESC, kills DESC
                        LIMIT 10;";

                    var topPlayers = connection.Query(topPlayersQuery).ToList();

                    if (topPlayers.Any())
                    {
                        string introMessage = ReplaceColorPlaceholders(config.TopKDRCommandIntroMessage);
                        player.PrintToChat(introMessage);

                        foreach (var topPlayerInfo in topPlayers)
                        {
                            string formattedKDR = topPlayerInfo.kdr.ToString("F2");
                            string playerMessage = config.TopKDRCommandPlayerMessage
                                .Replace("{INDEX}", (topPlayers.IndexOf(topPlayerInfo) + 1).ToString())
                                .Replace("{NAME}", topPlayerInfo.name)
                                .Replace("{KDR}", formattedKDR);
                            player.PrintToChat(ReplaceColorPlaceholders(playerMessage));
                        }
                    }
                    else
                    {
                        player.PrintToChat(ReplaceColorPlaceholders(config.TopKDRCommandNoDataMessage));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in OnTopKDRCommand: " + ex.Message);
                player.PrintToChat(ReplaceColorPlaceholders(config.TopKDRCommandErrorMessage));
            }
        }

        [ConsoleCommand("toptime", "Displays the top 10 players by server time")]
        public void OnTopTimeCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("This command can only be used by players.");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    var topPlayersQuery = @"
                        SELECT steam, name, playtime
                        FROM lvl_base
                        ORDER BY playtime DESC
                        LIMIT 10;";

                    var topPlayers = connection.Query(topPlayersQuery).ToList();

                    if (topPlayers.Any())
                    {
                        string introMessage = ReplaceColorPlaceholders(config.TopTimeCommandIntroMessage);
                        player.PrintToChat(introMessage);

                        for (int i = 0; i < topPlayers.Count; i++)
                        {
                            var topPlayerInfo = topPlayers[i];
                            TimeSpan timePlayed = TimeSpan.FromSeconds(topPlayerInfo.playtime);
                            string formattedTime = string.Format(config.TopTimeFormat,
                                timePlayed.Days, timePlayed.Hours, timePlayed.Minutes);
                            string playerMessage = config.TopTimeCommandPlayerMessage
                                .Replace("{INDEX}", (i + 1).ToString())
                                .Replace("{NAME}", topPlayerInfo.name)
                                .Replace("{TIME}", formattedTime);
                            player.PrintToChat(ReplaceColorPlaceholders(playerMessage));
                        }
                    }
                    else
                    {
                        player.PrintToChat(ReplaceColorPlaceholders(config.TopTimeCommandNoDataMessage));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in OnTopTimeCommand: " + ex.Message);
                player.PrintToChat(ReplaceColorPlaceholders(config.TopTimeCommandErrorMessage));
            }
        }

        [ConsoleCommand("resetstats", "Reset your statistics (can be used once every 3 hours)")]
        public void OnResetStatsCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null) return;

            var steamId64 = player.SteamID.ToString();
            var steamId = ConvertSteamID64ToSteamID(steamId64);

            if (playerResetTimes.TryGetValue(steamId, out var resetInfo))
            {
                if ((DateTime.UtcNow - resetInfo.LastResetTime).TotalHours < config.ResetStatsCooldownHours)
                {
                    string cooldownMessage = ReplaceColorPlaceholders(config.ResetStatsCooldownMessage);
                    player.PrintToChat(cooldownMessage);
                    return;
                }
            }

            ResetPlayerStats(steamId);
            playerResetTimes[steamId] = new PlayerResetInfo { LastResetTime = DateTime.UtcNow };
            
            string successMessage = ReplaceColorPlaceholders(config.ResetStatsSuccessMessage);
            player.PrintToChat(successMessage);
        }

        private void ResetPlayerStats(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var resetQuery = "UPDATE lvl_base SET kills = 0, deaths = 0, value = 0, shoots = 0, hits = 0, headshots = 0, assists = 0, round_win = 0, round_lose = 0, playtime = 0 WHERE steam = @SteamID;";
                connection.Execute(resetQuery, new { SteamID = steamId });
            }
        }

        [ConsoleCommand("rp_reloadconfig", "Reloads the configuration file Config.yml")]
        public void ReloadConfigCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                try
                {
                    config = LoadOrCreateConfig();

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
                    LoadRanksConfig();

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

        [ConsoleCommand("ranks", "Displays a list of all ranks and the experience required to obtain them")]
        public void OnRanksCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("This command can only be used by players.");
                return;
            }

            try
            {
                var ranksConfig = LoadRanksConfig();

                if (ranksConfig.Any())
                {
                    string introMessage = ReplaceColorPlaceholders(config.RanksCommandIntroMessage);
                    player.PrintToChat(introMessage);

                    foreach (var rank in ranksConfig)
                    {
                        string rankMessage = config.RanksCommandRankMessage
                            .Replace("{NAME}", rank.Name)
                            .Replace("{EXPERIENCE}", rank.MinExperience.ToString());
                        rankMessage = ReplaceColorPlaceholders(rankMessage);
                        player.PrintToChat(rankMessage);
                    }
                }
                else
                {
                    string noDataMessage = ReplaceColorPlaceholders(config.RanksCommandNoDataMessage);
                    player.PrintToChat(noDataMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception in OnRanksCommand: " + ex.Message);
                string errorMessage = ReplaceColorPlaceholders(config.RanksCommandErrorMessage);
                player.PrintToChat(errorMessage);
            }
        }
     
        [ConsoleCommand("lvl", "Displays a list of all available commands and their functions")]
        public void OnLvlCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("This command can only be used by players.");
                return;
            }

            string introMessage = ReplaceColorPlaceholders(config.LvlCommandIntroMessage);
            player.PrintToChat(introMessage);

            string[] commandDescriptions = new string[]
            {
                ReplaceColorPlaceholders(config.RankCommandDescription),
                ReplaceColorPlaceholders(config.TopCommandDescription),
                ReplaceColorPlaceholders(config.TopKillsCommandDescription),
                ReplaceColorPlaceholders(config.TopDeathsCommandDescription),
                ReplaceColorPlaceholders(config.TopKDRCommandDescription),
                ReplaceColorPlaceholders(config.TopTimeCommandDescription),
                ReplaceColorPlaceholders(config.ResetStatsCommandDescription),
                ReplaceColorPlaceholders(config.RanksCommandDescription)
            };

            foreach (var description in commandDescriptions)
            {
                player.PrintToChat(description);
            }
        }

        [ConsoleCommand("rp_resetranks", "Clears a player's statistics. Usage: rp_resetranks <steamid64> <data-type>")]
        [CommandHelper(minArgs: 2, usage: "<steamid64> <data-type>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void ResetRanksCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || player.IsBot) 
            {
                try
                {
                    var steamId64 = command.ArgByIndex(1);
                    var steamId = ConvertSteamID64ToSteamID(steamId64);
                    var dataType = command.ArgByIndex(2).ToLower();

                    if (steamId == null)
                    {
                        Console.WriteLine("Invalid SteamID64.");
                        return;
                    }

                    switch (dataType)
                    {
                        case "exp":
                            ResetPlayerExperience(steamId);
                            Console.WriteLine($"[RankPointsPlugin] Player experience and rank for {steamId} (SteamID64: {steamId64}) have been reset.");
                            break;
                        case "stats":
                            ResetPlayerStats(steamId);
                            Console.WriteLine($"[RankPointsPlugin] Player statistics for {steamId} (SteamID64: {steamId64}) have been reset.");
                            break;
                        case "time":
                            ResetPlayerPlaytime(steamId);
                            Console.WriteLine($"[RankPointsPlugin] Player playtime for {steamId} (SteamID64: {steamId64}) has been reset.");
                            break;
                        default:
                            Console.WriteLine("Invalid data type. Use 'exp', 'stats', or 'time'.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RankPointsPlugin] An error occurred while resetting statistics: {ex.Message}");
                }
            }
            else
            {
                player.PrintToChat($"{ChatColors.Red}This command is only available from the server console.");
            }
        }

        private void ResetPlayerExperience(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var resetQuery = "UPDATE lvl_base SET value = 0, rank = 1 WHERE steam = @SteamID;";
                connection.Execute(resetQuery, new { SteamID = steamId });
            }
        }

        private void ResetPlayerStats2(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var resetQuery = "UPDATE lvl_base SET kills = 0, deaths = 0, shoots = 0, hits = 0, headshots = 0, assists = 0, round_win = 0, round_lose = 0 WHERE steam = @SteamID;";
                connection.Execute(resetQuery, new { SteamID = steamId });
            }
        }

        private void ResetPlayerPlaytime(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var resetQuery = "UPDATE lvl_base SET playtime = 0 WHERE steam = @SteamID;";
                connection.Execute(resetQuery, new { SteamID = steamId });
            }
        }
        private void CreateTable()
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                var createTableQuery = string.Format(SQL_CreateTable, "lvl_base", "", "");
                connection.Execute(createTableQuery);
            }
        }

        private string ConnectionString
        {
            get
            {
                if (dbConfig?.DbHost == null || dbConfig?.DbUser == null || dbConfig?.DbPassword == null || dbConfig?.DbName == null)
                    throw new InvalidOperationException("Database configuration is not properly set.");
                
                return $"Server={dbConfig.DbHost};User ID={dbConfig.DbUser};Password={dbConfig.DbPassword};Database={dbConfig.DbName};";
            }
        }

        private const string SQL_CreateTable = "CREATE TABLE IF NOT EXISTS `{0}` ( `steam` varchar(22){1} PRIMARY KEY, `name` varchar(32){2}, `value` int NOT NULL DEFAULT 0, `rank` int NOT NULL DEFAULT 0, `kills` int NOT NULL DEFAULT 0, `deaths` int NOT NULL DEFAULT 0, `shoots` int NOT NULL DEFAULT 0, `hits` int NOT NULL DEFAULT 0, `headshots` int NOT NULL DEFAULT 0, `assists` int NOT NULL DEFAULT 0, `round_win` int NOT NULL DEFAULT 0, `round_lose` int NOT NULL DEFAULT 0, `playtime` int NOT NULL DEFAULT 0, `lastconnect` int NOT NULL DEFAULT 0);";
        public override string ModuleAuthor => PluginAuthor;
        public override string ModuleName => PluginName;
        public override string ModuleVersion => PluginVersion;
    }

    public class DatabaseConfig
    {
        public string? DbHost { get; set; }
        public string? DbUser { get; set; }
        public string? DbPassword { get; set; }
        public string? DbName { get; set; }

        public static DatabaseConfig ReadFromJsonFile(string filePath)
        {
            string jsonConfig = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<DatabaseConfig>(jsonConfig) ?? new DatabaseConfig();
        }
    }
}