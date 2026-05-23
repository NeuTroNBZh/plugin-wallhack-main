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

        // No args → toggle speed for self at default multiplier
        if (string.IsNullOrWhiteSpace(query))
        {
            Toggle(caller!, caller!);
            return;
        }

        float multiplier = Speed.DefaultMultiplier;
        bool explicitMultiplier = false;
        string playerQuery = query;

        var tokens = query.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        if (float.TryParse(tokens[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float parsed) && parsed > 0f)
        {
            multiplier = Math.Clamp(parsed, 0.1f, 10f);
            explicitMultiplier = true;
            playerQuery = tokens.Length >= 2 ? tokens[1] : "";
        }

        // Numeric-only arg → apply to self
        if (explicitMultiplier && string.IsNullOrWhiteSpace(playerQuery))
        {
            ToggleWithMultiplier(caller!, caller!, multiplier, explicitMultiplier);
            return;
        }

        if (!Util.TryResolveSinglePlayer(playerQuery, out var target, out var error, includeBots: true) || target == null)
        {
            Util.ServerPrintToChat(caller, error);
            return;
        }

        ToggleWithMultiplier(caller!, target, multiplier, explicitMultiplier);
    }

    // Used by SimpleAdmin bridge — toggles at default multiplier.
    public static void Toggle(CCSPlayerController caller, CCSPlayerController target) =>
        ToggleWithMultiplier(caller, target, Speed.DefaultMultiplier, false);

    private static void ToggleWithMultiplier(CCSPlayerController caller, CCSPlayerController target,
        float multiplier, bool explicitMultiplier)
    {
        bool isSelf = caller.Slot == target.Slot;
        bool silent = SimpleAdminBridge.IsAdminSilent(caller);

        if (!explicitMultiplier && Globals.SpeedPlayers.Remove(target))
        {
            if (isSelf)
                Util.ServerPrintToChat(caller, "Speed boost OFF.");
            else
            {
                Util.ServerPrintToChat(caller, $"Speed boost OFF for {target.PlayerName}.");
                if (!silent) Util.ServerPrintToChat(target, "Speed boost has been removed.");
            }
        }
        else
        {
            Globals.SpeedPlayers[target] = multiplier;
            if (isSelf)
                Util.ServerPrintToChat(caller, $"Speed x{multiplier:0.##} ON.");
            else
            {
                Util.ServerPrintToChat(caller, $"Speed x{multiplier:0.##} ON for {target.PlayerName}.");
                if (!silent) Util.ServerPrintToChat(target, $"You now have {multiplier:0.##}x speed!");
            }
        }
    }
}
