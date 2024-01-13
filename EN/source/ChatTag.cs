using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Cvars;
using System.Numerics;

namespace RanksPointsNamespace;

public partial class RanksPoints
{

    private HookResult PlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController player = @event.Userid;

        if (player == null || !player.IsValid || player.IsBot || player.IsHLTV || !player.PlayerPawn.IsValid)
            return HookResult.Continue;
        var client = player.Index;
        player.Clan = g_Player[client].ClanTag;
        return HookResult.Continue;
    }
    private HookResult OnPlayerChat(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || info.GetArg(1).Length == 0) return HookResult.Continue;
        
        if (info.GetArg(1).StartsWith("!") || info.GetArg(1).StartsWith("@") || info.GetArg(1).StartsWith("/") || info.GetArg(1).StartsWith(".") || info.GetArg(1) == "rtv") return HookResult.Continue;

        string deadIcon = !player.PawnIsAlive ? $"{ChatColors.White}☠ {ChatColors.Default}" : "";
        var client = player.Index;
        var steamID64 = player.SteamID.ToString();
        string prefix = g_Player[client].ClanTag ?? "";
        string nickColor = TeamnickColor(player.TeamNum);
        string messageColor = ChatColors.Default.ToString();

        Server.PrintToChatAll(ReplaceTags($" {deadIcon}{ChatColors.Grey}{"[ALL] - "}{ChatColors.Olive}{prefix} {nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}", player.TeamNum));

        return HookResult.Handled;

    }
    private HookResult OnPlayerChatTeam(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.IsValid || info.GetArg(1).Length == 0) return HookResult.Continue;

        if (info.GetArg(1).StartsWith("!") || info.GetArg(1).StartsWith("@") || info.GetArg(1).StartsWith("/") || info.GetArg(1).StartsWith(".") || info.GetArg(1) == "rtv") return HookResult.Continue;


        string deadIcon = !player.PawnIsAlive ? $"{ChatColors.White}☠ {ChatColors.Default}" : "";
        var client = player.Index;
        var steamID64 = player.SteamID.ToString();
        string prefix = g_Player[client].ClanTag ?? "";
        string nickColor = TeamnickColor(player.TeamNum);
        string messageColor = ChatColors.Default.ToString();

        foreach (var p in Utilities.GetPlayers().Where(p => p.TeamNum == player.TeamNum && p.IsValid && !p.IsBot))
        {
            string messageToSend = $"{deadIcon}{TeamName(player.TeamNum)} {ChatColors.Olive}{prefix} {nickColor}{player.PlayerName}{ChatColors.Default}: {messageColor}{info.GetArg(1)}";
            p.PrintToChat($" {ReplaceTags(messageToSend, p.TeamNum)}");
        }

        return HookResult.Handled;
        
    }

    private string ReplaceTags(string message, int teamNum = 0)
    {
        if (message.Contains('{'))
        {
            string modifiedValue = message;
            foreach (FieldInfo field in typeof(ChatColors).GetFields())
            {
                string pattern = $"{{{field.Name}}}";
                if (message.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                {
                    modifiedValue = modifiedValue.Replace(pattern, field.GetValue(null)!.ToString(), StringComparison.OrdinalIgnoreCase);
                }
            }
            return modifiedValue.Replace("{TEAMCOLOR}", TeamColor(teamNum));
        }

        return message;
    }

    private string TeamnickColor(int teamNum)
    {
        string nickColor = "";

        switch (teamNum)
        {
            case 0:
                nickColor = $"{ChatColors.Default}";
                break;
            case 1:
                nickColor = $"{ChatColors.Default}";
                break;
            case 2:
                nickColor = $"{ChatColors.LightYellow}";
                break;
            case 3:
                nickColor = $"{ChatColors.LightBlue}";
                break;
        }

        return nickColor;
    }
    private string TeamName(int teamNum)
    {
        string teamName = "";

        switch (teamNum)
        {
            case 0:
                teamName = $"(NONE)";
                break;
            case 1:
                teamName = $"(SPEC)";
                break;
            case 2:
                teamName = $"{ChatColors.Yellow}(T) - ";
                break;
            case 3:
                teamName = $"{ChatColors.Blue}(CT) - ";
                break;
        }

        return teamName;
    }
    private string TeamColor(int teamNum)
    {
        string teamColor;

        switch (teamNum)
        {
            case 2:
                teamColor = $"{ChatColors.Gold}";
                break;
            case 3:
                teamColor = $"{ChatColors.Blue}";
                break;
            default:
                teamColor = "";
                break;
        }

        return teamColor;
    }
}
