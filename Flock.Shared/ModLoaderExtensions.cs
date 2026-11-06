using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Common;

namespace Flock.Shared;

public static class ModLoaderExtensions
{
    /// <summary>
    /// Retrieves the game's mod system instance of <typeparamref name="T"/>, returning <c>true</c> when that system is
    /// found, otherwise when it's not.
    /// </summary>
    public static bool TryGetModSystem<T>(this IModLoader modLoader, [MaybeNullWhen(false)] out T mod) where T : ModSystem
    {
        mod = modLoader.GetModSystem<T>();
        return mod != null;
    }
}
