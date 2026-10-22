using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace Flock.Attributes.Common;

public sealed class TraitDefinition
{
    public TraitDefinition(string code, EnumTraitType type)
    {
        Code = code;
        Type = type;
    }

    public string Code { get; }

    public EnumTraitType Type { get; }

    public Dictionary<string, double> Attributes { get; } = [];

    public Dictionary<string, EnumStatBlendType> AttributeBlendTypes { get; } = [];

    public List<string> ClassCodes { get; } = [];

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
