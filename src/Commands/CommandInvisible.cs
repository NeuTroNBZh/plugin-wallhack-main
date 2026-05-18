using System.Drawing;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using WallhackPluginCS2.Models;

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
        RestorePlayerVisibility(player);

        if (!wasInvisible)
        {
            Globals.InvisiblePlayers[player] = new SoundData(Server.CurrentTime - 0.01f, Server.CurrentTime - 0.01f)
            {
                HackyReload = false,
                RevealUntil = 0f
            };
        }

        string status = wasInvisible ? "now visible" : "now invisible";
        Util.ServerPrintToChat(caller, $"{player.PlayerName} is {status}");
    }

    private static void RestorePlayerVisibility(CCSPlayerController player)
    {
        var pawn = player.PlayerPawn?.Value;
        if (pawn == null || !pawn.IsValid)
            return;

        pawn.Render = Color.FromArgb(255, pawn.Render);
        Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

        pawn.ShadowStrength = 1.0f;
        Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_flShadowStrength");

        if (pawn.WeaponServices == null)
            return;

        foreach (var handle in pawn.WeaponServices.MyWeapons)
        {
            var weapon = handle.Value;
            if (weapon == null || !weapon.IsValid)
                continue;

            weapon.Render = Color.FromArgb(255, weapon.Render);
            Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");

            weapon.ShadowStrength = 1.0f;
            Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_flShadowStrength");
        }
    }
}