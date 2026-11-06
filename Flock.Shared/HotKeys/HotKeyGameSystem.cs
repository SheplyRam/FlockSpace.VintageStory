using Flock.Shared.API;
using System;
using Vintagestory.API.Client;

namespace Flock.Shared.HotKeys;

/// <summary>
/// Client-side system through which mods register their hotkeys with the game's input system.
/// </summary>
public sealed class HotKeyGameSystem : GameSystem
{
    public override string Id => "flockspacesharedlib:hotkeys";

    /// <summary>
    /// Registers the binding and its handler with the client input system.
    /// </summary>
    public void RegisterHotKey(string code, HotKeyEntry hotKey)
    {
        ArgumentNullException.ThrowIfNull(hotKey);

        var capi = dependencies.Resolve<ICoreClientAPI>();

        mod.Logger.Debug($"Registering new Hotkey '{code}'");

        capi.Input.RegisterHotKey(code, hotKey.Name, hotKey.Key, hotKey.Type, hotKey.AltPressed, hotKey.CtrlPressed, hotKey.ShiftPressed);
        capi.Input.SetHotKeyHandler(code, hotKey.Handler);
    }
}
