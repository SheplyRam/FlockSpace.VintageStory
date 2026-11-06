using Vintagestory.API.Common;

namespace Flock.Shared.Collections;

/// <summary>
/// Delivered to every part after the mod's common startup registered instances and initialized systems.
/// </summary>
public record ModStartEvent(ICoreAPI Api);
