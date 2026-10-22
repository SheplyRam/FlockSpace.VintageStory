using Flock.Shared.DependencyInjection;
using Flock.Shared.Events;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Flock.Shared.Collections;

public class GameSystemCollection<T> where T : GameSystemPart
{
    protected readonly IDependencyCollection dependencies;
    protected readonly IEventBus eventBus;
    protected readonly List<T> items;

    public GameSystemCollection(IDependencyCollection deps)
    {
        dependencies = new DependencyCollection(deps);
        eventBus = dependencies.Resolve<IEventBus>();
        items = [];
    }

    ~GameSystemCollection()
    {
        Shutdown();
    }

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

    public TItem Register<TItem>() where TItem : T, new()
    {
        dependencies.Register<TItem, TItem>();

        var item = dependencies.Resolve<TItem>();
        items.Add(item);
        return item;
    }

    public TItem Resolve<TItem>() where TItem : T, new()
    {
        return dependencies.Resolve<TItem>();
    }

    public bool TryResolve<TItem>([MaybeNullWhen(false)] out TItem instance) where TItem : T, new()
    {
        return dependencies.TryResolve(out instance);
    }
}
