using Flock.Shared;
using Flock.Shared.API;
using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;
using Flock.Shared.Events;
using Flock.Shared.HotKeys;
using Flock.Shared.Traits;
using Flock.Traits.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Flock.Traits;

/// <summary>
/// Entry-point shell of the traits gameplay mod: wires the IoC scope, registers the trait mechanics, and raises the
/// lifecycle events they subscribe to.
/// </summary>
public sealed class FlockTraitsModSystem : ModSystem
{
    /// <summary>
    /// Composition root of this mod; mechanics register here during <see cref="StartPre"/>.
    /// </summary>
    public GameSystemManager Systems { get; init; }

    /// <summary>The IoC scope the systems resolve their dependencies from.</summary>
    internal IIocManager Dependencies { get; init; }

    public FlockTraitsModSystem()
    {
        Dependencies = new IoCManager();
        Dependencies.Register<IEventBus, EventBus>();

        Systems = new GameSystemManager(Dependencies);
    }

    public override void StartPre(ICoreAPI api)
    {
        Dependencies.RegisterInstance(Mod);
        Dependencies.RegisterInstance(api);
        Dependencies.RegisterModSystem<FlockSharedTraitsModSystem>();

        Systems.Register<HotKeyGameSystem>();
        Systems.Register<GameTraitAttributeManagerSystem>();
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
