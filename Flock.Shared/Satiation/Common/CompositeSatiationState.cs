using System;

namespace Flock.Shared.Satiation.Common;

/// <summary>
/// Server-authoritative composition of one entity's hunger bar.
/// </summary>
public sealed class CompositeSatiationState
{
    /// <summary>
    /// Stored poison pool; occupies hunger-bar units inside the aggregate vanilla hunger value.
    /// </summary>
    public SatiationPool Poison { get; } = new();

    /// <summary>
    /// Stored psychedelic pool; occupies hunger-bar units inside the aggregate vanilla hunger value.
    /// </summary>
    public SatiationPool Psychedelic { get; } = new();

    /// <summary>
    /// Total hunger-bar units the special pools occupy inside the aggregate vanilla hunger value.
    /// </summary>
    public float SpecialAmount => Poison.Amount + Psychedelic.Amount;

    /// <summary>
    /// Normal satiation implied by the aggregate vanilla hunger value.
    /// </summary>
    public float NormalSaturation(float totalHunger) => Math.Max(0f, totalHunger - SpecialAmount);

    /// <summary>
    /// Adds freshly eaten poison units and their native load to the pool.
    /// </summary>
    public void AddPoison(float amount, float load)
    {
        if (amount <= 0f)
            return;

        Poison.Add(amount, load);
    }

    /// <summary>
    /// Adds freshly eaten psychedelic units and their native load to the pool.
    /// </summary>
    public void AddPsychedelic(float amount, float load)
    {
        if (amount <= 0f)
            return;

        Psychedelic.Add(amount, load);
    }

    /// <summary>
    /// Splits one eaten food portion over the hunger components.
    /// </summary>
    /// <param name="satiety">Full satiety of the consumed portion (already spoil-adjusted).</param>
    /// <param name="health">Food health effect (already spoil-adjusted); negative means poisonous.</param>
    /// <param name="psychedelic">Native psychedelic strength of the food.</param>
    /// <param name="room">Hunger-bar room below maximum hunger.</param>
    /// <param name="canStorePoison">Whether the eater has the poison storage attribute.</param>
    /// <param name="canStorePsychedelic">Whether the eater has the psychedelic storage attribute.</param>
    public static FoodAllocation AllocateFood(
        float satiety,
        float health,
        float psychedelic,
        float room,
        bool canStorePoison,
        bool canStorePsychedelic)
    {
        if (satiety <= 0f)
            return new FoodAllocation();

        float acceptedTotal = Math.Max(0f, Math.Min(satiety, room));
        float acceptedFraction = acceptedTotal / satiety;

        float poisonAmount = 0f;
        float psychedelicAmount = 0f;

        if (canStorePoison && health < 0f)
        {
            poisonAmount = SatiationBalance.SpecialFraction * acceptedTotal;
        }

        if (canStorePsychedelic && psychedelic > 0f)
        {
            psychedelicAmount = SatiationBalance.SpecialFraction * acceptedTotal;
        }

        return new FoodAllocation
        {
            PoisonAmount = poisonAmount,
            // load scales with the accepted fraction, preserving native potency.
            PoisonLoad = acceptedFraction * Math.Max(0f, -health),
            PsychedelicAmount = psychedelicAmount,
            PsychedelicLoad = acceptedFraction * psychedelic,
        };
    }

    /// <summary>
    /// Stores an accepted allocation into the pools.
    /// </summary>
    public void Store(in FoodAllocation allocation)
    {
        AddPoison(allocation.PoisonAmount, allocation.PoisonLoad);
        AddPsychedelic(allocation.PsychedelicAmount, allocation.PsychedelicLoad);
    }

    /// <summary>
    /// Applies an aggregate hunger reduction to the special pools, following the fixed priority.
    /// </summary>
    public SatiationConsumption Consume(float delta)
    {
        var consumption = new SatiationConsumption();
        if (delta <= 0f || SpecialAmount <= 0f)
            return consumption;

        float remaining = Math.Min(delta, SpecialAmount);

        remaining -= ConsumeFromPool(Psychedelic, poolIsPoison: false, remaining, consumption);
        if (remaining > 0f)
        {
            ConsumeFromPool(Poison, poolIsPoison: true, remaining, consumption);
        }

        return consumption;

        static float ConsumeFromPool(SatiationPool pool, bool poolIsPoison, float amount, SatiationConsumption target)
        {
            float taken = Math.Min(amount, pool.Amount);
            if (taken <= 0f)
                return 0f;

            float load = pool.RemoveAmount(taken);
            if (poolIsPoison)
            {
                target.PoisonAmount += taken;
                target.PoisonLoad += load;
            }
            else
            {
                target.PsychedelicAmount += taken;
                target.PsychedelicLoad += load;
            }

            return taken;
        }
    }

    /// <summary>
    /// Keeps the invariant special ≤ aggregate when the aggregate shrinks through external systems (revive, direct
    /// saturation writes).
    /// </summary>
    public void ClampToTotal(float totalHunger)
    {
        float overflow = SpecialAmount - Math.Max(0f, totalHunger);
        if (overflow <= 0f)
            return;

        Consume(overflow);
    }

    /// <summary>
    /// Allocates a bite over the pools.
    /// </summary>
    public SatiationBiteSpend TrySpendBite(float poisonRequest, float psyRequest, bool poisonEnabled, bool psyEnabled)
    {
        var spend = new SatiationBiteSpend();
        float poisonDesired = poisonEnabled ? Math.Max(0f, poisonRequest) : 0f;
        float psyDesired = psyEnabled ? Math.Max(0f, psyRequest) : 0f;

        if (poisonDesired + psyDesired <= 0f)
            return spend;

        spend.PoisonAmount = Math.Min(poisonDesired, Poison.Amount);
        float poisonDeficit = poisonDesired - spend.PoisonAmount;

        float psyTarget = psyDesired + (psyEnabled ? poisonDeficit : 0f);
        spend.PsychedelicAmount = Math.Min(psyTarget, Psychedelic.Amount);
        float psyDeficit = psyTarget - spend.PsychedelicAmount;

        float poisonTopUp = poisonEnabled ? Math.Min(psyDeficit, Poison.Amount - spend.PoisonAmount) : 0f;
        spend.PoisonAmount += poisonTopUp;

        spend.PoisonLoad = Poison.RemoveAmount(spend.PoisonAmount);
        spend.PsychedelicLoad = Psychedelic.RemoveAmount(spend.PsychedelicAmount);

        return spend;
    }

    /// <summary>
    /// Removes a whole pool because the storage attribute was lost. Returns the removed amount.
    /// </summary>
    public float ClearPoison()
    {
        float amount = Poison.Amount;
        Poison.Clear();
        return amount;
    }

    /// <summary>
    /// Removes a whole pool because the storage attribute was lost. Returns the removed amount.
    /// </summary>
    public float ClearPsychedelic()
    {
        float amount = Psychedelic.Amount;
        Psychedelic.Clear();
        return amount;
    }
}
