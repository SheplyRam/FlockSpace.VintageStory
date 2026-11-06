using Vintagestory.GameContent;

namespace Flock.Shared.Traits.Common;

/// <summary>
/// Registration surface for traits.
/// </summary>
public interface ITraitRegistryGameSystem
{
    /// <summary>
    /// Declares a new trait and returns the builder used to compose its attributes.
    /// </summary>
    public ITraitAttributeBuilder AddTrait(string code, EnumTraitType type);

    /// <summary>
    /// Adds an attribute to a trait, extending a vanilla trait when the code is not registered here.
    /// </summary>
    public void AddAttributesToTrait(string traitCode, string statCode, double value);
}
