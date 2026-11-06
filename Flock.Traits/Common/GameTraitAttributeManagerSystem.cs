using Flock.Shared.API;

namespace Flock.Traits.Common;

/// <summary>
/// Composes the gameplay trait mechanics (bite capability, satiation traits) into one system.
/// </summary>
public sealed class GameTraitAttributeManagerSystem : GameSystem
{
    private GameTraitLogicCollection traits = default!;

    public override string Id => "flockspacetraitsmod:traits";

    public override void Initialize()
    {
        traits = new GameTraitLogicCollection(dependencies);
        traits.Register<BiteTraitLogic>();
        traits.Register<SatiationTraitLogic>();

        traits.Initialize();
    }

    public override void Shutdown()
    {
        traits.Shutdown();
    }
}
