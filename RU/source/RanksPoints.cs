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
        private const string PluginName = "[RanksPoints]";
        private const string PluginVersion = "2.0.8";
        private const string DbConfigFileName = "dbconfig.json";
        private DatabaseConfig? dbConfig;
        private PluginConfig config;  
        private List<Action> _pendingActions = new List<Action>();
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
            public int PointsForBombExploded { get; set; } = 2; 
            public int PointsForBombPlanting { get; set; } = 2;
            public int PointsForBombDropping { get; set; } = -2;
            public int PointsForBombPickup { get; set; } = 1;     
            public int PointsForWallbang { get; set; } = 3;
            public int PointsForHostageFollows { get; set; } = 2; 
            public int PointsForHostageStopsFollowing { get; set; } = -2; 
            public int PointsForHostageRescued { get; set; } = 4;       
            public int PointsForKillThroughSmoke { get; set; } = 3;  
            public string KillThroughSmokeMessage { get; set; } = "убийство через дым"; 
            public string KillThroughSmokeMessageColor { get; set; } = "{Green}";    
            public int PointsForBlindKill { get; set; } = 5; 
            public string BlindKillMessage { get; set; } = "убийство в состоянии ослепления"; 
            public string BlindKillMessageColor { get; set; } = "{Yellow}";       
            public bool GivePointsForBotKills { get; set; } = false;        
            public bool EnableClanTags { get; set; } = true;
            public bool IsRankCommandEnabled { get; set; } = true;
            public bool IsTopCommandEnabled { get; set; } = true;
            public bool IsTopkillsCommandEnabled { get; set; } = true;
            public bool IsTopdeathsCommandEnabled { get; set; } = true;
            public bool IsTopkdrCommandEnabled { get; set; } = true;
            public bool IsToptimeCommandEnabled { get; set; } = true;
            public bool IsResetstatsCommandEnabled { get; set; } = true;
            public bool IsRanksCommandEnabled { get; set; } = true;
            public bool IsLvlCommandEnabled { get; set; } = true;       
            public bool IsTagRankCommandEnabled { get; set; } = true;  
            public bool EnableSpecialNicknameBonus { get; set; } = true;
            public double BonusMultiplierForSpecialNickname { get; set; } = 1.5; 
            public string SpecialNicknameContains { get; set; } = "example.com";                    
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
            public string BombExplodedMessage { get; set; } = "взрыв бомбы";             
            public string BombExplodedMessageColor { get; set; } = "{Green}";   
            public string BombPlantingMessage { get; set; } = "установку бомбы";
            public string BombPlantingMessageColor { get; set; } = "{Green}";  
            public string BombDroppingMessage { get; set; } = "выброс бомбы";
            public string BombDroppingMessageColor { get; set; } = "{Red}"; 
            public string BombPickupMessage { get; set; } = "поднятие бомбы";
            public string BombPickupMessageColor { get; set; } = "{Green}";   
            public string WallbangMessage { get; set; } = "прострел"; 
            public string WallbangMessageColor { get; set; } = "{Purple}";  
            public string HostageFollowsMessage { get; set; } = "заложник следует";
            public string HostageFollowsMessageColor { get; set; } = "{Green}";
            public string HostageStopsFollowingMessage { get; set; } = "заложник перестал следовать";
            public string HostageStopsFollowingMessageColor { get; set; } = "{Red}";
            public string HostageRescuedMessage { get; set; } = "заложник спасен";
            public string HostageRescuedMessageColor { get; set; } = "{Blue}";                                                         
            public string RankCommandMessage { get; set; } = "[ {Yellow}RanksPoints {White}] Звание: {Green}{RANK_NAME} {White}| Место: {Blue}{PLACE}/{TOTAL_PLAYERS} {White}| Опыт: {Gold}{POINTS} {White}| Убийства: {Green}{KILLS} {White}| Смерти: {Red}{DEATHS} {White}| KDR: {Yellow}{KDR} {White}| Время на сервере: {Gold}{PLAY_TIME}";                                                            
            public string TimeFormat { get; set; } = "{0}д {1}ч {2}мин";   
            public string TopCommandIntroMessage { get; set; } = "[ {Blue}Топ игроков{White} ]"; 
            public string TopCommandPlayerMessage { get; set; } = "{INDEX}. {Grey}{NAME} - {White}{RANK} {Grey}- {Blue}{POINTS} очков";
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
            public string TagRankCommandDescription { get; set; } = "- {Green}!tagrank {White}- Включает или выключает отображение вашего клан-тега";    
            public string RankUpMessage { get; set; } = "Ваше звание было повышено до {RANK_NAME}!";
            public string RankDownMessage { get; set; } = "Ваше звание было понижено до {RANK_NAME}.";
            public string TagRankEnabledMessage { get; set; } = "[ {Yellow}RanksPoints {White}] Клан-тег будет вновь отображаться, начиная с следующего раунда.";
            public string TagRankDisabledMessage { get; set; } = "[ {Yellow}RanksPoints {White}] Клан-тег больше не будет отображаться, начиная с следующего раунда.";

        } 
        public class RankConfig
        {
            public int Id { get; set; }
            public string? Name { get; set; }
            public int MinExperience { get; set; } 
            public string? ClanTag { get; set; }
        }
        public void SaveConfig(PluginConfig config, string filePath)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("# Конфигурационный файл для RankPoints");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# Количество выдаваемых очков");
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
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForBombExploded), config.PointsForBombExploded, "Очки за взрыв бомбы");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForBombPlanting), config.PointsForBombPlanting, "Очки за установку бомбы - количество очков, добавляемое игроку за успешную установку бомбы.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForBombDropping), config.PointsForBombDropping, "Очки за выброс бомбы - количество очков, вычитаемое у игрока за выброс бомбы.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForBombPickup), config.PointsForBombPickup, "Очки за поднятие бомбы - количество очков, добавляемое игроку за поднятие бомбы.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForWallbang), config.PointsForWallbang, "Очки за убийство через прострел.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForHostageFollows), config.PointsForHostageFollows, "Очки за поднятие заложника");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForHostageStopsFollowing), config.PointsForHostageStopsFollowing, "Очки за потерю заложника");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForHostageRescued), config.PointsForHostageRescued, "Очки за спасение заложника");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForKillThroughSmoke), config.PointsForKillThroughSmoke, "Очки за убийство через дым - количество очков, добавляемое игроку за убийство врага через дымовую завесу.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.PointsForBlindKill), config.PointsForBlindKill, "Очки за убийство в состоянии ослепления - количество очков, добавляемое игроку за убийство, когда он ослеплен.");


            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# Параметры RanksPoints");
            stringBuilder.AppendLine("# Отображение клан-тегов званий для игроков. true - включено, false - отключено.");
            stringBuilder.AppendLine($"EnableClanTags: {config.EnableClanTags}");
            stringBuilder.AppendLine("# Минимальное количество игроков для начисления опыта - игрокам начисляется опыт только если на сервере играет минимум это количество игроков.");
            stringBuilder.AppendLine($"GetActivePlayerCountMsg: \"{EscapeMessage(config.GetActivePlayerCountMsg)}\"");
            AppendConfigValue(stringBuilder, nameof(config.MinPlayersForExperience), config.MinPlayersForExperience);
            stringBuilder.AppendLine("# Включение или выключение дополнительного опыта для специальных никнеймов");
            stringBuilder.AppendLine($"EnableSpecialNicknameBonus: {config.EnableSpecialNicknameBonus.ToString().ToLower()}");
            stringBuilder.AppendLine("# Множитель опыта для специальных никнеймов");
            stringBuilder.AppendLine($"BonusMultiplierForSpecialNickname: {config.BonusMultiplierForSpecialNickname}");
            stringBuilder.AppendLine("# Строка, которую нужно искать в никнейме для применения множителя");
            stringBuilder.AppendLine($"SpecialNicknameContains: \"{EscapeMessage(config.SpecialNicknameContains)}\"");
            stringBuilder.AppendLine("# Включение или выключение начисления очков за убийство ботов. true - включено, false - выключено.");
            AppendConfigValue(stringBuilder, nameof(config.GivePointsForBotKills), config.GivePointsForBotKills);


            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# Все сообщения RanksPoints");
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
            stringBuilder.AppendLine($"BombExplodedMessage: \"{EscapeMessage(config.BombExplodedMessage)}\"");
            stringBuilder.AppendLine($"BombExplodedMessageColor: \"{EscapeMessage(config.BombExplodedMessageColor)}\"");       
            stringBuilder.AppendLine($"BombPlantingMessage: \"{EscapeMessage(config.BombPlantingMessage)}\"");
            stringBuilder.AppendLine($"BombPlantingMessageColor: \"{EscapeMessage(config.BombPlantingMessageColor)}\"");       
            stringBuilder.AppendLine($"BombDroppingMessage: \"{EscapeMessage(config.BombDroppingMessage)}\"");
            stringBuilder.AppendLine($"BombDroppingMessageColor: \"{EscapeMessage(config.BombDroppingMessageColor)}\"");    
            stringBuilder.AppendLine($"BombPickupMessage: \"{EscapeMessage(config.BombPickupMessage)}\"");
            stringBuilder.AppendLine($"BombPickupMessageColor: \"{EscapeMessage(config.BombPickupMessageColor)}\"");
            stringBuilder.AppendLine($"WallbangMessage: \"{EscapeMessage(config.WallbangMessage)}\"");
            stringBuilder.AppendLine($"WallbangMessageColor: \"{EscapeMessage(config.WallbangMessageColor)}\"");   
            stringBuilder.AppendLine($"HostageFollowsMessage: \"{EscapeMessage(config.HostageFollowsMessage)}\"");
            stringBuilder.AppendLine($"HostageFollowsMessageColor: \"{EscapeMessage(config.HostageFollowsMessageColor)}\"");
            stringBuilder.AppendLine($"HostageStopsFollowingMessage: \"{EscapeMessage(config.HostageStopsFollowingMessage)}\"");
            stringBuilder.AppendLine($"HostageStopsFollowingMessageColor: \"{EscapeMessage(config.HostageStopsFollowingMessageColor)}\"");
            stringBuilder.AppendLine($"HostageRescuedMessage: \"{EscapeMessage(config.HostageRescuedMessage)}\"");
            stringBuilder.AppendLine($"HostageRescuedMessageColor: \"{EscapeMessage(config.HostageRescuedMessageColor)}\"");
            stringBuilder.AppendLine($"KillThroughSmokeMessage: \"{EscapeMessage(config.KillThroughSmokeMessage)}\"");
            stringBuilder.AppendLine($"KillThroughSmokeMessageColor: \"{EscapeMessage(config.KillThroughSmokeMessageColor)}\"");
            stringBuilder.AppendLine($"BlindKillMessage: \"{EscapeMessage(config.BlindKillMessage)}\"");
            stringBuilder.AppendLine($"BlindKillMessageColor: \"{EscapeMessage(config.BlindKillMessageColor)}\"");

           
            stringBuilder.AppendLine();
            AppendConfigValueWithComment(stringBuilder, nameof(config.RankUpMessage), config.RankUpMessage, "Сообщение о повышении звания.");
            AppendConfigValueWithComment(stringBuilder, nameof(config.RankDownMessage), config.RankDownMessage, "Сообщение о понижении звания.");
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !rank");
            stringBuilder.AppendLine($"RankCommandMessage : \"{EscapeMessage(config.RankCommandMessage)}\"");   
            stringBuilder.AppendLine($"TimeFormat: \"{EscapeMessage(config.TimeFormat)}\"");  
            stringBuilder.AppendLine("# Включение или выключение команды !rank");
            stringBuilder.AppendLine($"IsRankCommandEnabled: {config.IsRankCommandEnabled.ToString().ToLower()}");
             
            
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !top");
            stringBuilder.AppendLine($"TopCommandIntroMessage : \"{EscapeMessage(config.TopCommandIntroMessage)}\"");   
            stringBuilder.AppendLine($"TopCommandPlayerMessage: \"{EscapeMessage(config.TopCommandPlayerMessage)}\"");       
            stringBuilder.AppendLine($"TopCommandNoDataMessage: \"{EscapeMessage(config.TopCommandNoDataMessage)}\"");     
            stringBuilder.AppendLine($"TopCommandErrorMessage: \"{EscapeMessage(config.TopCommandErrorMessage)}\"");     
            stringBuilder.AppendLine("# Включение или выключение команды !top");
            stringBuilder.AppendLine($"IsTopCommandEnabled: {config.IsTopCommandEnabled.ToString().ToLower()}");

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !topkills");       
            stringBuilder.AppendLine($"TopKillsCommandIntroMessage: \"{EscapeMessage(config.TopKillsCommandIntroMessage)}\"");
            stringBuilder.AppendLine($"TopKillsCommandPlayerMessage: \"{EscapeMessage(config.TopKillsCommandPlayerMessage)}\"");
            stringBuilder.AppendLine($"TopKillsCommandNoDataMessage: \"{EscapeMessage(config.TopKillsCommandNoDataMessage)}\"");
            stringBuilder.AppendLine($"TopKillsCommandErrorMessage: \"{EscapeMessage(config.TopKillsCommandErrorMessage)}\"");
            stringBuilder.AppendLine("# Включение или выключение команды !topkills");
            stringBuilder.AppendLine($"IsTopkillsCommandEnabled: {config.IsTopkillsCommandEnabled.ToString().ToLower()}");            

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !topdeaths");              
            stringBuilder.AppendLine($"TopDeathsCommandIntroMessage: \"{EscapeMessage(config.TopDeathsCommandIntroMessage)}\"");
            stringBuilder.AppendLine($"TopDeathsCommandPlayerMessage: \"{EscapeMessage(config.TopDeathsCommandPlayerMessage)}\"");
            stringBuilder.AppendLine($"TopDeathsCommandNoDataMessage: \"{EscapeMessage(config.TopDeathsCommandNoDataMessage)}\"");
            stringBuilder.AppendLine($"TopDeathsCommandErrorMessage: \"{EscapeMessage(config.TopDeathsCommandErrorMessage)}\"");
            stringBuilder.AppendLine("# Включение или выключение команды !topdeaths");
            stringBuilder.AppendLine($"IsTopdeathsCommandEnabled: {config.IsTopdeathsCommandEnabled.ToString().ToLower()}");            

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !topkdr");              
            stringBuilder.AppendLine($"TopKDRCommandIntroMessage: \"{EscapeMessage(config.TopKDRCommandIntroMessage)}\"");
            stringBuilder.AppendLine($"TopKDRCommandPlayerMessage: \"{EscapeMessage(config.TopKDRCommandPlayerMessage)}\"");
            stringBuilder.AppendLine($"TopKDRCommandNoDataMessage: \"{EscapeMessage(config.TopKDRCommandNoDataMessage)}\"");
            stringBuilder.AppendLine($"TopKDRCommandErrorMessage: \"{EscapeMessage(config.TopKDRCommandErrorMessage)}\""); 
            stringBuilder.AppendLine("# Включение или выключение команды !topkdr");
            stringBuilder.AppendLine($"IsTopkdrCommandEnabled: {config.IsTopkdrCommandEnabled.ToString().ToLower()}");             

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !toptime");              
            stringBuilder.AppendLine($"TopTimeCommandIntroMessage: \"{EscapeMessage(config.TopTimeCommandIntroMessage)}\"");
            stringBuilder.AppendLine($"TopTimeCommandPlayerMessage: \"{EscapeMessage(config.TopTimeCommandPlayerMessage)}\"");
            stringBuilder.AppendLine($"TopTimeCommandNoDataMessage : \"{EscapeMessage(config.TopTimeCommandNoDataMessage)}\"");
            stringBuilder.AppendLine($"TopTimeCommandErrorMessage: \"{EscapeMessage(config.TopTimeCommandErrorMessage)}\"");   
            stringBuilder.AppendLine($"TopTimeFormat: \"{EscapeMessage(config.TopTimeFormat)}\"");   
            stringBuilder.AppendLine("# Включение или выключение команды !toptime");
            stringBuilder.AppendLine($"IsToptimeCommandEnabled: {config.IsToptimeCommandEnabled.ToString().ToLower()}");            
            
            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !resetstats");              
            stringBuilder.AppendLine($"ResetStatsCooldownMessage: \"{EscapeMessage(config.ResetStatsCooldownMessage)}\"");
            stringBuilder.AppendLine($"ResetStatsSuccessMessage: \"{EscapeMessage(config.ResetStatsSuccessMessage)}\""); 
            stringBuilder.AppendLine($"ResetStatsCooldownHours: \"{EscapeMessage(config.ResetStatsCooldownHours.ToString())}\"");   
            stringBuilder.AppendLine("# Включение или выключение команды !resetstats");
            stringBuilder.AppendLine($"IsResetstatsCommandEnabled: {config.IsResetstatsCommandEnabled.ToString().ToLower()}");              

            stringBuilder.AppendLine();
            stringBuilder.AppendLine("# !ranks");              
            stringBuilder.AppendLine($"RanksCommandIntroMessage: \"{EscapeMessage(config.RanksCommandIntroMessage)}\"");
            stringBuilder.AppendLine($"RanksCommandRankMessage: \"{EscapeMessage(config.RanksCommandRankMessage)}\""); 
            stringBuilder.AppendLine($"RanksCommandNoDataMessage: \"{EscapeMessage(config.RanksCommandNoDataMessage)}\"");   
            stringBuilder.AppendLine($"RanksCommandErrorMessage: \"{EscapeMessage(config.RanksCommandErrorMessage)}\"");     
            stringBuilder.AppendLine("# Включение или выключение команды !ranks");
            stringBuilder.AppendLine($"IsRanksCommandEnabled: {config.IsRanksCommandEnabled.ToString().ToLower()}");                

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
            stringBuilder.AppendLine($"TagRankCommandDescription: \"{EscapeMessage(config.TagRankCommandDescription)}\"");               
            stringBuilder.AppendLine("# Включение или выключение команды !lvl");
            stringBuilder.AppendLine($"IsLvlCommandEnabled: {config.IsLvlCommandEnabled.ToString().ToLower()}");                                                                                  
            
            stringBuilder.AppendLine();   
            stringBuilder.AppendLine("# !tagrank");
            stringBuilder.AppendLine($"TagRankEnabledMessage: \"{EscapeMessage(config.TagRankEnabledMessage)}\"");
            stringBuilder.AppendLine($"TagRankDisabledMessage: \"{EscapeMessage(config.TagRankDisabledMessage)}\"");
            stringBuilder.AppendLine("# Включение или выключение команды !tagrank");
            stringBuilder.AppendLine($"IsTagRankCommandEnabled: {config.IsTagRankCommandEnabled.ToString().ToLower()}");

            File.WriteAllText(filePath, stringBuilder.ToString());
        }

        private string EscapeMessage(string message)
        {
            return message.Replace("\"", "\\\"").Replace("\n", "\\n");
        }
        public List<WeaponPoints> WeaponPointsConfig { get; set; }
        private List<WeaponPoints> weaponPointsConfig;

        private void AppendConfigValueWithComment(StringBuilder sb, string key, object value, string comment)
        {
            sb.AppendLine($"# {comment}");
            sb.AppendLine($"{key}: {value}");
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
        private void SetPlayerClanTag(CCSPlayerController player)
        {
            if (!config.EnableClanTags) return;

            var steamID64 = player.SteamID.ToString();
            var tagSettings = LoadTagSettings();

            if (tagSettings.DisabledTags.Contains(steamID64))
            {
                player.Clan = ""; 
                return;
            }

            try
            {
                var rank = GetCurrentRank(steamID64); 

                if (rank != null && !string.IsNullOrEmpty(rank.ClanTag))
                {
                    player.Clan = rank.ClanTag;
                }
                else
                {
                    Console.WriteLine("[SetPlayerClanTag] Не удалось установить клан-тег: игрок или ранг отсутствует.");
                }
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
        private bool isWarmup = true;
        public override void Load(bool hotReload)
        {
            base.Load(hotReload);
            CreateDbConfigIfNotExists();
            dbConfig = DatabaseConfig.ReadFromJsonFile(Path.Combine(ModuleDirectory, DbConfigFileName));
            RegisterListener<Listeners.OnClientConnected>(OnClientConnected);
            RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
            RegisterListener<Listeners.OnMapEnd>(OnMapEnd);
            RegisterEventHandler<EventRoundEnd>(OnRoundEnd);
            RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
            RegisterEventHandler<EventWeaponFire>(OnWeaponFire);
            RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
            RegisterEventHandler<EventRoundMvp>(OnPlayerMVP);
            RegisterEventHandler<EventRoundStart>(OnRoundStart);
            RegisterEventHandler<EventBombExploded>(OnBombExploded);
            RegisterEventHandler<EventBombDefused>(OnBombDefused);
            RegisterEventHandler<EventRoundAnnounceMatchStart>(OnMatchStart);
            RegisterEventHandler<EventRoundAnnounceWarmup>(OnWarmupStart);      
            RegisterEventHandler<EventBombPlanted>(OnBombPlanted);      
            RegisterEventHandler<EventBombDropped>(OnBombDropped);
            RegisterEventHandler<EventBombPickup>(OnBombPickup);
            RegisterEventHandler<EventHostageFollows>(OnHostageFollows);
            RegisterEventHandler<EventHostageStopsFollowing>(OnHostageStopsFollowing);
            RegisterEventHandler<EventHostageRescued>(OnHostageRescued);
            isActiveRoundForPoints = true; 
            CreateTable();
            config = LoadOrCreateConfig();
            LoadRanksConfig();
            LoadTagSettings();
            weaponPointsConfig = LoadWeaponPointsConfig();

            CreateDbConfigIfNotExists();
            dbConfig = DatabaseConfig.ReadFromJsonFile(Path.Combine(ModuleDirectory, DbConfigFileName));            
        }      
        private void ClearAllPendingActions()
        {
            lock (_pendingActions)
            {
                _pendingActions.Clear();
            }
        }
        private void OnMapEnd()
        {
            ClearAllPendingActions();
        }

        private HookResult OnHostageFollows(EventHostageFollows hostageFollowsEvent, GameEventInfo info)
        {
            if (GetActivePlayerCount() < config.MinPlayersForExperience)
            {
                return HookResult.Continue;
            }   

            if (isWarmup)
            {
                return HookResult.Continue;
            }

            if (!isActiveRoundForPoints) return HookResult.Continue;

            var playerSteamId64 = hostageFollowsEvent.Userid.SteamID.ToString();
            var playerSteamId = ConvertSteamID64ToSteamID(playerSteamId64);

            if (config.PointsForHostageFollows != 0)
            {
                var pointsTask = AddOrRemovePoints(playerSteamId, config.PointsForHostageFollows, hostageFollowsEvent.Userid, config.HostageFollowsMessage, config.HostageFollowsMessageColor);
            }

            return HookResult.Continue;
        }
        private HookResult OnHostageStopsFollowing(EventHostageStopsFollowing hostageStopsFollowingEvent, GameEventInfo info)
        {
            if (GetActivePlayerCount() < config.MinPlayersForExperience)
            {
                return HookResult.Continue;
            }   

            if (isWarmup)
            {
                return HookResult.Continue;
            }

            if (!isActiveRoundForPoints) return HookResult.Continue;

            var playerSteamId64 = hostageStopsFollowingEvent.Userid.SteamID.ToString();
            var playerSteamId = ConvertSteamID64ToSteamID(playerSteamId64);

            if (config.PointsForHostageStopsFollowing != 0)
            {
                var pointsTask = AddOrRemovePoints(playerSteamId, config.PointsForHostageStopsFollowing, hostageStopsFollowingEvent.Userid, config.HostageStopsFollowingMessage, config.HostageStopsFollowingMessageColor);
            }

            return HookResult.Continue;
        }
        private HookResult OnHostageRescued(EventHostageRescued hostageRescuedEvent, GameEventInfo info)
        {
            if (GetActivePlayerCount() < config.MinPlayersForExperience)
            {
                return HookResult.Continue;
            }   

            if (isWarmup)
            {
                return HookResult.Continue;
            }

            if (!isActiveRoundForPoints) return HookResult.Continue;

            var playerSteamId64 = hostageRescuedEvent.Userid.SteamID.ToString();
            var playerSteamId = ConvertSteamID64ToSteamID(playerSteamId64);

            if (config.PointsForHostageRescued != 0)
            {
                var pointsTask = AddOrRemovePoints(playerSteamId, config.PointsForHostageRescued, hostageRescuedEvent.Userid, config.HostageRescuedMessage, config.HostageRescuedMessageColor);
            }

            return HookResult.Continue;
        }

        private const string TagSettingsFileName = "tags.json";
        public class TagSettings
        {
            public HashSet<string> DisabledTags { get; set; } = new HashSet<string>();
        }
        private TagSettings LoadTagSettings()
        {
            var filePath = Path.Combine(ModuleDirectory, TagSettingsFileName);
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<TagSettings>(json) ?? new TagSettings();
            }
            return new TagSettings();
        }

        private void SaveTagSettings(TagSettings settings)
        {
            var filePath = Path.Combine(ModuleDirectory, TagSettingsFileName);
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(filePath, json);
        }

        private HookResult OnBombPickup(EventBombPickup bombPickupEvent, GameEventInfo info)
        { 
            if (GetActivePlayerCount() < config.MinPlayersForExperience)
            {
                return HookResult.Continue;
            }   

            if (isWarmup)
            {
                return HookResult.Continue;
            }

            var pickerSteamId64 = bombPickupEvent.Userid.SteamID.ToString();
            var pickerSteamId = ConvertSteamID64ToSteamID(pickerSteamId64);
            Console.WriteLine($"Bomb Pickup by {pickerSteamId}");

            if (config.PointsForBombPickup != 0)
            {
                string BombPickupMessageColor = ReplaceColorPlaceholders(config.BombPickupMessageColor);       
                var pointsTask = AddOrRemovePoints(pickerSteamId, config.PointsForBombPickup, bombPickupEvent.Userid, config.BombPickupMessage, BombPickupMessageColor);
            }    

            return HookResult.Continue;
        } 
 
        private HookResult OnBombDropped(EventBombDropped bombDroppedEvent, GameEventInfo info)
        {
            if (GetActivePlayerCount() < config.MinPlayersForExperience)
            {
                return HookResult.Continue;
            }   

            var dropperSteamId64 = bombDroppedEvent.Userid.SteamID.ToString();
            var dropperSteamId = ConvertSteamID64ToSteamID(dropperSteamId64);

            if (config.PointsForBombDropping != 0)
            {
                string BombDroppingMessageColor = ReplaceColorPlaceholders(config.BombDroppingMessageColor);       
                var pointsTask = AddOrRemovePoints(dropperSteamId, config.PointsForBombDropping, bombDroppedEvent.Userid, config.BombDroppingMessage, BombDroppingMessageColor);
            }    

            return HookResult.Continue;
        }
        private HookResult OnBombPlanted(EventBombPlanted bombPlantedEvent, GameEventInfo info)
        {
            if (GetActivePlayerCount() < config.MinPlayersForExperience)
            {
                return HookResult.Continue;
            }   

            var planterSteamId64 = bombPlantedEvent.Userid.SteamID.ToString();
            var planterSteamId = ConvertSteamID64ToSteamID(planterSteamId64);

            if (config.PointsForBombPlanting != 0)
            {
                string BombPlantingMessageColor = ReplaceColorPlaceholders(config.BombPlantingMessageColor);       
                var pointsTask = AddOrRemovePoints(planterSteamId, config.PointsForBombPlanting, bombPlantedEvent.Userid, config.BombPlantingMessage, BombPlantingMessageColor);
            }    

            return HookResult.Continue;
        }
        Dictionary<uint, PlayerIteam> g_Player = new Dictionary<uint, PlayerIteam>();
        private void OnClientConnected(int playerSlot)
        {
            var player = Utilities.GetPlayerFromSlot(playerSlot);
            var client = player.Index;
            g_Player[client] = new PlayerIteam
            {
                ClanTag = null,
                value = 0,
                valuechange = 0,
                rank = 0
            };
            if (player != null && !player.IsBot)
            {
                var steamId64 = player.SteamID.ToString();
                var steamId = ConvertSteamID64ToSteamID(steamId64); 
                var playerName = GetPlayerNickname(steamId64);
                var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var getdateTask = GetPlayerConnectionAsync(steamId, playerName, currentTime, client, player);

                activePlayers.Add(steamId);  

            }
        }
        private async Task GetPlayerConnectionAsync(string steamId, string playerName, long currentTime, uint client, CCSPlayerController player)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var insertQuery = $"INSERT INTO `{dbConfig.Name}` (steam, name, lastconnect) VALUES (@SteamID, @Name, @LastConnect) ON DUPLICATE KEY UPDATE lastconnect = @LastConnect, name = @Name;";
                    await connection.ExecuteAsync(insertQuery, new { SteamID = steamId, Name = playerName, LastConnect = currentTime });
                    string query = $"SELECT * FROM `{dbConfig.Name}` WHERE steam = '{steamId}'";
                    
                    var playerdata = await connection.QueryFirstOrDefaultAsync(query);
                    
                    Server.NextFrame(() => 
                    {
                        if (playerdata != null)
                        {
                            g_Player[client].value = playerdata.value;
                            g_Player[client].rank = playerdata.rank;

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
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetPlayerConnectionAsync: {ex.Message}");
                Server.NextFrame(() => Console.WriteLine($"Error processing player data in main thread: {ex.Message}"));
            }
        }
        public class PlayerIteam
        {

            public string? ClanTag { get; set; }
            public int? value { get; set; }
            public int? valuechange { get; set; }
            public int? rank { get; set; }

            public PlayerIteam()
            {
                Init();
            }

            public void Init()
            {
                ClanTag = null;
                value = 0;
                valuechange = 0;    
                rank = 0;
            }
        }        
        private HookResult OnRoundStart(EventRoundStart roundStartEvent, GameEventInfo info)
        {
            if (isWarmup)
            {
                isActiveRoundForPoints = false;
            }
            else
            {
                isActiveRoundForPoints = GetActivePlayerCount() >= config.MinPlayersForExperience;
            }

            if (!isActiveRoundForPoints)
            {
                string message = config.GetActivePlayerCountMsg
                    .Replace("{MIN_PLAYERS}", config.MinPlayersForExperience.ToString());
                message = ReplaceColorPlaceholders(message); 
                BroadcastToPlayers(message); 
            }

            return HookResult.Continue;
        }
        
        private HookResult OnMatchStart(EventRoundAnnounceMatchStart matchStartEvent, GameEventInfo info)
        {
            isWarmup = false;
            return HookResult.Continue;
        }

        private HookResult OnWarmupStart(EventRoundAnnounceWarmup warmupStartEvent, GameEventInfo info)
        {
            isWarmup = true;
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

            if (config.PointsForBombExploded != 0)
            {
                string BombExplodedMessageColor = ReplaceColorPlaceholders(config.BombExplodedMessageColor);       

                var pointsTask = AddOrRemovePoints(planterSteamId, config.PointsForBombExploded, eventBombPlanted.Userid, config.BombExplodedMessage, BombExplodedMessageColor);         
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

                var pointsTask = AddOrRemovePoints(defuserSteamId, config.PointsForBombDefusal, eventBombDefused.Userid, config.BombDefusalMessage, BombDefusalMessageColor);
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
            try
            {            
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var updateQuery = $"UPDATE `{dbConfig.Name}` SET shoots = shoots + 1 WHERE steam = @SteamID;";
                    await connection.ExecuteAsync(updateQuery, new { SteamID = steamId });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateShootsAsync: {ex.Message}");
            }                
        }

        private async Task UpdateHitsAsync(string steamId)
        {
            try
            {            
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var updateQuery = $"UPDATE `{dbConfig.Name}` SET hits = hits + 1 WHERE steam = @SteamID;";
                    await connection.ExecuteAsync(updateQuery, new { SteamID = steamId });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateHitsAsync: {ex.Message}");
            }                
        }

        private HookResult OnWeaponFire(EventWeaponFire fireEvent, GameEventInfo info)
        {
            var shooterSteamId64 = fireEvent.Userid.SteamID.ToString();
            var shooterSteamId = ConvertSteamID64ToSteamID(shooterSteamId64);

            var updateTask = UpdateShootsAsync(shooterSteamId);

            return HookResult.Continue;
        }

        private HookResult OnPlayerHurt(EventPlayerHurt hurtEvent, GameEventInfo info)
        {
            if (hurtEvent.Attacker != null && IsValidPlayer(hurtEvent.Attacker))
            {
                var attackerSteamId64 = hurtEvent.Attacker.SteamID.ToString();
                var attackerSteamId = ConvertSteamID64ToSteamID(attackerSteamId64);

                var updateTask = UpdateHitsAsync(attackerSteamId);
            }

            return HookResult.Continue;
        }      
        private async Task UpdatePlayerDisconnectAsync(string steamId, long currentTime)
        {
            try
            {            
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    var playerData = await connection.QueryFirstOrDefaultAsync($"SELECT lastconnect, playtime FROM `{dbConfig.Name}` WHERE steam = @SteamID", new { SteamID = steamId });
                    
                    if (playerData != null)
                    {
                        var sessionTime = currentTime - playerData.lastconnect;
                        var newPlaytime = playerData.playtime + sessionTime;

                        var updateQuery = $"UPDATE `{dbConfig.Name}` SET playtime = @Playtime WHERE steam = @SteamID;";
                        await connection.ExecuteAsync(updateQuery, new { SteamID = steamId, Playtime = newPlaytime });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdatePlayerDisconnectAsync: {ex.Message}");
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

            if (isWarmup) 
            {
                return HookResult.Continue;
            }

            CsTeam winnerTeam = (CsTeam)roundEndEvent.Winner;

            for (int playerIndex = 0; playerIndex <= Server.MaxPlayers; playerIndex++) 
            {
                CCSPlayerController playerController = Utilities.GetPlayerFromUserid(playerIndex);

                if (playerController != null && playerController.IsValid && !playerController.IsBot && HasJoinedTeam(playerController))
                {
                    CsTeam playerTeam = (CsTeam)playerController.TeamNum;
                    var steamID = playerController.SteamID.ToString();
                    var steamId = ConvertSteamID64ToSteamID(steamID);

                    bool isWin = playerTeam == winnerTeam;

                    int pointsChange = isWin ? config.PointsPerRoundWin : config.PointsPerRoundLoss;
                    if (pointsChange != 0)
                    {                          
                        string messageColor = isWin ? ReplaceColorPlaceholders(config.RoundWinMessageColor) : ReplaceColorPlaceholders(config.RoundLossMessageColor);   
                        var pointsTask = AddOrRemovePoints(steamId, pointsChange, playerController, isWin ? config.RoundWinMessage : config.RoundLossMessage, messageColor);
                    }

                    var roundResultTask = UpdateRoundResultAsync(steamId, isWin);
                }
            }

            return HookResult.Continue;
        }

        private bool HasJoinedTeam(CCSPlayerController playerController)
        {
            if (playerController == null || !playerController.IsValid)
            {
                return false;
            }

            return playerController.TeamNum == 2 || playerController.TeamNum == 3;
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

                var pointsTask = AddOrRemovePoints(mvpPlayerSteamId, config.PointsPerMVP, mvpEvent.Userid, config.MVPMessage, MVPMessageColor);
            }

            return HookResult.Continue;
        }
        private async Task UpdateRoundResultAsync(string steamId, bool isWin)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();

                    string columnToUpdate = isWin ? "round_win" : "round_lose";
                    var updateQuery = $"UPDATE {dbConfig.Name} SET {columnToUpdate} = {columnToUpdate} + 1 WHERE steam = @SteamID;";
                    await connection.ExecuteAsync(updateQuery, new { SteamID = steamId });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdatePlayerConnectionAsync: {ex.Message}");
            }                
        }
        private HookResult OnPlayerDeath(EventPlayerDeath deathEvent, GameEventInfo info)
        {
            if ((deathEvent?.Userid?.IsBot ?? true) && !config.GivePointsForBotKills)
            {
                return HookResult.Continue;
            }

            if (isWarmup)
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
                        var pointsTask = AddOrRemovePoints(victimSteamId, config.PointsForSuicide, deathEvent.Userid, config.SuicideMessage, suicideMessageColor);
                    }
                }
                else
                {
                    if (config.PointsForDeath != 0)
                    {
                        string DeathMessageColor = ReplaceColorPlaceholders(config.DeathMessageColor);            
                        var deathPointsTask = AddOrRemovePoints(victimSteamId, config.PointsForDeath, deathEvent.Userid, config.DeathMessage, DeathMessageColor);
                    }
                    var updateKillsOrDeathsTask = UpdateKillsOrDeathsAsync(victimSteamId, false);

                    if (deathEvent.Attacker != null && IsValidPlayer(deathEvent.Attacker))
                    {
                        var killerSteamId64 = deathEvent.Attacker.SteamID.ToString();
                        var killerSteamId = ConvertSteamID64ToSteamID(killerSteamId64);

                        if (config.PointsForKill != 0)
                        {
                            string KillMessageColor = ReplaceColorPlaceholders(config.KillMessageColor);                                   
                            var killPointsTask = AddOrRemovePoints(killerSteamId, config.PointsForKill, deathEvent.Attacker, config.KillMessage, KillMessageColor);
                        }
                        var updateKillsTask = UpdateKillsOrDeathsAsync(killerSteamId, true);

                        if (deathEvent.Weapon == "awp" && deathEvent.Noscope && config.PointsForNoScopeAWP != 0)
                        {
                            string NoScopeAWPMessageColor = ReplaceColorPlaceholders(config.NoScopeAWPMessageColor);   
                            var noScopeTask = AddOrRemovePoints(killerSteamId, config.PointsForNoScopeAWP, deathEvent.Attacker, config.NoScopeAWPMessage, NoScopeAWPMessageColor);
                        }
                        
                        if (deathEvent.Headshot && config.PointsForHeadshot != 0)
                        {
                            string HeadshotMessageColor = ReplaceColorPlaceholders(config.HeadshotMessageColor);  
                            var headshotPointsTask = AddOrRemovePoints(killerSteamId, config.PointsForHeadshot, deathEvent.Attacker, config.HeadshotMessage, HeadshotMessageColor);
                            var updateHeadshotsTask = UpdateHeadshotsAsync(killerSteamId);
                        } 
                        
                        if (deathEvent.Penetrated > 0 && config.PointsForWallbang != 0)
                        {
                            string wallbangMessageColor = ReplaceColorPlaceholders(config.WallbangMessageColor);
                            var wallbangPointsTask = AddOrRemovePoints(killerSteamId, config.PointsForWallbang, deathEvent.Attacker, config.WallbangMessage, wallbangMessageColor);
                        }   

                        if (deathEvent.Thrusmoke && config.PointsForKillThroughSmoke != 0)
                        {
                            string messageColor = ReplaceColorPlaceholders(config.KillThroughSmokeMessageColor);
                            var pointsTask = AddOrRemovePoints(killerSteamId, config.PointsForKillThroughSmoke, deathEvent.Attacker, config.KillThroughSmokeMessage, messageColor);
                        }

                        if (deathEvent.Attackerblind && config.PointsForBlindKill != 0)
                        {
                            string messageColor = ReplaceColorPlaceholders(config.BlindKillMessageColor);
                            var pointsTask = AddOrRemovePoints(killerSteamId, config.PointsForBlindKill, deathEvent.Attacker, config.BlindKillMessage, messageColor);
                        }                        

                        var killerWeapon = deathEvent.Weapon.ToLowerInvariant();
                        WeaponPoints weaponConfig;

                        if (killerWeapon.Contains("knife"))
                        {
                            weaponConfig = weaponPointsConfig.FirstOrDefault(wp => wp.WeaponName.ToLowerInvariant() == "knife");
                        }
                        else
                        {
                            weaponConfig = weaponPointsConfig.FirstOrDefault(wp => wp.WeaponName.ToLowerInvariant() == killerWeapon);
                        }

                        if (weaponConfig != null)
                        {
                            string messageColor = ReplaceColorPlaceholders(weaponConfig.MessageColor);
                            var pointsTask = AddOrRemovePoints(killerSteamId, weaponConfig.Points, deathEvent.Attacker, weaponConfig.KillMessage, messageColor);
                        }                                         
                    }
                    if (deathEvent.Assister != null && IsValidPlayer(deathEvent.Assister) && config.PointsForAssist != 0)
                    {
                        var assisterSteamId64 = deathEvent.Assister.SteamID.ToString();
                        var assisterSteamId = ConvertSteamID64ToSteamID(assisterSteamId64);

                        string AssistMessageColor = ReplaceColorPlaceholders(config.AssistMessageColor);  
                        var assistPointsTask = AddOrRemovePoints(assisterSteamId, config.PointsForAssist, deathEvent.Assister, config.AssistMessage, AssistMessageColor);
                        var updateAssistsTask = UpdateAssistsAsync(assisterSteamId);
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
            try
            {            
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var updateQuery = $"UPDATE `{dbConfig.Name}` SET headshots = headshots + 1 WHERE steam = @SteamID;";
                    await connection.ExecuteAsync(updateQuery, new { SteamID = steamId });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateHeadshotsAsync: {ex.Message}");
            }                
        }

        private async Task UpdateAssistsAsync(string steamId)
        {
            try
            {            
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var updateQuery = $"UPDATE `{dbConfig.Name}` SET assists = assists + 1 WHERE steam = @SteamID;";
                    await connection.ExecuteAsync(updateQuery, new { SteamID = steamId });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateAssistsAsync: {ex.Message}");
            }                
        }
        private async Task UpdateKillsOrDeathsAsync(string steamId, bool isKill)
        {
            try
            {              
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    string columnToUpdate = isKill ? "kills" : "deaths";
                    var updateQuery = $"UPDATE `{dbConfig.Name}` SET `{columnToUpdate}` = `{columnToUpdate}` + 1 WHERE steam = @SteamID;";
                    await connection.ExecuteAsync(updateQuery, new { SteamID = steamId });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdateKillsOrDeathsAsync: {ex.Message}");
            }                
        }
        private async Task<int> UpdatePlayerPointsAsync(string steamId, int points, uint client)
        {
            int updatedPoints = 0;
            updatedPoints = (int)(g_Player[client].value + points);
            if (updatedPoints < 0) points = 0;

            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    using (var transaction = await connection.BeginTransactionAsync())
                    {
                        var updateQuery = $"UPDATE `{dbConfig.Name}` SET value = value + @NewPoints WHERE steam = @SteamID;";

                        await connection.ExecuteAsync(updateQuery, new { NewPoints = points, SteamID = steamId }, transaction);

                        await transaction.CommitAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdatePlayerPointsAsync: {ex.Message}");
            }

            return updatedPoints;
        }
        private int AddOrRemovePoints(string steamId, int points, CCSPlayerController playerController, string reason, string messageColor)
        {
            var client = playerController.Index;

            if (string.IsNullOrEmpty(steamId))
            {
                return 0;
            }

            if (playerController != null && !playerController.IsBot)
            {
                if (config.EnableSpecialNicknameBonus && playerController.PlayerName.Contains(config.SpecialNicknameContains, StringComparison.OrdinalIgnoreCase))
                {
                    if (points > 0 && config.BonusMultiplierForSpecialNickname > 1)
                    {
                        points = (int)(points * config.BonusMultiplierForSpecialNickname);
                    }
                }
            }
            
            int updatedPoints = (int)(g_Player[client].value + points);
            if (updatedPoints < 0) updatedPoints = 0;
            _ = UpdatePlayerPointsAsync(steamId, points, client);
            g_Player[playerController.Index].value = updatedPoints;


            Action chatUpdateAction = () =>
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
            };

            lock (_pendingActions)
            {
                _pendingActions.Add(chatUpdateAction);
            }

            Server.NextFrame(() =>
            {
                lock (_pendingActions)
                {
                    chatUpdateAction();
                    _pendingActions.Remove(chatUpdateAction);
                }
            });

            CheckAndUpdateRankAsync(steamId, updatedPoints);

            return updatedPoints;
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

                int currentRankId = await GetCurrentRankId(steamId);
                if (currentRankId != newRank.Id)
                {
                    bool isRankUpdated = await UpdatePlayerRankAsync(steamId, newRank.Id);
            
                    if (isRankUpdated)
                    {
                        bool isRankUp = newRank.Id > currentRankId;
                        NotifyPlayerOfRankChange(steamId, newRank.Name, isRankUp);
                        
                        string steamId64 = ConvertSteamIDToSteamID64(steamId);
                        foreach (var player in Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller"))
                        {
                            if (player != null && player.IsValid && !player.IsBot && player.SteamID.ToString() == steamId64)
                            {
                                Server.NextFrame(() =>
                                {
                                    player.Clan = $"[{Regex.Replace(newRank.Name, @"\{[A-Za-z]+}", "")}]";
                                    Utilities.SetStateChanged(player, "CCSPlayerController", "m_szClan");
                                });
                            }
                        }

                        
                        return true;
                    }


                }
            }

            return false;
        }
        private async Task<int> GetCurrentRankId(string steamId)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var currentRankQuery = $"SELECT `rank` FROM `{dbConfig.Name}` WHERE `steam` = @SteamID;";
                    var rankId = await connection.ExecuteScalarAsync<int>(currentRankQuery, new { SteamID = steamId });
                    return rankId;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetCurrentRankId] Error fetching rank for {steamId}: {ex.Message}");
                return -1; 
            }
        }


        private async Task<bool> UpdatePlayerRankAsync(string steamId, int newRankId)
        {
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    await connection.OpenAsync();
                    var updateRankQuery = $"UPDATE `{dbConfig.Name}` SET `rank` = @NewRankId WHERE `steam` = @SteamID;";
                    var affectedRows = await connection.ExecuteAsync(updateRankQuery, new { NewRankId = newRankId, SteamID = steamId });
                    return affectedRows > 0; 
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in UpdatePlayerRankAsync: {ex.Message}");
                return false;
            }
        }
        private List<WeaponPoints> LoadWeaponPointsConfig()
        {
            var filePath = Path.Combine(ModuleDirectory, "Weapons.yml");

            if (!File.Exists(filePath))
            {
                var defaultWeaponPoints = new List<WeaponPoints>
                {
                    new WeaponPoints { WeaponName = "knife", Points = 10, MessageColor = "{LightYellow}", KillMessage = "убийство ножом" },
                    new WeaponPoints { WeaponName = "awp", Points = 5, MessageColor = "{Blue}", KillMessage = "точный выстрел из AWP" }
                };

                var serializer = new SerializerBuilder().Build();
                var yaml = serializer.Serialize(defaultWeaponPoints);
                File.WriteAllText(filePath, yaml);

                return defaultWeaponPoints;
            }

            var deserializer = new DeserializerBuilder().Build();
            var yamlContents = File.ReadAllText(filePath);
            var weaponPoints = deserializer.Deserialize<List<WeaponPoints>>(yamlContents) ?? new List<WeaponPoints>();

            Console.WriteLine("Загружена конфигурация оружия:");
            foreach (var weaponPoint in weaponPoints)
            {
                Console.WriteLine($"Оружие: {weaponPoint.WeaponName}, Очки: {weaponPoint.Points}, Цвет сообщения: {weaponPoint.MessageColor}, Сообщение об убийстве: {weaponPoint.KillMessage}");
            }

            return weaponPoints;
        }
        private void NotifyPlayerOfRankChange(string steamId, string newRankName, bool isRankUp)
        {
            Server.NextFrame(() =>
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
            });
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
        private RankConfig? GetCurrentRank(string steamID64)
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
                connection.Open();
                var query = $"SELECT rank FROM {dbConfig.Name} WHERE steam = @SteamID;";
                var rankId = connection.QueryFirstOrDefault<int>(query, new { SteamID = steamID }); 

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
        private CCSPlayerController FindPlayerByUserId(int userId)
        {
            foreach (var player in Utilities.FindAllEntitiesByDesignerName<CCSPlayerController>("cs_player_controller"))
            {
                if (player != null && player.IsValid && !player.IsBot && player.UserId == userId)
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
                        (SELECT COUNT(*) FROM `{dbConfig.Name}` WHERE value > p.value) + 1 as place,
                        (SELECT COUNT(*) FROM `{dbConfig.Name}`) as totalPlayers
                    FROM `{dbConfig.Name}` p
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
            if (!config.IsRankCommandEnabled)
            {
                return;
            }

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
            if (!config.IsTopCommandEnabled)
            {
                return;
            }

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
                    var topPlayersQuery = @"
                        SELECT steam, name, value, `rank`
                        FROM `" + dbConfig.Name + @"`
                        ORDER BY value DESC
                        LIMIT 10;";
                    
                    var topPlayers = connection.Query(topPlayersQuery).ToList();

                    if (topPlayers.Any())
                    {
                        var ranksConfig = LoadRanksConfig(); 

                        string introMessage = ReplaceColorPlaceholders(config.TopCommandIntroMessage);
                        player.PrintToChat(introMessage);

                        for (int i = 0; i < topPlayers.Count; i++)
                        {
                            var topPlayerInfo = topPlayers[i];
                            var rankName = ranksConfig.FirstOrDefault(r => r.Id == topPlayerInfo.rank)?.Name ?? "Unknown Rank";
                            string playerMessage = config.TopCommandPlayerMessage
                                .Replace("{INDEX}", (i + 1).ToString())
                                .Replace("{NAME}", topPlayerInfo.name)
                                .Replace("{POINTS}", topPlayerInfo.value.ToString())
                                .Replace("{RANK}", rankName);
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
            if (!config.IsTopkillsCommandEnabled)
            {
                return;
            }

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
                    var topPlayersQuery = @"
                        SELECT steam, name, kills
                        FROM `" + dbConfig.Name + @"`
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
            if (!config.IsTopdeathsCommandEnabled)
            {
                return;
            }           
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
                        FROM `{dbConfig.Name}`
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
            if (!config.IsTopkdrCommandEnabled)
            {
                return;
            }           
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
                        SELECT steam, name, kills, deaths, 
                        CASE
                            WHEN deaths = 0 THEN kills
                            ELSE kills / deaths
                        END AS kdr
                        FROM `{dbConfig.Name}`
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
            if (!config.IsToptimeCommandEnabled)
            {
                return;
            }              
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
                    var topPlayersQuery = @"
                        SELECT steam, name, playtime
                        FROM `" + dbConfig.Name + @"`
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
            if (!config.IsResetstatsCommandEnabled)
            {
                return;
            }             
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
            try
            {
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    var resetQuery = $"UPDATE `{dbConfig.Name}` SET kills = 0, deaths = 0, `value` = 0, shoots = 0, hits = 0, headshots = 0, assists = 0, round_win = 0, round_lose = 0, playtime = 0 WHERE steam = @SteamID;";
                    int affectedRows = connection.Execute(resetQuery, new { SteamID = steamId });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating player stats: {ex.Message}");
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
        [ConsoleCommand("rp_reloadweapons", "Перезагружает конфигурационный файл Weapons.yml")]
        public void ReloadWeaponsConfigCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (player == null || player.IsBot) 
            {
                try
                {
                    weaponPointsConfig = LoadWeaponPointsConfig();
                    Console.WriteLine("[RankPointsPlugin] Конфигурация оружия успешно перезагружена.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[RankPointsPlugin] Ошибка при перезагрузке конфигурации оружия: {ex.Message}");
                }
            }
            else
            {
                player.PrintToChat($"{ChatColors.Red}Эта команда доступна только из консоли сервера.");
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
            if (!config.IsRanksCommandEnabled)
            {
                return;
            }             
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
            if (!config.IsLvlCommandEnabled)
            {
                return;
            }               
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
                ReplaceColorPlaceholders(config.RanksCommandDescription),
                ReplaceColorPlaceholders(config.TagRankCommandDescription)
            };

            foreach (var description in commandDescriptions)
            {
                player.PrintToChat(description);
            }
        }
        [ConsoleCommand("tagrank", "Включает или выключает отображение вашего клан-тега.")]
        public void OnTagRankCommand(CCSPlayerController? player, CommandInfo command)
        {
            if (!config.IsTagRankCommandEnabled)
            {
                return;
            }

            if (player == null) return;

            var steamId64 = player.SteamID.ToString();
            var tagSettings = LoadTagSettings();

            if (tagSettings.DisabledTags.Contains(steamId64))
            {
                tagSettings.DisabledTags.Remove(steamId64);
                string enabledMessage = ReplaceColorPlaceholders(config.TagRankEnabledMessage);
                player.PrintToChat(enabledMessage);
            }
            else
            {
                tagSettings.DisabledTags.Add(steamId64);
                string disabledMessage = ReplaceColorPlaceholders(config.TagRankDisabledMessage);
                player.PrintToChat(disabledMessage);
            }

            SaveTagSettings(tagSettings);
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
                var resetQuery = $"UPDATE `{dbConfig.Name}` SET `value` = 0, `rank` = 1 WHERE steam = @SteamID;";
                connection.Execute(resetQuery, new { SteamID = steamId });
            }
        }

        private void ResetPlayerStats2(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var resetQuery = $"UPDATE `{dbConfig.Name}` SET kills = 0, deaths = 0, shoots = 0, hits = 0, headshots = 0, assists = 0, round_win = 0, round_lose = 0 WHERE steam = @SteamID;";
                connection.Execute(resetQuery, new { SteamID = steamId });
            }
        }

        private void ResetPlayerPlaytime(string steamId)
        {
            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                var resetQuery = $"UPDATE `{dbConfig.Name}` SET playtime = 0 WHERE steam = @SteamID;";
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
    public class WeaponPoints
    {
        public string? WeaponName { get; set; }
        public int Points { get; set; }
        public string? MessageColor { get; set; }
        public string? KillMessage { get; set; }
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