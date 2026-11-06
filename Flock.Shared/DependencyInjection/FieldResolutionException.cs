using System;

namespace Flock.Shared.DependencyInjection;

/// <summary>
/// The IoC failed to resolve the field required by a managed IoC resource.
/// </summary>
[Serializable]
public class FieldResolutionException(string? message, Exception? innerException) : InvalidOperationException(message, innerException);
