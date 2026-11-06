using Flock.Shared.DependencyInjection;
using Flock.Shared.Events;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Flock.Shared.Collections;

/// <summary>
/// Owns the <typeparamref name="T"/> parts of one mod integration.
/// </summary>
/// <remarks>
/// Parts must be registered with <see cref="Register{TItem}"/> before <see cref="Initialize"/>.
/// </remarks>
public class GameSystemCollection<T> : IEnumerable<T> where T : GameSystemPart
{
    protected readonly IIocManager dependencies;
    protected readonly IEventBus eventBus;
    protected readonly List<T> items;

    public GameSystemCollection(IIocManager deps)
    {
        dependencies = new IoCManager(deps);
        eventBus = dependencies.Resolve<IEventBus>();
        items = [];
    }

    ~GameSystemCollection()
    {
        Shutdown();
    }

    /// <summary>
    /// Subscribes every registered part to the mod lifecycle events, then initializes them in registration order.
    /// </summary>
    public void Initialize()
    {
        foreach (var item in items)
        {
            eventBus.SubscribeEventOnce<ModStartEvent>(item.OnModStart);
            eventBus.SubscribeEventOnce<ClientSideStartEvent>(item.OnClientStart);
            eventBus.SubscribeEventOnce<ServerSideStartEvent>(item.OnServerStart);
            item.Initialize();
        }
    }

    /// <summary>
    /// Shuts parts down in reverse registration order and unsubscribes their events.
    /// </summary>
    public void Shutdown()
    {
        for (var i = items.Count - 1; i >= 0; i--)
        {
            var item = items[i];
            eventBus.UnsubscribeEvent<ServerSideStartEvent>(item.OnServerStart);
            eventBus.UnsubscribeEvent<ClientSideStartEvent>(item.OnClientStart);
            eventBus.UnsubscribeEvent<ModStartEvent>(item.OnModStart);
            item.Shutdown();
        }
    }

    /// <summary>
    /// Registers and instantiates a new part.
    /// </summary>
    public TItem Register<TItem>() where TItem : T, new()
    {
        dependencies.Register<TItem, TItem>();

        var item = dependencies.Resolve<TItem>();
        items.Add(item);
        return item;
    }

    /// <summary>
    /// Resolves an already-registered part by type.
    /// </summary>
    public TItem Resolve<TItem>() where TItem : T, new()
    {
        return dependencies.Resolve<TItem>();
    }

    /// <summary>
    /// Non-throwing variant of <see cref="Resolve{TItem}"/> for optional parts.
    /// </summary>
    public bool TryResolve<TItem>([MaybeNullWhen(false)] out TItem instance) where TItem : T, new()
    {
        return dependencies.TryResolve(out instance);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
