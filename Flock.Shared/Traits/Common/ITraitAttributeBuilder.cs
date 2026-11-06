namespace Flock.Shared.Traits.Common;

/// <summary>
/// Fluent builder for composing one trait's attributes, blend types, and class bindings.
/// </summary>
public interface ITraitAttributeBuilder
{
    /// <summary>
    /// Grants the trait an attribute value; repeated calls for the same stat accumulate.
    /// </summary>
    ITraitAttributeBuilder WithAttribute(string statCode, double value);

    /// <summary>
    /// Overrides how the game blends this stat when several sources grant it.
    /// </summary>
    ITraitAttributeBuilder WithBlendType(string statCode, Vintagestory.API.Common.EnumStatBlendType blendType);

    /// <summary>
    /// Binds the trait to character classes so new characters of those classes offer it.
    /// </summary>
    ITraitAttributeBuilder AddToClass(params string[] classCodes);
}
