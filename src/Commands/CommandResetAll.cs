using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using WallhackPluginCS2.Modules;

namespace WallhackPluginCS2.Commands;

public class CommandResetAll
{
    public static void OnResetAllCommand(CCSPlayerController? caller, CommandInfo command)
    {
        if (!Util.IsPlayerValid(caller))
            return;

        if (!AdminManager.PlayerHasPermissions(caller, Globals.Config.AdminPermission))
        {
            Util.ServerPrintToChat(caller, "You do not have permission to use this command.");
            return;
        }

        // Restore visibility for all invisible players before clearing
        foreach (var player in Globals.InvisiblePlayers.Keys.ToList())
        {
            if (Util.IsPlayerValid(player))
                Invisible.RestorePlayer(player);
        }

        Globals.Wallhackers.Clear();
        Globals.InvisiblePlayers.Clear();
        Globals.GodPlayers.Clear();
        Globals.SpeedPlayers.Clear();
        Globals.InfiniteMoneyPlayers.Clear();

        Util.Broadcast("All player privileges have been reset.");
    }
}
