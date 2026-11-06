using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;
using Vintagestory.API.Common;

namespace Flock.Shared.API;

/// <summary>
/// A <see cref="GameSystemPart"/>, which belongs to one mod and can reach other resources through the <c>ModSystem</c>.
/// </summary>
public abstract class GameSystem : GameSystemPart
{
    [IoC] protected readonly Mod mod = default!;
}
