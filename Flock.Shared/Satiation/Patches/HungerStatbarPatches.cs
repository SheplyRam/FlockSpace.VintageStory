using Flock.Shared.Satiation.Client;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Flock.Shared.Satiation.Patches;

/// <summary>
/// Swaps the vanilla hunger-bar element for <see cref="CompositeSatiationStatbar"/> at the only vanilla call site of
/// <c>AddInvStatbar</c> (<c>HudStatbar.ComposeGuis</c>), and feeds the composite element directly, skipping the vanilla
/// single-color fill composition whose results it never draws. Health and oxygen bars are untouched.
/// </summary>
internal static class HungerStatbarPatches
{
    internal static bool BeforeAddInvStatbar(GuiComposer composer, ElementBounds bounds, double[] color, string key, ref GuiComposer __result)
    {
        if (composer.Api is not ICoreClientAPI capi)
            return true; // non-client surface: vanilla path

        if (composer.Composed)
            return true; // vanilla guard: never add after compose

        if (composer.GetElement(key) == null)
        {
            composer.AddInteractiveElement(new CompositeSatiationStatbar(capi, bounds, color), key);
        }

        __result = composer;
        return false;
    }

    internal static bool BeforeSetValues(GuiElementStatbar __instance, float value, float max)
    {
        if (__instance is CompositeSatiationStatbar composite)
        {
            composite.NotifySetValues(value, max);
            return false;
        }

        return true;
    }

    internal static bool BeforeSetLineInterval(GuiElementStatbar __instance, float value)
    {
        if (__instance is CompositeSatiationStatbar composite)
        {
            composite.NotifySetLineInterval(value);
            return false;
        }

        return true;
    }
}
