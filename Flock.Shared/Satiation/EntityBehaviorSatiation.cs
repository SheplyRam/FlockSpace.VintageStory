using Flock.Shared.Satiation.Common;
using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Flock.Shared.Satiation;

/// <summary>
/// Composition root of the satiation library: owns the composite state, capacity policy, persistence, and digestion
/// effects of one entity's hunger bar.
/// </summary>
public sealed class EntityBehaviorSatiation : EntityBehavior
{
    /// <summary>
    /// Grace period after entity initialization during which a missing storage stat is assumed to be not-yet-restored
    /// rather than removed.
    /// </summary>
    private const float StorageGraceSeconds = 10f;

    private readonly CompositeSatiationState state = new();
    private readonly SatiationPersistence persistence;
    private readonly SatiationCapacity capacity;
    private readonly SatiationDigestionEffects digestionEffects;

    private EntityBehaviorHunger? hunger;
    private float reconcileCounter;
    private float aliveSeconds;
    private bool settledLogged;

    public EntityBehaviorSatiation(Entity entity) : base(entity)
    {
        persistence = new SatiationPersistence(entity, state);
        capacity = new SatiationCapacity(entity);
        digestionEffects = new SatiationDigestionEffects(entity);
    }

    public override string PropertyName() => SatiationCodes.BehaviorCode;

    /// <summary>
    /// The server-authoritative composite state of this entity's hunger bar.
    /// </summary>
    public CompositeSatiationState State => state;

    private EntityBehaviorHunger? Hunger => hunger ??= entity.GetBehavior<EntityBehaviorHunger>();

    /// <summary>
    /// Whether the entity currently has the poison storage attribute.
    /// </summary>
    public bool HasPoisonStorage => HasStat(SatiationCodes.PoisonStorageStat);

    /// <summary>
    /// Whether the entity currently has the psychedelic storage attribute.
    /// </summary>
    public bool HasPsychedelicStorage => HasStat(SatiationCodes.PsychedelicStorageStat);

    /// <summary>
    /// Blended poison resistance, clamped to <see cref="SatiationBalance.MaxResistance"/>.
    /// </summary>
    public float PoisonResistance => Resistance(SatiationCodes.PoisonResistanceStat);

    /// <summary>
    /// Blended psychedelic resistance, clamped to <see cref="SatiationBalance.MaxResistance"/>.
    /// </summary>
    public float PsychedelicResistance => Resistance(SatiationCodes.PsychedelicResistanceStat);

    public override void Initialize(EntityProperties properties, JsonObject typeAttributes)
    {
        persistence.Load(Hunger);

        entity.World.Logger.Notification(
            $"{SatiationLog.Prefix} Satiation initialized for {0}: poisonStorage={1} psyStorage={2} loaded poison={3}/{4} psy={5}/{6}",
            entity.Code, HasPoisonStorage, HasPsychedelicStorage,
            state.Poison.Amount, state.Poison.Load, state.Psychedelic.Amount, state.Psychedelic.Load);
    }

    public override void OnGameTick(float deltaTime)
    {
        aliveSeconds += deltaTime;
        reconcileCounter += deltaTime;
        if (reconcileCounter < 1f)
            return;

        reconcileCounter = 0f;
        Reconcile();
    }

    /// <summary>
    /// Whether this entity stores any component of the given food specially.
    /// </summary>
    public bool HandlesFood(FoodNutritionProperties nutrition)
    {
        return (nutrition.Health < 0f && HasPoisonStorage) || (nutrition.Psychedelic > 0f && HasPsychedelicStorage);
    }

    /// <summary>
    /// Splits an eaten portion over the hunger components using only the room below maximum hunger and preserving
    /// native potency through the accepted fraction.
    /// </summary>
    public FoodAllocation AllocateFood(float satiety, float health, float psychedelic)
    {
        var hunger = Hunger;
        float room = hunger == null ? float.MaxValue : Math.Max(0f, hunger.MaxSaturation - hunger.Saturation);

        return CompositeSatiationState.AllocateFood(satiety, health, psychedelic, room, HasPoisonStorage, HasPsychedelicStorage);
    }

    /// <summary>
    /// Stores an accepted allocation.
    /// </summary>
    public void StoreFood(in FoodAllocation allocation)
    {
        state.Store(allocation);
        persistence.MarkDirty();
    }

    /// <summary>
    /// Reconciles a reduction of the aggregate vanilla hunger value. Removed loads drive digestion effects.
    /// </summary>
    public void NotifySaturationReduced(float delta)
    {
        if (delta <= 0f || state.SpecialAmount <= 0f)
            return;

        float totalBefore = (Hunger?.Saturation ?? 0f) + delta;
        float normalBefore = state.NormalSaturation(totalBefore);
        float specialDelta = Math.Max(0f, delta - Math.Max(0f, normalBefore));
        if (specialDelta <= 0f)
            return;

        var consumption = state.Consume(specialDelta);
        persistence.MarkDirty();
        digestionEffects.Apply(consumption, PoisonResistance, PsychedelicResistance);
    }

