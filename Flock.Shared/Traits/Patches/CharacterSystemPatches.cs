using Flock.Shared.Traits.Common;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Vintagestory.GameContent;

namespace Flock.Shared.Traits.Patches;

/// <summary>
/// Hooks on the vanilla character system's asset loaders.
/// </summary>
public static class CharacterSystemPatches
{
    internal static TraitRegistryGameSystem? Registry { get; set; }

    private static readonly ConditionalWeakTable<CharacterSystem, MergeState> States = [];

    internal static void AfterLoadTraits(CharacterSystem __instance)
    {
        var registry = Registry;

        if (registry == null || __instance.TraitsByCode == null) return;

        var state = States.GetValue(__instance, _ => new MergeState());

        foreach (var definition in registry.Definitions)
        {
            // Vanilla content wins: only codes the game does not already provide are added.
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
    internal static void AfterLoadClasses(CharacterSystem __instance)
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

    // Kept on the attributes-domain prefix even after the namespace rename: the string is
    // only log output, and changing it would break log-based troubleshooting notes.
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
