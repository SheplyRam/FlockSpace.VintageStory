using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Flock.Shared.DependencyInjection;

/// <summary>
/// Inversion of Control Manager, a simple yet powerful instrument to inject dependencies.
/// </summary>
public sealed class IoCManager : IIocManager
{
    private readonly IoCManager? ascendingScope;

    private Dictionary<Type, Func<object>> Providers { get; } = [];

    /// <summary>
    /// Creates a root scope with no parent to ascend to.
    /// </summary>
    public IoCManager()
    {
        ascendingScope = null;
    }

    /// <summary>
    /// Creates a child scope that falls back to <paramref name="ascendingDependencies"/> for types it does not register
    /// itself.
    /// </summary>
    public IoCManager(IIocManager ascendingDependencies)
    {
        if (ascendingDependencies is not IoCManager deps)
            throw new ArgumentException($"Provided dependency collection is not ours: '{nameof(ascendingDependencies)}'");

        ascendingScope = deps;

        // Registering itself lets injected parts reach their own scope, mirroring how the parent exposes itself to the
        // mod systems.
        RegisterInstance<IIocManager>(this);
    }

    public void InjectDependencies(object target)
    {
        ArgumentNullException.ThrowIfNull(target);

        for (var type = target.GetType(); type != null; type = type.BaseType)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            foreach (var field in type.GetFields(flags))
            {
                if (field.GetCustomAttribute<IoCAttribute>() == null) continue;

                field.SetValue(target, ResolveForField(field));
            }
        }
    }

    public void Register<TInterface, TImpl>() where TImpl : TInterface
    {
        object? instance = null;

        Providers.TryAdd(typeof(TInterface), () => instance ??= Construct(typeof(TImpl)));
    }

    public void RegisterInstance<T>(T instance) where T : notnull
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (HasProvider(typeof(T)))
            return;

        Providers.TryAdd(typeof(T), () => instance);
    }

    public T Resolve<T>() where T : notnull
    {
        return (T)Resolve(typeof(T));
    }

    public object Resolve(Type type)
    {
        if (Providers.TryGetValue(type, out var provider))
            return provider();

        if (ascendingScope != null && ascendingScope.HasProvider(type))
            return ascendingScope.Resolve(type);

        if (type.IsInterface || type.IsAbstract)
            throw new InvalidOperationException($"Couldn't register for type '{type}'. Register before resolving it.");

        return Construct(type);
    }

    public bool TryResolve<T>([MaybeNullWhen(false)] out T instance) where T : notnull
    {
        try
        {
            instance = Resolve<T>();
            return true;
        }
        catch (InvalidOperationException e)
        {
            // FieldResolveException wraps the real failure in an InnerException; only a bare "not registered"
            // InvalidOperationException means "absent", so re-throw wraps.
            if (e.InnerException != null)
                throw;
            instance = default;
            return false;
        }
    }

    internal bool HasProvider(Type type)
    {
        return Providers.ContainsKey(type) || (ascendingScope != null && ascendingScope.HasProvider(type));
    }

    private object Construct(Type type)
    {
        var ctor = type.GetConstructor(Type.EmptyTypes) ?? throw new InvalidOperationException("Invalid constructor");

        var instance = ctor.Invoke(null);
        InjectDependencies(instance);

        return instance;
    }

    private object ResolveForField(FieldInfo field)
    {
        try
        {
            return Resolve(field.FieldType);
        }
        catch (InvalidOperationException e)
        {
            throw new FieldResolutionException(
                $"Failed to inject field '{field.DeclaringType?.Name}.{field.Name}' of type '{field.FieldType}': {e.Message}",
                e);
        }
    }
}
