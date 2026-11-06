namespace Flock.Shared.Satiation.Common;

/// <summary>
/// Amounts and loads removed from the pools by a single hunger-reduction event.
/// </summary>
public sealed class SatiationConsumption
{
    /// <summary>
    /// Poison hunger-bar units consumed by this event.
    /// </summary>
    public float PoisonAmount { get; internal set; }

    /// <summary>
    /// Poison load removed together with <see cref="PoisonAmount"/>.
    /// </summary>
    public float PoisonLoad { get; internal set; }

    /// <summary>
    /// Psychedelic hunger-bar units consumed by this event.
    /// </summary>
    public float PsychedelicAmount { get; internal set; }

    /// <summary>
    /// Psychedelic load removed together with <see cref="PsychedelicAmount"/>.
    /// </summary>
    public float PsychedelicLoad { get; internal set; }

    /// <summary>
    /// Whether any special component was consumed at all.
    /// </summary>
    public bool Any => PoisonAmount > 0f || PsychedelicAmount > 0f;

    /// <summary>
    /// Weighted potency of the consumed poison units; zero if none consumed.
    /// </summary>
    public float PoisonPotency => PoisonAmount > 0f ? PoisonLoad / PoisonAmount : 0f;

    /// <summary>
    /// Weighted potency of the consumed psychedelic units; zero if none consumed.
    /// </summary>
    public float PsychedelicPotency => PsychedelicAmount > 0f ? PsychedelicLoad / PsychedelicAmount : 0f;
}
