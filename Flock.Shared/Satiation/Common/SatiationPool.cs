using System;

namespace Flock.Shared.Satiation.Common;

/// <summary>
/// One special hunger component: an <see cref="Amount"/> of hunger-bar units plus the <see cref="Load"/> (native
/// strength) carried by those units.
/// </summary>
public sealed class SatiationPool
{
    /// <summary>
    /// Hunger-bar units this pool occupies inside the aggregate vanilla hunger value.
    /// </summary>
    public float Amount { get; private set; }

    /// <summary>
    /// Total native strength carried by <see cref="Amount"/>; consumed proportionally with it.
    /// </summary>
    public float Load { get; private set; }

    /// <summary>
    /// Native strength per hunger unit; zero when the pool is empty.
    /// </summary>
    public float Potency => Amount > 0f ? Load / Amount : 0f;

    /// <summary>
    /// Adds freshly stored units together with their native load; non-positive amounts are ignored.
    /// </summary>
    public void Add(float amount, float load)
    {
        if (amount <= 0f)
            return;

        Amount += amount;
        Load += Math.Max(0f, load);
    }

    /// <summary>
    /// Removes up to <paramref name="amount"/> hunger units together with the proportional share of the load.
    /// </summary>
    public float RemoveAmount(float amount)
    {
        if (amount <= 0f || Amount <= 0f)
        {
            Sanitize();
            return 0f;
        }

        float removed = Math.Min(amount, Amount);
        float removedLoad = Load * (removed / Amount);

        Amount -= removed;
        Load -= removedLoad;

        Sanitize();
        return removedLoad;
    }

    /// <summary>
    /// Empties the pool entirely; used when the storage attribute is lost.
    /// </summary>
    public void Clear()
    {
        Amount = 0f;
        Load = 0f;
    }

    private void Sanitize()
    {
        if (Amount <= 0.0001f)
        {
            Amount = 0f;
            Load = 0f;
        }

        if (Load < 0f)
        {
            Load = 0f;
        }
    }
}
