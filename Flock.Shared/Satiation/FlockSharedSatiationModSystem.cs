using Flock.Shared.API;
using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;
using Flock.Shared.Events;
using Flock.Shared.Satiation.Client;
using Flock.Shared.Satiation.Commands;
using Flock.Shared.Satiation.Common;
using Flock.Shared.Satiation.Patches;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace Flock.Shared.Satiation;

/// <summary>
/// Entry-point shell of the satiation library: wires the IoC scope, registers the satiation systems, and raises the
/// lifecycle events they subscribe to.
/// </summary>
public class FlockSharedSatiationModSystem : ModSystem
{
    /// <summary>
    /// The satiation systems this shell owns.
    /// </summary>
    public GameSystemManager Systems { get; init; }

    /// <summary>
    /// The IoC scope the systems resolve their dependencies from.
    /// </summary>
    internal IIocManager Dependencies { get; init; }

    public FlockSharedSatiationModSystem()
    {
        Dependencies = new IoCManager();
        Dependencies.Register<IEventBus, EventBus>();

        Systems = new GameSystemManager(Dependencies);
    }

    public override void StartPre(ICoreAPI api)
    {
        Dependencies.RegisterInstance(Mod);

        Systems.Register<SatiationPatchGameSystem>();
        Systems.Register<SatiationCommandManagerSystem>();
        Systems.Initialize();
    }

    public override void Start(ICoreAPI api)
    {
        Dependencies.RegisterInstance(api);

        api.RegisterEntityBehaviorClass(SatiationCodes.BehaviorCode, typeof(EntityBehaviorSatiation));

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
