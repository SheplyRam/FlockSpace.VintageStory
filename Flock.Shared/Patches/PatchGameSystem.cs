using Flock.Shared.Common;
using HarmonyLib;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Flock.Shared.Patches;

public abstract class PatchGameSystem : GameSystem
{
    public PatchGameSystem()
    {
        Harmony = new(Id);
    }

    public Harmony Harmony { get; init; }

    public static bool HasPatches(MethodBase? method, [MaybeNullWhen(false)] out HarmonyLib.Patches patches)
    {
        if (method == null)
        {
            patches = null;
            return false;
        }

        patches = Harmony.GetPatchInfo(method);
        return patches != null;
    }

    public override void Shutdown()
    {
        Harmony.UnpatchAll();
    }
}
