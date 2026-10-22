using Flock.Shared.Collections;
using Flock.Shared.Patches;
using System;

namespace Flock.Attributes.Patches;

public sealed class AttributesPatchGameSystem : PatchGameSystem
{
    public override string Id => "flockspaceattributesmod:patches";

    public override void OnModStart(ref ModStartEvent evt)
    {
        try
        {
            Harmony.PatchAll();
        }
        catch (Exception exception)
        {
            evt.Api.World.Logger.Error("[flockspaceattributesmod] Failed to apply patches:\n{0}", exception);
        }
    }
}