    /// <summary>
    /// Spends stored poison/psychedelic contents for a bite.
    /// </summary>
    public SatiationBiteSpend TrySpendBite(bool venomous, bool psychedelic)
    {
        // Bite attributes only spend the matching stored component.
        var canPoison = venomous && HasPoisonStorage;
        var canPsy = psychedelic && HasPsychedelicStorage;

        // With both bite attributes the cost splits 75/25.
        // With only one, that component covers the full bite cost.
        float totalCost = SatiationBalance.BiteTotalCost;
        float poisonRequest = canPoison ? totalCost * SatiationBalance.BitePoisonFraction : 0f;
        float psyRequest = canPsy ? totalCost * (1f - SatiationBalance.BitePoisonFraction) : 0f;
        if (canPoison != canPsy)
        {
            poisonRequest = canPoison ? totalCost : 0f;
            psyRequest = canPsy ? totalCost : 0f;
        }

        var spend = state.TrySpendBite(poisonRequest, psyRequest, canPoison, canPsy);
        if (!spend.Any)
            return spend;

        var hunger = Hunger;
        hunger?.Saturation = Math.Max(0f, hunger.Saturation - spend.TotalAmount);

        persistence.MarkDirty();
        return spend;
    }

    /// <summary>
    /// Testing hook for debug commands.
    /// </summary>
    public void SetDebugPools(
        float? poisonAmount = null,
        float? poisonLoad = null,
        float? psyAmount = null,
        float? psyLoad = null)
    {
        if (poisonAmount.HasValue || poisonLoad.HasValue)
        {
            state.Poison.Clear();
            state.Poison.Add(poisonAmount ?? 0f, poisonLoad ?? 0f);
        }

        if (psyAmount.HasValue || psyLoad.HasValue)
        {
            state.Psychedelic.Clear();
            state.Psychedelic.Add(psyAmount ?? 0f, psyLoad ?? 0f);
        }

        var hunger = Hunger;
        if (hunger != null)
        {
            state.ClampToTotal(hunger.Saturation);
        }

        persistence.MarkDirty();
    }

    private void Reconcile()
    {
        var hunger = Hunger;
        if (hunger == null)
            return;

        capacity.Apply(hunger, persistence.BaseMaxSaturation(hunger.MaxSaturation), HasPoisonStorage || HasPsychedelicStorage);

        // Drop the now-unsupported pool and its aggregate share if outside the join grace period.
        bool storageSettled = aliveSeconds >= StorageGraceSeconds;
        if (storageSettled && !HasPoisonStorage && state.Poison.Amount > 0f)
        {
            entity.World.Logger.Notification(
                $"{SatiationLog.Prefix} Dropping poison pool ({0} amount) from {1}: storage stat still missing after {2}s",
                state.Poison.Amount, entity.Code, StorageGraceSeconds);
            hunger.Saturation = Math.Max(0f, hunger.Saturation - state.ClearPoison());
            persistence.MarkDirty();
        }

        if (storageSettled && !HasPsychedelicStorage && state.Psychedelic.Amount > 0f)
        {
            entity.World.Logger.Notification(
                $"{SatiationLog.Prefix} Dropping psychedelic pool ({0} amount) from {1}: storage stat still missing after {2}s",
                state.Psychedelic.Amount, entity.Code, StorageGraceSeconds);
            hunger.Saturation = Math.Max(0f, hunger.Saturation - state.ClearPsychedelic());
            persistence.MarkDirty();
        }

        // External aggregate changes (revive, other systems) must never orphan special amount.
        float before = state.SpecialAmount;
        state.ClampToTotal(hunger.Saturation);
        if (state.SpecialAmount != before)
        {
            persistence.MarkDirty();
        }

        digestionEffects.FlushPoisonBatch(force: false);

        if (!settledLogged && aliveSeconds >= StorageGraceSeconds)
        {
            settledLogged = true;
            entity.World.Logger.Notification(
                $"{SatiationLog.Prefix} Satiation settled for {0}: poisonStorage={1} psyStorage={2} poison={3}/{4} psy={5}/{6} saturation={7}/{8}",
                entity.Code, HasPoisonStorage, HasPsychedelicStorage,
                state.Poison.Amount, state.Poison.Load, state.Psychedelic.Amount, state.Psychedelic.Load,
                hunger.Saturation, hunger.MaxSaturation);
        }
    }

    private bool HasStat(string code)
    {
        return entity.Stats.Any(stat => stat.Key == code);
    }

    private float Resistance(string code)
    {
        return Math.Clamp(entity.Stats.GetBlended(code) - 1f, 0f, SatiationBalance.MaxResistance);
    }
}
