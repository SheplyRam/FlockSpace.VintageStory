using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Common;

namespace Flock.Shared;

public static class ModLoaderExtensions
{
    public static bool TryGetModSystem<T>(this IModLoader modLoader, [MaybeNullWhen(false)] out T mod) where T : ModSystem
    {
        mod = modLoader.GetModSystem<T>();
        return mod != null;
    }
}
