using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using WallhackPluginCS2.Models;
using WallhackPluginCS2.Modules;

namespace WallhackPluginCS2.Commands;

public class CommandInvisible
{
    public static void OnInvisibleCommand(CCSPlayerController? caller, CommandInfo command)
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
            Util.ServerPrintToChat(caller, "Usage: !invis <player> | !invisible <player>");
            return;
        }

        if (!Util.TryResolveSinglePlayer(query, out var player, out var error, includeBots: true) || player == null)
        {
            Util.ServerPrintToChat(caller, error);
            return;
        }

        bool wasInvisible = Globals.InvisiblePlayers.Remove(player);
        Invisible.RestorePlayer(player);

        if (!wasInvisible)
        {
            Globals.InvisiblePlayers[player] = new SoundData(Server.CurrentTime - 0.01f, Server.CurrentTime - 0.01f)
            {
                HackyReload = false,
                RevealUntil = 0f
            };
        }

        if (wasInvisible)
        {
            Util.ServerPrintToChat(caller, $"{player.PlayerName} is now visible.");
            Util.ServerPrintToChat(player, "You are now visible.");
        }
        else
        {
            Util.ServerPrintToChat(caller, $"{player.PlayerName} is now invisible.");
            Util.ServerPrintToChat(player, "You are now invisible!");
        }
    }
}