using Flock.Shared.Common;

namespace Flock.Traits.Common;

public sealed class GameActionManagerSystem : GameSystem
{
    public override string Id => "flockspacetraitsmod:traits";
    private GameActionCollection _actions = default!;

    public override void Initialize()
    {
        _actions = new GameActionCollection(dependencies);
        _actions.Register<SummonLightningAction>();
        _actions.Register<UnarmedBiteAction>();

        _actions.Initialize();
    }

    public override void Shutdown()
    {
        _actions.Shutdown();
    }
}
