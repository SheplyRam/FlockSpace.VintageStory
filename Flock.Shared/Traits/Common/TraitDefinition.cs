using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Flock.Shared.Traits.Common;

/// <summary>
/// Mutable builder state for one trait.
/// </summary>
public sealed class TraitDefinition
{
    /// <summary>
    /// Persisted trait code the vanilla character system indexes by.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Whether the game presents the trait as positive, negative, or neutral.
    /// </summary>
    public EnumTraitType Type { get; }

    /// <summary>
    /// Attribute stat values the trait grants, keyed by stat code.
    /// </summary>
    public Dictionary<string, double> Attributes { get; } = [];

    /// <summary>
    /// Per-stat blend type overrides; empty means the game default applies.
    /// </summary>
    public Dictionary<string, EnumStatBlendType> AttributeBlendTypes { get; } = [];

    /// <summary>
    /// Character class codes this trait is offered to during character creation.
    /// </summary>
    public List<string> ClassCodes { get; } = [];

    public TraitDefinition(string code, EnumTraitType type)
    {
        Code = code;
        Type = type;
    }

    /// <summary>
    /// Creates the vanilla trait object. Copies the dictionaries so later builder mutations cannot leak into it.
    /// </summary>
    public Trait ToTrait()
    {
        return new Trait
        {
            Code = Code,
            Type = Type,
            Attributes = new Dictionary<string, double>(Attributes),
            AttributeBlendTypes = AttributeBlendTypes.Count > 0 ? new Dictionary<string, EnumStatBlendType>(AttributeBlendTypes) : null
        };
    }
}
