using System;
using System.Globalization;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using WallhackPluginCS2.Modules;

namespace WallhackPluginCS2.Commands;

public class CommandSpeed
{
    public static void OnSpeedCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (!Util.IsPlayerValid(caller))
            return;

        if (!AdminManager.PlayerHasPermissions(caller, Globals.Config.AdminPermission))
        {
            Util.ServerPrintToChat(caller, "You do not have permission to use this command.");
            return;
        }

        string query = command.ArgString.Trim();
        if (string.IsNullOrWhiteSpace(query))
        {
            Util.ServerPrintToChat(caller, "Usage: !speed <player>  |  !speed <multiplier> <player>");
            return;
        }

        // If first token is a number treat it as the multiplier
        float multiplier = Speed.DefaultMultiplier;
        bool explicitMultiplier = false;
        string playerQuery = query;

        var tokens = query.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (tokens.Length == 2 &&
            float.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) &&
            parsed > 0f)
        {
            multiplier = Math.Clamp(parsed, 0.1f, 10f);
            playerQuery = tokens[1];
            explicitMultiplier = true;
        }

        if (!Util.TryResolveSinglePlayer(playerQuery, out var target, out var error, includeBots: true) || target == null)
        {
            Util.ServerPrintToChat(caller, error);
            return;
        }

        if (!explicitMultiplier && Globals.SpeedPlayers.Remove(target))
        {
            // Pure toggle: had speed → remove it
            Util.ServerPrintToChat(caller, $"Speed boost OFF for {target.PlayerName}.");
            Util.ServerPrintToChat(target, "Speed boost has been removed.");
        }
        else
        {
            Globals.SpeedPlayers[target] = multiplier;
            Util.ServerPrintToChat(caller, $"Speed x{multiplier:0.##} ON for {target.PlayerName}.");
            Util.ServerPrintToChat(target, $"You now have {multiplier:0.##}x speed!");
        }
    }
}
