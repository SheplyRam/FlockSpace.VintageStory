using Flock.Shared.Satiation.Common;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Flock.Shared.Satiation.Patches;

internal static class FoodEatingPatches
{
    internal static bool BeforeTryEatStop(CollectibleObject __instance, float secondsUsed, ItemSlot slot, EntityAgent byEntity)
    {
        var nutrition = __instance.GetNutritionProperties(byEntity.World, slot.Itemstack, byEntity);
        if (byEntity.World is not IServerWorldAccessor world || nutrition == null || secondsUsed < 0.95f)
            return true; // vanilla guards

        if (!byEntity.TryGetBehavior<EntityBehaviorSatiation>(out var satiation) || !satiation.HandlesFood(nutrition))
            return true; // vanilla behavior

        float spoilState = __instance.UpdateAndGetTransitionState(byEntity.Api.World, slot, EnumTransitionType.Perish)?.TransitionLevel ?? 0f;
        float satSpoilMul = GlobalConstants.FoodSpoilageSatLossMul(spoilState, slot.Itemstack, byEntity);
        float healthSpoilMul = GlobalConstants.FoodSpoilageHealthLossMul(spoilState, slot.Itemstack, byEntity);

        float satiety = nutrition.Satiety * satSpoilMul;
        float health = nutrition.Health * healthSpoilMul;

        var allocation = satiation.AllocateFood(satiety, health, nutrition.Psychedelic);

        // Aggregate hunger increases by the accepted total; vanilla nutrition handling keeps per-category levels and
        // loss delays. StoreFood only records the special components.
        byEntity.ReceiveSaturation(satiety, nutrition.FoodCategory, nutrition.SaturationLossDelay);
        satiation.StoreFood(allocation);

        var player = byEntity is EntityPlayer playerEntity
            ? world.PlayerByUid(playerEntity.PlayerUID)
            : null;

        slot.TakeOut(1);

        if (nutrition.EatenStack is { } eatenStack)
        {
            var eatenStackClone = eatenStack.ResolvedItemstack?.Clone();
            if (slot.Empty)
            {
                slot.Itemstack = eatenStackClone;
            }
            else if (player == null || !player.InventoryManager.TryGiveItemstack(eatenStackClone, slotNotifyEffect: true))
            {
                byEntity.World.SpawnItemEntity(eatenStackClone, byEntity.Pos.XYZ);
            }
        }

        // Suppress only the native effects of stored components.
        if (!allocation.StoresPoison)
        {
            float intoxication = byEntity.WatchedAttributes.GetFloat(SatiationCodes.VanillaIntoxicationKey);
            byEntity.WatchedAttributes.SetFloat(SatiationCodes.VanillaIntoxicationKey, Math.Min(1.1f, intoxication + nutrition.Intoxication));
        }

        if (!allocation.StoresPsychedelic)
        {
            float psychedelic = byEntity.WatchedAttributes.GetFloat(SatiationCodes.VanillaPsychedelicKey);
            byEntity.WatchedAttributes.SetFloat(SatiationCodes.VanillaPsychedelicKey, Math.Min(SatiationBalance.VanillaPsychedelicCap, psychedelic + nutrition.Psychedelic));
        }

        float effectiveHealth = allocation.StoresPoison ? Math.Max(0f, health) : health;
        if (effectiveHealth != 0f)
        {
            float durationSec = (slot.Itemstack?.Collectible?.Attributes?["eatHealthEffectDurationSec"].AsFloat()).GetValueOrDefault();
            int ticksPerDuration = slot.Itemstack?.Collectible?.Attributes?["eatHealthEffectTicks"].AsInt(1) ?? 1;
            byEntity.ReceiveDamage(new DamageSource
            {
                Source = EnumDamageSource.Internal,
                Type = effectiveHealth > 0f ? EnumDamageType.Heal : EnumDamageType.Poison,
                Duration = TimeSpan.FromSeconds(durationSec),
                TicksPerDuration = ticksPerDuration,
                DamageOverTimeTypeEnum = effectiveHealth > 0f ? EnumDamageOverTimeEffectType.Unknown : EnumDamageOverTimeEffectType.Poison,
            }, Math.Abs(effectiveHealth));
        }

        slot.MarkDirty();
        player?.InventoryManager.BroadcastHotbarSlot();

        return false; // vanilla body replaced
    }
}
