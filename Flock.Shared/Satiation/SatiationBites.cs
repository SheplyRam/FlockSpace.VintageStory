using Flock.Shared.Satiation.Common;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace Flock.Shared.Satiation;

/// <summary>
/// Server-authoritative bite execution.
/// </summary>
public static class SatiationBites
{
    /// <summary>
    /// Attempts a special bite from <paramref name="attacker"/> into <paramref name="target"/>.
    /// </summary>
    public static SatiationBiteSpend Execute(EntityAgent attacker, Entity target, bool venomous, bool psychedelic)
    {
        if (!attacker.TryGetBehavior<EntityBehaviorSatiation>(out var satiation) || target == null)
            return SatiationBiteSpend.Empty;

        var spend = satiation.TrySpendBite(venomous, psychedelic);
        if (!spend.Any)
            return spend;

        float targetPoisonResistance = TargetResistance(target, SatiationCodes.PoisonResistanceStat);
        float targetPsyResistance = TargetResistance(target, SatiationCodes.PsychedelicResistanceStat);

        if (spend.PoisonLoad > 0f)
        {
            float damage = SatiationPotency.Dampen(spend.PoisonAmount, spend.PoisonLoad, SatiationBalance.PoisonPotencyDampener)
                * (1f - targetPoisonResistance) * SatiationBalance.PoisonGlobalMultiplier;
            target.ReceiveDamage(new DamageSource
            {
                Source = EnumDamageSource.Player,
                SourceEntity = attacker,
                Type = EnumDamageType.Poison,
                Duration = TimeSpan.FromMilliseconds(SatiationBalance.BitePoisonDurationMs),
                TicksPerDuration = SatiationBalance.BitePoisonDamageTicks,
                DamageOverTimeTypeEnum = EnumDamageOverTimeEffectType.Poison,
            }, damage);
        }

        if (spend.PsychedelicLoad > 0f && target is EntityAgent)
        {
            // Native psychedelic visuals are entity state, not damage.
            float effect = SatiationPotency.Dampen(spend.PsychedelicAmount, spend.PsychedelicLoad, SatiationBalance.PsychedelicPotencyDampener)
                * (1f - targetPsyResistance) * SatiationBalance.PsychedelicGlobalMultiplier;
            float current = target.WatchedAttributes.GetFloat(SatiationCodes.VanillaPsychedelicKey, 0f);
            target.WatchedAttributes.SetFloat(
                SatiationCodes.VanillaPsychedelicKey,
                Math.Min(SatiationBalance.VanillaPsychedelicCap, current + effect));
        }

        return spend;
    }

    private static float TargetResistance(Entity target, string code)
    {
        return target is EntityAgent agent
            ? Math.Clamp(agent.Stats.GetBlended(code) - 1f, 0f, SatiationBalance.MaxResistance)
            : 0f;
    }
}
