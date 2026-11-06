namespace Flock.Shared.Satiation.Common;

/// <summary>
/// Converts stored poison load and psychedelic load into applied damage and effect strength through one dampening
/// curve, so bites and digestion stay consistent.
/// </summary>
public static class SatiationPotency
{
    /// <summary>
    /// Applies the dampening curve to one load expenditure.
    /// </summary>
    /// <param name="amount">Satiation amount spent or consumed.</param>
    /// <param name="load">Native load carried by <paramref name="amount"/>.</param>
    /// <param name="dampener">Curve knee.</param>
    public static float Dampen(float amount, float load, float dampener)
    {
        if (amount <= 0f || load <= 0f)
            return 0f;

        float potency = load / amount;
        return amount * potency / (1f + potency / dampener);
    }
}
