# EN
Apologies for my poor English.

The current version may not be completely stable as this is my first plugin, and it's quite possible that I missed some important details during the development process.

# RanksPoints
The RanksPoints system is based on a simple principle: players perform various actions in the game, resulting in either gaining or losing experience points. Achieving or losing a certain amount of these points leads to obtaining the corresponding rank. The number of available ranks can be configured and edited at discretion.

# Installation
1. Install [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master) and [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)

2. Download RankPoints

3. Unpack the archive and upload it to the game server

# Main Config
```
# Configuration file for RankPoints
# Points per kill - the number of points added to the player for killing an opponent.
PointsPerKill: 5
# Points deducted for death - the number of points subtracted from the player for dying.
PointsPerDeath: -5
# Points for assists - the number of points added to the player for assisting in a kill.
PointsPerAssist: 1
# Points for suicides - the number of points subtracted from the player for committing suicide.
PointsPerSuicide: -6
# Points for headshots - additional points for killing with a headshot.
PointsPerHeadshot: 1
# Points for round wins - the number of points added to the player for winning a round for their team.
PointsPerRoundWin: 2
# Points for round losses - the number of points subtracted from the player for losing a round for their team.
PointsPerRoundLoss: -2
# Points for MVP - the number of points added to the player for receiving the MVP award of the round.
PointsPerMVP: 3
# Points for no-scope kills - additional points for killing without using a scope.
PointsPerNoScope: 2

# Experience points for bots
AwardPointsForBots: False

# Minimum number of players for awarding experience - players are awarded experience only if there are at least this many players on the server.
MinPlayersForExperience: 4

# Event messages
MvpAwardMessage: "{White}[ {Red}RanksPoints {White}] Your experience: {LightYellow}{POINTS} [+{MVP_POINTS} for MVP]"
RoundWinMessage: "{White}[ {Red}RanksPoints {White}] Your experience: {Green}{POINTS} [+{ROUND_WIN_POINTS} for round win]"
RoundLossMessage: "{White}[ {Red}RanksPoints {White}] Your experience: {Red}{POINTS} [{ROUND_LOSS_POINTS} for round loss]"
SuicideMessage: "{White}[ {Red}RanksPoints {White}] Your experience: {Red}{POINTS} [{SUICIDE_POINTS} for suicide]"
NoScopeKillMessage: "{White}[ {Red}RanksPoints {White}] Your experience: {Blue}{POINTS} [+{NOSCOPE_POINTS} for no-scope kill]"
KillMessage: "{White}[ {Red}RanksPoints {White}] Your experience: {Green}{POINTS} [+{KILL_POINTS} for kill]"
HeadshotMessage: "{White}[ {Red}RanksPoints {White}] Your experience: {Yellow}{POINTS} [+{HEADSHOT_POINTS} for headshot]"
AssistMessage: "{White}[ {Red}RanksPoints {White}] Your experience: {Blue}{POINTS} [+{ASSIST_POINTS} for assist]"
DeathMessage: "{White}[ {Red}RanksPoints {White}] Your experience: {Red}{POINTS} [{DEATH_POINTS} for death]"

# Message for insufficient player count
GetActivePlayerCountMsg: "{White}[ {Red}RanksPoints {White}] At least {Red}{MIN_PLAYERS} {White}players required for gaining experience."

# !rank command messages
NoRankMessage: "{White}[ {Red}RanksPoints {White}] You don't have a rank yet."
CurrentRankMessage: "{White}[ {Red}RanksPoints {White}] Your current rank: {Yellow}{RANK_NAME}{White}."
NextRankMessage: "{White}To the next rank {Yellow}{NEXT_RANK_NAME}{White}, you need {Green}{POINTS_TO_NEXT_RANK} {White}experience points."
MaxRankMessage: "{White}Congratulations, you have achieved the {Yellow}{RANK_NAME}{White} rank!"
StatsMessage: "{White}Total experience: {Green}{POINTS}{White} Position: {Yellow}{RANK_POSITION}/{TOTAL_PLAYERS} {White}Kills: {Green}{KILLS}{White} Deaths: {Red}{DEATHS} {White}K/D Ratio: {Yellow}{KDRATIO}"

# !top command messages
TopCommandIntroMessage: "{White}[ {Red}Top Players {White}]"
TopPlayerMessage: "{White}{POSITION}. {Grey}{NICKNAME}{White} - {Green}{POINTS} points"

# !topdeaths command messages
TopDeathsIntroMessage: "{White}[ {Red}Top in Deaths {White}]"
TopDeathsPlayerMessage: "{POSITION}. {Grey}{NICKNAME}{White} - {Green}{DEATHS} deaths"

# !topkdr command messages
TopKdrIntroMessage: "{White}[ {Red}Top KDR {White}]"
TopKdrPlayerMessage: "{White}{POSITION}. {Grey}{NICKNAME}{White} - KDR: {Yellow}{KDR}"

# !topkills command messages
TopKillsIntroMessage: "{White}[ {Red}Top in Kills {White}]"
TopKillsPlayerMessage: "{White}{POSITION}. {Grey}{NICKNAME}{White} - {Green}{KILLS} kills"

# Messages for rank promotion or demotion
RankUpMessage: "Congratulations! Your new rank: {RANK}."
RankDownMessage: "Your rank has been decreased to: {RANK}."
```

