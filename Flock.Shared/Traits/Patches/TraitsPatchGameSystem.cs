using Flock.Shared.API;
using Flock.Shared.Collections;
using Flock.Shared.Patches;
using HarmonyLib;
using System;
using Vintagestory.GameContent;

namespace Flock.Shared.Traits.Patches;

/// <summary>
/// Applies the trait-merge patches to the vanilla character system once per process.
/// </summary>
public sealed class TraitsPatchGameSystem : PatchGameSystem
{
    public override string Id => "flockspacesharedlib:traits-patches";

    public override void OnModStart(ref ModStartEvent evt)
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
        var characterSystemType = typeof(CharacterSystem);

        // Both hooks run after the game's own loaders so the merge happens on the complete
        // vanilla data; the loaders re-run on world restart, which is why the merge must be
        // idempotent per CharacterSystem instance.
        var loadTraitsMethod = AccessTools.Method(characterSystemType, "LoadTraits");
        var loadTraitsPostfix = new HarmonyMethod(typeof(CharacterSystemPatches), nameof(CharacterSystemPatches.AfterLoadTraits));
        harmony.Patch(loadTraitsMethod, postfix: loadTraitsPostfix);

        var loadClassesMethod = AccessTools.Method(characterSystemType, "LoadClasses");
        var loadClassesPostfix = new HarmonyMethod(typeof(CharacterSystemPatches), nameof(CharacterSystemPatches.AfterLoadClasses));
        harmony.Patch(loadClassesMethod, postfix: loadClassesPostfix);
    }
}
