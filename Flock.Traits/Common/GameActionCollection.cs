using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;

namespace Flock.Traits.Common;

public sealed class GameActionCollection(IDependencyCollection deps) : GameSystemCollection<GameAction>(deps) { }
