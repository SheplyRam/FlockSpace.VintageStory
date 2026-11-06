using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Common.Entities;

namespace Flock.Shared;

public static class EntityExtensions
{
    /// <summary>
    /// a <see cref="Entity.GetBehavior{T}" /> alternative which asserts the behavior. Useful in <c>if</c> statements.
    /// </summary>
    public static bool TryGetBehavior<T>(this Entity entity, [MaybeNullWhen(false)] out T behavior) where T : EntityBehavior
    {
        behavior = entity.GetBehavior<T>();
        return behavior != null;
    }
}
