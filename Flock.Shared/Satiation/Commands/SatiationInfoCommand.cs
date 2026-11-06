using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Privileges = Vintagestory.API.Server.Privilege;

namespace Flock.Shared.Satiation.Commands;

public sealed class SatiationInfoCommand : SatiationGameCommand
{
    public override string Name => "fsatinfo";

    public override string Description => Lang.Get(SatiationCommandLang.InfoDescription);

    public override string Privilege => Privileges.chat;

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

        return TextCommandResult.Success(Describe(satiation, HungerOf(player)));
    }
}
