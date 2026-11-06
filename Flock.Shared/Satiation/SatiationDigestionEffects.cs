using Flock.Shared.Satiation.Common;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace Flock.Shared.Satiation;

internal sealed class SatiationDigestionEffects(Entity entity)
{
    private float pendingPoisonDamage;
    private long lastPoisonBatchMs;

    /// <summary>
    /// Converts one consumption result into pending damage and immediate psychedelic effect.
    /// </summary>
    public void Apply(SatiationConsumption consumption, float poisonResistance, float psychedelicResistance)
    {
        if (consumption.PoisonLoad > 0f)
        {
            pendingPoisonDamage += SatiationPotency.Dampen(consumption.PoisonAmount, consumption.PoisonLoad, SatiationBalance.PoisonPotencyDampener)
                * (1f - poisonResistance) * SatiationBalance.PoisonGlobalMultiplier;
        }

        if (consumption.PsychedelicLoad > 0f)
        {
            // Additive like poison damage: each consumed unit contributes its load, so the effect grows with what is
            // actually digested. Never stacks above the vanilla cap.
            float effect = SatiationPotency.Dampen(consumption.PsychedelicAmount, consumption.PsychedelicLoad, SatiationBalance.PsychedelicPotencyDampener)
                * (1f - psychedelicResistance) * SatiationBalance.PsychedelicGlobalMultiplier;
            float current = entity.WatchedAttributes.GetFloat(SatiationCodes.VanillaPsychedelicKey, 0f);
            entity.WatchedAttributes.SetFloat(SatiationCodes.VanillaPsychedelicKey, Math.Min(SatiationBalance.VanillaPsychedelicCap, current + effect));
        }
    }

    /// <summary>
    /// Applies the pending poison damage when the batch interval elapsed, or immediately when forced.
    /// </summary>
    public void FlushPoisonBatch(bool force)
    {
        if (pendingPoisonDamage <= 0f)
            return;

        long now = entity.World.ElapsedMilliseconds;
        if (!force && now - lastPoisonBatchMs < SatiationBalance.PoisonBatchIntervalMs)
            return;

        float damage = pendingPoisonDamage;
        pendingPoisonDamage = 0f;
        lastPoisonBatchMs = now;

        entity.ReceiveDamage(new DamageSource
        {
            Source = EnumDamageSource.Internal,
            Type = EnumDamageType.Poison,
            TicksPerDuration = SatiationBalance.DigestionPoisonDamageTicks,
            DamageOverTimeTypeEnum = EnumDamageOverTimeEffectType.Poison,
        }, damage);
    }
}
