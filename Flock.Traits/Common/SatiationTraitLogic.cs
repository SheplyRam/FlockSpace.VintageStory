using Flock.Shared.Collections;
using Flock.Shared.Satiation.Common;
using Flock.Shared.Traits.Common;
using Vintagestory.GameContent;

namespace Flock.Traits.Common;

/// <summary>
/// Player-facing traits composed from satiation attributes.
/// </summary>
public sealed class SatiationTraitLogic : GameTraitLogic
{
    /// <summary>
    /// Trait code of the venomous bites trait.
    /// </summary>
    public const string VenomousBitesCode = "ft_venomousbites";

    /// <summary>
    /// Trait code of the psychedelic bites trait.
    /// </summary>
    public const string PsychedelicBitesCode = "ft_psychedelicbites";

    public override string Id { get; } = "flockspacetraitsmod:satiationtrait";

    public override void OnModStart(ref ModStartEvent evt)
    {
        var registry = traitsModSystem.Traits;

        registry.AddTrait(VenomousBitesCode, EnumTraitType.Positive)
            .WithAttribute(SatiationCodes.PoisonStorageStat, 1)
            .WithAttribute(SatiationCodes.VenomousBiteStat, 1)
            .WithAttribute(SatiationCodes.PoisonResistanceStat, System.Math.Round(SatiationBalance.MaxResistance, 4))
            .AddToClass(TraitCodes.VanillaCommonerClassCode);

        registry.AddTrait(PsychedelicBitesCode, EnumTraitType.Positive)
            .WithAttribute(SatiationCodes.PsychedelicStorageStat, 1)
            .WithAttribute(SatiationCodes.PsychedelicBiteStat, 1)
            .WithAttribute(SatiationCodes.PsychedelicResistanceStat, System.Math.Round(SatiationBalance.MaxResistance, 4))
            .AddToClass(TraitCodes.VanillaCommonerClassCode);
    }
}
