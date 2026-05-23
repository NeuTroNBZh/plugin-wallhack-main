using System.Drawing;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using WallhackPluginCS2.Commands;

namespace WallhackPluginCS2.Modules;

public class Invisible
{
    private static readonly Dictionary<CEntityInstance, CCSPlayerController> HiddenEntities = new();

    public static void OnPlayerTransmit(CCheckTransmitInfo info, CCSPlayerController viewer)
    {
        if (!Util.IsPlayerValid(viewer))
            return;

        foreach (var (entity, owner) in HiddenEntities.ToList())
        {
            if (!entity.IsValid || !Util.IsPlayerEntityValid(owner))
            {
                HiddenEntities.Remove(entity);
                continue;
            }

            if (owner.Slot != viewer.Slot)
                info.TransmitEntities.Remove(entity);
        }
    }

    public static void OnTick()
    {
        HiddenEntities.Clear();

        foreach (var owner in Globals.InvisiblePlayers.ToList())
        {
            if (!Util.IsPlayerEntityValid(owner) ||
                owner.Connected != PlayerConnectedState.PlayerConnected)
            {
                Globals.InvisiblePlayers.Remove(owner);
                continue;
            }

            if (!owner.PawnIsAlive)
                continue;

            var pawn = owner.PlayerPawn?.Value;
            if (pawn == null || !pawn.IsValid)
                continue;

            ApplyPawnVisuals(pawn, 0);
            ApplyWeaponVisuals(pawn, 0);

            pawn.EntitySpottedState.Spotted = false;
            pawn.EntitySpottedState.SpottedByMask[0] = 0;

            HiddenEntities[pawn] = owner;

            if (pawn.WeaponServices != null)
            {
                foreach (var handle in pawn.WeaponServices.MyWeapons)
                {
                    var weapon = handle.Value;
                    if (weapon == null || !weapon.IsValid)
                        continue;

                    HiddenEntities[weapon] = owner;
                }
            }
        }
    }

    public static HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerValid(player) || !Globals.InvisiblePlayers.Contains(player))
            return HookResult.Continue;

        Server.NextFrame(() =>
        {
            if (!Util.IsPlayerValid(player) || !player.PawnIsAlive)
                return;

            var pawn = player.PlayerPawn?.Value;
            if (pawn == null || !pawn.IsValid)
                return;

            ApplyPawnVisuals(pawn, 0);
            ApplyWeaponVisuals(pawn, 0);
        });

        return HookResult.Continue;
    }

    public static HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (player != null)
            Globals.InvisiblePlayers.Remove(player);

        return HookResult.Continue;
    }

    public static void RestorePlayer(CCSPlayerController player)
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

    private static void ApplyPawnVisuals(CCSPlayerPawn pawn, byte alpha)
    {
        pawn.Render = Color.FromArgb(alpha, pawn.Render);
        Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_clrRender");

        pawn.ShadowStrength = 0.0f;
        Utilities.SetStateChanged(pawn, "CBaseModelEntity", "m_flShadowStrength");
    }

    private static void ApplyWeaponVisuals(CCSPlayerPawn pawn, byte alpha)
    {
        if (pawn.WeaponServices == null)
            return;

        foreach (var handle in pawn.WeaponServices.MyWeapons)
        {
            var weapon = handle.Value;
            if (weapon == null || !weapon.IsValid)
                continue;

            weapon.Render = Color.FromArgb(alpha, weapon.Render);
            Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_clrRender");

            weapon.ShadowStrength = 0.0f;
            Utilities.SetStateChanged(weapon, "CBaseModelEntity", "m_flShadowStrength");
        }
    }

    public static void Setup()
    {
        Globals.Plugin.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn, HookMode.Post);
        Globals.Plugin.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        Globals.Plugin.AddCommand("css_invisible", "Makes a player invisible", CommandInvisible.OnInvisibleCommand);
        Globals.Plugin.AddCommand("css_invis",     "Makes a player invisible", CommandInvisible.OnInvisibleCommand);
        Globals.Plugin.AddCommand("invisible",     "Makes a player invisible", CommandInvisible.OnInvisibleCommand);
        Globals.Plugin.AddCommand("invis",         "Makes a player invisible", CommandInvisible.OnInvisibleCommand);
    }

    public static void Cleanup()
    {
        HiddenEntities.Clear();

        foreach (var player in Util.GetValidPlayers())
            RestorePlayer(player);

        Globals.InvisiblePlayers.Clear();
    }
}
