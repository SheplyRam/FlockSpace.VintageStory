using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Flock.Shared.DependencyInjection;

public sealed class DependencyCollection : IDependencyCollection
{
    private readonly DependencyCollection? _ascendingDependencies;

    public DependencyCollection()
    {
        _ascendingDependencies = null;
    }

    public DependencyCollection(IDependencyCollection ascendingDependencies)
    {
        if (ascendingDependencies is not DependencyCollection) throw new ArgumentException($"Provided dependency collection is not ours: '{nameof(ascendingDependencies)}'");

        _ascendingDependencies = (DependencyCollection)ascendingDependencies;

        // Always refer to itself to scope which dependencies other systems have.
        RegisterInstance<IDependencyCollection>(this);
    }

    internal Dictionary<Type, Func<object>> Providers { get; } = [];

    public void InjectDependencies(object target)
    {
        ArgumentNullException.ThrowIfNull(target);

        for (var type = target.GetType(); type != null; type = type.BaseType)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
            foreach (var field in type.GetFields(flags))
            {
                if (field.GetCustomAttribute<DependencyAttribute>() == null) continue;

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

        if (_ascendingDependencies != null && _ascendingDependencies.HasProvider(type))
            return _ascendingDependencies.Resolve(type);

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
        catch
        {
            instance = default;
            return false;
        }
    }

    internal bool HasProvider(Type type)
    {
        return Providers.ContainsKey(type) || (_ascendingDependencies != null && _ascendingDependencies.HasProvider(type));
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
            throw new InvalidOperationException($"Failed to inject field '{field.DeclaringType?.Name}.{field.Name}' of type '{field.FieldType}': {e.Message}", e);
        }
    }
}
