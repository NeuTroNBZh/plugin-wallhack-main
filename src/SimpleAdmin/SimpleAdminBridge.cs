using System;
using System.Reflection;
using CounterStrikeSharp.API.Core;
using Microsoft.Extensions.Logging;
using WallhackPluginCS2.Commands;

namespace WallhackPluginCS2;

// Runtime integration with CS2-SimpleAdmin (https://github.com/daffyyyy/CS2-SimpleAdmin).
// Uses reflection so no compile-time dependency on CS2-SimpleAdminApi.dll is needed.
// If SimpleAdmin is not installed, all methods are no-ops.
public static class SimpleAdminBridge
{
    private static dynamic? _api;

    public static bool Available => _api != null;

    // Call from OnAllPluginsLoaded so SimpleAdmin's capability is registered before we look it up.
    public static void TryInit()
    {
        _api = null;
        try
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                var ifaceType = asm.GetType("CS2_SimpleAdminApi.ICS2_SimpleAdminApi");
                if (ifaceType == null) continue;

                var capField = ifaceType.GetField("PluginCapability", BindingFlags.Public | BindingFlags.Static);
                var cap = capField?.GetValue(null);
                if (cap == null) continue;

                var api = cap.GetType().GetMethod("Get")?.Invoke(cap, null);
                if (api != null)
                {
                    _api = api;
                    break;
                }
            }
        }
        catch { }

        if (_api != null)
            Globals.Plugin.Logger.LogInformation("[WallhackPlugin] SimpleAdmin API found — menus will be registered.");
        else
            Globals.Plugin.Logger.LogInformation("[WallhackPlugin] SimpleAdmin not found — running standalone.");
    }

    // Returns true when the caller has SimpleAdmin silent mode active.
    public static bool IsAdminSilent(CCSPlayerController caller)
    {
        if (_api == null) return false;
        try { return (bool)_api.IsAdminSilent(caller); }
        catch { return false; }
    }

    public static void RegisterMenus()
    {
        if (_api == null) return;
        try
        {
            _api.RegisterMenuCategory("wallhackplugin", "Wallhack Plugin", Globals.Config.AdminPermission);

            if (Globals.Config.WallhackEnabled)
                RegisterPlayerMenu("wh_menu", "Wallhack", "css_wh",
                    (a, t) => CommandWallhack.Toggle(a, t));

            if (Globals.Config.InvisibleEnabled)
                RegisterPlayerMenu("invis_menu", "Invisible", "css_invis",
                    (a, t) => CommandInvisible.Toggle(a, t));

            RegisterPlayerMenu("god_menu", "God Mode", "css_god",
                (a, t) => CommandGod.Toggle(a, t));

            RegisterPlayerMenu("speed_menu", "Speed Boost", "css_speed",
                (a, t) => CommandSpeed.Toggle(a, t));

            RegisterHpMenu();
        }
        catch (Exception ex)
        {
            Globals.Plugin.Logger.LogWarning(ex, "[WallhackPlugin] Error registering SimpleAdmin menus");
        }
    }

    private static void RegisterPlayerMenu(string menuId, string menuName, string? cmd,
        Action<CCSPlayerController, CCSPlayerController> action)
    {
        try
        {
            _api!.RegisterMenu("wallhackplugin", menuId, menuName,
                (Func<CCSPlayerController, object>)(admin =>
                    (object)_api.CreateMenuWithPlayers(menuName, "wallhackplugin", admin,
                        (Func<CCSPlayerController, bool>)(p => Util.IsPlayerValid(p)),
                        (Action<CCSPlayerController, CCSPlayerController>)((a, t) => action(a, t)))),
                Globals.Config.AdminPermission, cmd);
        }
        catch (Exception ex)
        {
            Globals.Plugin.Logger.LogWarning(ex, "[WallhackPlugin] Error registering menu {MenuId}", menuId);
        }
    }

    private static void RegisterHpMenu()
    {
        try
        {
            _api!.RegisterMenu("wallhackplugin", "hp_menu", "Set HP",
                (Func<CCSPlayerController, object>)(admin =>
                {
                    object menu = (object)_api.CreateMenuWithBack("Set HP", "wallhackplugin", admin);
                    foreach (int hp in new[] { 100, 150, 200, 250, 300 })
                    {
                        int h = hp;
                        _api.AddSubMenu(menu, $"{h} HP",
                            (Func<CCSPlayerController, object>)(a =>
                                (object)_api.CreateMenuWithPlayers($"{h} HP", "wallhackplugin", a,
                                    (Func<CCSPlayerController, bool>)(p => Util.IsPlayerValid(p) && p.PawnIsAlive),
                                    (Action<CCSPlayerController, CCSPlayerController>)((adm, tgt) =>
                                        CommandHp.SetHp(adm, tgt, h)))));
                    }
                    return menu;
                }),
                Globals.Config.AdminPermission, "css_hp");
        }
        catch (Exception ex)
        {
            Globals.Plugin.Logger.LogWarning(ex, "[WallhackPlugin] Error registering HP menu");
        }
    }

    public static void UnregisterMenus()
    {
        if (_api == null) return;
        try
        {
            foreach (var id in new[] { "wh_menu", "invis_menu", "god_menu", "speed_menu", "hp_menu" })
            {
                try { _api.UnregisterMenu("wallhackplugin", id); }
                catch { }
            }
        }
        catch { }
    }
}
