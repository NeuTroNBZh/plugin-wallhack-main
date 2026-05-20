using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace WallhackPluginCS2.Commands;

public class CommandSlay
{
    public static void OnSlayCommand(CCSPlayerController? caller, CommandInfo command)
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
            Util.ServerPrintToChat(caller, "Usage: !slay <player>");
            return;
        }

        if (!Util.TryResolveSinglePlayer(query, out var target, out var error, includeBots: true) || target == null)
        {
            Util.ServerPrintToChat(caller, error);
            return;
        }

        if (!target.PawnIsAlive)
        {
            Util.ServerPrintToChat(caller, $"{target.PlayerName} is already dead.");
            return;
        }

        target.PlayerPawn?.Value?.CommitSuicide(false, true);
        Util.ServerPrintToChat(caller, $"Slayed {target.PlayerName}.");
    }
}
