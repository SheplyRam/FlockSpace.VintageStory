using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace Flock.Shared.Satiation.Commands;

public sealed class SatiationClearCommand : SatiationGameCommand
{
    public override string Name => "fsatclear";

    public override string Description => Lang.Get(SatiationCommandLang.ClearDescription);

    public override ICommandArgumentParser[] Arguments(CommandArgumentParsers parsers)
    {
        return [];
    }

    public override TextCommandResult Execute(TextCommandCallingArgs args)
    {
        if (!TryGetCaller(args, out var player, out var satiation))
        {
            return RequiresPlayerError();
        }

        satiation.SetDebugPools(
            poisonAmount: 0f,
            poisonLoad: 0f,
            psyAmount: 0f,
            psyLoad: 0f);

        return TextCommandResult.Success(Describe(satiation, HungerOf(player)));
    }
}
