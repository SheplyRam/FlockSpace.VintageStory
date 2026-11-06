using System;

namespace Flock.Shared.DependencyInjection;

/// <summary>
/// Points to a dependency field managed by <see cref="IIocManager"/>
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public sealed class IoCAttribute : Attribute;
