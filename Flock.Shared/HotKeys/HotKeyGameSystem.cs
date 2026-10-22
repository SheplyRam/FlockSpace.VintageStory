using Flock.Shared.Common;
using System;
using Vintagestory.API.Client;

namespace Flock.Shared.HotKeys;

public sealed class HotKeyGameSystem : GameSystem
{
    public override string Id => "flockspacesharedlib:hotkeys";

    public void RegisterHotKey(string code, HotKeyEntry hotKey)
    {
        ArgumentNullException.ThrowIfNull(hotKey);

        var capi = dependencies.Resolve<ICoreClientAPI>();

        mod.Logger.Debug($"Registering new Hotkey '{code}'");

        capi.Input.RegisterHotKey(code, hotKey.Name, hotKey.Key, hotKey.Type, hotKey.AltPressed, hotKey.CtrlPressed, hotKey.ShiftPressed);
        capi.Input.SetHotKeyHandler(code, hotKey.Handler);
    }
}
