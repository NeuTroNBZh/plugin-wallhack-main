using System.Drawing;
using System.Linq;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using WallhackPluginCS2.Commands;
using WallhackPluginCS2.Models;

namespace WallhackPluginCS2.Modules;

public class Invisible
{
    private const int GaugeSegments = 20;
    private const float RevealWindow = 2.0f;

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
        if (Globals.Config.InvisibleRevealOnMove)
            RevealTick();
        else
            PermanentTick();
    }

    private static void PermanentTick()
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

    private static void RevealTick()
    {
        HiddenEntities.Clear();

        foreach (var tracked in Globals.InvisibleReveal.Keys.ToList())
        {
            if (!Globals.InvisiblePlayers.Contains(tracked) || !Util.IsPlayerEntityValid(tracked))
                Globals.InvisibleReveal.Remove(tracked);
        }

        foreach (var owner in Globals.InvisiblePlayers.ToList())
        {
            if (!Util.IsPlayerEntityValid(owner) ||
                owner.Connected != PlayerConnectedState.PlayerConnected)
            {
                Globals.InvisiblePlayers.Remove(owner);
                Globals.InvisibleReveal.Remove(owner);
                continue;
            }

            if (!owner.PawnIsAlive)
                continue;

            var pawn = owner.PlayerPawn?.Value;
            if (pawn == null || !pawn.IsValid)
                continue;

            if (!Globals.InvisibleReveal.TryGetValue(owner, out var data))
                data = new SoundData();

            HandleReloadReveal(pawn, ref data);

            float alpha = GetAlpha(data);
            byte alphaByte = (byte)Math.Clamp((int)alpha, 0, 255);

            int progress = (int)Util.Map(alpha, 0f, 255f, 0f, GaugeSegments);
            owner.PrintToCenterHtml(
                string.Concat(Enumerable.Repeat("&#9608;", progress)) +
                string.Concat(Enumerable.Repeat("&#9617;", GaugeSegments - progress))
            );

            ApplyPawnVisuals(pawn, alphaByte);
            ApplyWeaponVisuals(pawn, alphaByte);

            pawn.EntitySpottedState.Spotted = false;
            pawn.EntitySpottedState.SpottedByMask[0] = 0;

            bool fullyHiddenFromOthers = Server.CurrentTime > data.RevealUntil;
            if (fullyHiddenFromOthers)
            {
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

            Globals.InvisibleReveal[owner] = data;
        }
    }

    public static HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerValid(player) || !Globals.InvisiblePlayers.Contains(player))
            return HookResult.Continue;

        if (Globals.Config.InvisibleRevealOnMove)
            ResetInvisibleState(player!);

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
        {
            Globals.InvisiblePlayers.Remove(player);
            Globals.InvisibleReveal.Remove(player);
        }

        return HookResult.Continue;
    }

    public static HookResult OnPlayerSound(EventPlayerSound @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerValid(player))
            return HookResult.Continue;

        SetPlayerInvisibleFor(player, @event.Duration * 2f);
        ExtendRevealUntil(player);
        return HookResult.Continue;
    }

    public static HookResult OnPlayerShoot(EventBulletImpact @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerValid(player))
            return HookResult.Continue;

        SetPlayerInvisibleFor(player, 0.5f);
        ExtendRevealUntil(player);
        return HookResult.Continue;
    }

    public static HookResult OnPlayerHurt(EventPlayerHurt @event, GameEventInfo info)
    {
        var player = @event.Userid;
        if (!Util.IsPlayerValid(player))
            return HookResult.Continue;

        SetPlayerInvisibleFor(player, 0.5f);
        ExtendRevealUntil(player);
        return HookResult.Continue;
    }

    public static HookResult OnPlayerStartPlant(EventBombBeginplant @event, GameEventInfo info)
    {
        SetPlayerInvisibleFor(@event.Userid, 1f);
        return HookResult.Continue;
    }

    public static HookResult OnPlayerStartDefuse(EventBombBegindefuse @event, GameEventInfo info)
    {
        SetPlayerInvisibleFor(@event.Userid, 1f);
        return HookResult.Continue;
    }

    public static void RestorePlayer(CCSPlayerController player)
    {
        Globals.InvisibleReveal.Remove(player);

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

    private static void ResetInvisibleState(CCSPlayerController player)
    {
        SoundData data = Globals.InvisibleReveal.TryGetValue(player, out var existing)
            ? existing
            : new SoundData();

        data.StartTime = Server.CurrentTime - 0.01f;
        data.EndTime = Server.CurrentTime - 0.01f;
        data.HackyReload = false;
        data.RevealUntil = 0f;

        Globals.InvisibleReveal[player] = data;
    }

    private static void HandleReloadReveal(CCSPlayerPawn pawn, ref SoundData data)
    {
        var weaponServices = pawn.WeaponServices;
        if (weaponServices == null)
        {
            data.HackyReload = false;
            return;
        }

        var activeWeaponHandle = weaponServices.ActiveWeapon;
        if (!activeWeaponHandle.IsValid)
        {
            data.HackyReload = false;
            return;
        }

        var activeWeapon = activeWeaponHandle.Get()?.As<CCSWeaponBase>();
        if (activeWeapon == null || !activeWeapon.IsValid)
        {
            data.HackyReload = false;
            return;
        }

        if (!activeWeapon.InReload)
        {
            data.HackyReload = false;
            return;
        }

        if (data.HackyReload)
            return;

        var vData = activeWeapon.VData;
        if (vData == null)
            return;

        data.HackyReload = true;
        data.StartTime = Server.CurrentTime;
        data.EndTime = Server.CurrentTime + vData.DisallowAttackAfterReloadStartDuration;
        data.RevealUntil = Math.Max(data.RevealUntil, Server.CurrentTime + RevealWindow);
    }

    private static float GetAlpha(SoundData data)
    {
        if (data.EndTime <= Server.CurrentTime)
            return 0f;

        float half = data.StartTime + ((data.EndTime - data.StartTime) / 2f);
        if (Server.CurrentTime <= half)
            return 255f;

        return Util.Map(Server.CurrentTime, half, data.EndTime, 255f, 0f);
    }

    private static void SetPlayerInvisibleFor(CCSPlayerController? player, float time)
    {
        if (!Util.IsPlayerValid(player) || player == null)
            return;

        if (!Globals.InvisiblePlayers.Contains(player))
            return;

        if (!Globals.InvisibleReveal.TryGetValue(player, out var data))
            data = new SoundData();

        data.StartTime = Server.CurrentTime;
        data.EndTime = Server.CurrentTime + time;
        Globals.InvisibleReveal[player] = data;
    }

    private static void ExtendRevealUntil(CCSPlayerController player)
    {
        if (!Globals.InvisiblePlayers.Contains(player))
            return;

        if (!Globals.InvisibleReveal.TryGetValue(player, out var data))
            data = new SoundData();

        data.RevealUntil = Server.CurrentTime + RevealWindow;
        Globals.InvisibleReveal[player] = data;
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

        if (Globals.Config.InvisibleRevealOnMove)
        {
            Globals.Plugin.RegisterEventHandler<EventPlayerSound>(OnPlayerSound);
            Globals.Plugin.RegisterEventHandler<EventBulletImpact>(OnPlayerShoot);
            Globals.Plugin.RegisterEventHandler<EventPlayerHurt>(OnPlayerHurt);
            Globals.Plugin.RegisterEventHandler<EventBombBeginplant>(OnPlayerStartPlant);
            Globals.Plugin.RegisterEventHandler<EventBombBegindefuse>(OnPlayerStartDefuse);
        }

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
        Globals.InvisibleReveal.Clear();
    }
}
