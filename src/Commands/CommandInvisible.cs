using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
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
            Toggle(caller!, caller!);
            return;
        }

        if (!Util.TryResolveSinglePlayer(query, out var player, out var error, includeBots: true) || player == null)
        {
            Util.ServerPrintToChat(caller, error);
            return;
        }

        Toggle(caller!, player);
    }

    public static void Toggle(CCSPlayerController caller, CCSPlayerController target)
    {
        bool isSelf = caller.Slot == target.Slot;
        bool silent = SimpleAdminBridge.IsAdminSilent(caller);

        bool wasInvisible = Globals.InvisiblePlayers.Remove(target);
        if (wasInvisible)
            Invisible.RestorePlayer(target);
        else
            Globals.InvisiblePlayers.Add(target);

        if (isSelf)
        {
            Util.ServerPrintToChat(caller, wasInvisible ? "Invisible OFF." : "Invisible ON.");
        }
        else
        {
            Util.ServerPrintToChat(caller, wasInvisible
                ? $"{target.PlayerName} is now visible."
                : $"{target.PlayerName} is now invisible.");
            if (!silent)
                Util.ServerPrintToChat(target, wasInvisible ? "You are now visible." : "You are now invisible!");
        }
    }
}
