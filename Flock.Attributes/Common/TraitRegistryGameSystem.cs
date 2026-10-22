using System.Collections.Generic;
using Flock.Attributes.Patches;
using Flock.Shared.Common;
using Vintagestory.GameContent;

namespace Flock.Attributes.Common;

public sealed class TraitRegistryGameSystem : GameSystem
{
    private readonly List<TraitDefinition> definitions = [];

    private readonly List<TraitExtension> extensions = [];

    public override string Id => "flockspaceattributesmod:traits";

    internal IReadOnlyCollection<TraitDefinition> Definitions => definitions;

    internal IReadOnlyCollection<TraitExtension> Extensions => extensions;

    public ITraitAttributeBuilder AddTrait(string code, EnumTraitType type)
    {
        var definition = new TraitDefinition(code, type);
        definitions.Add(definition);

        return new TraitAttributeBuilder(definition);
    }

    public void AddAttributesToTrait(string traitCode, string statCode, double value)
    {
        var definition = definitions.Find(candidate => candidate.Code == traitCode);

        if (definition != null)
        {
            AddAttribute(definition, statCode, value);
            return;
        }

        extensions.Add(new TraitExtension(traitCode, statCode, value));
    }

    public override void Initialize()
    {
        CharacterSystemPatches.Registry = this;
    }

    public override void Shutdown()
    {
        if (ReferenceEquals(CharacterSystemPatches.Registry, this))
        {
            CharacterSystemPatches.Registry = null;
        }
    }

    private static void AddAttribute(TraitDefinition definition, string statCode, double value)
    {
        definition.Attributes[statCode] = definition.Attributes.TryGetValue(statCode, out var existing) ? existing + value : value;
    }

    public sealed record TraitExtension(string TraitCode, string StatCode, double Value);

    private sealed class TraitAttributeBuilder(TraitDefinition definition) : ITraitAttributeBuilder
    {
        public ITraitAttributeBuilder WithAttribute(string statCode, double value)
        {
            AddAttribute(definition, statCode, value);
            return this;
        }

        public ITraitAttributeBuilder WithBlendType(string statCode, Vintagestory.API.Common.EnumStatBlendType blendType)
        {
            definition.AttributeBlendTypes.Add(statCode, blendType);
            return this;
        }

        public ITraitAttributeBuilder AddToClass(params string[] classCodes)
        {
            definition.ClassCodes.AddRange(classCodes);
            return this;
        }
    }
}
