using System;
using System.Diagnostics.CodeAnalysis;

namespace Flock.Shared.DependencyInjection;

/// <summary>
/// Field-injection container.
/// </summary>
public interface IIocManager
{
    /// <summary>
    /// Fills every <see cref="IoCAttribute"/> field on <paramref name="target"/>.
    /// </summary>
    void InjectDependencies(object target);

    /// <summary>
    /// Maps <typeparamref name="TInterface"/> to a lazily parameterless constructed <typeparamref name="TImpl"/>.
    /// </summary>
    void Register<TInterface, TImpl>() where TImpl : TInterface;

    /// <summary>
    /// Serves <paramref name="instance"/> for <typeparamref name="T"/> (only the first registration is persisted).
    /// </summary>
    void RegisterInstance<T>(T instance) where T : notnull;

    /// <summary>
    /// Returns the registered or constructible instance. Throws when an interface/abstraction was never registered.
    /// </summary>
    T Resolve<T>() where T : notnull;

    /// <summary>
    /// Type-based variant of <see cref="Resolve{T}"/>.
    /// </summary>
    object Resolve(Type type);

    /// <summary>
    /// Non-throwing variant of <see cref="Resolve{T}"/>.
    /// </summary>
    bool TryResolve<T>([MaybeNullWhen(false)] out T instance) where T : notnull;
}
