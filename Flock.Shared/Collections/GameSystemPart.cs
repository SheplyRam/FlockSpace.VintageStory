using Flock.Shared.DependencyInjection;
using Flock.Shared.Events;

namespace Flock.Shared.Collections;

/// <summary>
/// Smallest composable unit of mod behaviour. A part declares a stable <see cref="Id"/> and reacts to lifecycle events
/// instead of owning startup code.
/// </summary>
public abstract class GameSystemPart
{
    [IoC] protected readonly IIocManager dependencies = default!;

    [IoC] protected readonly IEventBus eventBus = default!;

    /// <summary>
    /// Stable unique identifier of this part.
    /// </summary>
    public abstract string Id { get; }

    /// <summary>
    /// Called once when the owning collection initializes, before any start event fires.
    /// </summary>
    public virtual void Initialize() { }

    /// <summary>
    /// Called when the owning collection shuts down.
    /// </summary>
    public virtual void Shutdown() { }

    /// <summary>
    /// Raised on both sides once the mod's common startup has registered instances and initialized systems.
    /// </summary>
    public virtual void OnModStart(ref ModStartEvent evt) { }

    /// <summary>
    /// Raised on the client when the mod's client-side startup has registered the client API.
    /// </summary>
    public virtual void OnClientStart(ref ClientSideStartEvent evt) { }

    /// <summary>
    /// Raised on the server when the mod's server-side startup has registered the server API.
    /// </summary>
    public virtual void OnServerStart(ref ServerSideStartEvent evt) { }
}
