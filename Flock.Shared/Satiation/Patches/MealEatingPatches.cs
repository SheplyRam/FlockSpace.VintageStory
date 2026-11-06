using Flock.Shared.Satiation.Common;
using System;
using System.Linq;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Flock.Shared.Satiation.Patches;

internal static class MealEatingPatches
{
    internal static bool BeforeConsume(
        BlockMeal __instance,
        IWorldAccessor world,
        IPlayer eatingPlayer,
        ItemSlot inSlot,
        ItemStack[] contentStacks,
        float remainingServings,
        bool mulwithStackSize,
        ref float __result)
    {
        if (world is not IServerWorldAccessor)
            return true;

        var entity = eatingPlayer.Entity;
        if (!entity.TryGetBehavior<EntityBehaviorSatiation>(out var satiation))
            return true;

        float[] nutritionHealthMul = __instance.GetNutritionHealthMul(null, inSlot, entity);
        FoodNutritionProperties[] contents = BlockMeal.GetContentNutritionProperties(
            world, inSlot, contentStacks, entity, mulwithStackSize, nutritionHealthMul[0], nutritionHealthMul[1]);

        if (contents == null || !contents.Any(content => content != null && satiation.HandlesFood(content)))
            return true; // vanilla behavior

        if (!entity.TryGetBehavior<EntityBehaviorHunger>(out var hunger))
            return true;

        float room = hunger.MaxSaturation - hunger.Saturation;
        float totalSatiety = contents.Where(content => content != null).Sum(content => content.Satiety);
        float servingFraction = GameMath.Clamp(room / Math.Max(1f, totalSatiety), 0f, 1f);
        float servings = Math.Min(remainingServings, servingFraction);

        // Vanilla body-temperature adjustment.
        float temperature = inSlot.Itemstack?.Collectible.GetTemperature(world, inSlot.Itemstack) ?? 20f;
        if (entity.TryGetBehavior<EntityBehaviorBodyTemperature>(out var bodyTemperature) && Math.Abs(temperature - bodyTemperature.CurBodyTemperature) > 10f)
        {
            float changeStrength = Math.Min(1f, (temperature - bodyTemperature.CurBodyTemperature) / 30f);
            bodyTemperature.CurBodyTemperature += GameMath.Clamp(totalSatiety * servings / 80f * changeStrength, 0f, 5f);
        }

        float healthEffect = 0f;
        float psychedelicEffect = 0f;
        float intoxicationEffect = 0f;

        foreach (var content in contents)
        {
            if (content == null)
                continue;

            float satiety = servings * content.Satiety;
            float health = servings * content.Health;
            float psychedelic = servings * content.Psychedelic;

            var allocation = satiation.AllocateFood(satiety, health, psychedelic);

            float delay = Math.Min(1.3f, servings * 3f) * 10f + satiety / 70f * 60f;
            entity.ReceiveSaturation(satiety, content.FoodCategory, delay, 1f);
            satiation.StoreFood(allocation);

            if (content.EatenStack?.ResolvedItemstack != null
                && !eatingPlayer.InventoryManager.TryGiveItemstack(content.EatenStack.ResolvedItemstack.Clone(), true))
            {
                world.SpawnItemEntity(content.EatenStack.ResolvedItemstack.Clone(), entity.Pos.XYZ, null);
            }

            if (!allocation.StoresPoison)
            {
                healthEffect += health;
                intoxicationEffect += servings * content.Intoxication;
            }

            if (!allocation.StoresPsychedelic)
            {
                psychedelicEffect += psychedelic;
            }
        }

        if (intoxicationEffect > 0f)
        {
            float intoxication = entity.WatchedAttributes.GetFloat(SatiationCodes.VanillaIntoxicationKey, 0f);
            entity.WatchedAttributes.SetFloat(SatiationCodes.VanillaIntoxicationKey, Math.Min(1.1f, intoxication + intoxicationEffect));
        }

        if (psychedelicEffect > 0f)
        {
            float psychedelic = entity.WatchedAttributes.GetFloat(SatiationCodes.VanillaPsychedelicKey, 0f);
            entity.WatchedAttributes.SetFloat(SatiationCodes.VanillaPsychedelicKey, Math.Min(SatiationBalance.VanillaPsychedelicCap, psychedelic + psychedelicEffect));
        }

        if (healthEffect != 0f)
        {
            entity.ReceiveDamage(new DamageSource
            {
                Source = EnumDamageSource.Internal,
                Type = healthEffect > 0f ? EnumDamageType.Heal : EnumDamageType.Poison,
            }, Math.Abs(healthEffect));
        }

        __result = Math.Max(0f, remainingServings - servings);
        return false; // vanilla body replaced
    }
}
