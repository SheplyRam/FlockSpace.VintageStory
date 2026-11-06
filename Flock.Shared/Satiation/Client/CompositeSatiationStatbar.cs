using Cairo;
using Flock.Shared.Satiation.Common;
using System;
using System.Collections.Generic;
using Vintagestory.API.Client;
using Vintagestory.API.MathTools;

namespace Flock.Shared.Satiation.Client;

/// <summary>
/// Hunger-bar statbar that renders the composite contents inline instead of painting over the vanilla element.
/// </summary>
public sealed class CompositeSatiationStatbar : GuiElementStatbar
{
    private static readonly double[] PoisonColor = [0.42, 0.18, 0.57];
    private static readonly double[] PsychedelicColor = [0.88, 0.50, 0.10];

    private readonly ICoreClientAPI capi;
    private readonly double[] barColor;
    private LoadedTexture chromeTexture;
    private LoadedTexture fillTexture;
    private LoadedTexture glowTexture;
    private LoadedTexture hoverTexture;

    private bool valuesSet;
    private bool glowComposed;
    private float curValue;
    private float curMax = 100f;
    private float lineInterval = 100f;
    private int fillHeight;
    private float poisonAmount;
    private float poisonLoad;
    private float psyAmount;
    private float psyLoad;

    public CompositeSatiationStatbar(ICoreClientAPI capi, ElementBounds bounds, double[] color)
        : base(capi, bounds, color, rightToLeft: true, hideable: false)
    {
        this.capi = capi;
        barColor = color;
        chromeTexture = new LoadedTexture(capi);
        fillTexture = new LoadedTexture(capi);
        glowTexture = new LoadedTexture(capi);
        hoverTexture = new LoadedTexture(capi);
        onGetStatbarValue = CompositeValueText;
        capi.Event.RegisterGameTickListener(SlowPoll, 1000);
    }

    /// <summary>
    /// Fully self-sufficient refresh once per second.
    /// </summary>
    private void SlowPoll(float dt)
    {
        RefreshFromTrees();
    }

    /// <summary>
    /// Re-reads the hunger aggregate and the pool state from the watched-attribute trees and recomposes on any
    /// difference.
    /// </summary>
    internal void RefreshFromTrees()
    {
        var watched = capi.World?.Player?.Entity?.WatchedAttributes;
        if (watched == null)
            return;

        var hungerTree = watched.GetTreeAttribute(SatiationCodes.VanillaHungerTreeCode);
        if (hungerTree == null)
            return;

        float total = hungerTree.GetFloat(SatiationCodes.VanillaCurrentSaturationKey);
        float max = hungerTree.GetFloat(SatiationCodes.VanillaMaxSaturationKey);

        SnapshotPools(out float poison, out float poisonLoad, out float psy, out float psyLoad);

        if (valuesSet && total == curValue && max == curMax
            && poison == poisonAmount && poisonLoad == this.poisonLoad
            && psy == psyAmount && psyLoad == this.psyLoad)
        {
            return;
        }

        curValue = total;
        curMax = max;
        poisonAmount = poison;
        this.poisonLoad = poisonLoad;
        psyAmount = psy;
        this.psyLoad = psyLoad;
        valuesSet = true;

        RecomposeFill();
        RecomposeHover();
    }

    /// <summary>
    /// Stashes values and pool state, then recomposes. Called from a Harmony prefix on the main thread.
    /// </summary>
    internal void NotifySetValues(float value, float max)
    {
        curValue = value;
        curMax = max;
        SnapshotPools(out float poison, out float poisonLoad, out float psy, out float psyLoad);

        poisonAmount = poison;
        this.poisonLoad = poisonLoad;
        psyAmount = psy;
        this.psyLoad = psyLoad;
        valuesSet = true;

        RecomposeFill();
        RecomposeHover();
    }

    /// <summary>
    /// Reads the pool snapshot from the client's watched-attribute tree. Missing trees report zeroed pools.
    /// </summary>
    private void SnapshotPools(out float poison, out float poisonLoad, out float psy, out float psyLoad)
    {
        var tree = capi.World?.Player?.Entity?.WatchedAttributes.GetTreeAttribute(SatiationCodes.WatchedTreeCode);

        poison = tree?.GetFloat(SatiationCodes.PoisonAmountKey) ?? 0f;
        poisonLoad = tree?.GetFloat(SatiationCodes.PoisonLoadKey) ?? 0f;
        psy = tree?.GetFloat(SatiationCodes.PsyAmountKey) ?? 0f;
        psyLoad = tree?.GetFloat(SatiationCodes.PsyLoadKey) ?? 0f;
    }

    /// <summary>
    /// Stashes the tick-mark interval. Called from a Harmony prefix.
    /// </summary>
    internal void NotifySetLineInterval(float interval)
    {
        lineInterval = interval;
        if (valuesSet)
            RecomposeFill();
    }

