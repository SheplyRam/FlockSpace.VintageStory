namespace Flock.Shared.Satiation.Common;

/// <summary>
/// Describes how one eaten food portion is distributed over hunger components.
/// </summary>
public sealed class FoodAllocation
{
    /// <summary>
    /// Portion granted to vanilla satiation/nutrition handling.
    /// </summary>
    public float NormalAmount { get; init; }

    /// <summary>
    /// Poison amount accepted into the pool (30% of the portion when storable).
    /// </summary>
    public float PoisonAmount { get; init; }

    /// <summary>
    /// Native poison load carried by <see cref="PoisonAmount"/>.
    /// </summary>
    public float PoisonLoad { get; init; }

    /// <summary>
    /// Psychedelic amount accepted into the pool (30% of the portion when storable).
    /// </summary>
    public float PsychedelicAmount { get; init; }

    /// <summary>
    /// Native psychedelic load carried by <see cref="PsychedelicAmount"/>.
    /// </summary>
    public float PsychedelicLoad { get; init; }

    /// <summary>
    /// Whether any poison was stored, and its native damage effect should be suppressed.
    /// </summary>
    public bool StoresPoison => PoisonAmount > 0f;

    /// <summary>
    /// Whether any psychedelics were stored, and the native visual effect should be suppressed.
    /// </summary>
    public bool StoresPsychedelic => PsychedelicAmount > 0f;

    /// <summary>
    /// Whether the eater stores any special component of this food.
    /// </summary>
    public bool StoresAnySpecial => StoresPoison || StoresPsychedelic;
}
