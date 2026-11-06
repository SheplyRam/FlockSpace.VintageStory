using Flock.Shared.Satiation.Common;
using System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Flock.Shared.Satiation.Commands;

public sealed class SatiationGiveCommand : SatiationGameCommand
{
    public override string Name => "fsatgive";

    public override string Description => Lang.Get(SatiationCommandLang.GiveDescription);

    public override ICommandArgumentParser[] Arguments(CommandArgumentParsers parsers)
    {
        return [parsers.WordRange("component", "poison", "psy"), parsers.Float("amount"), parsers.OptionalFloat("load", -1f)];
    }

    public override TextCommandResult Execute(TextCommandCallingArgs args)
    {
        if (!TryGetCaller(args, out var player, out var satiation))
        {
            return RequiresPlayerError();
        }

        var hunger = HungerOf(player);
        if (hunger == null)
        {
            return NoHungerError();
        }

        string component = ((string)args[0]).ToLowerInvariant();
        float amount = Math.Max(0f, (float)args[1]);
        float loadArgument = (float)args[2];

        // The word parser is constrained to the listed values; "psy" covers psychedelics.
        if (component == "poison")
        {
            if (!satiation.HasPoisonStorage)
            {
                return TextCommandResult.Error(Lang.Get(SatiationCommandLang.RequiresPoisonStorage));
            }

            satiation.SetDebugPools(
                poisonAmount: satiation.State.Poison.Amount + amount,
                poisonLoad: satiation.State.Poison.Load + (loadArgument >= 0f ? loadArgument : amount));
        }
        else
        {
            if (!satiation.HasPsychedelicStorage)
            {
                return TextCommandResult.Error(Lang.Get(SatiationCommandLang.RequiresPsyStorage));
            }

            satiation.SetDebugPools(
                psyAmount: satiation.State.Psychedelic.Amount + amount,
                psyLoad: satiation.State.Psychedelic.Load + (loadArgument >= 0f ? loadArgument : amount));
        }

        // Make room so the new pool survives the reconcile clamp; the bar may read full.
        hunger.Saturation += amount;

        return TextCommandResult.Success(Describe(satiation, hunger));
    }
}
