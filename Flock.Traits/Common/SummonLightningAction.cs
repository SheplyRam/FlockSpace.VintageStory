using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;
using Flock.Shared.HotKeys;
using ProtoBuf;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Flock.Traits.Common;

/// <summary>
/// This should be viewed as an example of integration only.
/// </summary>
public sealed class SummonLightningAction : GameAction
{
    public override GlKeys HotKey => GlKeys.P;
    public override string Id { get; } = "flockspacetraitsmod:summonlightning";
    [Dependency] private readonly HotKeyGameSystem _hotKeySystem = default!;

    public override void OnClientStart(ref ClientSideStartEvent evt)
    {
        evt.ClientApi.Network.RegisterChannel(Id)
            .RegisterMessageType<TraitSummonLightningMessage>();

        var hotKeyCode = "traitsystem:summonlightning";
        _hotKeySystem.RegisterHotKey(hotKeyCode, new HotKeyEntry("Summon Lightning! >:D", OnSummonLightningHotKey, HotKey, HotkeyType.CharacterControls));
    }

    public override void OnServerStart(ref ServerSideStartEvent evt)
    {
        evt.ServerApi.Network.RegisterChannel(Id)
            .RegisterMessageType<TraitSummonLightningMessage>()
            .SetMessageHandler<TraitSummonLightningMessage>(OnSummonLightningRequest);
    }

    public void RequestClientSummonLightningFlash(Vec3d position)
    {
        var capi = dependencies.Resolve<ICoreClientAPI>();
        capi.Network.GetChannel(Id)
                .SendPacket(new TraitSummonLightningMessage() { TargetX = position.X, TargetY = position.Y, TargetZ = position.Z });
    }

    private bool OnSummonLightningHotKey(KeyCombination keyCombination)
    {
        var capi = dependencies.Resolve<ICoreClientAPI>();

        var aimedEntity = capi.World.Player.CurrentEntitySelection;
        var aimedBlock = capi.World.Player.CurrentBlockSelection;

        Vec3d? position = null;

        if (aimedEntity != null)
            position = aimedEntity.Position.Clone();

        if (aimedBlock != null)
            position = aimedBlock.FullPosition.Clone();

        if (position != null)
        {
            RequestClientSummonLightningFlash(position);
            return true;
        }

        return false;
    }

    private void OnSummonLightningRequest(IServerPlayer fromPlayer, TraitSummonLightningMessage message)
    {
        var api = dependencies.Resolve<ICoreAPI>();

        var weather = api.ModLoader.GetModSystem<WeatherSystemServer>();

        var position = new Vec3d(message.TargetX, message.TargetY, message.TargetZ);
        weather.SpawnLightningFlash(position);
    }

    [ProtoContract]
    private readonly struct TraitSummonLightningMessage
    {
        [ProtoMember(1)] public double TargetX { get; init; }
        [ProtoMember(2)] public double TargetY { get; init; }
        [ProtoMember(3)] public double TargetZ { get; init; }
    }
}
