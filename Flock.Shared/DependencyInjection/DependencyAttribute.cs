using System;

namespace Flock.Shared.DependencyInjection;

[AttributeUsage(AttributeTargets.Field)]
public sealed class DependencyAttribute : Attribute;