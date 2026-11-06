using Flock.Shared.API;
using Flock.Shared.Collections;
using Vintagestory.API.Server;

namespace Flock.Shared.Commands;

/// <summary>
/// Composes its concrete <see cref="GameCommand"/> parts and registers them with the game's command registry once.
/// </summary>
public abstract class GameCommandManagerSystem : GameSystem
{
    private GameCommandCollection commands = default!;

    public override void Initialize()
    {
        commands = new GameCommandCollection(dependencies);
        RegisterCommands();
        commands.Initialize();
    }

    /// <summary>
    /// Internally called to register concrete command parts.
    /// </summary>
    protected abstract void RegisterCommands();

    /// <summary>
    /// Adds a command part to this manager's collection.
    /// </summary>
    protected void Register<TCommand>() where TCommand : GameCommand, new()
    {
        commands.Register<TCommand>();
    }

    public override void OnServerStart(ref ServerSideStartEvent evt)
    {
        var api = evt.ServerApi;

        foreach (var command in commands)
        {
            mod.Logger.Debug($"Registering command /{command.Name}");

            api.ChatCommands.Create(command.Name)
                .WithDescription(command.Description)
                .RequiresPrivilege(command.Privilege)
                .WithArgs(command.Arguments(api.ChatCommands.Parsers))
                .HandleWith(command.Execute);
        }
    }

    public override void Shutdown()
    {
        commands.Shutdown();
    }
}