# Rank Settings Config
```
# This is the rank configuration file for RankPoints
- name: Silver - I
  pointsRequired: 0
- name: Silver - II
  pointsRequired: 10
- name: Silver - III
  pointsRequired: 25
- name: Silver - IV
  pointsRequired: 50
- name: Silver Elite
  pointsRequired: 75
- name: Silver Master Elite
  pointsRequired: 100
- name: Gold Nova - I
  pointsRequired: 150
- name: Gold Nova - II
  pointsRequired: 200
- name: Gold Nova - III
  pointsRequired: 300
- name: Gold Nova Master
  pointsRequired: 500
- name: Master Guardian - I
  pointsRequired: 750
- name: Master Guardian - II
  pointsRequired: 1000
- name: Master Guardian Elite
  pointsRequired: 1500
- name: Distinguished Master Guardian
  pointsRequired: 2000
- name: Legendary Eagle
  pointsRequired: 3000
- name: Legendary Eagle Master
  pointsRequired: 5000
- name: Supreme Master First Class
  pointsRequired: 7500
- name: Global Elite
  pointsRequired: 10000
```

# Chat Commands
- `!rank` shows statistics: current rank, points needed for the next rank, your experience, number of kills, deaths, and kill/death ratio (KDR).
- `!top` displays a list of the top ten players by experience points.
- `!topkills` displays a list of the top ten players by kills.
- `!topdeaths` displays a list of the top ten players by deaths.
- `!topkdr` displays a list of the top ten players by KDR.

# Console Commands
- `!rp_reloadconfig` reloads the Config.yml configuration file.
- `!rp_reloadranks` reloads the settings_ranks.yaml configuration file.

# Planned
- Implement MySQL database support.
- Expand the list of events for which experience is awarded.
- Fix all compilation warnings.
- This list will be further expanded.

#RU
Текущая версия может быть не полностью стабильной, поскольку это мой первый плагин, и вполне возможно, что в ходе разработки я упустил некоторые важные детали

# RanksPoints
RanksPoints система базируется на простом принципе: игроки совершают разнообразные действия в игре, в результате которых они либо приобретают, либо теряют очки опыта. Достижение или потеря определенного объема этих очков ведет к получению соответствующего ранга. Количество доступных рангов может быть настроено и отредактировано по усмотрению.

