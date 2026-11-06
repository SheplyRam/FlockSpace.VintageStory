using Flock.Shared.API;
using Flock.Shared.Collections;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace Flock.Shared.Patches;

/// <summary>
/// For systems that apply Harmony patches. Gives each patch set a stable <see cref="GameSystemPart.Id"/> and keeps
/// process-wide apply/unpatch bookkeeping.
/// </summary>
public abstract class PatchGameSystem : GameSystem
{
    private static readonly HashSet<string> AppliedPatchIds = [];

    /// <summary>
    /// The Harmony instance carrying this system's patch IDs.
    /// </summary>
    public Harmony Harmony { get; init; }

    /// <summary>
    /// Creates the Harmony instance named after this system's <see cref="GameSystemPart.Id"/>.
    /// </summary>
    public PatchGameSystem()
    {
        Harmony = new(Id);
    }

    public override void Shutdown()
    {
        Harmony.UnpatchAll();
        AppliedPatchIds.Remove(Id);
    }

    /// <summary>
    /// Patch systems apply their patches on <see cref="OnModStart"/>.
    /// </summary>
    public override sealed void OnClientStart(ref ClientSideStartEvent evt) { }

    /// <summary>
    /// Patch systems apply their patches on <see cref="OnModStart"/>.
    /// </summary>
    public override sealed void OnServerStart(ref ServerSideStartEvent evt) { }

    /// <summary>
    /// Applies this system's patches once per process unless unpatch intervened.
    /// </summary>
    /// <returns>
    /// <c>true</c> when patches were applied now; <c>false</c> when already active.
    /// </returns>
    protected bool ApplyPatchesOnce()
    {
        if (!AppliedPatchIds.Add(Id))
            return false;

        try
        {
            ApplyPatches(Harmony);
            return true;
        }
        catch
        {
            AppliedPatchIds.Remove(Id);
            throw new Exception($"Couldn't apply patches once for {Id}");
        }
    }

    /// <summary>
    /// Registers exactly the patches this system owns.
    /// </summary>
    protected abstract void ApplyPatches(Harmony harmony);
}
