using Flock.Attributes.Common;
using HarmonyLib;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using Vintagestory.GameContent;

namespace Flock.Attributes.Patches;

[HarmonyPatch(typeof(CharacterSystem))]
public static class CharacterSystemPatches
{
    public static TraitRegistryGameSystem? Registry { get; set; }

    private static readonly ConditionalWeakTable<CharacterSystem, MergeState> States = new();

    [SuppressMessage("Usage", "IDE0051", Justification = "Harmony Patch")]
    [HarmonyPostfix]
    [HarmonyPatch("LoadTraits")]
    private static void AfterLoadTraits(CharacterSystem __instance)
    {
        var registry = Registry;

        if (registry == null || __instance.TraitsByCode == null) return;

        var state = States.GetValue(__instance, _ => new MergeState());

        foreach (var definition in registry.Definitions)
        {
            if (__instance.TraitsByCode.ContainsKey(definition.Code)) continue;

            var trait = definition.ToTrait();
            __instance.TraitsByCode[definition.Code] = trait;
            __instance.traits?.Add(trait);
            state.AddedTraits.Add(definition.Code);

            Log(__instance, $"Merged trait '{definition.Code}'");
        }

        foreach (var extension in registry.Extensions)
        {
            if (!__instance.TraitsByCode.TryGetValue(extension.TraitCode, out var trait)) continue;
            if (!state.AppliedExtensions.Add(extension.TraitCode + ":" + extension.StatCode)) continue;

            trait.Attributes ??= [];

            var attribute = trait.Attributes.TryGetValue(extension.StatCode, out var existing) ? existing + extension.Value : extension.Value;
            trait.Attributes[extension.StatCode] = attribute;

            Log(__instance, $"Added attribute '{extension.StatCode}' ({extension.Value}) to trait '{extension.TraitCode}'");
        }
    }

    [SuppressMessage("Usage", "IDE0051", Justification = "Harmony Patch")]
    [HarmonyPostfix]
    [HarmonyPatch("LoadClasses")]
    private static void AfterLoadClasses(CharacterSystem __instance)
    {
        var registry = Registry;

        if (registry == null || __instance.characterClasses == null || __instance.TraitsByCode == null) return;

        var state = States.GetValue(__instance, _ => new MergeState());

        foreach (var definition in registry.Definitions)
        {
            if (definition.ClassCodes.Count == 0) continue;
            if (!__instance.TraitsByCode.ContainsKey(definition.Code)) continue;

            foreach (var classCode in definition.ClassCodes)
            {
                var characterClass = __instance.characterClasses.Find(candidate => candidate.Code == classCode);

                if (characterClass == null)
                {
                    Log(__instance, $"Character class '{classCode}' not found; trait '{definition.Code}' was not bound to it");
                    continue;
                }

                if (!state.AddedToClasses.Add(definition.Code + "@" + classCode)) continue;
                if (characterClass.Traits != null && characterClass.Traits.Contains(definition.Code)) continue;

                characterClass.Traits = characterClass.Traits == null
                    ? [definition.Code]
                    : [.. characterClass.Traits, definition.Code];

                Log(__instance, $"Bound trait '{definition.Code}' to character class '{classCode}'");
            }
        }
    }

    private static void Log(CharacterSystem system, string message)
    {
        system.Mod?.Logger?.Notification("[flockspaceattributesmod] {0}", message);
    }

    private sealed class MergeState
    {
        public HashSet<string> AddedTraits { get; } = [];

        public HashSet<string> AppliedExtensions { get; } = [];

        public HashSet<string> AddedToClasses { get; } = [];
    }
}
