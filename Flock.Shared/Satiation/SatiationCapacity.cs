using Flock.Shared.Satiation.Common;
using System;
using Vintagestory.API.Common.Entities;
using Vintagestory.GameContent;

namespace Flock.Shared.Satiation;

/// <summary>
/// Capacity policy for the composite hunger bar.
/// </summary>
internal sealed class SatiationCapacity(Entity entity)
{
    /// <summary>
    /// Reapplies the capacity multiplier from the persisted base maximum.
    /// </summary>
    public void Apply(EntityBehaviorHunger hunger, float baseMaxSaturation, bool hasAnyStorage)
    {
        float generalModifier =
            (entity.Stats.GetBlended(SatiationCodes.MaxHungerIncreaseStat) - 1f)
            + (entity.Stats.GetBlended(SatiationCodes.MaxHungerDecreaseStat) - 1f);

        float multiplier = Math.Max(0f, 1f + generalModifier + (hasAnyStorage ? SatiationBalance.StorageCapacityBonus : 0f));
        float target = baseMaxSaturation * multiplier;

        if (Math.Abs(target - hunger.MaxSaturation) > 0.01f)
        {
            hunger.MaxSaturation = target;
        }
    }
}
