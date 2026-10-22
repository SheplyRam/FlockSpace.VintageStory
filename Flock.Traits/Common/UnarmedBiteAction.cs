using Flock.Attributes;
using Flock.Attributes.Common;
using Flock.Shared;
using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;
using Flock.Shared.HotKeys;
using ProtoBuf;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;
using Vintagestory.Server;

namespace Flock.Traits.Common;

/// <summary>
/// Plays the "unarmedbite" animation on the player who triggered the hotkey.
/// </summary>
public sealed class UnarmedBiteAction : GameAction
{
    public const string StatCode = "unarmedbite";

    public override GlKeys HotKey => GlKeys.B;
    public override string Id { get; } = "flockspacetraitsmod:unarmedbite";

    [Dependency] private readonly HotKeyGameSystem _hotKeySystem = default!;

    public override void OnModStart(ref ModStartEvent evt)
    {
        if (!evt.Api.ModLoader.TryGetModSystem<FlockAttributesModSystem>(out var attributes)) return;
        if (!attributes.Systems.TryResolve<TraitRegistryGameSystem>(out var traits)) return;

        //traits.AddTrait("goferal", EnumTraitType.Positive)
        //    .WithAttribute(StatCode, 2f)
        //    .AddToClass("malefactor");
    }

    public override void OnClientStart(ref ClientSideStartEvent evt)
    {
        evt.ClientApi.Network.RegisterChannel(Id)
            .RegisterMessageType<UnarmedBiteMessage>();

        _hotKeySystem.RegisterHotKey(Id, new HotKeyEntry("Unarmed Bite", OnUnarmedBiteHotKey, HotKey, HotkeyType.CharacterControls));
    }

    public override void OnServerStart(ref ServerSideStartEvent evt)
    {
        evt.ServerApi.Network.RegisterChannel(Id)
            .RegisterMessageType<UnarmedBiteMessage>()
            .SetMessageHandler<UnarmedBiteMessage>(OnUnarmedBiteRequest);
    }

    private bool OnUnarmedBiteHotKey(KeyCombination keyCombination)
    {
        var capi = dependencies.Resolve<ICoreClientAPI>();

        var player = capi.World.Player;
        if (player?.Entity is not { } playerEntity)
            return false;

        if (!PlayerEntityCanUnarmedBite(playerEntity))
            return false;

        var animation = new AnimationMetaData
        {
            Code = StatCode,
            Animation = StatCode,
        }.Init();

        if (!playerEntity.AnimManager.TryStartAnimation(animation))
            return false;

        capi.Network.GetChannel(Id).SendPacket(new UnarmedBiteMessage());

        return true;
    }

    private void OnUnarmedBiteRequest(IServerPlayer fromPlayer, UnarmedBiteMessage message)
    {
        var playerEntity = fromPlayer.Entity;

        // We restrict the action to unarmedbite attribute.
        var hasUnarmedBiteStats = playerEntity.Stats.Any(stat => stat.Key == StatCode);
        if (!PlayerEntityCanUnarmedBite(playerEntity))
            return;

        var selection = fromPlayer.CurrentEntitySelection;

        if (selection?.Entity == null || !selection.Entity.Alive || selection.Entity == playerEntity)
            return;

        // Sanity-check the aimed target against the player's own reach; the client
        // only produces entity selections within its picking (attack) range.
        var playerPosCopy = playerEntity.Pos.XYZ.Clone();
        var eyePosition = playerPosCopy.AddCopy(0, playerEntity.LocalEyePos.Y, 0);

        if (eyePosition.DistanceTo(selection.Position) > fromPlayer.WorldData.PickingRange + 0.25)
            return;

        var damageSource = new DamageSource
        {
            Source = EnumDamageSource.Player,
            SourceEntity = playerEntity,
            Type = EnumDamageType.PiercingAttack,
            HitPosition = selection.Position
        };

        var damage = playerEntity.Stats.GetBlended(StatCode);
        selection.Entity.ReceiveDamage(damageSource, damage);
    }

    private bool PlayerEntityCanUnarmedBite(EntityPlayer playerEntity)
    {
        return playerEntity.Stats.Any(stat => stat.Key == StatCode);
    }

    [ProtoContract]
    private readonly struct UnarmedBiteMessage;
}
