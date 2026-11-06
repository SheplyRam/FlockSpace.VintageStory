using Flock.Shared.Commands;

namespace Flock.Shared.Satiation.Commands;

public sealed class SatiationCommandManagerSystem : GameCommandManagerSystem
{
    public override string Id => "flockspacesharedlib:commands";

    protected override void RegisterCommands()
    {
        Register<SatiationInfoCommand>();
        Register<SatiationSetCommand>();
        Register<SatiationGiveCommand>();
        Register<SatiationDrainCommand>();
        Register<SatiationClearCommand>();
    }
}
