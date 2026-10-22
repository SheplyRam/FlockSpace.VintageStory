using Flock.Shared.Collections;
using Vintagestory.API.Client;

namespace Flock.Traits.Common;

public abstract class GameAction : GameSystemPart
{
    public abstract GlKeys HotKey { get; }
}
