namespace Flock.Attributes.Common;

public interface ITraitAttributeBuilder
{
    ITraitAttributeBuilder WithAttribute(string statCode, double value);

    ITraitAttributeBuilder WithBlendType(string statCode, Vintagestory.API.Common.EnumStatBlendType blendType);

    ITraitAttributeBuilder AddToClass(params string[] classCodes);
}
