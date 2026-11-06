using Flock.Shared.Collections;
using Vintagestory.API.Common;
using Privileges = Vintagestory.API.Server.Privilege;

namespace Flock.Shared.Commands;

/// <summary>
/// One game chat command as a composable part.
/// </summary>
public abstract class GameCommand : GameSystemPart
{
    /// <summary>
    /// Slash-command name as typed by players (without the leading slash).
    /// </summary>
    public abstract string Name { get; }

    public override string Id => $"flockspacesharedlib:commands:{Name}";

    /// <summary>
    /// One-line help text when listing the command.
    /// </summary>
    public abstract string Description { get; }

    /// <summary>
    /// Privilege level a caller needs (defaults to <c>controlserver</c>).
    /// </summary>
    public virtual string Privilege => Privileges.controlserver;

    /// <summary>
    /// Builds the command's argument parsers against the shared parser factory.
    /// </summary>
    public abstract ICommandArgumentParser[] Arguments(CommandArgumentParsers parsers);

    /// <summary>
    /// Runs the command and return <see cref="TextCommandResult.Error(string)"/> for user-visible failures.
    /// </summary>
    public abstract TextCommandResult Execute(TextCommandCallingArgs args);
}
