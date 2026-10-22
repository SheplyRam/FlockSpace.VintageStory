using Flock.Shared.DependencyInjection;
using Flock.Shared.Events;

namespace Flock.Shared.Collections;

public abstract class GameSystemPart
{
    [Dependency] protected readonly IDependencyCollection dependencies = default!;
    [Dependency] protected readonly IEventBus eventBus = default!;

    public abstract string Id { get; }

    public virtual void Initialize() { }

    public virtual void Shutdown() { }

    public virtual void OnModStart(ref ModStartEvent evt) { }

    public virtual void OnClientStart(ref ClientSideStartEvent evt) { }

    public virtual void OnServerStart(ref ServerSideStartEvent evt) { }
}
