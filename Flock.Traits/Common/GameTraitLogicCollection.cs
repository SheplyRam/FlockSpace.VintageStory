using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;

namespace Flock.Traits.Common;

/// <summary>
/// The <see cref="GameTraitLogic"/> parts of the traits mod.
/// </summary>
public sealed class GameTraitLogicCollection(IIocManager deps) : GameSystemCollection<GameTraitLogic>(deps) { }
