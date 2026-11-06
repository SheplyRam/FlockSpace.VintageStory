using Flock.Shared.DependencyInjection;
using System;
using Vintagestory.API.Common;

namespace Flock.Shared;

public static class DependenciesExtension
{
    /// <summary>
    /// Finds the game's mod system instance of <typeparamref name="T"/> and registers it as an IoC instance, so they
    /// can be injected the same way as mod-owned services.
    /// </summary>
    /// <exception cref="InvalidOperationException">When the API or the mod system is not available (yet).</exception>
    public static T RegisterModSystem<T>(this IIocManager dependencies) where T : ModSystem
    {
        if (!dependencies.TryResolve<ICoreAPI>(out var api))
            throw new InvalidOperationException(
                $"Cannot register ModSystem {nameof(T)}: {nameof(ICoreAPI)} has not been registered yet.");

        if (!api.ModLoader.TryGetModSystem<T>(out var mod))
            throw new InvalidOperationException($"Cannot register ModSystem {nameof(T)}: mod system not found");

        dependencies.RegisterInstance(mod);
        return mod;
    }
}
