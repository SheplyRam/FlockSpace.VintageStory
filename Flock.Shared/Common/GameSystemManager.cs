using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;

namespace Flock.Shared.Common;

public sealed class GameSystemManager(IDependencyCollection deps) : GameSystemCollection<GameSystem>(deps) { }
