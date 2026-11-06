namespace Flock.Shared.Satiation.Common;

/// <summary>
/// Actual amounts and loads removed from the pools by a special bite.
/// </summary>
public sealed class SatiationBiteSpend
{
    /// <summary>
    /// Shared instance for "nothing spent"; bites that find no usable contents return this.
    /// </summary>
    public static readonly SatiationBiteSpend Empty = new();

    /// <summary>
    /// Poison hunger-bar units spent.
    /// </summary>
    public float PoisonAmount { get; internal set; }

    /// <summary>
    /// Poison load removed with <see cref="PoisonAmount"/>; drives the target's damage.
    /// </summary>
    public float PoisonLoad { get; internal set; }

    /// <summary>
    /// Psychedelic hunger-bar units spent.
    /// </summary>
    public float PsychedelicAmount { get; internal set; }

    /// <summary>
    /// Psychedelic load removed with <see cref="PsychedelicAmount"/>; drives the target's effect.
    /// </summary>
    public float PsychedelicLoad { get; internal set; }

    /// <summary>
    /// Whether any component was spent at all.
    /// </summary>
    public bool Any => PoisonAmount > 0f || PsychedelicAmount > 0f;

    /// <summary>
    /// Total hunger-bar units spent; the vanilla hunger value drops by this.
    /// </summary>
    public float TotalAmount => PoisonAmount + PsychedelicAmount;

    /// <summary>
    /// Weighted potency of the spent poison; zero when none was spent.
    /// </summary>
    public float PoisonPotency => PoisonAmount > 0f ? PoisonLoad / PoisonAmount : 0f;

    /// <summary>
    /// Weighted potency of the spent psychedelics; zero when none was spent.
    /// </summary>
    public float PsychedelicPotency => PsychedelicAmount > 0f ? PsychedelicLoad / PsychedelicAmount : 0f;
}
