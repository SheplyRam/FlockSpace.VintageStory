using System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Flock.Shared.Satiation.Commands;

public sealed class SatiationDrainCommand : SatiationGameCommand
{
    public override string Name => "fsatdrain";

    public override string Description => Lang.Get(SatiationCommandLang.DrainDescription);

    public override ICommandArgumentParser[] Arguments(CommandArgumentParsers parsers)
    {
        return [parsers.Float("amount")];
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

        float before = hunger.Saturation;
        float after = Math.Max(0f, before - Math.Max(0f, (float)args[0]));
        hunger.Saturation = after;

        // Mirror what the hunger-drain Harmony postfix observes on a natural tick.
        satiation.NotifySaturationReduced(before - after);

        return TextCommandResult.Success(Describe(satiation, hunger));
    }
}
