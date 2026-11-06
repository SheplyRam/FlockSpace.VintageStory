using Flock.Shared.Commands;
using System.Diagnostics.CodeAnalysis;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.GameContent;

namespace Flock.Shared.Satiation.Commands;

/// <summary>
/// Base of the satiation debug commands.
/// </summary>
public abstract class SatiationGameCommand : GameCommand
{
    protected static bool TryGetCaller(TextCommandCallingArgs args, [MaybeNullWhen(false)] out EntityPlayer player, [MaybeNullWhen(false)] out EntityBehaviorSatiation satiation)
    {
        player = null;
        satiation = null;

        if (args.Caller.Player?.Entity is not EntityPlayer caller)
            return false;

        player = caller;
        return caller.TryGetBehavior(out satiation);
    }

    /// <summary>
    /// Error result shared by every command when the caller cannot carry satiation state.
    /// </summary>
    protected static TextCommandResult RequiresPlayerError()
    {
        return TextCommandResult.Error(Lang.Get(SatiationCommandLang.RequiresPlayer));
    }

    /// <summary>
    /// Error result shared by commands that need the caller's hunger behavior.
    /// </summary>
    protected static TextCommandResult NoHungerError()
    {
        return TextCommandResult.Error(Lang.Get(SatiationCommandLang.NoHunger));
    }

    protected static EntityBehaviorHunger? HungerOf(EntityPlayer player)
    {
        return player.GetBehavior<EntityBehaviorHunger>();
    }

    protected static string Describe(EntityBehaviorSatiation satiation, EntityBehaviorHunger? hunger)
    {
        return Lang.Get(
            SatiationCommandLang.InfoLine,
            hunger?.Saturation ?? 0f,
            hunger?.MaxSaturation ?? 0f,
            satiation.State.Poison.Amount,
            satiation.State.Poison.Load,
            satiation.State.Psychedelic.Amount,
            satiation.State.Psychedelic.Load,
            satiation.HasPoisonStorage,
            satiation.HasPsychedelicStorage,
            satiation.PoisonResistance,
            satiation.PsychedelicResistance);
    }
}
