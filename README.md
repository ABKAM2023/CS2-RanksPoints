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

# Сообщения при повышении звания
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

# Команды
- `!rank` показывает статистику: текущее звание, необходимое количество очков до следующего звания, ваш опыт, количество убийств, смертей и коэффициент убийств к смертям (KDR).
- `!top` выводит список десяти лучших игроков по очкам опыта.

# Планируется
- Реализовать поддержку базы данных MySQL.
- Создать топ игроков по коэффициенту убийств к смертям (KDR).
- Создать топ игроков по количеству убийств.
- Создать топ игроков по количеству смертей.
- Расширить список событий, за которые начисляется опыт.
- Возможность менять сообщения в чате через конфиг.
- Данный список будет ещё дополнен.
