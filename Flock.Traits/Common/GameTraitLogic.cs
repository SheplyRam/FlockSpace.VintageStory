using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;
using Flock.Shared.Traits;

namespace Flock.Traits.Common;

/// <summary>
/// One gameplay behaviour built out of trait attributes. Mechanics are composable parts with access to the shared
/// library's systems, so trait effects stay attribute-driven and server-authoritative.
/// </summary>
public abstract class GameTraitLogic : GameSystemPart
{
    [IoC] protected readonly FlockSharedTraitsModSystem traitsModSystem = default!;
}
