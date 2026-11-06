using Vintagestory.API.Client;

namespace Flock.Shared.Collections;

/// <summary>
/// Delivered to every part on the client once the client API is registered and raised.
/// </summary>
public record ClientSideStartEvent(ICoreClientAPI ClientApi);
