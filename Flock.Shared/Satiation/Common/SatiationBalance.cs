namespace Flock.Shared.Satiation.Common;

/// <summary>
/// Balance constants for the composite satiation mechanics.
/// </summary>
public static class SatiationBalance
{
    /// <summary>
    /// Fraction of a food's satiety that becomes poison/psychedelic amount when stored.
    /// </summary>
    public const float SpecialFraction = 0.30f;

    /// <summary>
    /// Non-stacking maximum-hunger bonus granted by having either storage attribute.
    /// </summary>
    public const float StorageCapacityBonus = 0.40f;

    /// <summary>
    /// Upper clamp for poison/psychedelic resistance.
    /// </summary>
    public const float MaxResistance = 0.15f;

    /// <summary>
    /// Value granted by the maximum-hunger increase attribute (provisional).
    /// </summary>
    public const float MaxHungerIncreaseValue = 0.25f;

    /// <summary>
    /// Value granted by the maximum-hunger decrease attribute (provisional).
    /// </summary>
    public const float MaxHungerDecreaseValue = -0.25f;

    /// <summary>
    /// Total hunger-bar units spent by a special bite (provisional).
    /// </summary>
    public const float BiteTotalCost = 40f;

    /// <summary>
    /// Poison share of a mixed bite; the remainder is psychedelics.
    /// </summary>
    public const float BitePoisonFraction = 0.75f;

    /// <summary>
    /// Server-enforced bite cooldown in milliseconds, matched to the bite animation.
    /// </summary>
    public const int BiteCooldownMs = 1500;

    /// <summary>
    /// Global multiplier on poison damage from digestion and bites (provisional).
    /// </summary>
    public const float PoisonGlobalMultiplier = 1.0f;

    /// <summary>
    /// Global multiplier on psychedelic potency from digestion and bites (provisional).
    /// </summary>
    public const float PsychedelicGlobalMultiplier = 1.0f;

    /// <summary>
    /// Potency dampener for poison damage, see <see cref="SatiationPotency.Dampen"/>.
    /// </summary>
    public const float PoisonPotencyDampener = 1f / 9f;

    /// <summary>
    /// Potency dampener for psychedelic effect buildup.
    /// </summary>
    public const float PsychedelicPotencyDampener = 1f;

    /// <summary>
    /// How long poison damage contributions are batched before one damage call.
    /// </summary>
    public const int PoisonBatchIntervalMs = 1000;

    /// <summary>
    /// How long one bite's poison load takes to fully apply to the victim.
    /// </summary>
    public const int BitePoisonDurationMs = 5000;

    /// <summary>
    /// How many damage-over-time ticks one bite's poison is split into.
    /// </summary>
    public const int BitePoisonDamageTicks = 4;

    /// <summary>
    /// How many ticks one digestion batch is split into. Batches already arrive every second while the bar drains,
    /// so 1 applies the batch at once and keeps the damage continuous.
    /// </summary>
    public const int DigestionPoisonDamageTicks = 1;

    /// <summary>
    /// Vanilla cap for the watched psychedelic value.
    /// </summary>
    public const float VanillaPsychedelicCap = 2f;
}
