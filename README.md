![GitHub Repo stars](https://img.shields.io/github/stars/ABKAM2023/CS2-RanksPoints?style=for-the-badge)
![GitHub issues](https://img.shields.io/github/issues/ABKAM2023/CS2-RanksPoints?style=for-the-badge)
![GitHub contributors](https://img.shields.io/github/contributors/ABKAM2023/CS2-RanksPoints?style=for-the-badge)
![GitHub all releases](https://img.shields.io/github/downloads/ABKAM2023/CS2-RanksPoints/total?style=for-the-badge)

I'm sorry for my poor English.

# EN
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
# Configuration file for RankPointsPlugin
# Points for kill - the number of points added to the player for killing an opponent.
PointsForKill: 5

# Points deducted for death - the number of points subtracted from the player for dying.
PointsForDeath: -5

# Points for assist - the number of points added to the player for assisting in a kill.
PointsForAssist: 1

# Points for suicide - the number of points subtracted from the player for committing suicide.
PointsForSuicide: -6

# Points for headshot - additional points for killing with a headshot.
PointsForHeadshot: 1

# Points for round win - the number of points added to the player for their team's victory in a round.
PointsPerRoundWin: 2

# Points for round loss - the number of points subtracted from the player for their team's loss in a round.
PointsPerRoundLoss: -2

# Points for MVP - the number of points added to the player for receiving the MVP title of the round.
PointsPerMVP: 3

# Points for AWP no-scope kill - additional points for a kill without using the scope.
PointsForNoScopeAWP: 1

# Points for bomb defusal
PointsForBombDefusal: 2

# Points for bomb planting
PointsForBombPlanting: 2

# Message for rank up.
RankUpMessage: Your rank has been increased to {RANK_NAME}!

# Message for rank down.
RankDownMessage: Your rank has been decreased to {RANK_NAME}.

# Minimum number of players for experience calculation - players receive experience only if at least this number of players is on the server.
GetActivePlayerCountMsg: "[ {Yellow}RanksPoints {White}] At least {Red}{MIN_PLAYERS} {White}players are required to earn experience."
MinPlayersForExperience: 4

# Messages upon receiving experience
PointsChangeMessage: "[ {Yellow}RanksPoints{White} ] Your experience:{COLOR} {POINTS} [{SIGN}{CHANGE_POINTS} for {REASON}]"
# Events
SuicideMessage: "suicide"
SuicideMessageColor: "{Red}"
DeathMessage: "death"
DeathMessageColor: "{Red}"
KillMessage: "kill"
KillMessageColor: "{Green}"
NoScopeAWPMessage: "kill with AWP without a scope"
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
BombPlantingMessage: "bomb planting"
BombPlantingMessageColor: "{Green}"

# !rank
RankCommandMessage : "[ {Yellow}RanksPoints {White}] Rank: {Green}{RANK_NAME} {White}| Position: {Blue}{PLACE}/{TOTAL_PLAYERS} {White}| Experience: {Gold}{POINTS} {White}| Kills: {Green}{KILLS} {White}| Deaths: {Red}{DEATHS} {White}| KDR: {Yellow}{KDR} {White}| Time on server: {Gold}{PLAY_TIME}"
TimeFormat: "{0}d {1}h {2}min"

# !top
TopCommandIntroMessage : "[ {Blue}Top players{White} ]"
TopCommandPlayerMessage: "{INDEX}. {Grey}{NAME} - {Blue}{POINTS} points{White}"
TopCommandNoDataMessage: "[ {Red}Error{White} ] No data on top players."
TopCommandErrorMessage: "[ {Red}Error{White} ] An error occurred while executing the command."

# !topkills
TopKillsCommandIntroMessage: "[ {Green}Top players by kills{White} ]"
TopKillsCommandPlayerMessage: "{INDEX}. {Grey}{NAME} - {Green}{KILLS} kills{White}"
TopKillsCommandNoDataMessage: "[ {Red}Error{White} ] No data on top players by kills."
TopKillsCommandErrorMessage: "[ {Red}Error{White} ] An error occurred while executing the command."

# !topdeaths
TopDeathsCommandIntroMessage: "[ {Red}Top players by deaths{White} ]"
TopDeathsCommandPlayerMessage: "{INDEX}. {Grey}{NAME}{White} - {Red}{DEATHS} deaths{White}"
TopDeathsCommandNoDataMessage: "[ {Red}Error{White} ] No data on top players by deaths."
TopDeathsCommandErrorMessage: "[ {Red}Error{White} ] An error occurred while executing the command."

# !topkdr
TopKDRCommandIntroMessage: "[ {Yellow}Top players by KDR{White} ]"
TopKDRCommandPlayerMessage: "{INDEX}. {Grey}{NAME}{White} - {Yellow}KDR: {KDR}"
TopKDRCommandNoDataMessage: "[ {Red}Error{White} ] No data on top players by KDR."
TopKDRCommandErrorMessage: "[ {Red}Error{White} ] An error occurred while executing the command."

# !toptime
TopTimeCommandIntroMessage: "[ {Gold}Top players by server time{White} ]"
TopTimeCommandPlayerMessage: "{INDEX}. {Grey}{NAME} - {Gold}{TIME}{White}"
TopTimeCommandNoDataMessage : "[ {Red}Error{White} ] No data on top players by server time."
TopTimeCommandErrorMessage: "[ {Red}Error{White} ] An error occurred while executing the command."
TopTimeFormat: "{0}d {1}h {2}min"

# !resetstats
ResetStatsCooldownMessage: "[ {Red}RanksPoints {White}] You can reset your statistics only once every 3 hours."
ResetStatsSuccessMessage: "[ {Yellow}RanksPoints {White}] Your statistics have been reset."
ResetStatsCooldownHours: "3"

# !ranks
RanksCommandIntroMessage: "[ {Gold}List of ranks{White} ]"
RanksCommandRankMessage: "{NAME} - {Green}{EXPERIENCE} experience{White}"
RanksCommandNoDataMessage: "[ {Red}Error{White} ] No data on ranks."
RanksCommandErrorMessage: "[ {Red}Error{White} ] An error occurred while executing the command."

# !lvl
LvlCommandIntroMessage: "[ {Gold}List of available commands{White} ]"
RankCommandDescription: "- {Green}!rank {White}- Displays your current rank and statistics"
TopCommandDescription: "- {Green}!top {White}- Displays the top-10 players by points"
TopKillsCommandDescription: "- {Green}!topkills {White}- Displays the top-10 players by kills"
TopDeathsCommandDescription: "- {Green}!topdeaths {White}- Displays the top-10 players by deaths"
TopKDRCommandDescription: "- {Green}!topkdr {White}- Displays the top-10 players by KDR"
TopTimeCommandDescription: "- {Green}!toptime {White}- Displays the top-10 players by server time"
ResetStatsCommandDescription: "- {Green}!resetstats {White}- Reset your statistics (can be used once every 3 hours)"
RanksCommandDescription: "- {Green}!ranks {White}- Displays a list of all ranks and the experience required to obtain them"
```
# Rank Configuration (settings_ranks.yml)
```
- id: 1
  name: Silver - I
  minExperience: 0
- id: 2
  name: Silver - II
  minExperience: 10
- id: 3
  name: Silver - III
  minExperience: 25
- id: 4
  name: Silver - IV
  minExperience: 50
- id: 5
  name: Silver Elite
  minExperience: 75
- id: 6
  name: Silver - Grand Master
  minExperience: 100
- id: 7
  name: Gold Star - I
  minExperience: 150
- id: 8
  name: Gold Star - II
  minExperience: 200
- id: 9
  name: Gold Star - III
  minExperience: 300
- id: 10
  name: Gold Star - Master
  minExperience: 500
- id: 11
  name: Master Guardian - I
  minExperience: 750
- id: 12
  name: Master Guardian - II
  minExperience: 1000
- id: 13
  name: Master Guardian - Elite
  minExperience: 1500
- id: 14
  name: Distinguished Master Guardian
  minExperience: 2000
- id: 15
  name: Legendary Eagle
  minExperience: 3000
- id: 16
  name: Legendary Eagle Master
  minExperience: 5000
- id: 17
  name: Supreme Master - Highest Rank
  minExperience: 7500
- id: 18
  name: Global Elite
  minExperience: 10000
```

# Database Connection Configuration (dbconfig.json)
```
{
  "DbHost": "YourHost",
  "DbUser": "YourUser",
  "DbPassword": "YourPassword",
  "DbName": "YourDatabase"
}
```
# Chat Commands
- `!rank` shows statistics: current rank, required number of points to the next rank, your experience, number of kills, deaths, and kill-to-death ratio (KDR).
- `!top` displays a list of the top ten players by experience points.
- `!topkills` displays a list of the top ten players by kills.
- `!topdeaths` displays a list of the top ten players by deaths.
- `!topkdr` displays a list of the top ten players by KDR.
- `!toptime` shows the top ten players by time on the server.
- `!resetstats` Resets your statistics (can be used once every 3 hours).
- `!ranks` shows a list of all ranks and the experience required to achieve them.
- `!lvl` shows a list of all available commands and their functions.

# Console Commands
- `rp_reloadconfig` reloads the configuration file Config.yml.
- `rp_reloadranks` reloads the configuration file settings_ranks.yaml.
- `rp_resetranks` Clears a player's statistics. Usage: rp_resetranks <steamid64> <data-type> (`data-type`: `exp` clears values, rank; `stats` clears kills, deaths, shoots, hits, headshots, assists, round_win, round_lose; `time` clears playtime).

# Planned Features
- Expand the list of events for which experience is awarded.
- Fix all warnings during compilation.
- This list will be further expanded.

# RU
Плагин был разработан, вдохновляясь плагинами [Levels Ranks], и заимствует большую часть своих функций, а также базу данных. Это означает, что плагин RanksPoints может быть интегрирован с LrWeb или GameCMS аналогично [Levels Ranks]. В процессе разработки не было возможности полностью проверить плагин, поэтому в его работе могут проявляться ошибки. Если вы обнаружите какие-либо проблемы, сообщите об этом для их исправления.

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
# Конфигурационный файл для RankPointsPlugin
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

# Очки за установку бомбы
PointsForBombPlanting: 2

# Сообщение о повышении звания.
RankUpMessage: Ваше звание было повышено до {RANK_NAME}!

# Сообщение о понижении звания.
RankDownMessage: Ваше звание было понижено до {RANK_NAME}.

# Минимальное количество игроков для начисления опыта - игрокам начисляется опыт только если на сервере играет минимум это количество игроков.
GetActivePlayerCountMsg: "[ {Yellow}RanksPoints {White}] Необходимо минимум {Red}{MIN_PLAYERS} {White}игроков для начисления опыта."
MinPlayersForExperience: 1

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
BombPlantingMessage: "установку бомбы"
BombPlantingMessageColor: "{Green}"

# !rank
RankCommandMessage : "[ {Yellow}RanksPoints {White}] Звание: {Green}{RANK_NAME} {White}| Место: {Blue}{PLACE}/{TOTAL_PLAYERS} {White}| Опыт: {Gold}{POINTS} {White}| Убийства: {Green}{KILLS} {White}| Смерти: {Red}{DEATHS} {White}| KDR: {Yellow}{KDR} {White}| Время на сервере: {Gold}{PLAY_TIME}"
TimeFormat: "{0}д {1}ч {2}мин"

# !top
TopCommandIntroMessage : "[ {Blue}Топ игроков{White} ]"
TopCommandPlayerMessage: "{INDEX}. {Grey}{NAME} - {Blue}{POINTS} очков{White}"
TopCommandNoDataMessage: "[ {Red}Ошибка{White} ] Нет данных о топ игроках."
TopCommandErrorMessage: "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."

# !topkills
TopKillsCommandIntroMessage: "[ {Green}Топ игроков по убийствам{White} ]"
TopKillsCommandPlayerMessage: "{INDEX}. {Grey}{NAME} - {Green}{KILLS} убийств{White}"
TopKillsCommandNoDataMessage: "[ {Red}Ошибка{White} ] Нет данных о топ игроках по убийствам."
TopKillsCommandErrorMessage: "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."

# !topdeaths
TopDeathsCommandIntroMessage: "[ {Red}Топ игроков по смертям{White} ]"
TopDeathsCommandPlayerMessage: "{INDEX}. {Grey}{NAME}{White} - {Red}{DEATHS} смертей{White}"
TopDeathsCommandNoDataMessage: "[ {Red}Ошибка{White} ] Нет данных о топ игроках по смертям."
TopDeathsCommandErrorMessage: "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."

# !topkdr
TopKDRCommandIntroMessage: "[ {Yellow}Топ игроков по KDR{White} ]"
TopKDRCommandPlayerMessage: "{INDEX}. {Grey}{NAME}{White} - {Yellow}KDR: {KDR}"
TopKDRCommandNoDataMessage: "[ {Red}Ошибка{White} ] Нет данных о топ игроках по KDR."
TopKDRCommandErrorMessage: "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."

# !toptime
TopTimeCommandIntroMessage: "[ {Gold}Топ игроков по времени на сервере{White} ]"
TopTimeCommandPlayerMessage: "{INDEX}. {Grey}{NAME} - {Gold}{TIME}{White}"
TopTimeCommandNoDataMessage : "[ {Red}Ошибка{White} ] Нет данных о топ игроках по времени на сервере."
TopTimeCommandErrorMessage: "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."
TopTimeFormat: "{0}д {1}ч {2}мин"

# !resetstats
ResetStatsCooldownMessage: "[ {Red}RanksPoints {White}] Сбросить статистику можно только раз в 3 часа."
ResetStatsSuccessMessage: "[ {Yellow}RanksPoints {White}] Ваша статистика сброшена."
ResetStatsCooldownHours: "3"

# !ranks
RanksCommandIntroMessage: "[ {Gold}Список званий{White} ]"
RanksCommandRankMessage: "{NAME} - {Green}{EXPERIENCE} опыта{White}"
RanksCommandNoDataMessage: "[ {Red}Ошибка{White} ] Нет данных о званиях."
RanksCommandErrorMessage: "[ {Red}Ошибка{White} ] Произошла ошибка при выполнении команды."

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

# Конфиг подключения базы данных (dbconfig.json)
```
{
  "DbHost": "YourHost",
  "DbUser": "YourUser",
  "DbPassword": "YourPassword",
  "DbName": "YourDatabase"
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

# Команды для консоли
- `rp_reloadconfig` перезагружает конфигурационный файл Config.yml
- `rp_reloadranks` перезагружает конфигурационный файл settings_ranks.yaml
- `rp_resetranks` Очищает статистику игрока. Использование: rp_resetranks <steamid64> <data-type> (`data-type`: `exp` очистка values, rank; `stats` очистка kills, deaths, shoots, hits, headshots, assists, round_win, round_lose; `time` очистка playtime)

# Планируется
- Расширить список событий, за которые начисляется опыт.
- Исправить все предупреждения при компиляции.
- Данный список будет ещё дополнен.
