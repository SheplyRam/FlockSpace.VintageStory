using Flock.Shared.Collections;
using Flock.Shared.Common;
using Flock.Shared.DependencyInjection;
using Flock.Shared.Events;
using Flock.Shared.HotKeys;
using Flock.Traits.Common;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

[assembly: ModInfo("Flock Traits", "flockspacetraitsmod",
    Authors = new string[] { "sheply" },
    Description = "A mod that introduces traits and actions!",
    Version = "1.0.0")]

namespace Flock.Traits;

public sealed class FlockTraitsModSystem : ModSystem
{
    public FlockTraitsModSystem()
    {
        Dependencies = new DependencyCollection();
        Dependencies.Register<IEventBus, EventBus>();

        Systems = new GameSystemManager(Dependencies);
    }

    public GameSystemManager Systems { get; init; }

    internal IDependencyCollection Dependencies { get; init; }

    public override void StartPre(ICoreAPI api)
    {
        Dependencies.RegisterInstance(Mod);

        Systems.Register<HotKeyGameSystem>();
        Systems.Register<GameActionManagerSystem>();
        Systems.Initialize();
    }


    // Called on server and client
    // Useful for registering block/entity classes on both sides
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
