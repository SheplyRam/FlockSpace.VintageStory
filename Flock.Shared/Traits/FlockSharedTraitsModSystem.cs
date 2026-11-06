using Flock.Shared.API;
using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;
using Flock.Shared.Events;
using Flock.Shared.HotKeys;
using Flock.Shared.Traits.Common;
using Flock.Shared.Traits.Patches;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Flock.Shared.Traits;

/// <summary>
/// Entry-point shell of the shared traits library.
/// </summary>
public class FlockSharedTraitsModSystem : ModSystem
{
    /// <summary>
    /// The trait-domain systems this shell owns.
    /// </summary>
    public GameSystemManager Systems { get; init; }

    /// <summary>
    /// Registration surface gameplay mods add their traits through.
    /// </summary>
    public ITraitRegistryGameSystem Traits => Systems.Resolve<TraitRegistryGameSystem>();

    /// <summary>
    /// The IoC scope the systems resolve their dependencies from.
    /// </summary>
    internal IIocManager Dependencies { get; init; }

    public FlockSharedTraitsModSystem()
    {
        Dependencies = new IoCManager();
        Dependencies.Register<IEventBus, EventBus>();

        Systems = new GameSystemManager(Dependencies);
    }

    public override void StartPre(ICoreAPI api)
    {
        Dependencies.RegisterInstance(Mod);

        Systems.Register<HotKeyGameSystem>();
        Systems.Register<TraitsPatchGameSystem>();
        Systems.Register<TraitRegistryGameSystem>();
        Systems.Initialize();
    }

    public override void Start(ICoreAPI api)
    {
        Dependencies.RegisterInstance(api);

        var eventBus = Dependencies.Resolve<IEventBus>();
        var modStartEvent = new ModStartEvent(api);
        eventBus.RaiseEvent(ref modStartEvent);
    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        Dependencies.RegisterInstance(api);

        var eventBus = Dependencies.Resolve<IEventBus>();
        var serverSideStartEvent = new ServerSideStartEvent(api);
        eventBus.RaiseEvent(ref serverSideStartEvent);
    }

    public override void StartClientSide(ICoreClientAPI api)
    {
        Dependencies.RegisterInstance(api);

        var eventBus = Dependencies.Resolve<IEventBus>();
        var clientSideStartEvent = new ClientSideStartEvent(api);
        eventBus.RaiseEvent(ref clientSideStartEvent);
    }
}
