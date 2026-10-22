using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;
using Vintagestory.API.Common;

namespace Flock.Shared.Common;

public abstract class GameSystem : GameSystemPart
{
    [Dependency] protected readonly Mod mod = default!;
}
