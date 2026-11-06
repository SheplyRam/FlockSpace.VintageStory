using Flock.Shared.Patches;
using HarmonyLib;
using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.Client.NoObf;
using Vintagestory.GameContent;

namespace Flock.Shared.Satiation.Patches;

public sealed class SatiationPatchGameSystem : PatchGameSystem
{
    public override string Id => "flockspacesharedlib:patches:satiation";

    public override void OnModStart(ref Collections.ModStartEvent evt)
    {
        try
        {
            ApplyPatchesOnce();
        }
        catch (Exception exception)
        {
            evt.Api.World.Logger.Error($"[{Id}] Failed to apply patches:\n{exception.Message}");
        }
    }

    protected override void ApplyPatches(Harmony harmony)
    {
        var guiComposerHelpersAddInvStatBarMethod = AccessTools.Method(typeof(Vintagestory.API.Client.GuiComposerHelpers), "AddInvStatbar");
        var guiComposerHelpersAddInvStatBarMethodPrefix = new HarmonyMethod(typeof(HungerStatbarPatches), nameof(HungerStatbarPatches.BeforeAddInvStatbar));
        harmony.Patch(guiComposerHelpersAddInvStatBarMethod, prefix: guiComposerHelpersAddInvStatBarMethodPrefix);

        var guiElementStatbarSetValuesMethod = AccessTools.Method(typeof(GuiElementStatbar), "SetValues");
        var guiElementStatbarSetValuesMethodPrefix = new HarmonyMethod(typeof(HungerStatbarPatches), nameof(HungerStatbarPatches.BeforeSetValues));
        harmony.Patch(guiElementStatbarSetValuesMethod, prefix: guiElementStatbarSetValuesMethodPrefix);

        var guiElementStatbarSetLineIntervalMethod = AccessTools.Method(typeof(GuiElementStatbar), "SetLineInterval");
        var guiElementStatbarSetLineIntervalMethodPrefix = new HarmonyMethod(typeof(HungerStatbarPatches), nameof(HungerStatbarPatches.BeforeSetLineInterval));
        harmony.Patch(guiElementStatbarSetLineIntervalMethod, prefix: guiElementStatbarSetLineIntervalMethodPrefix);

        var collectibleObjectTryEatStopMethod = AccessTools.Method(typeof(CollectibleObject), "tryEatStop");
        var collectibleObjectTryEatStopMethodPrefix = new HarmonyMethod(typeof(FoodEatingPatches), nameof(FoodEatingPatches.BeforeTryEatStop));
        harmony.Patch(collectibleObjectTryEatStopMethod, prefix: collectibleObjectTryEatStopMethodPrefix);

        var blockMealConsumeMethod = AccessTools.Method(typeof(BlockMeal), "Consume");
        var blockMealConsumeMethodPrefix = new HarmonyMethod(typeof(MealEatingPatches), nameof(MealEatingPatches.BeforeConsume));
        harmony.Patch(blockMealConsumeMethod, prefix: blockMealConsumeMethodPrefix);

        var entityBehaviorHungerReduceSaturation = AccessTools.Method(typeof(EntityBehaviorHunger), "ReduceSaturation");
        var entityBehaviorHungerReduceSaturationPrefix = new HarmonyMethod(typeof(HungerDrainPatches), nameof(HungerDrainPatches.BeforeReduceSaturation));
        var entityBehaviorHungerReduceSaturationPostfix = new HarmonyMethod(typeof(HungerDrainPatches), nameof(HungerDrainPatches.AfterReduceSaturation));
        harmony.Patch(entityBehaviorHungerReduceSaturation, prefix: entityBehaviorHungerReduceSaturationPrefix, postfix: entityBehaviorHungerReduceSaturationPostfix);
    }
}
