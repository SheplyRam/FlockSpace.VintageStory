using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Flock.Shared.Satiation.Patches;

internal static class HungerDrainPatches
{
    internal static void BeforeReduceSaturation(EntityBehaviorHunger __instance, out float __state)
    {
        __state = __instance.Saturation;
    }
    internal static void AfterReduceSaturation(EntityBehaviorHunger __instance, float __state)
    {
        if (__instance.entity.Api.Side != EnumAppSide.Server)
            return;

        float delta = __state - __instance.Saturation;
        if (delta <= 0f)
            return;

        if (!__instance.entity.TryGetBehavior<EntityBehaviorSatiation>(out var satiation))
            return;

        satiation.NotifySaturationReduced(delta);
    }
}
