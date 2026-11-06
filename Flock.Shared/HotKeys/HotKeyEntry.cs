using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Flock.Shared.HotKeys;

/// <summary>
/// One hotkey registration, with parameters expected when registering for a hotkey.
/// </summary>
public record HotKeyEntry(
    string Name,
    ActionConsumable<KeyCombination> Handler,
    GlKeys Key,
    HotkeyType Type = HotkeyType.CharacterControls,
    bool AltPressed = false,
    bool CtrlPressed = false,
    bool ShiftPressed = false);
