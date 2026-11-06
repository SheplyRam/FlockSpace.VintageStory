using System;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Flock.Shared.Satiation.Commands;

public sealed class SatiationSetCommand : SatiationGameCommand
{
    public override string Name => "fsatset";

    public override string Description => Lang.Get(SatiationCommandLang.SetDescription);

    public override ICommandArgumentParser[] Arguments(CommandArgumentParsers parsers)
    {
        return [parsers.Float("saturation")];
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

        float value = Math.Max(0f, (float)args[0]);
        hunger.Saturation = value;

        // Clamp stored specials to the new aggregate immediately instead of waiting for the next reconcile tick.
        satiation.SetDebugPools();

        return TextCommandResult.Success(Describe(satiation, hunger));
    }
}
