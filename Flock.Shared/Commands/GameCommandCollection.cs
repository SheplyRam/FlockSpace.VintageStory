using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;

namespace Flock.Shared.Commands;

/// <summary>
/// The <see cref="GameCommand"/> parts one manager system owns.
/// </summary>
public sealed class GameCommandCollection(IIocManager deps) : GameSystemCollection<GameCommand>(deps);
