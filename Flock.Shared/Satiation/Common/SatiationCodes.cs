namespace Flock.Shared.Satiation.Common;

/// <summary>
/// Stable codes shared by the satiation library, gameplay mods, and assets.
/// </summary>
public static class SatiationCodes
{
    /// <summary>
    /// Entity behavior code the library registers with the game.
    /// </summary>
    public const string BehaviorCode = "flocksatiation";

    /// <summary>
    /// Watched-attribute tree that persists the special pools.
    /// </summary>
    public const string WatchedTreeCode = "flocksatiation";

    /// <summary>
    /// Attribute stat granting a maximum-hunger increase.
    /// </summary>
    public const string MaxHungerIncreaseStat = "ft_maxhungerincrease";

    /// <summary>
    /// Attribute stat granting a maximum-hunger decrease.
    /// </summary>
    public const string MaxHungerDecreaseStat = "ft_maxhungerdecrease";

    /// <summary>
    /// Attribute stat allowing poison storage in the hunger bar.
    /// </summary>
    public const string PoisonStorageStat = "ft_poisonstorage";

    /// <summary>
    /// Attribute stat allowing psychedelic storage in the hunger bar.
    /// </summary>
    public const string PsychedelicStorageStat = "ft_psychedelicstorage";

    /// <summary>
    /// Attribute stat enabling the venomous bite.
    /// </summary>
    public const string VenomousBiteStat = "ft_venomousbite";

    /// <summary>
    /// Attribute stat enabling the psychedelic bite.
    /// </summary>
    public const string PsychedelicBiteStat = "ft_psychedelicbite";

    /// <summary>
    /// Attribute stat reducing received poison damage.
    /// </summary>
    public const string PoisonResistanceStat = "ft_poisonresistance";

    /// <summary>
    /// Attribute stat reducing received psychedelic effect.
    /// </summary>
    public const string PsychedelicResistanceStat = "ft_psychedelicresistance";

    /// <summary>
    /// Watched key of the stored poison amount inside the pool tree.
    /// </summary>
    public const string PoisonAmountKey = "poisonamount";

    /// <summary>
    /// Watched key of the stored poison load inside the pool tree.
    /// </summary>
    public const string PoisonLoadKey = "poisonload";

    /// <summary>
    /// Watched key of the stored psychedelic amount inside the pool tree.
    /// </summary>
    public const string PsyAmountKey = "psyamount";

    /// <summary>
    /// Watched key of the stored psychedelic load inside the pool tree.
    /// </summary>
    public const string PsyLoadKey = "psyload";

    /// <summary>
    /// Vanilla watched tree holding the hunger aggregate.
    /// </summary>
    public const string VanillaHungerTreeCode = "hunger";

    /// <summary>
    /// Vanilla watched key of the current saturation inside the hunger tree.
    /// </summary>
    public const string VanillaCurrentSaturationKey = "currentsaturation";

    /// <summary>
    /// Vanilla watched key of the maximum saturation inside the hunger tree.
    /// </summary>
    public const string VanillaMaxSaturationKey = "maxsaturation";

    /// <summary>
    /// Vanilla watched key of the active psychedelic effect strength.
    /// </summary>
    public const string VanillaPsychedelicKey = "psychedelic";

    /// <summary>
    /// Vanilla watched key of the intoxication level.
    /// </summary>
    public const string VanillaIntoxicationKey = "intoxication";
}
