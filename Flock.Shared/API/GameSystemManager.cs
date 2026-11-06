using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;

namespace Flock.Shared.API;

/// <summary>
/// The <see cref="GameSystem"/> registry which a <c>ModSystem</c> shell owns.
/// </summary>
public sealed class GameSystemManager(IIocManager deps) : GameSystemCollection<GameSystem>(deps);