    /// <summary>
    /// Dante's 5th circle of hell.
    /// </summary>
    public override void ComposeElements(Context ctx, ImageSurface surface)
    {
        Bounds.CalcWorldBounds();
        using var chromeSurface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt + 1, Bounds.OuterHeightInt + 1);
        using var chromeContext = new Context(chromeSurface);
        GuiElement.RoundRectangle(chromeContext, 0.0, 0.0, Bounds.InnerWidth, Bounds.InnerHeight, 1.0);
        chromeContext.SetSourceRGBA(0.15, 0.15, 0.15, 1.0);
        chromeContext.Fill();
        EmbossRoundRectangleElement(chromeContext, 0.0, 0.0, Bounds.InnerWidth, Bounds.InnerHeight, inverse: false, 3, 1);
        generateTexture(chromeSurface, ref chromeTexture);

        if (valuesSet)
        {
            RecomposeFill();
            RecomposeHover();
        }
    }

    private void RecomposeFill()
    {
        Bounds.CalcWorldBounds();
        fillHeight = Bounds.OuterHeightInt + 1;

        using var surface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt + 1, fillHeight);
        using var ctx = new Context(surface);

        double filledFraction = curMax > 0f ? Math.Min(1.0, Math.Max(0.0, curValue / curMax)) : 0.0;
        if (filledFraction > 0.01)
        {
            double filledWidth = Bounds.OuterWidth * filledFraction;
            double x = Bounds.OuterWidth - filledWidth; // right-anchored fill

            double poisonWidth = curValue > 0f ? filledWidth * (poisonAmount / curValue) : 0.0;
            double psyWidth = curValue > 0f ? filledWidth * (psyAmount / curValue) : 0.0;
            double normalWidth = filledWidth - poisonWidth - psyWidth;

            foreach (var (width, color) in Segments(normalWidth, poisonWidth, psyWidth))
            {
                if (width <= 0.0)
                    continue;

                GuiElement.RoundRectangle(ctx, x, 0.0, width, Bounds.OuterHeight, 1.0);
                ctx.SetSourceRGB(color[0], color[1], color[2]);
                ctx.FillPreserve();
                ctx.SetSourceRGB(color[0] * 0.4, color[1] * 0.4, color[2] * 0.4);
                ctx.LineWidth = GuiElement.scaled(2.0);
                ctx.StrokePreserve();
                x += width;
            }

            SurfaceTransformBlur.BlurFull(surface, 2.0);

            // Emboss on inner bounds, mirroring the vanilla outer -> inner proportions.
            double innerScale = Bounds.OuterWidth > 0.0 ? Bounds.InnerWidth / Bounds.OuterWidth : 1.0;
            double innerX = (Bounds.OuterWidth - filledWidth) * innerScale;
            foreach (var (width, _) in Segments(normalWidth, poisonWidth, psyWidth))
            {
                if (width <= 0.0)
                {
                    continue;
                }

                double innerWidth = width * innerScale;
                EmbossRoundRectangleElement(ctx, innerX, 0.0, innerWidth, Bounds.InnerHeight, inverse: false, 2, 1);
                innerX += innerWidth;
            }

            // Separator lines between special segments and at the special/normal boundary... Still asking about its
            // width.
            ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.5);
            ctx.LineWidth = 1.0;
            double scale = filledWidth > 0.0 ? Bounds.InnerWidth / Bounds.OuterWidth : 1.0;
            double marker = (Bounds.OuterWidth - filledWidth) * scale;
            foreach (var (width, _) in Segments(normalWidth, poisonWidth, psyWidth))
            {
                if (width <= 0.0)
                    continue;

                marker += width * scale;
                ctx.NewPath();
                ctx.MoveTo(marker, 0.0);
                ctx.LineTo(marker, Math.Max(3.0, Bounds.InnerHeight - 1.0));
                ctx.ClosePath();
                ctx.Stroke();
            }

            // Vanilla tick marks across the whole bar.
            int lineCount = Math.Min(50, (int)(curMax / lineInterval));
            for (int i = 1; i < lineCount; i++)
            {
                ctx.NewPath();
                ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.5);
                double tickX = Bounds.InnerWidth * (double)i / lineCount;
                ctx.MoveTo(tickX, 0.0);
                ctx.LineTo(tickX, Math.Max(3.0, Bounds.InnerHeight - 1.0));
                ctx.ClosePath();
                ctx.Stroke();
            }
        }

        generateTexture(surface, ref fillTexture);
    }

    /// <summary>
    /// Segments in drain order.
    /// </summary>
    private IEnumerable<(double Width, double[] Color)> Segments(double normalWidth, double poisonWidth, double psyWidth)
    {
        yield return (normalWidth, barColor);
        yield return (psyWidth, PsychedelicColor);
        yield return (poisonWidth, PoisonColor);
    }

    private void RecomposeHover()
    {
        if (!ShowValueOnHover)
            return;

        capi.Gui.TextTexture.GenOrUpdateTextTexture(onGetStatbarValue(), valueFont, ref hoverTexture, new TextBackground
        {
            FillColor = GuiStyle.DialogStrongBgColor,
            Padding = 5,
            BorderWidth = 2.0,
        });
    }

    /// <summary>
    /// Dante's 7th circle of hell.
    /// </summary>
    private void EnsureGlowComposed()
    {
        if (glowComposed)
            return;

        glowComposed = true;
        fillHeight = Bounds.OuterHeightInt + 1;
        using var surface = new ImageSurface(Format.Argb32, Bounds.OuterWidthInt + (int)GuiElement.scaled(28.0), Bounds.OuterHeightInt + (int)GuiElement.scaled(28.0));
        using var ctx = new Context(surface);
        ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.0);
        ctx.Paint();
        GuiElement.RoundRectangle(ctx, (int)GuiElement.scaled(12.0), (int)GuiElement.scaled(12.0), Bounds.OuterWidthInt + (int)GuiElement.scaled(4.0), Bounds.OuterHeightInt + (int)GuiElement.scaled(4.0), 1.0);
        ctx.SetSourceRGB(barColor[0], barColor[1], barColor[2]);
        ctx.FillPreserve();
        SurfaceTransformBlur.BlurFull(surface, GuiElement.scaled(4.0));
        ctx.Fill();
        SurfaceTransformBlur.BlurFull(surface, GuiElement.scaled(2.0));
        GuiElement.RoundRectangle(ctx, (int)GuiElement.scaled(15.0), (int)GuiElement.scaled(15.0), Bounds.OuterWidthInt - 2, Bounds.OuterHeightInt - 2, 1.0);
        ctx.Operator = Operator.Source;
        ctx.SetSourceRGBA(0.0, 0.0, 0.0, 0.0);
        ctx.Fill();
        generateTexture(surface, ref glowTexture);
    }

    public override void RenderInteractiveElements(float deltaTime)
    {
        double renderX = Bounds.renderX;
        double renderY = Bounds.renderY;
        if (curValue >= curMax && HideWhenFull)
            return;

        float flash = 0f;
        if (ShouldFlash)
        {
            FlashTime += 6f * deltaTime;
            flash = GameMath.Sin(FlashTime);
            if (flash < 0f)
            {
                ShouldFlash = false;
                FlashTime = 0f;
            }

            if (FlashTime < (float)Math.PI / 2f)
            {
                flash = Math.Min(1f, flash * 3f);
            }
        }

        int shakeX = (int)GuiElement.scaled((capi.World.Rand.NextDouble() * 10.0 - 5.0) * flash);
        int shakeY = (int)GuiElement.scaled((capi.World.Rand.NextDouble() * 10.0 - 5.0) * flash);
        renderX += shakeX;
        renderY += shakeY;

        capi.Render.RenderTexture(chromeTexture.TextureId, renderX, renderY, Bounds.OuterWidthInt + 1, Bounds.OuterHeightInt + 1);

        if (flash > 0f)
        {
            EnsureGlowComposed();
            capi.Render.RenderTexture(glowTexture.TextureId, renderX - (int)GuiElement.scaled(14.0), renderY - (int)GuiElement.scaled(14.0), Bounds.OuterWidthInt + (int)GuiElement.scaled(28.0), Bounds.OuterHeightInt + (int)GuiElement.scaled(28.0), 50f, new Vec4f(1.5f, 1f, 1f, flash));
        }

        if (fillTexture.TextureId > 0)
        {
            capi.Render.RenderTexture(fillTexture.TextureId, renderX, renderY, Bounds.OuterWidthInt + 1, fillHeight);
        }

        if (ShowValueOnHover && Bounds.PointInside(capi.Input.MouseX, capi.Input.MouseY))
        {
            double posX = capi.Input.MouseX + 16.0;
            double posY = capi.Input.MouseY + hoverTexture.Height - 4.0;
            capi.Render.RenderTexture(hoverTexture.TextureId, posX, posY, hoverTexture.Width, hoverTexture.Height, 2000f);
        }
    }

    private string CompositeValueText()
    {
        double shownMax = Math.Round(curMax);
        float special = poisonAmount + psyAmount;
        if (special <= 0.01f)
            return $"{Math.Round(curValue, 1)} / {shownMax}";

        float normal = Math.Max(0f, curValue - special);
        var text = $"{Math.Round(normal, 1)} sat";
        if (poisonAmount > 0.01f)
        {
            text += $" · Poison {Math.Round(poisonAmount, 1)} ({Math.Round(poisonLoad, 1)})";
        }

        if (psyAmount > 0.01f)
        {
            text += $" · Psy {Math.Round(psyAmount, 1)} ({Math.Round(psyLoad, 1)})";
        }

        return $"{text} / {shownMax}";
    }

    public override void Dispose()
    {
        base.Dispose();
        chromeTexture.Dispose();
        fillTexture.Dispose();
        glowTexture.Dispose();
        hoverTexture.Dispose();
    }
}
