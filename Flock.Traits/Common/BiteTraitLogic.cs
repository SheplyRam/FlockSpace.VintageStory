using Flock.Shared.Collections;
using Flock.Shared.DependencyInjection;
using Flock.Shared.HotKeys;
using Flock.Shared.Satiation;
using Flock.Shared.Satiation.Common;
using Flock.Shared.Traits.Common;
using ProtoBuf;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.GameContent;

namespace Flock.Traits.Common;

/// <summary>
/// Defines the bite mechanic and the trait that grants the bite capability the venomous/psychedelic bite traits build
/// upon.
/// </summary>
public sealed class BiteTraitLogic : GameTraitLogic
{
    /// <summary>Attribute stat code that grants the unarmed bite capability and scales its base damage.</summary>
    public const string StatCode = "ft_bite";

    /// <summary>Default key binding of the unarmed bite.</summary>
    public const GlKeys HotKey = GlKeys.B;

    public override string Id { get; } = "flockspacetraitsmod:unarmedbite";

    [IoC] private readonly HotKeyGameSystem hotKeySystem = default!;

    // Last accepted bite per player, so the server (not the client's key repeat) paces bites.
    private readonly Dictionary<string, long> lastBiteTotalMsByPlayerUid = [];

    public override void OnModStart(ref ModStartEvent evt)
    {
        var registry = traitsModSystem.Traits;

        registry.AddTrait(StatCode, EnumTraitType.Positive)
            .WithAttribute(StatCode, 1)
            .AddToClass(TraitCodes.VanillaCommonerClassCode);
    }

    public override void OnClientStart(ref ClientSideStartEvent evt)
    {
        evt.ClientApi.Network.RegisterChannel(Id)
            .RegisterMessageType<UnarmedBiteMessage>();

        hotKeySystem.RegisterHotKey(Id, new HotKeyEntry("Unarmed Bite", OnUnarmedBiteHotKey, HotKey, HotkeyType.CharacterControls));
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
            WithFpVariant = true
        }.Init();

        if (!playerEntity.AnimManager.TryStartAnimation(animation))
            return false;

        capi.Network.GetChannel(Id).SendPacket(new UnarmedBiteMessage());

        return true;
    }

    // Everything from here on runs on the server off a client packet: every guard must
    // revalidate against server state, never trust what the client already checked.
    private void OnUnarmedBiteRequest(IServerPlayer fromPlayer, UnarmedBiteMessage message)
    {
        var playerEntity = fromPlayer.Entity;

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

        // Server-enforced cooldown so network spam cannot bypass animation timing or
        // amplify resource spending and damage.
        var world = fromPlayer.Entity.World;
        long lastBiteTotalMs = lastBiteTotalMsByPlayerUid.TryGetValue(fromPlayer.PlayerUID, out long previous) ? previous : 0;
        if (world.ElapsedMilliseconds - lastBiteTotalMs < SatiationBalance.BiteCooldownMs)
            return;

        lastBiteTotalMsByPlayerUid[fromPlayer.PlayerUID] = world.ElapsedMilliseconds;

        // Spend stored poison/psychedelic contents for characters with the bite attributes.
        // The satiation library revalidates storage and bite attributes and spends only on
        // this valid hit; with no usable contents the bite falls back to damage only.
        bool venomous = playerEntity.Stats.Any(stat => stat.Key == SatiationCodes.VenomousBiteStat);
        bool psychedelic = playerEntity.Stats.Any(stat => stat.Key == SatiationCodes.PsychedelicBiteStat);

        if (venomous || psychedelic)
        {
            SatiationBites.Execute(playerEntity, selection.Entity, venomous, psychedelic);
        }

        var damageSource = new DamageSource
        {
            Source = EnumDamageSource.Player,
            SourceEntity = playerEntity,
            Type = EnumDamageType.PiercingAttack,
            HitPosition = selection.Position
        };

        var damage = playerEntity.Stats.GetBlended(StatCode) - 1f;
        selection.Entity.ReceiveDamage(damageSource, damage);
    }

    private static bool PlayerEntityCanUnarmedBite(EntityPlayer playerEntity)
    {
        return playerEntity.Stats.Any(stat => stat.Key == StatCode);
    }

    [ProtoContract]
    private readonly struct UnarmedBiteMessage;
}
