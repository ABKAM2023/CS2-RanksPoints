![GitHub Repo stars](https://img.shields.io/github/stars/ABKAM2023/CS2-RanksPoints?style=for-the-badge)
![GitHub issues](https://img.shields.io/github/issues/ABKAM2023/CS2-RanksPoints?style=for-the-badge)
![GitHub contributors](https://img.shields.io/github/contributors/ABKAM2023/CS2-RanksPoints?style=for-the-badge)
![GitHub all releases](https://img.shields.io/github/downloads/ABKAM2023/CS2-RanksPoints/total?style=for-the-badge)

I'm sorry for my poor English.

# EN
**To facilitate easier assistance, I have created a dedicated Discord server. You can join it using the following link: [https://discord.gg/yQm8edwV](https://discord.gg/saz3uGTfKR)**

The plugin was developed inspired by the [Levels Ranks] plugins and borrows most of its functions as well as the database. This means that the RanksPoints plugin can be integrated with LrWeb or GameCMS similar to [Levels Ranks]. During development, there was no opportunity to fully test the plugin, so there may be errors in its operation. If you find any issues, please report them for correction.

# RanksPoints
The RanksPoints system is based on a simple principle: players perform various actions in the game, as a result of which they either gain or lose experience points. Achieving or losing a certain amount of these points leads to obtaining a corresponding rank. The number of available ranks can be configured and edited as desired.

# Installation
1. Install [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master) and [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)

2. Download RankPoints.

3. Unzip the archive and upload it to your game server.

4. Start the server to create the necessary configuration files.

5. Connect the plugin to the database by entering the required data in the dbconfig.json file. Make sure the entered data is correct.

# Main Configuration (Config.yml)
```
# Configuration file for RankPoints

# Number of awarded points
# Points for Kill - the number of points added to a player for killing an opponent.
PointsForKill: 5
# Points for Death - the number of points subtracted from a player for dying.
PointsForDeath: -5
# Points for Assist - the number of points added to a player for assisting in a kill.
PointsForAssist: 1
# Points for Suicide - the number of points subtracted from a player for committing suicide.
PointsForSuicide: -6
# Points for Headshot - additional points for a headshot kill.
PointsForHeadshot: 1
# Points per Round Win - the number of points added to a player for their team winning a round.
PointsPerRoundWin: 2
# Points per Round Loss - the number of points subtracted from a player for their team losing a round.
PointsPerRoundLoss: -2
# Points for MVP - the number of points added to a player for earning the MVP title of a round.
PointsPerMVP: 3
# Points for NoScope AWP Kill - additional points for killing without using the AWP scope.
PointsForNoScopeAWP: 1
# Points for Bomb Defusal
PointsForBombDefusal: 2
# Points for Bomb Exploded
PointsForBombExploded: 2
# Points for Bomb Planting - the number of points added to a player for successfully planting the bomb.
PointsForBombPlanting: 2
# Points for Bomb Dropping - the number of points subtracted from a player for dropping the bomb.
PointsForBombDropping: -2
# Points for Bomb Pickup - the number of points added to a player for picking up the bomb.
PointsForBombPickup: 1
# Points for Wallbang - the number of points added to a player for getting a kill through a wall.
PointsForWallbang: 3
# Points for Hostage Follows
PointsForHostageFollows: 2
# Points for Hostage Stops Following
PointsForHostageStopsFollowing: -2
# Points for Hostage Rescued
PointsForHostageRescued: 4
# Points for Kill Through Smoke - the number of points added to a player for killing an enemy through a smoke screen.
PointsForKillThroughSmoke: 3
# Points for Blind Kill - the number of points added to a player for getting a kill while blinded.
PointsForBlindKill: 5

# RanksPoints Parameters
# Displaying clan tags for ranks of players. true - enabled, false - disabled.
EnableClanTags: True
# Minimum number of players for experience gain - players only earn experience if this number of players is on the server.
GetActivePlayerCountMsg: "[ {Yellow}RanksPoints {White}] A minimum of {Red}{MIN_PLAYERS} {White}players is required for experience gain."
MinPlayersForExperience: 4
# Enabling or disabling bonus experience for special nicknames.
EnableSpecialNicknameBonus: true
# Experience multiplier for special nicknames.
BonusMultiplierForSpecialNickname: 1.5
# String to search for in the nickname to apply the multiplier.
SpecialNicknameContains: "example.com"
# Enabling or disabling awarding points for bot kills. true - enabled, false - disabled.
GivePointsForBotKills: False

# All RanksPoints Messages
# Messages for experience gain
PointsChangeMessage: "[ {Yellow}RanksPoints{White} ] Your experience: {COLOR} {POINTS} [{SIGN}{CHANGE_POINTS} for {REASON}]"
# Events
SuicideMessage: "suicide"
SuicideMessageColor: "{Red}"
DeathMessage: "death"
DeathMessageColor: "{Red}"
KillMessage: "kill"
KillMessageColor: "{Green}"
NoScopeAWPMessage: "AWP kill without scope"
NoScopeAWPMessageColor: "{Blue}"
HeadshotMessage: "headshot"
HeadshotMessageColor: "{Yellow}"
AssistMessage: "assist"
AssistMessageColor: "{Blue}"
RoundWinMessage: "round win"
RoundWinMessageColor: "{Green}"
RoundLossMessage: "round loss"
RoundLossMessageColor: "{Red}"
MVPMessage: "MVP"
MVPMessageColor: "{Gold}"
BombDefusalMessage: "bomb defusal"
BombDefusalMessageColor: "{Green}"
BombExplodedMessage: "bomb exploded"
BombExplodedMessageColor: "{Green}"
BombPlantingMessage: "bomb planting"
BombPlantingMessageColor: "{Green}"
BombDroppingMessage: "bomb dropping"
BombDroppingMessageColor: "{Red}"
BombPickupMessage: "bomb pickup"
BombPickupMessageColor: "{Green}"
WallbangMessage: "wallbang"
WallbangMessageColor: "{Purple}"
HostageFollowsMessage: "hostage follows"
HostageFollowsMessageColor: "{Green}"
HostageStopsFollowingMessage: "hostage stops following"
HostageStopsFollowingMessageColor: "{Red}"
HostageRescuedMessage: "hostage rescued"
HostageRescuedMessageColor: "{Blue}"
KillThroughSmokeMessage: "убийство через дым"
KillThroughSmokeMessageColor: "{Green}"
BlindKillMessage: "убийство в состоянии ослепления"
BlindKillMessageColor: "{Yellow}"

# Rank Up Message.
RankUpMessage: Your rank has been upgraded to {RANK_NAME}!
# Rank Down Message.
RankDownMessage: Your rank has been downgraded to {RANK_NAME}.

# !rank
RankCommandMessage : "[ {Yellow}RanksPoints {White}] Rank: {Green}{RANK_NAME} {White}| Position: {Blue}{PLACE}/{TOTAL_PLAYERS} {White}| Experience: {Gold}{POINTS} {White}| Kills: {Green}{KILLS} {White}| Deaths: {Red}{DEATHS} {White}| KDR: {Yellow}{KDR} {White}| Server Time: {Gold}{PLAY_TIME}"
TimeFormat: "{0}d {1}h {2}min"
# Enabling or disabling the !rank command
IsRankCommandEnabled: true

# !top
TopCommandIntroMessage : "[ {Blue}Top Players{White} ]"
TopCommandPlayerMessage: "{INDEX}. {Grey}{NAME} - {White}{RANK} {Grey}- {Blue}{POINTS} points"
TopCommandNoDataMessage: "[ {Red}Error{White} ] No data available for top players."
TopCommandErrorMessage: "[ {Red}Error{White} ] An error occurred while executing the command."
# Enabling or disabling the !top command
IsTopCommandEnabled: true

# !topkills
TopKillsCommandIntroMessage: "[ {Green}Top Killers{White} ]"
TopKillsCommandPlayerMessage: "{INDEX}. {Grey}{NAME} - {Green}{KILLS} kills{White}"
TopKillsCommandNoDataMessage: "[ {Red}Error{White} ] No data available for top killers."
TopKillsCommandErrorMessage: "[ {Red}Error{White} ] An error occurred while executing the command."
# Enabling or disabling the !topkills command
IsTopkillsCommandEnabled: true

# !topdeaths
TopDeathsCommandIntroMessage: "[ {Red}Top Deaths{White} ]"
TopDeathsCommandPlayerMessage: "{INDEX}. {Grey}{NAME}{White} - {Red}{DEATHS} deaths{White}"
TopDeathsCommandNoDataMessage: "[ {Red}Error{White} ] No data available for top deaths."
TopDeathsCommandErrorMessage: "[ {Red}Error{White} ] An error occurred while executing the command."
# Enabling or disabling the !topdeaths command
IsTopdeathsCommandEnabled: true

# !topkdr
TopKDRCommandIntroMessage: "[ {Yellow}Top KDR{White} ]"
TopKDRCommandPlayerMessage: "{INDEX}. {Grey}{NAME}{White} - {Yellow}KDR: {KDR}"
TopKDRCommandNoDataMessage: "[ {Red}Error{White} ] No data available for top KDR."
TopKDRCommandErrorMessage: "[ {Red}Error{White} ] An error occurred while executing the command."
# Enabling or disabling the !topkdr command
IsTopkdrCommandEnabled: true

# !toptime
TopTimeCommandIntroMessage: "[ {Gold}Top Playtime{White} ]"
TopTimeCommandPlayerMessage: "{INDEX}. {Grey}{NAME} - {Gold}{TIME}{White}"
TopTimeCommandNoDataMessage : "[ {Red}Error{White} ] No data available for top playtime."
TopTimeCommandErrorMessage: "[ {Red}Error{White} ] An error occurred while executing the command."
TopTimeFormat: "{0}d {1}h {2}min"
# Enabling or disabling the !toptime command
IsToptimeCommandEnabled: true

# !resetstats
ResetStatsCooldownMessage: "[ {Red}RanksPoints {White}] You can only reset your stats once every 3 hours."
ResetStatsSuccessMessage: "[ {Yellow}RanksPoints {White}] Your stats have been reset."
ResetStatsCooldownHours: "3"
# Enabling or disabling the !resetstats command
IsResetstatsCommandEnabled: true

# !ranks
RanksCommandIntroMessage: "[ {Gold}Rank List{White} ]"
RanksCommandRankMessage: "{NAME} - {Green}{EXPERIENCE} experience{White}"
RanksCommandNoDataMessage: "[ {Red}Error{White} ] No data available for ranks."
RanksCommandErrorMessage: "[ {Red}Error{White} ] An error occurred while executing the command."
# Enabling or disabling the !ranks command
IsRanksCommandEnabled: true

# !lvl
LvlCommandIntroMessage: "[ {Gold}Available Commands{White} ]"
RankCommandDescription: "- {Green}!rank {White}- Shows your current rank and stats"
TopCommandDescription: "- {Green}!top {White}- Shows the top 10 players by points"
TopKillsCommandDescription: "- {Green}!topkills {White}- Shows the top 10 players by kills"
TopDeathsCommandDescription: "- {Green}!topdeaths {White}- Shows the top 10 players by deaths"
TopKDRCommandDescription: "- {Green}!topkdr {White}- Shows the top 10 players by KDR"
TopTimeCommandDescription: "- {Green}!toptime {White}- Shows the top 10 players by playtime"
ResetStatsCommandDescription: "- {Green}!resetstats {White}- Reset your stats (can be used once every 3 hours)"
RanksCommandDescription: "- {Green}!ranks {White}- Shows a list of all ranks and the required experience to achieve them"
TagRankCommandDescription: "- {Green}!tagrank {White}- Enables or disables displaying your clan tag"
# Enabling or disabling the !lvl command
IsLvlCommandEnabled: true

# !tagrank
TagRankEnabledMessage: "[ {Yellow}RanksPoints {White}] Your clan tag will be displayed again starting from the next round."
TagRankDisabledMessage: "[ {Yellow}RanksPoints {White}] Your clan tag will no longer be displayed starting from the next round."
# Enabling or disabling the !tagrank command
IsTagRankCommandEnabled: true
```
# Rank Configuration (settings_ranks.yml)
```
- id: 0
  name: Silver - I
  minExperience: 0
  clanTag: '[Silver - I]'
- id: 1
  name: Silver - II
  minExperience: 10
  clanTag: '[Silver - II]'
- id: 2
  name: Silver - III
  minExperience: 25
  clanTag: '[Silver - III]'
- id: 3
  name: Silver - IV
  minExperience: 50
  clanTag: '[Silver - IV]'
- id: 4
  name: Silver Elite
  minExperience: 75
  clanTag: '[Silver Elite]'
- id: 5
  name: Silver - Master Guardian
  minExperience: 100
  clanTag: '[Silver - MG]'
- id: 6
  name: Gold Star - I
  minExperience: 150
  clanTag: '[Gold Star - I]'
- id: 7
  name: Gold Star - II
  minExperience: 200
  clanTag: '[Gold Star - II]'
- id: 8
  name: Gold Star - III
  minExperience: 300
  clanTag: '[Gold Star - III]'
- id: 9
  name: Gold Star - Master
  minExperience: 500
  clanTag: '[Gold Star - M]'
- id: 10
  name: Master Guardian - I
  minExperience: 750
  clanTag: '[Master Guardian - I]'
- id: 11
  name: Master Guardian - II
  minExperience: 1000
  clanTag: '[Master Guardian - II]'
- id: 12
  name: Master Guardian Elite
  minExperience: 1500
  clanTag: '[Master Guardian Elite]'
- id: 13
  name: Distinguished Master Guardian
  minExperience: 2000
  clanTag: '[Distinguished MG]'
- id: 14
  name: Legendary Eagle
  minExperience: 3000
  clanTag: '[Legendary Eagle]'
- id: 15
  name: Legendary Eagle Master
  minExperience: 5000
  clanTag: '[Legendary Eagle M]'
- id: 16
  name: Supreme Master First Class
  minExperience: 7500
  clanTag: '[Supreme M-FC]'
- id: 17
  name: Global Elite
  minExperience: 10000
  clanTag: '[Global Elite]'
```

# Configuration file for setting up experience points awarded for kills with specific types of weapons (Weapons.yml).
Additional weapon types can also be added, for example, 'weapon_knife' corresponds to 'knife'.
```
- WeaponName: knife
  Points: 10
  MessageColor: '{LightYellow}'
  KillMessage: knife kill
- WeaponName: awp
  Points: 5
  MessageColor: '{Blue}'
  KillMessage: precise AWP shot
```

# Database Connection Configuration (dbconfig.json)
```
{
  "DbHost": "YourHost",
  "DbUser": "YourUser",
  "DbPassword": "YourPassword",
  "DbName": "YourDatabase",
  "DbPort": "3306"
  "Name": "lvl_base"
}
```
# Chat Commands
- `!rank` shows statistics: current rank, required number of points to the next rank, your experience, number of kills, deaths, and kill-to-death ratio (KDR).
- `!top` displays a list of the top ten players by experience points.
- `!topkills` displays a list of the top ten players by kills.
- `!topdeaths` displays a list of the top ten players by deaths.
- `!topkdr` displays a list of the top ten players by KDR.
- `!toptime` shows the top ten players by time on the server.
- `!resetstats` resets your statistics (can be used once every 3 hours).
- `!ranks` shows a list of all ranks and the experience required to achieve them.
- `!lvl` shows a list of all available commands and their functions.
- `!tagrank` enables or disables the clan tag

# Console Commands
- `rp_reloadconfig` reloads the configuration file Config.yml.
- `rp_reloadranks` reloads the configuration file settings_ranks.yaml.
- `rp_reloadweapons` reloads the configuration file Weapons.yaml.
- `rp_resetranks` Clears a player's statistics. Usage: rp_resetranks <steamid64> <data-type> (`data-type`: `exp` clears values, rank; `stats` clears kills, deaths, shoots, hits, headshots, assists, round_win, round_lose; `time` clears playtime).

# Planned Features
- Expand the list of events for which experience is awarded.
- Fix all warnings during compilation.
- This list will be further expanded.

# RU
**Для удобства и лучшей организации помощи я создал специальный сервер в Discord. Вы можете присоединиться к нему по следующей ссылке: [https://discord.gg/yQm8edwV](https://discord.gg/saz3uGTfKR)**

Плагин был разработан, вдохновляясь плагином [Levels Ranks], и заимствует большую часть своих функций, а также базу данных. Это означает, что плагин RanksPoints может быть интегрирован с LrWeb или GameCMS аналогично [Levels Ranks]. В процессе разработки не было возможности полностью проверить плагин, поэтому в его работе могут проявляться ошибки. Если вы обнаружите какие-либо проблемы, сообщите об этом для их исправления.

# RanksPoints
RanksPoints система базируется на простом принципе: игроки совершают разнообразные действия в игре, в результате которых они либо приобретают, либо теряют очки опыта. Достижение или потеря определенного объема этих очков ведет к получению соответствующего ранга. Количество доступных рангов может быть настроено и отредактировано по усмотрению.

# Установка
1. Установите [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master) и [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)

2. Скачайте RankPoints

3. Распакуйте архив и загрузите его на игровой сервер

4. Запустите сервер, чтобы создать необходимые конфигурационные файлы.

5. Подключите плагин к базе данных, введя необходимые данные в файл dbconfig.json. Убедитесь в корректности введенных данных.

# Основной конфиг (Config.yml)
```
# Конфигурационный файл для RankPoints

# Количество выдаваемых очков
# Очки за убийство - количество очков, добавляемое игроку за убийство противника.
PointsForKill: 5
# Очки отнимаемые за смерть - количество очков, вычитаемое у игрока за смерть.
PointsForDeath: -5
# Очки за помощь - количество очков, добавляемое игроку за помощь в убийстве.
PointsForAssist: 1
# Очки за самоубийство - количество очков, вычитаемое у игрока за самоубийство.
PointsForSuicide: -6
# Очки за выстрел в голову - дополнительные очки за убийство с выстрелом в голову.
PointsForHeadshot: 1
# Очки за победу в раунде - количество очков, добавляемое игроку за победу его команды в раунде.
PointsPerRoundWin: 2
# Очки за проигрыш в раунде - количество очков, вычитаемое у игрока за проигрыш его команды в раунде.
PointsPerRoundLoss: -2
# Очки за MVP - количество очков, добавляемое игроку за получение звания MVP раунда.
PointsPerMVP: 3
# Очки за убийство с AWP без прицела - дополнительные очки за убийство без использования прицела.
PointsForNoScopeAWP: 1
# Очки за обезвреживание бомбы
PointsForBombDefusal: 2
# Очки за взрыв бомбы
PointsForBombExploded: 2
# Очки за установку бомбы - количество очков, добавляемое игроку за успешную установку бомбы.
PointsForBombPlanting: 2
# Очки за выброс бомбы - количество очков, вычитаемое у игрока за выброс бомбы.
PointsForBombDropping: -2
# Очки за поднятие бомбы - количество очков, добавляемое игроку за поднятие бомбы.
PointsForBombPickup: 1
# Очки за убийство через прострел.
PointsForWallbang: 3
# Очки за поднятие заложника
PointsForHostageFollows: 2
# Очки за потерю заложника
PointsForHostageStopsFollowing: -2
# Очки за спасение заложника
PointsForHostageRescued: 4
# Очки за убийство через дым - количество очков, добавляемое игроку за убийство врага через дымовую завесу.
PointsForKillThroughSmoke: 3
# Очки за убийство в состоянии ослепления - количество очков, добавляемое игроку за убийство, когда он ослеплен.
PointsForBlindKill: 5

# Параметры RanksPoints
# Отображение клан-тегов званий для игроков. true - включено, false - отключено.
EnableClanTags: True
# Минимальное количество игроков для начисления опыта - игрокам начисляется опыт только если на сервере играет минимум это количество игроков.
GetActivePlayerCountMsg: "[ {Yellow}RanksPoints {White}] Необходимо минимум {Red}{MIN_PLAYERS} {White}игроков для начисления опыта."
MinPlayersForExperience: 4
# Включение или выключение дополнительного опыта для специальных никнеймов
EnableSpecialNicknameBonus: true
# Множитель опыта для специальных никнеймов
BonusMultiplierForSpecialNickname: 1.5
# Строка, которую нужно искать в никнейме для применения множителя
SpecialNicknameContains: "example.com"
# Включение или выключение начисления очков за убийство ботов. true - включено, false - выключено.
GivePointsForBotKills: false

# Все сообщения RanksPoints
# Сообщения при получении опыта
PointsChangeMessage: "[ {Yellow}RanksPoints{White} ] Ваш опыт:{COLOR} {POINTS} [{SIGN}{CHANGE_POINTS} за {REASON}]"
# События
SuicideMessage: "самоубийство"
SuicideMessageColor: "{Red}"
DeathMessage: "смерть"
DeathMessageColor: "{Red}"
KillMessage: "убийство"
KillMessageColor: "{Green}"
NoScopeAWPMessage: "убийство с AWP без прицела"
NoScopeAWPMessageColor: "{Blue}"
HeadshotMessage: "выстрел в голову"
HeadshotMessageColor: "{Yellow}"
AssistMessage: "ассист"
AssistMessageColor: "{Blue}"
RoundWinMessage: "победа в раунде"
RoundWinMessageColor: "{Green}"
RoundLossMessage: "проигрыш в раунде"
RoundLossMessageColor: "{Red}"
MVPMessage: "MVP"
MVPMessageColor: "{Gold}"
BombDefusalMessage: "обезвреживание бомбы"
BombDefusalMessageColor: "{Green}"
BombExplodedMessage: "взрыв бомбы"
BombExplodedMessageColor: "{Green}"
BombPlantingMessage: "установку бомбы"
BombPlantingMessageColor: "{Green}"
BombDroppingMessage: "выброс бомбы"
BombDroppingMessageColor: "{Red}"
BombPickupMessage: "поднятие бомбы"
BombPickupMessageColor: "{Green}"
WallbangMessage: "прострел"
WallbangMessageColor: "{Purple}"
HostageFollowsMessage: "заложник следует"
HostageFollowsMessageColor: "{Green}"
HostageStopsFollowingMessage: "заложник перестал следовать"
HostageStopsFollowingMessageColor: "{Red}"
HostageRescuedMessage: "заложник спасен"
HostageRescuedMessageColor: "{Blue}"
KillThroughSmokeMessage: "убийство через дым"
KillThroughSmokeMessageColor: "{Green}"
BlindKillMessage: "убийство в состоянии ослепления"
BlindKillMessageColor: "{Yellow}"

# Сообщение о повышении звания.
RankUpMessage: Ваше звание было повышено до {RANK_NAME}!
# Сообщение о понижении звания.
RankDownMessage: Ваше звание было понижено до {RANK_NAME}.

# !rank
RankCommandMessage : "[ {Yellow}RanksPoints {White}] Звание: {Green}{RANK_NAME} {White}| Место: {Blue}{PLACE}/{TOTAL_PLAYERS} {White}| Опыт: {Gold}{POINTS} {White}| Убийства: {Green}{KILLS} {White}| Смерти: {Red}{DEATHS} {White}| KDR: {Yellow}{KDR} {White}| Время на сервере: {Gold}{PLAY_TIME}"
TimeFormat: "{0}д {1}ч {2}мин"
# Включение или выключение команды !rank
IsRankCommandEnabled: true

# !top
TopCommandIntroMessage : "[ {Blue}Топ игроков{White} ]"
TopCommandPlayerMessage: "{INDEX}. {Grey}{NAME} - {White}{RANK} {Grey}- {Blue}{POINTS} очков"
TopCommandNoDataMessage: "[ {Red}Ошибка{White} ] Нет данных о топ игроках."
TopCommandErrorMessage: "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."
# Включение или выключение команды !top
IsTopCommandEnabled: true

# !topkills
TopKillsCommandIntroMessage: "[ {Green}Топ игроков по убийствам{White} ]"
TopKillsCommandPlayerMessage: "{INDEX}. {Grey}{NAME} - {Green}{KILLS} убийств{White}"
TopKillsCommandNoDataMessage: "[ {Red}Ошибка{White} ] Нет данных о топ игроках по убийствам."
TopKillsCommandErrorMessage: "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."
# Включение или выключение команды !topkills
IsTopkillsCommandEnabled: true

# !topdeaths
TopDeathsCommandIntroMessage: "[ {Red}Топ игроков по смертям{White} ]"
TopDeathsCommandPlayerMessage: "{INDEX}. {Grey}{NAME}{White} - {Red}{DEATHS} смертей{White}"
TopDeathsCommandNoDataMessage: "[ {Red}Ошибка{White} ] Нет данных о топ игроках по смертям."
TopDeathsCommandErrorMessage: "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."
# Включение или выключение команды !topdeaths
IsTopdeathsCommandEnabled: true

# !topkdr
TopKDRCommandIntroMessage: "[ {Yellow}Топ игроков по KDR{White} ]"
TopKDRCommandPlayerMessage: "{INDEX}. {Grey}{NAME}{White} - {Yellow}KDR: {KDR}"
TopKDRCommandNoDataMessage: "[ {Red}Ошибка{White} ] Нет данных о топ игроках по KDR."
TopKDRCommandErrorMessage: "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."
# Включение или выключение команды !topkdr
IsTopkdrCommandEnabled: true

# !toptime
TopTimeCommandIntroMessage: "[ {Gold}Топ игроков по времени на сервере{White} ]"
TopTimeCommandPlayerMessage: "{INDEX}. {Grey}{NAME} - {Gold}{TIME}{White}"
TopTimeCommandNoDataMessage : "[ {Red}Ошибка{White} ] Нет данных о топ игроках по времени на сервере."
TopTimeCommandErrorMessage: "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."
TopTimeFormat: "{0}д {1}ч {2}мин"
# Включение или выключение команды !toptime
IsToptimeCommandEnabled: true

# !resetstats
ResetStatsCooldownMessage: "[ {Red}RanksPoints {White}] Сбросить статистику можно только раз в 3 часа."
ResetStatsSuccessMessage: "[ {Yellow}RanksPoints {White}] Ваша статистика сброшена."
ResetStatsCooldownHours: "3"
# Включение или выключение команды !resetstats
IsResetstatsCommandEnabled: true

# !ranks
RanksCommandIntroMessage: "[ {Gold}Список званий{White} ]"
RanksCommandRankMessage: "{NAME} - {Green}{EXPERIENCE} опыта{White}"
RanksCommandNoDataMessage: "[ {Red}Ошибка{White} ] Нет данных о званиях."
RanksCommandErrorMessage: "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."
# Включение или выключение команды !ranks
IsRanksCommandEnabled: true

# !lvl
LvlCommandIntroMessage: "[ {Gold}Список доступных команд{White} ]"
RankCommandDescription: "- {Green}!rank {White}- Показывает ваше текущее звание и статистику"
TopCommandDescription: "- {Green}!top {White}- Показывает топ-10 игроков по очкам"
TopKillsCommandDescription: "- {Green}!topkills {White}- Показывает топ-10 игроков по убийствам"
TopDeathsCommandDescription: "- {Green}!topdeaths {White}- Показывает топ-10 игроков по смертям"
TopKDRCommandDescription: "- {Green}!topkdr {White}- Показывает топ-10 игроков по KDR"
TopTimeCommandDescription: "- {Green}!toptime {White}- Показывает топ-10 игроков по времени на сервере"
ResetStatsCommandDescription: "- {Green}!resetstats {White}- Сбросить свою статистику (можно использовать раз в 3 часа)"
RanksCommandDescription: "- {Green}!ranks {White}- Показывает список всех званий и опыта, необходимого для их получения"
TagRankCommandDescription: "- {Green}!tagrank {White}- Включает или выключает отображение вашего клан-тега"
# Включение или выключение команды !lvl
IsLvlCommandEnabled: true

# !tagrank
TagRankEnabledMessage: "[ {Yellow}RanksPoints {White}] Клан-тег будет вновь отображаться, начиная с следующего раунда."
TagRankDisabledMessage: "[ {Yellow}RanksPoints {White}] Клан-тег больше не будет отображаться, начиная с следующего раунда."
# Включение или выключение команды !tagrank
IsTagRankCommandEnabled: true
```

# Конфиг настройки званий (settings_ranks.yml)
```
- id: 1
  name: Серебро - I
  minExperience: 0
- id: 2
  name: Серебро - II
  minExperience: 10
- id: 3
  name: Серебро - III
  minExperience: 25
- id: 4
  name: Серебро - IV
  minExperience: 50
- id: 5
  name: Серебро Элита
  minExperience: 75
- id: 6
  name: Серебро - Великий Магистр
  minExperience: 100
- id: 7
  name: Золотая Звезда - I
  minExperience: 150
- id: 8
  name: Золотая Звезда - II
  minExperience: 200
- id: 9
  name: Золотая Звезда - III
  minExperience: 300
- id: 10
  name: Золотая Звезда - Магистр
  minExperience: 500
- id: 11
  name: Магистр-хранитель - I
  minExperience: 750
- id: 12
  name: Магистр-хранитель - II
  minExperience: 1000
- id: 13
  name: Магистр-хранитель - Элита
  minExperience: 1500
- id: 14
  name: Заслуженный Магистр-хранитель
  minExperience: 2000
- id: 15
  name: Легендарный Беркут
  minExperience: 3000
- id: 16
  name: Легендарный Беркут-магистр
  minExperience: 5000
- id: 17
  name: Великий Магистр - Высшего Ранга
  minExperience: 7500
- id: 18
  name: Всемирная Элита
  minExperience: 10000
```

# Конфигурационный файл для настройки начисления опыта за убийства с использованием определенных видов оружия (Weapons.yml).
Также можно добавить другие виды оружия, например, 'weapon_knife', это 'knife'
```
- WeaponName: knife
  Points: 10
  MessageColor: '{Red}'
  KillMessage: убийство ножом
- WeaponName: awp
  Points: 5
  MessageColor: '{Blue}'
  KillMessage: точный выстрел из AWP
```

# Конфиг подключения базы данных (dbconfig.json)
```
{
  "DbHost": "YourHost",
  "DbUser": "YourUser",
  "DbPassword": "YourPassword",
  "DbName": "YourDatabase"
  "Name": "lvl_base"
}
```

# Команды для чата
- `!rank` показывает статистику: текущее звание, необходимое количество очков до следующего звания, ваш опыт, количество убийств, смертей и коэффициент убийств к смертям (KDR).
- `!top` выводит список десяти лучших игроков по очкам опыта.
- `!topkills` выводит список десяти лучших игроков по убийствам.
- `!topdeaths` выводит список десяти лучших игроков по смертям.
- `!topkdr` выводит список десяти лучших игроков по KDR.
- `toptime` показывает топ-10 игроков по времени на сервере
- `!resetstats` Cбросить свою статистику (можно использовать раз в 3 часа)
- `!ranks` показывает список всех званий и опыта, необходимого для их получения
- `!lvl` показывает список всех доступных команд и их функций
- `!tagrank` включает или выключает отображение клан-тега

# Команды для консоли
- `rp_reloadconfig` перезагружает конфигурационный файл Config.yml
- `rp_reloadranks` перезагружает конфигурационный файл settings_ranks.yaml
- `rp_reloadweapons` перезагружает конфигурационный файл Weapons.yaml 
- `rp_resetranks` очищает статистику игрока. Использование: rp_resetranks <steamid64> <data-type> (`data-type`: `exp` очистка values, rank; `stats` очистка kills, deaths, shoots, hits, headshots, assists, round_win, round_lose; `time` очистка playtime)

# Планируется
- Расширить список событий, за которые начисляется опыт.
- Исправить все предупреждения при компиляции.
- Данный список будет ещё дополнен.
