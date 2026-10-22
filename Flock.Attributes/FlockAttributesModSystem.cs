using Flock.Attributes.Common;
using Flock.Attributes.Patches;
using Flock.Shared.Collections;
using Flock.Shared.Common;
using Flock.Shared.DependencyInjection;
using Flock.Shared.Events;
using Flock.Shared.HotKeys;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

[assembly: ModInfo("Flock Attributes", "flockspaceattributesmod",
                    Authors = new string[] { "sheply" },
                    Description = "A mod that introduces a patch system for character traits and attributes",
                    Version = "1.0.0")]

namespace Flock.Attributes;

public class FlockAttributesModSystem : ModSystem
{
    public FlockAttributesModSystem()
    {
        Dependencies = new DependencyCollection();
        Dependencies.Register<IEventBus, EventBus>();

        Systems = new GameSystemManager(Dependencies);
    }

    internal IDependencyCollection Dependencies { get; init; }

    public GameSystemManager Systems { get; init; }

    public override void StartPre(ICoreAPI api)
    {
        Dependencies.RegisterInstance(Mod);

        Systems.Register<HotKeyGameSystem>();
        Systems.Register<AttributesPatchGameSystem>();
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
