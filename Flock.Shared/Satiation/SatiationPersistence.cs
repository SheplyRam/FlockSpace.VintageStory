using Flock.Shared.Satiation.Common;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.GameContent;

namespace Flock.Shared.Satiation;

/// <summary>
/// Watched-attribute persistence for one entity's composite satiation state.
/// </summary>
internal sealed class SatiationPersistence
{
    private const string BaseMaxSaturationKey = "basemaxsaturation";

    private readonly Entity entity;
    private readonly CompositeSatiationState state;

    private ITreeAttribute? tree;

    public SatiationPersistence(Entity entity, CompositeSatiationState state)
    {
        this.entity = entity;
        this.state = state;
    }

    /// <summary>
    /// The vanilla maximum captured before any capacity multiplier is applied.
    /// </summary>
    public float BaseMaxSaturation(float fallback)
    {
        return tree?.GetFloat(BaseMaxSaturationKey, fallback) ?? fallback;
    }

    /// <summary>
    /// Ensures the pool tree exists and loads the persisted pools into the state.
    /// </summary>
    public void Load(EntityBehaviorHunger? hunger)
    {
        tree = entity.WatchedAttributes.GetTreeAttribute(SatiationCodes.WatchedTreeCode);
        if (tree == null)
        {
            tree = new TreeAttribute();
            entity.WatchedAttributes[SatiationCodes.WatchedTreeCode] = tree;
        }

        LoadState();

        // Capture the vanilla maximum once, before any capacity multiplier is applied, so a world restart cannot
        // compound the multiplier across sessions.
        if (!tree.HasAttribute(BaseMaxSaturationKey) && hunger != null)
        {
            tree.SetFloat(BaseMaxSaturationKey, hunger.MaxSaturation);
            MarkDirty();
        }
    }

    /// <summary>
    /// Persists the state and marks the pool tree dirty for save and sync.
    /// </summary>
    public void MarkDirty()
    {
        SaveState();
        entity.WatchedAttributes.MarkPathDirty(SatiationCodes.WatchedTreeCode);
    }

    private void LoadState()
    {
        if (tree == null)
            return;

        state.Poison.Add(tree.GetFloat(SatiationCodes.PoisonAmountKey), tree.GetFloat(SatiationCodes.PoisonLoadKey));
        state.Psychedelic.Add(tree.GetFloat(SatiationCodes.PsyAmountKey), tree.GetFloat(SatiationCodes.PsyLoadKey));

        // An empty amount must never carry load.
        state.Poison.RemoveAmount(0f);
        state.Psychedelic.RemoveAmount(0f);
    }

    private void SaveState()
    {
        if (tree == null)
            return;

        tree.SetFloat(SatiationCodes.PoisonAmountKey, state.Poison.Amount);
        tree.SetFloat(SatiationCodes.PoisonLoadKey, state.Poison.Load);
        tree.SetFloat(SatiationCodes.PsyAmountKey, state.Psychedelic.Amount);
        tree.SetFloat(SatiationCodes.PsyLoadKey, state.Psychedelic.Load);
    }
}
