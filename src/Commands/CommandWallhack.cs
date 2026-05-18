using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace WallhackPluginCS2.Commands;

public class CommandWallhack
{
    public static void OnWallhackCommand(CCSPlayerController? caller, CommandInfo command)
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
            Util.ServerPrintToChat(caller, "Usage: !wh <player> | !wallhack <player>");
            return;
        }

        if (!Util.TryResolveSinglePlayer(query, out var target, out var error, includeBots: true) || target == null)
        {
            Util.ServerPrintToChat(caller, error);
            return;
        }

        if (Globals.Wallhackers.Contains(target))
        {
            Globals.Wallhackers.Remove(target);
            Util.ServerPrintToChat(caller, $"Wallhack OFF for {target.PlayerName}");
        }
        else
        {
            Globals.Wallhackers.Add(target);
            Util.ServerPrintToChat(caller, $"Wallhack ON for {target.PlayerName}");
        }
    }
}