using System;
using System.Diagnostics.CodeAnalysis;

namespace Flock.Shared.DependencyInjection;

public interface IDependencyCollection
{
    void InjectDependencies(object target);

    void Register<TInterface, TImpl>() where TImpl : TInterface;

    void RegisterInstance<T>(T instance) where T : notnull;

    T Resolve<T>() where T : notnull;

    object Resolve(Type type);

    bool TryResolve<T>([MaybeNullWhen(false)] out T instance) where T : notnull;
}
