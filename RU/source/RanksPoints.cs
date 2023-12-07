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
using System.Threading;
using System.Threading.Tasks;
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
        private const string PluginVersion = "2.0.3";
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
            public string GetActivePlayerCountMsg { get; set; } = "[ {Yellow}RanksPoints {White}] Необходимо минимум {Red}{MIN_PLAYERS} {White}игроков для начисления опыта.";
            public string PointsChangeMessage { get; set; } = "[ {Yellow}RanksPoints{White} ] Ваш опыт:{COLOR} {POINTS} [{SIGN}{CHANGE_POINTS} за {REASON}]";
            public string SuicideMessage { get; set; } = "самоубийство"; 
            public string SuicideMessageColor { get; set; } = "{Red}"; 
            public string DeathMessage { get; set; } = "смерть"; 
            public string DeathMessageColor { get; set; } = "{Red}"; 
            public string KillMessage { get; set; } = "убийство"; 
            public string KillMessageColor { get; set; } = "{Green}";  
            public string NoScopeAWPMessage { get; set; } = "убийство с AWP без прицела";
            public string NoScopeAWPMessageColor { get; set; } = "{Blue}";
            public string HeadshotMessage { get; set; } = "выстрел в голову"; 
            public string HeadshotMessageColor { get; set; } = "{Yellow}";
            public string AssistMessage { get; set; } = "ассист"; 
            public string AssistMessageColor { get; set; } = "{Blue}";  
            public string RoundWinMessage { get; set; } = "победа в раунде";
            public string RoundWinMessageColor { get; set; } = "{Green}";           
            public string RoundLossMessage { get; set; } = "проигрыш в раунде"; 
            public string RoundLossMessageColor { get; set; } = "{Red}";   
            public string MVPMessage { get; set; } = "MVP"; 
            public string MVPMessageColor { get; set; } = "{Gold}";    
            public string BombDefusalMessage { get; set; } = "обезвреживание бомбы";          
            public string BombDefusalMessageColor { get; set; } = "{Green}";     
            public string BombPlantingMessage { get; set; } = "установку бомбы";             
            public string BombPlantingMessageColor { get; set; } = "{Green}";          
            public string RankCommandMessage { get; set; } = "[ {Yellow}RanksPoints {White}] Звание: {Green}{RANK_NAME} {White}| Место: {Blue}{PLACE}/{TOTAL_PLAYERS} {White}| Опыт: {Gold}{POINTS} {White}| Убийства: {Green}{KILLS} {White}| Смерти: {Red}{DEATHS} {White}| KDR: {Yellow}{KDR} {White}| Время на сервере: {Gold}{PLAY_TIME}";                                                            
            public string TimeFormat { get; set; } = "{0}д {1}ч {2}мин";   
            public string TopCommandIntroMessage { get; set; } = "[ {Blue}Топ игроков{White} ]"; 
            public string TopCommandPlayerMessage { get; set; } = "{INDEX}. {Grey}{NAME} - {Blue}{POINTS} очков{White}"; 
            public string TopCommandNoDataMessage { get; set; } = "[ {Red}Ошибка{White} ] Нет данных о топ игроках."; 
            public string TopCommandErrorMessage { get; set; } = "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."; 
            public string TopKillsCommandIntroMessage { get; set; } = "[ {Green}Топ игроков по убийствам{White} ]";
            public string TopKillsCommandPlayerMessage { get; set; } = "{INDEX}. {Grey}{NAME} - {Green}{KILLS} убийств{White}";
            public string TopKillsCommandNoDataMessage { get; set; } = "[ {Red}Ошибка{White} ] Нет данных о топ игроках по убийствам.";
            public string TopKillsCommandErrorMessage { get; set; } = "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды.";
            public string TopDeathsCommandIntroMessage { get; set; } = "[ {Red}Топ игроков по смертям{White} ]";
            public string TopDeathsCommandPlayerMessage { get; set; } = "{INDEX}. {Grey}{NAME}{White} - {Red}{DEATHS} смертей{White}";
            public string TopDeathsCommandNoDataMessage { get; set; } = "[ {Red}Ошибка{White} ] Нет данных о топ игроках по смертям.";
            public string TopDeathsCommandErrorMessage { get; set; } = "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."; 
            public string TopKDRCommandIntroMessage { get; set; } = "[ {Yellow}Топ игроков по KDR{White} ]";
            public string TopKDRCommandPlayerMessage { get; set; } = "{INDEX}. {Grey}{NAME}{White} - {Yellow}KDR: {KDR}";
            public string TopKDRCommandNoDataMessage { get; set; } = "[ {Red}Ошибка{White} ] Нет данных о топ игроках по KDR.";
            public string TopKDRCommandErrorMessage { get; set; } = "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды.";
            public string TopTimeCommandIntroMessage { get; set; } = "[ {Gold}Топ игроков по времени на сервере{White} ]";
            public string TopTimeCommandPlayerMessage { get; set; } = "{INDEX}. {Grey}{NAME} - {Gold}{TIME}{White}";
            public string TopTimeCommandNoDataMessage { get; set; } = "[ {Red}Ошибка{White} ] Нет данных о топ игроках по времени на сервере.";
            public string TopTimeCommandErrorMessage { get; set; } = "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды.";
            public string TopTimeFormat { get; set; } = "{0}д {1}ч {2}мин";
            public string ResetStatsCooldownMessage { get; set; } = "[ {Red}RanksPoints {White}] Сбросить статистику можно только раз в 3 часа.";
            public string ResetStatsSuccessMessage { get; set; } = "[ {Yellow}RanksPoints {White}] Ваша статистика сброшена.";
            public double ResetStatsCooldownHours { get; set; } = 3.0; 
            public string RanksCommandIntroMessage { get; set; } = "[ {Gold}Список званий{White} ]";
            public string RanksCommandRankMessage { get; set; } = "{NAME} - {Green}{EXPERIENCE} опыта{White}";
            public string RanksCommandNoDataMessage { get; set; } = "[ {Red}Ошибка{White} ] Нет данных о званиях.";
            public string RanksCommandErrorMessage { get; set; } = "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды.";
            public string LvlCommandIntroMessage { get; set; } = "[ {Gold}Список доступных команд{White} ]"; 
            public string RankCommandDescription { get; set; } = "- {Green}!rank {White}- Показывает ваше текущее звание и статистику";
            public string TopCommandDescription { get; set; } = "- {Green}!top {White}- Показывает топ-10 игроков по очкам";
            public string TopKillsCommandDescription { get; set; } = "- {Green}!topkills {White}- Показывает топ-10 игроков по убийствам";  
            public string TopDeathsCommandDescription { get; set; } = "- {Green}!topdeaths {White}- Показывает топ-10 игроков по смертям";
            public string TopKDRCommandDescription { get; set; } = "- {Green}!topkdr {White}- Показывает топ-10 игроков по KDR";
            public string TopTimeCommandDescription { get; set; } = "- {Green}!toptime {White}- Показывает топ-10 игроков по времени на сервере";
            public string ResetStatsCommandDescription { get; set; } = "- {Green}!resetstats {White}- Сбросить свою статистику (можно использовать раз в 3 часа)";
            public string RanksCommandDescription { get; set; } = "- {Green}!ranks {White}- Показывает список всех званий и опыта, необходимого для их получения";       
            public string RankUpMessage { get; set; } = "Ваше звание было повышено до {RANK_NAME}!";
            public string RankDownMessage { get; set; } = "Ваше звание было понижено до {RANK_NAME}.";

        } 
        public class RankConfig
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public int MinExperience { get; set; } 
            public string ClanTag { get; set; }
        }
        public void SaveConfig(PluginConfig config, string filePath)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("# Конфигурационный файл для RankPointsPlugin");

            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForKill), config.PointsForKill, "Очки за убийство - количество очков, добавляемое игроку за убийство противника.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForDeath), config.PointsForDeath, "Очки отнимаемые за смерть - количество очков, вычитаемое у игрока за смерть.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForAssist), config.PointsForAssist, "Очки за помощь - количество очков, добавляемое игроку за помощь в убийстве.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForSuicide), config.PointsForSuicide, "Очки за самоубийство - количество очков, вычитаемое у игрока за самоубийство.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForHeadshot), config.PointsForHeadshot, "Очки за выстрел в голову - дополнительные очки за убийство с выстрелом в голову.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsPerRoundWin), config.PointsPerRoundWin, "Очки за победу в раунде - количество очков, добавляемое игроку за победу его команды в раунде.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsPerRoundLoss), config.PointsPerRoundLoss, "Очки за проигрыш в раунде - количество очков, вычитаемое у игрока за проигрыш его команды в раунде.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsPerMVP), config.PointsPerMVP, "Очки за MVP - количество очков, добавляемое игроку за получение звания MVP раунда.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForNoScopeAWP), config.PointsForNoScopeAWP, "Очки за убийство с AWP без прицела - дополнительные очки за убийство без использования прицела.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForBombDefusal), config.PointsForBombDefusal, "Очки за обезвреживание бомбы");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForBombPlanting), config.PointsForBombPlanting, "Очки за установку бомбы");
            AppendConfigValueWithComment(stringBuilder, nameof(config.RankUpMessage), config.RankUpMessage, "Сообщение о повышении звания.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.RankDownMessage), config.RankDownMessage, "Сообщение о понижении звания.");



            stringBuilder.AppendLine("# Минимальное количество игроков для начисления опыта - игрокам начисляется опыт только если на сервере играет минимум это количество игроков.");
            stringBuilder.AppendLine($"GetActivePlayerCountMsg: \"{EscapeMessage(config.GetActivePlayerCountMsg)}\"");
            AppendConfigValue(stringBuilder, nameof(config.MinPlayersForExperience), config.MinPlayersForExperience);
 
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# Сообщения при получении опыта");                  
            stringBuilder.AppendLine($"PointsChangeMessage: \"{EscapeMessage(config.PointsChangeMessage)}\"");
            stringBuilder.AppendLine("# События");            
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
                    new RankConfig { Id = 0, Name = "Серебро - I", MinExperience = 0, ClanTag = "[Серебро - I]" },
                    new RankConfig { Id = 1, Name = "Серебро - II", MinExperience = 10, ClanTag = "[Серебро - II]" },
                    new RankConfig { Id = 2, Name = "Серебро - III", MinExperience = 25, ClanTag = "[Серебро - III]" },
                    new RankConfig { Id = 3, Name = "Серебро - IV", MinExperience = 50, ClanTag = "[Серебро - IV]" },
                    new RankConfig { Id = 4, Name = "Серебро Элита", MinExperience = 75, ClanTag = "[Серебро Элита]" },
                    new RankConfig { Id = 5, Name = "Серебро - Великий Магистр", MinExperience = 100, ClanTag = "[Серебро - ВМ]" },
                    new RankConfig { Id = 6, Name = "Золотая Звезда - I", MinExperience = 150, ClanTag = "[Золотая Звезда - I]" },
                    new RankConfig { Id = 7, Name = "Золотая Звезда - II", MinExperience = 200, ClanTag = "[Золотая Звезда - II]" },
                    new RankConfig { Id = 8, Name = "Золотая Звезда - III", MinExperience = 300, ClanTag = "[Золотая Звезда - III]" },
                    new RankConfig { Id = 9, Name = "Золотая Звезда - Магистр", MinExperience = 500, ClanTag = "[Золотая Звезда - М]" },
                    new RankConfig { Id = 10, Name = "Магистр-хранитель - I", MinExperience = 750, ClanTag = "[Магистр-хранитель - I]" },
                    new RankConfig { Id = 11, Name = "Магистр-хранитель - II", MinExperience = 1000, ClanTag = "[Магистр-хранитель - II]" },
                    new RankConfig { Id = 12, Name = "Магистр-хранитель - Элита", MinExperience = 1500, ClanTag = "[Магистр-хранитель - Э]" },
                    new RankConfig { Id = 13, Name = "Заслуженный Магистр-хранитель", MinExperience = 2000, ClanTag = "[Заслуженный М-х]" },
                    new RankConfig { Id = 14, Name = "Легендарный Беркут", MinExperience = 3000, ClanTag = "[Легендарный Беркут]" },
                    new RankConfig { Id = 15, Name = "Легендарный Беркут-магистр", MinExperience = 5000, ClanTag = "[Лег. Беркут-м]" },
                    new RankConfig { Id = 16, Name = "Великий Магистр - Высшего Ранга", MinExperience = 7500, ClanTag = "[Великий М-ВР]" },
                    new RankConfig { Id = 17, Name = "Всемирная Элита", MinExperience = 10000, ClanTag = "[Всемирная Элита]" }
                };

                var serializer = new SerializerBuilder()
                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                    .Build();

                var yaml = serializer.Serialize(defaultRanks);
                File.WriteAllText(filePath, yaml);

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
                Console.WriteLine($"Ошибка при чтении файла конфигурации званий: {ex.Message}");
                return new List<RankConfig>();
            }
        }
        private async void SetPlayerClanTag(CCSPlayerController player)
        {
            if (player == null || !player.IsValid || player.IsBot || player.IsHLTV)
            {
                return;
            }
            try
            {
                var steamID64 = player.SteamID.ToString();
                var rank = await GetCurrentRankAsync(steamID64);

                ExecuteOnMainThread(() => 
                {
                    if (player != null && player.IsValid && rank != null && !string.IsNullOrEmpty(rank.ClanTag))
                    {
                        player.Clan = rank.ClanTag;
                    }
                    else
                    {
                        Console.WriteLine("[SetPlayerClanTag] Не удалось установить клан-тег: игрок или ранг отсутствует.");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SetPlayerClanTag] Ошибка при получении ранга: {ex.Message}");
            }
        }

        public static string ConvertSteamID64ToSteamID(string steamId64)
        {
            if (ulong.TryParse(steamId64, out var communityId) && communityId > 76561197960265728)
            {
                var authServer = (communityId - 76561197960265728) % 2;
                var authId = (communityId - 76561197960265728 - authServer) / 2;
                return $"STEAM_1:{authServer}:{authId}";
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
        private async Task UpdatePlayerConnectionAsync(string steamId, string playerName, long currentTime)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                var insertQuery = $"INSERT INTO {dbConfig.Name} (steam, name, lastconnect) VALUES (@SteamID, @Name, @LastConnect) ON DUPLICATE KEY UPDATE lastconnect = @LastConnect;";
                await connection.ExecuteAsync(insertQuery, new { SteamID = steamId, Name = playerName, LastConnect = currentTime });
            }
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

                var updateTask = UpdatePlayerConnectionAsync(steamId, playerName, currentTime);
                HandleAsyncOperation(updateTask);

                activePlayers.Add(steamId);  
                SetPlayerClanTag(player);
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
            if (GetActivePlayerCount() < config.MinPlayersForExperience)
            {
                return HookResult.Continue;
            }   

            var planterSteamId64 = eventBombPlanted.Userid.SteamID.ToString();
            var planterSteamId = ConvertSteamID64ToSteamID(planterSteamId64);

            if (config.PointsForBombPlanting != 0)
            {
                string BombPlantingMessageColor = ReplaceColorPlaceholders(config.BombPlantingMessageColor);       

                var pointsTask = AddOrRemovePointsAsync(planterSteamId, config.PointsForBombPlanting, eventBombPlanted.Userid, config.BombPlantingMessage, BombPlantingMessageColor);
                HandleAsyncPointsOperation(pointsTask);
            }    

            return HookResult.Continue;
        }

        private void HandleAsyncPointsOperation(Task task)
        {
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Console.WriteLine($"Error in async points operation: {t.Exception}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
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

                var pointsTask = AddOrRemovePointsAsync(defuserSteamId, config.PointsForBombDefusal, eventBombDefused.Userid, config.BombDefusalMessage, BombDefusalMessageColor);
                HandleAsyncPointsOperation(pointsTask);
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
        private async Task UpdateShootsAsync(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var updateQuery = $"UPDATE {dbConfig.Name} SET shoots = shoots + 1 WHERE steam = @SteamID;";
                await connection.ExecuteAsync(updateQuery, new { SteamID = steamId });
            }
        }

        private async Task UpdateHitsAsync(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var updateQuery = $"UPDATE {dbConfig.Name} SET hits = hits + 1 WHERE steam = @SteamID;";
                await connection.ExecuteAsync(updateQuery, new { SteamID = steamId });
            }
        }

        private HookResult OnWeaponFire(EventWeaponFire fireEvent, GameEventInfo info)
        {
            var shooterSteamId64 = fireEvent.Userid.SteamID.ToString();
            var shooterSteamId = ConvertSteamID64ToSteamID(shooterSteamId64);

            var updateTask = UpdateShootsAsync(shooterSteamId);
            HandleAsyncOperation(updateTask);

            return HookResult.Continue;
        }

        private HookResult OnPlayerHurt(EventPlayerHurt hurtEvent, GameEventInfo info)
        {
            if (hurtEvent.Attacker != null && IsValidPlayer(hurtEvent.Attacker))
            {
                var attackerSteamId64 = hurtEvent.Attacker.SteamID.ToString();
                var attackerSteamId = ConvertSteamID64ToSteamID(attackerSteamId64);

                var updateTask = UpdateHitsAsync(attackerSteamId);
                HandleAsyncOperation(updateTask);
            }

            return HookResult.Continue;
        }

        private void HandleAsyncOperation(Task task)
        {
            task.ContinueWith(t =>
            {
                if (t.Exception != null)
                {
                    Console.WriteLine($"Error in async operation: {t.Exception}");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }        
        private async Task UpdatePlayerDisconnectAsync(string steamId, long currentTime)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                var playerData = await connection.QueryFirstOrDefaultAsync($"SELECT lastconnect, playtime FROM {dbConfig.Name} WHERE steam = @SteamID", new { SteamID = steamId });

                if (playerData != null)
                {
                    var sessionTime = currentTime - playerData.lastconnect;
                    var newPlaytime = playerData.playtime + sessionTime;

                    var updateQuery = $"UPDATE {dbConfig.Name} SET playtime = @Playtime WHERE steam = @SteamID;";
                    await connection.ExecuteAsync(updateQuery, new { SteamID = steamId, Playtime = newPlaytime });
                }
            }
        }
        private HookResult OnPlayerDisconnect(EventPlayerDisconnect disconnectEvent, GameEventInfo info)
        {
            if (disconnectEvent?.Userid != null && !disconnectEvent.Userid.IsBot)
            {
                var steamId64 = disconnectEvent.Userid.SteamID.ToString();
                var steamId = ConvertSteamID64ToSteamID(steamId64);  
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var disconnectTask = UpdatePlayerDisconnectAsync(steamId, currentTime);
                HandleAsyncOperation(disconnectTask);

                activePlayers.Remove(steamId);

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

                    bool isWin = playerTeam == winnerTeam;

                    int pointsChange = isWin ? config.PointsPerRoundWin : config.PointsPerRoundLoss;
                    if (pointsChange != 0)
                    {                          
                        string messageColor = isWin ? ReplaceColorPlaceholders(config.RoundWinMessageColor) : ReplaceColorPlaceholders(config.RoundLossMessageColor);   
                        var pointsTask = AddOrRemovePointsAsync(steamId, pointsChange, playerController, isWin ? config.RoundWinMessage : config.RoundLossMessage, messageColor);
                        HandleAsyncOperation(pointsTask);
                    }

                    var roundResultTask = UpdateRoundResultAsync(steamId, isWin);
                    HandleAsyncOperation(roundResultTask);
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

                var pointsTask = AddOrRemovePointsAsync(mvpPlayerSteamId, config.PointsPerMVP, mvpEvent.Userid, config.MVPMessage, MVPMessageColor);
                HandleAsyncPointsOperation(pointsTask);
            }

            return HookResult.Continue;
        }
        private async Task UpdateRoundResultAsync(string steamId, bool isWin)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();

                string columnToUpdate = isWin ? "round_win" : "round_lose";
                var updateQuery = $"UPDATE {dbConfig.Name} SET {columnToUpdate} = {columnToUpdate} + 1 WHERE steam = @SteamID;";
                await connection.ExecuteAsync(updateQuery, new { SteamID = steamId });
            }
        }
        private HookResult OnPlayerDeath(EventPlayerDeath deathEvent, GameEventInfo info)
        {
            if (deathEvent.Userid.IsBot || (deathEvent.Attacker != null && deathEvent.Attacker.IsBot))
            {
                return HookResult.Continue;
            }

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
                        var pointsTask = AddOrRemovePointsAsync(victimSteamId, config.PointsForSuicide, deathEvent.Userid, config.SuicideMessage, suicideMessageColor);
                        HandleAsyncPointsOperation(pointsTask);
                    }
                }
                else
                {
                    if (config.PointsForDeath != 0)
                    {
                        string DeathMessageColor = ReplaceColorPlaceholders(config.DeathMessageColor);            
                        var deathPointsTask = AddOrRemovePointsAsync(victimSteamId, config.PointsForDeath, deathEvent.Userid, config.DeathMessage, DeathMessageColor);
                        HandleAsyncPointsOperation(deathPointsTask);
                    }
                    var updateKillsOrDeathsTask = UpdateKillsOrDeathsAsync(victimSteamId, false);
                    HandleAsyncOperation(updateKillsOrDeathsTask);

                    if (deathEvent.Attacker != null && IsValidPlayer(deathEvent.Attacker))
                    {
                        var killerSteamId64 = deathEvent.Attacker.SteamID.ToString();
                        var killerSteamId = ConvertSteamID64ToSteamID(killerSteamId64);

                        if (config.PointsForKill != 0)
                        {
                            string KillMessageColor = ReplaceColorPlaceholders(config.KillMessageColor);                                   
                            var killPointsTask = AddOrRemovePointsAsync(killerSteamId, config.PointsForKill, deathEvent.Attacker, config.KillMessage, KillMessageColor);
                            HandleAsyncPointsOperation(killPointsTask);
                        }
                        var updateKillsTask = UpdateKillsOrDeathsAsync(killerSteamId, true);
                        HandleAsyncOperation(updateKillsTask);

                        if (deathEvent.Weapon == "awp" && deathEvent.Noscope && config.PointsForNoScopeAWP != 0)
                        {
                            string NoScopeAWPMessageColor = ReplaceColorPlaceholders(config.NoScopeAWPMessageColor);   
                            var noScopeTask = AddOrRemovePointsAsync(killerSteamId, config.PointsForNoScopeAWP, deathEvent.Attacker, config.NoScopeAWPMessage, NoScopeAWPMessageColor);
                            HandleAsyncPointsOperation(noScopeTask);
                        }
                        
                        if (deathEvent.Headshot && config.PointsForHeadshot != 0)
                        {
                            string HeadshotMessageColor = ReplaceColorPlaceholders(config.HeadshotMessageColor);  
                            var headshotPointsTask = AddOrRemovePointsAsync(killerSteamId, config.PointsForHeadshot, deathEvent.Attacker, config.HeadshotMessage, HeadshotMessageColor);
                            HandleAsyncPointsOperation(headshotPointsTask);
                            var updateHeadshotsTask = UpdateHeadshotsAsync(killerSteamId);
                            HandleAsyncOperation(updateHeadshotsTask);
                        }  
                    }
                    if (deathEvent.Assister != null && IsValidPlayer(deathEvent.Assister) && config.PointsForAssist != 0)
                    {
                        var assisterSteamId64 = deathEvent.Assister.SteamID.ToString();
                        var assisterSteamId = ConvertSteamID64ToSteamID(assisterSteamId64);

                        string AssistMessageColor = ReplaceColorPlaceholders(config.AssistMessageColor);  
                        var assistPointsTask = AddOrRemovePointsAsync(assisterSteamId, config.PointsForAssist, deathEvent.Assister, config.AssistMessage, AssistMessageColor);
                        HandleAsyncPointsOperation(assistPointsTask);
                        var updateAssistsTask = UpdateAssistsAsync(assisterSteamId);
                        HandleAsyncOperation(updateAssistsTask);
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

        private async Task UpdateHeadshotsAsync(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var updateQuery = $"UPDATE {dbConfig.Name} SET headshots = headshots + 1 WHERE steam = @SteamID;";
                await connection.ExecuteAsync(updateQuery, new { SteamID = steamId });
            }
        }

        private async Task UpdateAssistsAsync(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var updateQuery = $"UPDATE {dbConfig.Name} SET assists = assists + 1 WHERE steam = @SteamID;";
                await connection.ExecuteAsync(updateQuery, new { SteamID = steamId });
            }
        }
        private async Task UpdateKillsOrDeathsAsync(string steamId, bool isKill)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                string columnToUpdate = isKill ? "kills" : "deaths";
                var updateQuery = $"UPDATE {dbConfig.Name} SET {columnToUpdate} = {columnToUpdate} + 1 WHERE steam = @SteamID;";
                await connection.ExecuteAsync(updateQuery, new { SteamID = steamId });
            }
        }
        private void ExecuteOnMainThread(Action action)
        {
            Server.NextFrame(action);
        }
        private readonly Dictionary<string, SemaphoreSlim> playerSemaphores = new Dictionary<string, SemaphoreSlim>();

        private async Task<int> AddOrRemovePointsAsync(string steamId, int points, CCSPlayerController playerController, string reason, string messageColor)
        {
            if (string.IsNullOrEmpty(steamId))
            {
                return 0; 
            }        

            if (!playerSemaphores.ContainsKey(steamId))
            {
                playerSemaphores[steamId] = new SemaphoreSlim(1, 1);
            }

            int updatedPoints = 0;

            await playerSemaphores[steamId].WaitAsync();

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var transaction = await connection.BeginTransactionAsync();

                    try
                    {
                        var currentPointsQuery = $"SELECT value FROM {dbConfig.Name} WHERE steam = @SteamID;";
                        var currentPoints = await connection.ExecuteScalarAsync<int>(currentPointsQuery, new { SteamID = steamId }, transaction);

                        updatedPoints = currentPoints + points;

                        if (updatedPoints < 0)
                        {
                            updatedPoints = 0;
                        }

                        var updateQuery = $"UPDATE {dbConfig.Name} SET value = @NewPoints WHERE steam = @SteamID;";
                        await connection.ExecuteAsync(updateQuery, new { NewPoints = updatedPoints, SteamID = steamId }, transaction);

                        await transaction.CommitAsync();
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        Console.WriteLine("Exception in AddOrRemovePointsAsync: " + ex.Message);
                    }
                }

                ExecuteOnMainThread(() => {
                    if (playerController != null && playerController.IsValid && !playerController.IsBot)
                    {
                        string sign = points >= 0 ? "+" : "-";
                        string rawMessage = config.PointsChangeMessage
                            .Replace("{COLOR}", messageColor)
                            .Replace("{POINTS}", updatedPoints.ToString())
                            .Replace("{SIGN}", sign)
                            .Replace("{CHANGE_POINTS}", Math.Abs(points).ToString())
                            .Replace("{REASON}", reason);

                        string formattedMessage = ReplaceColorPlaceholders(rawMessage);
                        playerController.PrintToChat(formattedMessage);
                    }
                });
            }
            finally
            {
                playerSemaphores[steamId].Release();
            }

            await CheckAndUpdateRankAsync(steamId, updatedPoints);
            return updatedPoints;
        }

        private void UpdatePlayerUI(string steamId, int updatedPoints, int points, string reason, string messageColor)
        {
            var steamId64 = ConvertSteamIDToSteamID64(steamId);
            var playerController = FindPlayerBySteamID(steamId64);
            if (playerController != null && playerController.IsValid && !playerController.IsBot)
            {
                SendPointsUpdateMessage(playerController, updatedPoints, points, reason, messageColor);
            }
        }
        private void SendPointsUpdateMessage(CCSPlayerController playerController, int updatedPoints, int points, string reason, string messageColor)
        {
            if (playerController != null && playerController.IsValid && !playerController.IsBot)
            {
                string sign = points >= 0 ? "+" : "-";
                string rawMessage = config.PointsChangeMessage
                    .Replace("{COLOR}", messageColor)
                    .Replace("{POINTS}", updatedPoints.ToString())
                    .Replace("{SIGN}", sign)
                    .Replace("{CHANGE_POINTS}", Math.Abs(points).ToString())
                    .Replace("{REASON}", reason);

                string formattedMessage = ReplaceColorPlaceholders(rawMessage);
                playerController.PrintToChat(formattedMessage);
            }
        }
        private async Task<bool> CheckAndUpdateRankAsync(string steamId, int updatedPoints)
        {
            var ranksConfig = LoadRanksConfig();
            var newRankIndex = -1;

            for (int i = 0; i < ranksConfig.Count; i++)
            {
                if (updatedPoints >= ranksConfig[i].MinExperience)
                {
                    newRankIndex = i;
                }
                else
                {
                    break; 
                }
            }

            if (newRankIndex != -1)
            {
                var newRank = ranksConfig[newRankIndex];
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var currentRankQuery = $"SELECT rank FROM {dbConfig.Name} WHERE steam = @SteamID;";
                    var currentRankId = await connection.ExecuteScalarAsync<int>(currentRankQuery, new { SteamID = steamId });

                    if (currentRankId != newRank.Id)
                    {
                        var updateRankQuery = $"UPDATE {dbConfig.Name} SET rank = @NewRankId WHERE steam = @SteamID;";
                        await connection.ExecuteAsync(updateRankQuery, new { NewRankId = newRank.Id, SteamID = steamId });

                        var player = FindPlayerBySteamID(ConvertSteamIDToSteamID64(steamId));
                        if (player != null)
                        {
                            SetPlayerClanTag(player);
                        }

                        bool isRankUp = newRank.Id > currentRankId;
                        ExecuteOnMainThread(() => NotifyPlayerOfRankChange(steamId, newRank.Name, isRankUp));
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
                Console.WriteLine($"[ConvertSteamIDToSteamID64] Ошибка при конвертации SteamID: {ex.Message}");
                return null;
            }
        }
        private async Task<RankConfig?> GetCurrentRankAsync(string steamID64)
        {
            var steamID = ConvertSteamID64ToSteamID(steamID64);
            if (steamID == null)
            {
                Console.WriteLine("Invalid SteamID64 format.");
                return null;
            }

            var ranksConfig = LoadRanksConfig();

            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var query = $"SELECT rank FROM {dbConfig.Name} WHERE steam = @SteamID;";
                var rankId = await connection.QueryFirstOrDefaultAsync<int>(query, new { SteamID = steamID });

                RankConfig? defaultRank = ranksConfig.FirstOrDefault(r => r.Id == 0);
                RankConfig? currentRank = ranksConfig.FirstOrDefault(r => r.Id == rankId);

                return currentRank ?? defaultRank;
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
                    DbName = "YourDatabase",
                    DbPort = "3306" 
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

        private async Task<PlayerStats> GetPlayerStatsAsync(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                await connection.OpenAsync();
                var playerData = await connection.QueryFirstOrDefaultAsync($@"
                    SELECT p.rank, p.value as points, p.kills, p.deaths, p.playtime,
                        (SELECT COUNT(*) FROM {dbConfig.Name} WHERE value > p.value) + 1 as place,
                        (SELECT COUNT(*) FROM {dbConfig.Name}) as totalPlayers
                    FROM {dbConfig.Name} p
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

            var stats = GetPlayerStatsAsync(steamID).GetAwaiter().GetResult(); 

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
        [ConsoleCommand("top", "Показывает топ-10 игроков по очкам")]
        public void OnTopCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("Эту команду может использовать только игрок.");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    var topPlayersQuery = $@"
                        SELECT steam, name, value
                        FROM {dbConfig.Name}
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
        [ConsoleCommand("topkills", "Показывает топ-10 игроков по убийствам")]
        public void OnTopKillsCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("Эту команду может использовать только игрок.");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    var topPlayersQuery = $@"
                        SELECT steam, name, kills
                        FROM {dbConfig.Name}
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

        [ConsoleCommand("topdeaths", "Показывает топ-10 игроков по смертям")]
        public void OnTopDeathsCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("Эту команду может использовать только игрок.");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    var topPlayersQuery = $@"
                        SELECT steam, name, deaths
                        FROM {dbConfig.Name}
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

        [ConsoleCommand("topkdr", "Показывает топ-10 игроков по KDR")]
        public void OnTopKDRCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("Эту команду может использовать только игрок.");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    var topPlayersQuery = $@"
                        SELECT steam, name, kills, deaths, IF(deaths = 0, kills, kills/deaths) AS kdr
                        FROM {dbConfig.Name}
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

        [ConsoleCommand("toptime", "Показывает топ-10 игроков по времени на сервере")]
        public void OnTopTimeCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("Эту команду может использовать только игрок.");
                return;
            }

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    var topPlayersQuery = $@"
                        SELECT steam, name, playtime
                        FROM {dbConfig.Name}
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

        [ConsoleCommand("resetstats", "Сбросить свою статистику (можно использовать раз в 3 часа)")]
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
                var resetQuery = $"UPDATE {dbConfig.Name} SET kills = 0, deaths = 0, value = 0, shoots = 0, hits = 0, headshots = 0, assists = 0, round_win = 0, round_lose = 0, playtime = 0 WHERE steam = @SteamID;";
                connection.Execute(resetQuery, new { SteamID = steamId });
            }
        }

        [ConsoleCommand("rp_reloadconfig", "Перезагружает конфигурационный файл Config.yml")]
        public void ReloadConfigCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                try
                {
                    config = LoadOrCreateConfig();

                    Console.WriteLine("[RankPointsPlugin] Конфигурация успешно перезагружена.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RankPointsPlugin] Ошибка при перезагрузке конфигурации: {ex.Message}");
                }
            }
            else
            {
                player.PrintToChat("Эта команда доступна только из консоли сервера.");
            }
        }

        [ConsoleCommand("rp_reloadranks", "Перезагружает конфигурационный файл settings_ranks.yaml")]
        public void ReloadRanksCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                try
                {
                    LoadRanksConfig();

                    Console.WriteLine("[RankPointsPlugin] Конфигурация рангов успешно перезагружена.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RankPointsPlugin] Ошибка при перезагрузке конфигурации рангов: {ex.Message}");
                }
            }
            else
            {
                player.PrintToChat("{Red}Эта команда доступна только из консоли сервера.");
            }
        }

        [ConsoleCommand("ranks", "Показывает список всех званий и опыта, необходимого для их получения")]
        public void OnRanksCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("Эту команду может использовать только игрок.");
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
     
        [ConsoleCommand("lvl", "Показывает список всех доступных команд и их функций")]
        public void OnLvlCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null)
            {
                Console.WriteLine("Эту команду может использовать только игрок.");
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


        [ConsoleCommand("rp_resetranks", "Очищает статистику игрока. Использование: rp_resetranks <steamid64> <data-type>")]
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
                        Console.WriteLine("Некорректный SteamID64.");
                        return;
                    }

                    switch (dataType)
                    {
                        case "exp":
                            ResetPlayerExperience(steamId);
                            Console.WriteLine($"[RankPointsPlugin] Опыт и ранг игрока {steamId} (SteamID64: {steamId64}) были сброшены.");
                            break;
                        case "stats":
                            ResetPlayerStats(steamId);
                            Console.WriteLine($"[RankPointsPlugin] Статистика игрока {steamId} (SteamID64: {steamId64}) была сброшена.");
                            break;
                        case "time":
                            ResetPlayerPlaytime(steamId);
                            Console.WriteLine($"[RankPointsPlugin] Время игры игрока {steamId} (SteamID64: {steamId64}) было сброшено.");
                            break;
                        default:
                            Console.WriteLine("Неверный тип данных. Используйте 'exp', 'stats' или 'time'.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RankPointsPlugin] Произошла ошибка при сбросе статистики: {ex.Message}");
                }
            }
            else
            {
                player.PrintToChat($"{ChatColors.Red}Эта команда доступна только из консоли сервера.");
            }
        }

        private void ResetPlayerExperience(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var resetQuery = $"UPDATE {dbConfig.Name} SET value = 0, rank = 1 WHERE steam = @SteamID;";
                connection.Execute(resetQuery, new { SteamID = steamId });
            }
        }

        private void ResetPlayerStats2(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var resetQuery = $"UPDATE {dbConfig.Name} SET kills = 0, deaths = 0, shoots = 0, hits = 0, headshots = 0, assists = 0, round_win = 0, round_lose = 0 WHERE steam = @SteamID;";
                connection.Execute(resetQuery, new { SteamID = steamId });
            }
        }

        private void ResetPlayerPlaytime(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var resetQuery = $"UPDATE {dbConfig.Name} SET playtime = 0 WHERE steam = @SteamID;";
                connection.Execute(resetQuery, new { SteamID = steamId });
            }
        }
        private void CreateTable()
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();

                var createTableQuery = string.Format(SQL_CreateTable, $"{dbConfig.Name}", "", "");
                connection.Execute(createTableQuery);
            }
        }

        private string ConnectionString
        {
            get
            {
                if (dbConfig?.DbHost == null || dbConfig?.DbUser == null || dbConfig?.DbPassword == null || dbConfig?.DbName == null || dbConfig?.DbPort == null)
                    throw new InvalidOperationException("Database configuration is not properly set.");
                
                return $"Server={dbConfig.DbHost};Port={dbConfig.DbPort};User ID={dbConfig.DbUser};Password={dbConfig.DbPassword};Database={dbConfig.DbName};";
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
        public string? DbPort { get; set; }
        public string? Name { get; set; } = "lvl_base";

        public static DatabaseConfig ReadFromJsonFile(string filePath)
        {
            string jsonConfig = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<DatabaseConfig>(jsonConfig) ?? new DatabaseConfig();
        }
    }
}