# Установка
1. Установите [Metamod:Source](https://www.sourcemm.net/downloads.php/?branch=master) и [CounterStrikeSharp](https://github.com/roflmuffin/CounterStrikeSharp)

2. Скачайте RankPoints

3. Распакуйте архив и загрузите его на игровой сервер


# Основной конфиг
```
# Конфигурационный файл для RankPoints
# Очки за убийство - количество очков, добавляемое игроку за убийство противника.
PointsPerKill: 5
# Очки отнимаемые за смерть - количество очков, вычитаемое у игрока за смерть.
PointsPerDeath: -5
# Очки за помощь - количество очков, добавляемое игроку за помощь в убийстве.
PointsPerAssist: 1
# Очки за самоубийство - количество очков, вычитаемое у игрока за самоубийство.
PointsPerSuicide: -6
# Очки за выстрел в голову - дополнительные очки за убийство с выстрелом в голову.
PointsPerHeadshot: 1
# Очки за победу в раунде - количество очков, добавляемое игроку за победу его команды в раунде.
PointsPerRoundWin: 2
# Очки за проигрыш в раунде - количество очков, вычитаемое у игрока за проигрыш его команды в раунде.
PointsPerRoundLoss: -2
# Очки за MVP - количество очков, добавляемое игроку за получение звания MVP раунда.
PointsPerMVP: 3
# Очки за убийство без прицела (no-scope) - дополнительные очки за убийство без использования прицела.
PointsPerNoScope: 2

# Начисление опыта за ботов
AwardPointsForBots: False

# Минимальное количество игроков для начисления опыта - игрокам начисляется опыт только если на сервере играет минимум это количество игроков.
MinPlayersForExperience: 4

# Сообщения событий
MvpAwardMessage: "{White}[ {Red}RanksPoints {White}] Ваш опыт:{LightYellow} {POINTS} [+{MVP_POINTS} за MVP]"
RoundWinMessage: "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Green} {POINTS} [+{ROUND_WIN_POINTS} за победу в раунде]"
RoundLossMessage: "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Red} {POINTS} [{ROUND_LOSS_POINTS} за проигрыш в раунде]"
SuicideMessage: "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Red} {POINTS} [{SUICIDE_POINTS} за самоубийство]"
NoScopeKillMessage: "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Blue} {POINTS} [+{NOSCOPE_POINTS} за убийство без прицела]"
KillMessage: "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Green} {POINTS} [+{KILL_POINTS} за убийство]"
HeadshotMessage: "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Yellow} {POINTS} [+{HEADSHOT_POINTS} за выстрел в голову]"
AssistMessage: "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Blue} {POINTS} [+{ASSIST_POINTS} за помощь]"
DeathMessage: "{White}[ {Red}RanksPoints {White}] Ваш опыт:{Red} {POINTS} [{DEATH_POINTS} за смерть]"

# Сообщение, если не хватает необходимого количества игроков.
GetActivePlayerCountMsg: "{White}[ {Red}RanksPoints {White}] Необходимо минимум {Red}{MIN_PLAYERS} {White}игрока для начисления опыта."

# Сообщения команды !rank
NoRankMessage: "{White}[ {Red}RanksPoints {White}] У вас еще нет звания."
CurrentRankMessage: "{White}[ {Red}RanksPoints {White}] Ваше текущее звание: {Yellow}{RANK_NAME}{White}."
NextRankMessage: "{White}До следующего звания {Yellow}{NEXT_RANK_NAME}{White} вам необходимо {Green}{POINTS_TO_NEXT_RANK} {White}опыта."
MaxRankMessage: "{White}Поздравляем, вы достигли {Yellow}{RANK_NAME}{White}!"
StatsMessage: "{White}Всего опыта: {Green}{POINTS}{White} Позиция: {Yellow}{RANK_POSITION}/{TOTAL_PLAYERS} {White}Убийств: {Green}{KILLS}{White} Смертей: {Red}{DEATHS} {White}K/D Ratio: {Yellow}{KDRATIO}"

# Сообщения команды !top
TopCommandIntroMessage: "{White}[ {Red}Топ игроков {White}]"
TopPlayerMessage: "{White}{POSITION}. {Grey}{NICKNAME}{White} - {Green}{POINTS} очков"

# Сообщения команды !topdeaths
TopDeathsIntroMessage: "{White}[ {Red}Топ по смертям {White}]"
TopDeathsPlayerMessage: "{POSITION}. {Grey}{NICKNAME}{White} - {Green}{DEATHS} смертей"

# Сообщения команды !topkdr
TopKdrIntroMessage: "{White}[ {Red}Топ KDR {White}]"
TopKdrPlayerMessage: "{White}{POSITION}. {Grey}{NICKNAME}{White} - KDR: {Yellow}{KDR}"

# Сообщения команды !topkills
TopKillsIntroMessage: "{White}[ {Red}Топ по убийствам {White}]"
TopKillsPlayerMessage: "{White}{POSITION}. {Grey}{NICKNAME}{White} - {Green}{KILLS} убийств"

# Сообщения при повышении или понижении звания
RankUpMessage: "Поздравляем! Ваше новое звание: {RANK}."
RankDownMessage: "Ваше звание понизилось до: {RANK}."
```

# Конфиг настройки званий
```
- name: Серебро - I 
  pointsRequired: 0
- name: Серебро - II # Названия звания
  pointsRequired: 10 # Сколько нужно очков до звания
- name: Серебро - III
  pointsRequired: 25
- name: Серебро - IV
  pointsRequired: 50
- name: Серебро Элита
  pointsRequired: 75
- name: Серебро - Великий Магистр
  pointsRequired: 100
- name: Золотая Звезда - I
  pointsRequired: 150
- name: Золотая Звезда - II
  pointsRequired: 200
- name: Золотая Звезда - III
  pointsRequired: 300
- name: Золотая Звезда - Магистр
  pointsRequired: 500
- name: Магистр-хранитель - I
  pointsRequired: 750
- name: Магистр-хранитель - II
  pointsRequired: 1000
- name: Магистр-хранитель - Элита
  pointsRequired: 1500
- name: Заслуженный Магистр-хранитель
  pointsRequired: 2000
- name: Легендарный Беркут
  pointsRequired: 3000
- name: Легендарный Беркут-магистр
  pointsRequired: 5000
- name: Великий Магистр - Высшего Ранга
  pointsRequired: 7500
- name: Всемирная Элита
  pointsRequired: 10000
```

# Команды для чата
- `!rank` показывает статистику: текущее звание, необходимое количество очков до следующего звания, ваш опыт, количество убийств, смертей и коэффициент убийств к смертям (KDR).
- `!top` выводит список десяти лучших игроков по очкам опыта.
- `!topkills` выводит список десяти лучших игроков по убийствам.
- `!topdeaths` выводит список десяти лучших игроков по смертям.
- `!topkdr` выводит список десяти лучших игроков по KDR.

# Команды для консоли
- `!rp_reloadconfig` перезагружает конфигурационный файл Config.yml
- `!rp_reloadranks` перезагружает конфигурационный файл settings_ranks.yaml

# Планируется
- Реализовать поддержку базы данных MySQL.
- Расширить список событий, за которые начисляется опыт.
- Исправить все предупреждения при компиляции.
- Данный список будет ещё дополнен.
