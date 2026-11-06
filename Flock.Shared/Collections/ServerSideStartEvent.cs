using Vintagestory.API.Server;

namespace Flock.Shared.Collections;

/// <summary>
/// Delivered to every part on the server once the server API is registered and raised.
/// </summary>
public record ServerSideStartEvent(ICoreServerAPI ServerApi);
